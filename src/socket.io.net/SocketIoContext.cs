namespace SocketIoDotNet
{
    using System;
    using System.Collections.Generic;
    using System.IO;

    public class SocketIoContext
    {
        private IDictionary<string, object> environment;
        private int responseStatusCode;

        public SocketIoContext(IDictionary<string, object> environment)
        {
            this.environment = environment;
            Path = (string)environment["owin.RequestPath"];
            environment["socketiodotnet.RequestPath"] = Path;
            RequestBody = (Stream)environment["owin.RequestBody"];
            ResponseBody = (Stream)environment["owin.ResponseBody"];
            RequestHeaders = (IDictionary<string, string[]>) environment["owin.RequestHeaders"];
            ResponseHeaders = (IDictionary<string, string[]>) environment["owin.ResponseHeaders"];
            ResponseStatusCode = 200;

            var match = Path.IndexOf("/socket.io", StringComparison.Ordinal) >= 0;
            var pieces = Path.Split('/');

            if (Path.IndexOf("/", StringComparison.Ordinal) == 0)
            {
                if (!match)
                {
                    int protocol = 0;
                    if (pieces.Length > 1)
                    {
                        int.TryParse(pieces[1], out protocol);

                        if (pieces.Length > 2)
                        {
                            environment["socketiodotnet.Transport"] = Transport = pieces[2];
                            if (pieces.Length > 3)
                                environment["socketiodotnet.Id"] = Id = pieces[3];
                        }
                    }

                    environment["socketiodotnet.RequestProtocol"] = Protocol = protocol;
                }

                IsStatic = match;
            }
        }

        public string Path { get; private set; }
        public string Transport { get; private set; }
        public string Id { get; private set; }
        public int Protocol { get; private set; }
        public bool IsStatic { get; private set; }

        public Stream RequestBody { get; private set; }
        public Stream ResponseBody { get; private set; }

        public IDictionary<string, string[]> RequestHeaders { get; private set; }
        public IDictionary<string, string[]> ResponseHeaders { get; private set; }

        public int ResponseStatusCode
        {
            get { return responseStatusCode; }
            set
            {
                responseStatusCode = value;
                this.environment["owin.ResponseStatusCode"] = value;
            }
        }
    }
}