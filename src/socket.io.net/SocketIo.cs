namespace SocketIoDotNet
{
    using System;
    using System.Globalization;
    using System.Threading.Tasks;

    using AppFunc = System.Func<System.Collections.Generic.IDictionary<string, object>, System.Threading.Tasks.Task>;

    public class SocketIo
    {
        public int Protocol
        {
            get { return 1; }
        }

        public AppFunc App()
        {
            return
                async env =>
                {
                    var context = new SocketIoContext(env);

                    if (context.IsStatic || string.IsNullOrEmpty(context.Transport) && context.Protocol == 0)
                    {
                        var browserClient = true;
                        if (context.IsStatic && browserClient)
                        {
                            if (context.Path == "/socket.io.min.js")
                                await context.StaticResource("SocketIoDotNet.assets.socket.io.min.js", "text/javascript");
                            else if (context.Path == "/socket.io.js")
                                await context.StaticResource("SocketIoDotNet.assets.socket.io.js", "text/javascript");
                            else if (context.Path == "/WebSocketMain.swf")
                                await context.StaticResource("SocketIoDotNet.assets.WebSocketMain.swf", "application/x-shockwave-flash");
                            else if (context.Path == "WebSocketMainInsecure.swf")
                                await context.StaticResource("SocketIoDotNet.assets.WebSocketMainInsecure.swf", "application/x-shockwave-flash");
                            else
                                await context.WriteString(":( not found", 404);
                            return;
                        }
                        else
                        {
                            await context.WriteString("Welcome to socket.io", 200);
                            return;
                        }
                    }

                    if (context.Protocol != Protocol)
                    {
                        await context.WriteString("Protocol version not supported", 500);
                        return;
                    }

                    if (string.IsNullOrWhiteSpace(context.Id))
                        await HandleHandshake(context);
                    else
                        await HandleHttpRequest(context);
                };
        }

        private async Task HandleHandshake(SocketIoContext context)
        {
            string error;
            bool authorized = await OnAuthorize(context, out error);

            if (!string.IsNullOrEmpty(error))
            {
                await context.WriteString(error, 500);
                return;
            }

            if (!authorized)
            {
                context.WriteString("handshake unauthorized", 403);
                return;
            }

            var id = "123"; // todo: call id generator
            int? heartbeats = 40;
            int? closeTimeout = 40;
            string supportedTransports = "xhr-polling";

            var hs = string.Join(":",
                 id,
                 heartbeats.HasValue ? heartbeats.Value.ToString(CultureInfo.InvariantCulture) : string.Empty,
                 closeTimeout.HasValue ? closeTimeout.Value.ToString(CultureInfo.InvariantCulture) : string.Empty,
                 supportedTransports);

            var jsonP = false;

            if (jsonP)
            {
                throw new NotImplementedException();
            }
            else
            {
                await context.WriteString(hs, 200);
            }
        }

        private Task<bool> OnAuthorize(SocketIoContext context, out string error)
        {
            error = null;
            return Task.FromResult(true);
        }

        private async Task HandleHttpRequest(SocketIoContext context)
        {
            throw new NotImplementedException();
        }
    }
}