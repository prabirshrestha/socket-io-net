using System;
using System.Collections.Generic;
using System.IO;
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
    public class SocketIoJsonpTransport : ISocketIoTransport
    {
        public string Name
        {
            get { return "jsonp-polling"; }
        }

        public Task<ResultTuple> HandleRequest(SocketIoRequestData data, IDictionary<string, object> environment, IDictionary<string, string[]> headers, Stream body)
        {
            throw new NotImplementedException();
        }
    }
}
