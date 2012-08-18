using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using ResultTuple = System.Tuple< //Result
    System.Collections.Generic.IDictionary<string, object>, // Properties
    int, // Status
    System.Collections.Generic.IDictionary<string, string[]>, // Headers
    System.Func< // CopyTo
        System.IO.Stream, // Body
        System.Threading.Tasks.Task>>; // Done

namespace SocketIoDotNet.Transports
{
    public class SocketIoXhrPollingTransport : ISocketIoTransport
    {
        public string Name
        {
            get { return "xhr-polling"; }
        }

        public Task<ResultTuple> HandleRequest(string id, IDictionary<string, object> environment, IDictionary<string, string[]> headers, System.IO.Stream body)
        {
            throw new NotImplementedException();
        }
    }
}
