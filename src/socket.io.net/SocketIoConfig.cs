using SocketIoDotNet.Transports; // generated id
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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

        private int _heartbeats = DefaultHeartbeats;
        private int _closeTimeout = DefaultCloseTimeout;

        public IEnumerable<ISocketIoTransport> Transports { get; set; }

        public IdGeneratorFunc GenerateId { get; set; }

        public int Heartbeats
        {
            get
            {
                if (_heartbeats < 0)
                    _heartbeats = DefaultHeartbeats;
                return _heartbeats;
            }
            set { _heartbeats = value; }
        }

        public int CloseTimeout
        {
            get
            {
                if (_closeTimeout < 0)
                    _closeTimeout = DefaultCloseTimeout;
                return _closeTimeout;
            }
            set { _closeTimeout = value; }
        }
    }
}
