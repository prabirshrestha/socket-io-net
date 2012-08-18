using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Reflection;
using System.Globalization;
using SocketIoDotNet.Transports;

using AppAction = System.Func< // Call
        System.Collections.Generic.IDictionary<string, object>, // Environment
        System.Collections.Generic.IDictionary<string, string[]>, // Headers
        System.IO.Stream, // Body
        System.Threading.Tasks.Task<System.Tuple< // Result
            System.Collections.Generic.IDictionary<string, object>, // Properties
            int, // Status
            System.Func< // CopyTo
            System.Collections.Generic.IDictionary<string, string[]>, // Headers
                System.IO.Stream, // Body
                System.Threading.Tasks.Task>>>>; // Done

using ResultTuple = System.Tuple< //Result
    System.Collections.Generic.IDictionary<string, object>, // Properties
    int, // Status
    System.Collections.Generic.IDictionary<string, string[]>, // Headers
    System.Func< // CopyTo
        System.IO.Stream, // Body
        System.Threading.Tasks.Task>>; // Done

using BodyAction = System.Func< // CopyTo
    System.IO.Stream, // Body
    System.Threading.Tasks.Task>; // Done

using IdGeneratorFunc = System.Func<
    System.Collections.Generic.IDictionary<string, object>, // environment
    System.Collections.Generic.IDictionary<string, string[]>, // headers
    string>; // id

namespace SocketIoDotNet
{
    public class SocketIo
    {
        private readonly SocketIoConfig _config;
        private readonly IdGeneratorFunc _idGenerator;
        private readonly IDictionary<string, ISocketIoTransport> _transports;
        private readonly string _transportCommaList;
        private readonly string[] _tranportArrays;

        private static readonly IEnumerable<ISocketIoTransport> DefaultTransports =
            new ISocketIoTransport[] 
            { 
                new SocketIoWebSocketTransport(),
                new SocketIoHtmlFileTransport(),
                new SocketIoXhrPollingTransport(),
                new SocketIoJsonpTransport(),
            };

        public SocketIo()
            : this(new SocketIoConfig())
        {
        }

        public SocketIo(SocketIoConfig config)
        {
            if (config == null)
                throw new ArgumentNullException("config");

            _config = config;
            _idGenerator = config.GenerateId ?? DefaultIdGenerator;

            var transports = config.Transports ?? DefaultTransports;

            _transports = new Dictionary<string, ISocketIoTransport>();

            // cache transports in dictionary for faster lookup
            foreach (var transport in transports)
            {
                if (transport == null)
                    continue;
                _transports[transport.Name] = transport;
            }

            // cache so we don't have to join for every request
            _transportCommaList = string.Join(",", _transports.Keys);
            _tranportArrays = _transports.Keys.ToArray();
        }

        public int Protocol
        {
            get { return 1; }
        }

        public Task<ResultTuple> App(IDictionary<string, object> environment, IDictionary<string, string[]> headers, Stream body)
        {
            var data = CheckRequest(environment, headers);
            environment["socketiodotnet.HostProtocol"] = data.SocketIoProtcol = Protocol;
            environment["socketiodotnet.Transports"] = _transports;

            //environment["socketionet.Transports"] = _transports.Keys.ToArray();

            if (data.IsStatic || string.IsNullOrEmpty(data.Transport) && data.Protocol == 0)
            {
                var browserClient = true;
                if (data.IsStatic && browserClient)
                {
                    if (data.Path == "/socket.io.min.js")
                        return AssetResultTuple("SocketIoDotNet.assets.socket.io.min.js", "text/javascript");
                    if (data.Path == "/socket.io.js")
                        return AssetResultTuple("SocketIoDotNet.assets.socket.io.js", "text/javascript");
                    if (data.Path == "WebSocketMain.swf")
                        return AssetResultTuple("SocketIoDotNet.assets.WebSocketMain.swf", "application/x-shockwave-flash");
                    if (data.Path == "WebSocketMainInsecure.swf")
                        return AssetResultTuple("SocketIoDotNet.assets.WebSocketMainInsecure.swf", "application/x-shockwave-flash");
                    return StringResultTuple(":( not found", 404);
                }
                else
                {
                    return StringResultTuple("Welcome to socket.io.", 200);
                }
            }

            if (data.Protocol != Protocol)
                return StringResultTuple("Protocol version not supported", 500);

            if (string.IsNullOrEmpty(data.Id))
                return HandleHandshake(data, environment, headers, body);
            else
                return HandleHttpRequest(data, environment, headers, body);

            throw new NotImplementedException();
        }

