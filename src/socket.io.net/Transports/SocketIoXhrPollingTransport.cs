using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SocketIoDotNet.Transports
{
    public class SocketIoXhrPollingTransport : ISocketIoTransport
    {
        public string Name
        {
            get { return "xhr-polling"; }
        }

        public Task<Tuple<IDictionary<string, object>, int, IDictionary<string, string[]>, Func<System.IO.Stream, Task>>> HandleRequest(string id, IDictionary<string, object> environment, IDictionary<string, string[]> headers, System.IO.Stream body)
        {
            throw new NotImplementedException();
        }
    }
}
