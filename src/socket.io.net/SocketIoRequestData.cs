
namespace SocketIoDotNet
{
    public class SocketIoRequestData
    {
        internal string Path { get; set; }
        internal bool IsStatic;
        
        public string Transport { get; set; }
        public int RequestProtocol { get; set; }

        public string Id { get; set; }
        public int HostProtocol { get; set; }

        public SocketIoConfig Config { get; set; }
    }
}