        private static SocketIoRequestData CheckRequest(IDictionary<string, object> environment, IDictionary<string, string[]> headers)
        {
            var data = new SocketIoRequestData();

            var path = (string)environment["owin.RequestPath"];
            data.Path = path;

            var match = path.IndexOf("/socket.io") >= 0;

            var pieces = path.Split('/');

            environment["socketiodotnet.RequestPath"] = data.Path = path;

            if (path.IndexOf("/") == 0)
            {
                if (!match)
                {
                    int protocol = 0;
                    if (pieces.Length > 1)
                    {
                        int.TryParse(pieces[1], out protocol);

                        if (pieces.Length > 2)
                        {
                            environment["socketiodotnet.Transport"] = data.Transport = pieces[2];
                            if (pieces.Length > 3)
                                environment["socketiodotnet.Id"] = data.Id = pieces[3];
                        }
                    }

                    environment["socketiodotnet.Protocol"] = data.Protocol = protocol;
                }

                data.IsStatic = match;
            }


            return data;
        }

        private async Task<ResultTuple> HandleHandshake(SocketIoRequestData data, IDictionary<string, object> environment, IDictionary<string, string[]> headers, Stream body)
        {
            string error;
            bool authorized = await OnAuthorize(environment, headers, out error);

            if (!string.IsNullOrEmpty(error))
            {
                return await StringResultTuple(error, 500);
            }

            if (!authorized)
            {
                return await StringResultTuple("handshake unauthorized", 403);
            }

            var id = _idGenerator(environment, headers);

            var hs = string.Join(":",
                id,
                _config.Heartbeats == 0 ? "" : _config.Heartbeats.ToString(),
                _config.CloseTimeout == 0 ? "" : _config.CloseTimeout.ToString(),
                _transportCommaList);

            var jsonP = false;

            if (jsonP)
            {
                throw new NotImplementedException();
                return await StringResultTuple("todo", 200, "application/javascript");
            }
            else
            {
                return await StringResultTuple(hs, 200);
            }
        }

        private async Task<ResultTuple> HandleHttpRequest(SocketIoRequestData data, IDictionary<string, object> environment, IDictionary<string, string[]> headers, Stream body)
        {
            ISocketIoTransport transport = null;
            if (!_transports.TryGetValue(data.Transport, out transport))
                return await StringResultTuple("transport not supported", 500);

            try
            {
                return await transport.HandleRequest(data.Id, environment, headers, body);
            }
            catch (Exception ex)
            {
                throw new NotImplementedException();
            }
        }

        protected virtual Task<bool> OnAuthorize(IDictionary<string, object> environment, IDictionary<string, string[]> headers, out string error)
        {
            error = null;
            return Task.FromResult<bool>(true);
        }

        private static Task<ResultTuple> AssetResultTuple(string assetName, string contentType)
        {
            var owinResponseProperties = new Dictionary<string, object>();
            var owinResponseStatus = 200;
            var owinResponseHeaders = new Dictionary<string, string[]>();
            owinResponseHeaders.Add("Content-Type", new[] { contentType });

            var resultTuple = new ResultTuple(
                owinResponseProperties,
                owinResponseStatus,
                owinResponseHeaders,
                async output =>
                {
                    using (var stream = Assembly.GetExecutingAssembly().GetManifestResourceStream(assetName))
                    {
                        await stream.CopyToAsync(output);
                    }
                });

            return Task.FromResult<ResultTuple>(resultTuple);
        }

        internal static Task<ResultTuple> StringResultTuple(string str, int statusCode, string contentType = "text/plain")
        {
            var owinResponseProperties = new Dictionary<string, object>();
            var owinResponseStatus = statusCode;
            var owinResponseHeaders = new Dictionary<string, string[]>();
            owinResponseHeaders.Add("Content-Type", new[] { contentType });

            var resultTuple = new ResultTuple(
                owinResponseProperties,
                owinResponseStatus,
                owinResponseHeaders,
                async output =>
                {
                    var buffer = Encoding.UTF8.GetBytes(str);
                    await output.WriteAsync(buffer, 0, buffer.Length);
                });

            return Task.FromResult<ResultTuple>(resultTuple);
        }

        internal static string DefaultIdGenerator(IDictionary<string, object> environment, IDictionary<string, string[]> headers)
        {
            return Guid.NewGuid().ToString("N", CultureInfo.InvariantCulture);
        }

        private class SocketIoRequestData
        {
            public string Path;
            public string Transport;
            public int Protocol;
            public string Id;

            public int SocketIoProtcol;

            internal bool IsStatic;

        }
    }
}
