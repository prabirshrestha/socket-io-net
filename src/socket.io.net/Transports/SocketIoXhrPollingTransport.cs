using System;
using System.Threading.Tasks;

namespace SocketIoDotNet.Transports
{
    public class SocketIoXhrPollingTransport : ISocketIoTransport
    {
        public string Name
        {
            get { return "xhr-polling"; }
        }

        public async Task HandleRequest(SocketIoContext context)
        {
            throw new NotImplementedException();
        }
    }
}
