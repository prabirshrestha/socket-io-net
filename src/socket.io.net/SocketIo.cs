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
        public Task<ResultTuple> App(IDictionary<string, object> environment, IDictionary<string, string[]> headers, Stream body)
        {
            var data = CheckRequest(environment, headers);
            if (data.IsStatic)
            {
                var browserClient = true;
                if (data.IsStatic && browserClient)
                {
                    if(data.Path == "/socket.io.min.js")
                        return AssetResultTuple("SocketIoDotNet.assets.socket.io.min.js", "text/javascript");
                    if (data.Path == "/socket.io.js")
                        return AssetResultTuple("SocketIoDotNet.assets.socket.io.js", "text/javascript");
                    if(data.Path == "WebSocketMain.swf")
                        return AssetResultTuple("SocketIoDotNet.assets.WebSocketMain.swf", "application/x-shockwave-flash");
                    if (data.Path == "WebSocketMainInsecure.swf")
                        return AssetResultTuple("SocketIoDotNet.assets.WebSocketMainInsecure.swf", "application/x-shockwave-flash");
                }
            }

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
                }

                requestData.IsStatic = match;
            }

            return requestData;
        }

        private Task<ResultTuple> AssetResultTuple(string assetName, string contentType)
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

        private class RequestData
        {
            public string Path;
            public string Transport;
            public int Protocol;
            public bool IsStatic;
        }
    }
}
