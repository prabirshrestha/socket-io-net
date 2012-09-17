using System.Threading.Tasks;

namespace SocketIoDotNet.Transports
{
    public interface ISocketIoTransport
    {
        string Name { get; }

        Task HandleRequest(SocketIoContext context);
    }
}
