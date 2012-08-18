using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;

using ResultTuple = System.Tuple< //Result
    System.Collections.Generic.IDictionary<string, object>, // Properties
    int, // Status
    System.Collections.Generic.IDictionary<string, string[]>, // Headers
    System.Func< // CopyTo
        System.IO.Stream, // Body
        System.Threading.Tasks.Task>>; // Done

namespace SocketIoDotNet.Transports
{
    public interface ISocketIoTransport
    {
        string Name { get; }

        Task<ResultTuple> HandleRequest(string id, IDictionary<string, object> environment, IDictionary<string, string[]> headers, Stream body);
    }
}
