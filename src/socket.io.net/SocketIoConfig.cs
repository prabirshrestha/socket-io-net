using SocketIoDotNet.Transports; // generated id
using System.Collections.Generic;

using IdGeneratorFunc = System.Func<
    System.Collections.Generic.IDictionary<string, object>, // environment
    System.Collections.Generic.IDictionary<string, string[]>, // headers
    string>; // id

namespace SocketIoDotNet
{
    public class SocketIoConfig
    {
        private const int DefaultHeartbeats = 25;
        private const int DefaultCloseTimeout = 60;

        public SocketIoConfig()
        {
            Heartbeats = DefaultHeartbeats;
            CloseTimeout = DefaultCloseTimeout;
        }

        public IEnumerable<ISocketIoTransport> Transports { get; set; }

        public IdGeneratorFunc IdGenerator { get; set; }

        public int? Heartbeats { get; set; }

        public int? CloseTimeout { get; set; }

        public bool HostSupportsWebSockets { get; set; }
    }
}
