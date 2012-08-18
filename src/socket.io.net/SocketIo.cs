using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Reflection;

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

namespace SocketIoDotNet
{
    public class SocketIo
    {
        public int Protocol
        {
            get { return 1; }
        }

        public Task<ResultTuple> App(IDictionary<string, object> environment, IDictionary<string, string[]> headers, Stream body)
        {
            var data = CheckRequest(environment, headers);
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

        private RequestData CheckRequest(IDictionary<string, object> environment, IDictionary<string, string[]> headers)
        {
            var requestData = new RequestData();

            var path = (string)environment["owin.RequestPath"];
            requestData.Path = path;

            var match = path.IndexOf("/socket.io") >= 0;

            var pieces = path.Split('/');

            requestData.Path = path;
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
                            requestData.Transport = pieces[2];
                            if (pieces.Length > 3)
                                requestData.Id = pieces[3];
                        }
                    }

                    requestData.Protocol = protocol;
                }

                requestData.IsStatic = match;
            }

            return requestData;
        }

        private async Task<ResultTuple> HandleHandshake(RequestData data, IDictionary<string, object> environment, IDictionary<string, string[]> headers, Stream body)
        {
            string error;
            bool authorized = await OnAuthorize(environment, headers, out error);

            var owinResponseProperties = new Dictionary<string, object>();
            var owinResponseStatus = 200;
            var owinResponseHeaders = new Dictionary<string, string[]>();
            owinResponseHeaders.Add("Content-Type", new[] { "text/plain" });

            if (!string.IsNullOrEmpty(error))
            {
                return await StringResultTuple(error, 500);
            }

            if (!authorized)
            {
                return await StringResultTuple("handshake unauthorized", 403);
            }
               
            var id = GenerateId(environment, headers);
            

            throw new NotImplementedException();
        }

        private Task<ResultTuple> HandleHttpRequest(RequestData data, IDictionary<string, object> environment, IDictionary<string, string[]> headers, Stream body)
        {
            throw new NotImplementedException();
        }

        protected virtual Task<bool> OnAuthorize(IDictionary<string, object> environment, IDictionary<string, string[]> headers, out string error)
        {
            error = null;
            return Task.FromResult<bool>(true);
        }

        protected string GenerateId(IDictionary<string, object> environment, IDictionary<string, string[]> headers)
        {
            return "idnumber1";
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

        private static Task<ResultTuple> StringResultTuple(string str, int statusCode)
        {
            var owinResponseProperties = new Dictionary<string, object>();
            var owinResponseStatus = statusCode;
            var owinResponseHeaders = new Dictionary<string, string[]>();
            owinResponseHeaders.Add("Content-Type", new[] { "text/plain" });

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

        private class RequestData
        {
            public string Path;
            public string Transport;
            public int Protocol;
            public bool IsStatic;
            public string Id;
        }
    }
}
