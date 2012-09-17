using SocketIoDotNet.Transports;
using System;
using System.Collections.Generic;
using System.Globalization;

namespace SocketIoDotNet
{
    using IdGeneratorFunc = System.Func< // generated id
        SocketIoContext, // environment
        string>; // id

    public class SocketIoConfig
    {
        private const int DefaultHeartbeats = 25;
        private const int DefaultCloseTimeout = 60;

        private static readonly ISocketIoTransport[] DefaultTransports =
           new ISocketIoTransport[] 
            { 
                //new SocketIoWebSocketTransport(),
                //new SocketIoHtmlFileTransport(),
                new SocketIoXhrPollingTransport(),
                //new SocketIoJsonpTransport(),
            };

        private IdGeneratorFunc _idGenerator;
        private ISocketIoTransport[] _transports;

        internal readonly IDictionary<string, ISocketIoTransport> TransportDictionary;
        internal string TransportsStringList;
        private bool _hostSupportsWebSockets;

        public SocketIoConfig()
        {
            TransportDictionary = new Dictionary<string, ISocketIoTransport>();
            UpdateInternalTransportData(Transports);
            Heartbeats = DefaultHeartbeats;
            CloseTimeout = DefaultCloseTimeout;
        }

        public IdGeneratorFunc IdGenerator
        {
            get { return _idGenerator ?? (_idGenerator = DefaultIdGenerator); }
            set { _idGenerator = value; }
        }

        public ISocketIoTransport[] Transports
        {
            get
            {
                var transports = _transports;
                if (transports == null)
                {
                    transports = DefaultTransports;
                    UpdateInternalTransportData(transports);
                }
                else if (transports.Length == 0)
                {
                    transports = DefaultTransports;
                    UpdateInternalTransportData(transports);
                }

                return _transports = transports;
            }
            set { _transports = value; }
        }

        public int? Heartbeats { get; set; }

        public int? CloseTimeout { get; set; }

        internal string GetHeartbeatStringValue()
        {
            return Heartbeats.HasValue ? Heartbeats.Value.ToString(CultureInfo.InvariantCulture) : "";
        }

        internal string GetCloseTimeoutStringValue()
        {
            return CloseTimeout.HasValue ? CloseTimeout.Value.ToString(CultureInfo.InvariantCulture) : "";
        }

        public bool HostSupportsWebSockets
        {
            get { return _hostSupportsWebSockets; }
            set
            {
                _hostSupportsWebSockets = value;
                UpdateInternalTransportData(Transports);
            }
        }

        private static string DefaultIdGenerator(SocketIoContext context)
        {
            return Guid.NewGuid().ToString("N", CultureInfo.InvariantCulture);
        }

        private void UpdateInternalTransportData(ISocketIoTransport[] transports)
        {
            TransportDictionary.Clear();

            foreach (var transport in transports)
            {
                if (!HostSupportsWebSockets && transport.Name == "websocket")
                    continue;
                TransportDictionary[transport.Name] = transport;
            }

            TransportsStringList = string.Join(",", TransportDictionary.Keys);
        }
    }
}
