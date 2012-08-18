using SimpleOwinAspNetHost;
using SocketIoDotNet;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Routing;

namespace AspNetSample.samples.echo
{
    public class EchoSample
    {
        public void Init()
        {
            const string root = "samples/echo/socketiodotnet";

            var io = new SocketIo();

            io.Configure(config =>
                             {
                                 config.Heartbeats = 25;
                                 config.CloseTimeout = 60;
                             });

            RouteTable.Routes.Add(new Route(root + "/{*pathInfo}", new SimpleOwinAspNetRouteHandler(io.App, root)));
        }
    }
}