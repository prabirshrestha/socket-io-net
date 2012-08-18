using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SocketIoDotNet.Transports
{
    public interface ISocketIoTransport
    {
        string Name { get; }
    }
}
