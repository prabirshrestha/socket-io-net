namespace SocketIoDotNet
{
    using System.Reflection;
    using System.Text;
    using System.Threading.Tasks;

    static class SocketIoContextExtensions
    {
        public async static Task StaticResource(this SocketIoContext context, string assetName, string contentType)
        {
            context.ResponseStatusCode = 200;
            context.ResponseHeaders["content-type"] = new[] { contentType };

            using (var stream = Assembly.GetExecutingAssembly().GetManifestResourceStream(assetName))
            {
                await stream.CopyToAsync(context.ResponseBody);
            }
        }

        public async static Task WriteString(this SocketIoContext context, string str, int statusCode, string contentType = "text/plain")
        {
            context.ResponseStatusCode = statusCode;
            context.ResponseHeaders["content-type"] = new[] { contentType };

            await context.ResponseBody
                .WriteStringAsync(str);
        }

        public static void WriteString(this System.IO.Stream stream, string str, Encoding encoding)
        {
            var data = encoding.GetBytes(str);
            stream.Write(data, 0, data.Length);
        }

        public static void WriteString(this System.IO.Stream stream, string str)
        {
            stream.WriteString(str, Encoding.UTF8);
        }

        public static Task WriteStringAsync(this System.IO.Stream stream, string str, Encoding encoding)
        {
            var data = encoding.GetBytes(str);
            return stream.WriteAsync(data, 0, data.Length, null);
        }

        public static Task WriteStringAsync(this System.IO.Stream stream, string str)
        {
            var data = Encoding.UTF8.GetBytes(str);
            return stream.WriteAsync(data, 0, data.Length, null);
        }

        private static Task WriteAsync(this  System.IO.Stream stream, byte[] data, int offset, int count, object state)
        {
            return Task.Factory.FromAsync(stream.BeginWrite, stream.EndWrite, data, offset, count, state);
        }
    }
}