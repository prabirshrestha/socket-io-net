using SocketIoDotNet.Transports;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

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

using BodyAction = System.Func< // CopyTo
    System.IO.Stream, // Body
    System.Threading.Tasks.Task>; // Done

using IdGeneratorFunc = System.Func<
    System.Collections.Generic.IDictionary<string, object>, // environment
    System.Collections.Generic.IDictionary<string, string[]>, // headers
    string>; // id

using ResultTuple = System.Tuple< //Result
    System.Collections.Generic.IDictionary<string, object>, // Properties
    int, // Status
    System.Collections.Generic.IDictionary<string, string[]>, // Headers
    System.Func< // CopyTo
        System.IO.Stream, // Body
        System.Threading.Tasks.Task>>; // Done

namespace SocketIoDotNet
{
    public class SocketIo
    {
        private readonly SocketIoConfig _config = new SocketIoConfig();

        public SocketIo Configure(Action<SocketIoConfig> config)
        {
            if (config == null) throw new ArgumentNullException("config");
            config(_config);
            return this;
        }

        public int Protocol
        {
            get { return 1; }
        }

        public Task<ResultTuple> App(IDictionary<string, object> environment, IDictionary<string, string[]> headers, Stream body)
        {
            var data = CheckRequest(environment, headers);
            data.Config = _config;
            environment["socketiodotnet.HostProtocol"] = data.HostProtocol = Protocol;
            environment["socketiodotnet.Transports"] = _config.Transports;

            if (data.IsStatic || string.IsNullOrEmpty(data.Transport) && data.RequestProtocol == 0)
            {
                var browserClient = true;
                if (data.IsStatic && browserClient)
                {
                    if (data.Path == "/socket.io.min.js")
                        return AssetResultTuple("SocketIoDotNet.assets.socket.io.min.js", "text/javascript");
                    if (data.Path == "/socket.io.js")
                        return AssetResultTuple("SocketIoDotNet.assets.socket.io.js", "text/javascript");
                    if (data.Path == "/WebSocketMain.swf")
                        return AssetResultTuple("SocketIoDotNet.assets.WebSocketMain.swf", "application/x-shockwave-flash");
                    if (data.Path == "/WebSocketMainInsecure.swf")
                        return AssetResultTuple("SocketIoDotNet.assets.WebSocketMainInsecure.swf", "application/x-shockwave-flash");
                    return StringResultTuple(":( not found", 404);
                }
                else
                {
                    return StringResultTuple("Welcome to socket.io.", 200);
                }
            }

            if (data.RequestProtocol != Protocol)
                return StringResultTuple("Protocol version not supported", 500);

            return string.IsNullOrEmpty(data.Id)
                       ? HandleHandshake(data, environment, headers, body)
                       : HandleHttpRequest(data, environment, headers, body);
        }

        private static SocketIoRequestData CheckRequest(IDictionary<string, object> environment, IDictionary<string, string[]> headers)
        {
            var data = new SocketIoRequestData();

            var path = (string)environment["owin.RequestPath"];
            data.Path = path;

            var match = path.IndexOf("/socket.io", StringComparison.Ordinal) >= 0;

            var pieces = path.Split('/');

            environment["socketiodotnet.RequestPath"] = data.Path = path;

            if (path.IndexOf("/", StringComparison.Ordinal) == 0)
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

                    environment["socketiodotnet.RequestProtocol"] = data.RequestProtocol = protocol;
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

            var id = _config.IdGenerator(environment, headers);

            var hs = string.Join(":",
                id,
                _config.Heartbeats.HasValue ? _config.Heartbeats.Value.ToString(CultureInfo.InvariantCulture) : string.Empty,
                _config.CloseTimeout.HasValue ? _config.CloseTimeout.Value.ToString(CultureInfo.InvariantCulture) : string.Empty,
                _config.TransportsStringList);

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
            ISocketIoTransport transport;
            if (!_config.TransportDictionary.TryGetValue(data.Transport, out transport))
                return await StringResultTuple("transport not supported", 500);

            try
            {
                return await transport.HandleRequest(data, environment, headers, body);
            }
            catch (Exception ex)
            {
                throw new NotImplementedException("not implemented", ex);
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

        internal static Task<ResultTuple> StringResponseResult(SocketIoRequestData data, string str)
        {
            return StringResultTuple(
                string.Format("{0}:{1}:{2}:{3}",
                              data.RequestProtocol,
                              data.Config.GetHeartbeatStringValue(),
                              data.Config.GetCloseTimeoutStringValue(), 
                              str),
                200);
        }
    }
}
