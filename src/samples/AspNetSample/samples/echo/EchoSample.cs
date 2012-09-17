using System.Web;
using AspNetSample.samples.echo;
using SocketIoDotNet;
using System.Web.Routing;
using SimpleOwin.Hosts.AspNet;

[assembly: PreApplicationStartMethod(
  typeof(EchoSample), "Initialize")]

namespace AspNetSample.samples.echo
{
    public class EchoSample
    {
        public static void Initialize()
        {
            var io = new SocketIo();

            // aspnet specific
            const string root = "samples/echo/socketiodotnet";
            RouteTable.Routes.Add(
                new Route(root + "/{*pathInfo}", new SimpleOwinAspNetRouteHandler(io.App(), root)));
        }
    }
}