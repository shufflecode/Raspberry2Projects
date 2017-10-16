namespace libCore
{
    using System;
    using System.Collections.Generic;
    using Windows.ApplicationModel.Core;
    using Windows.Networking;
    using Windows.Networking.Connectivity;
    using Windows.Networking.Sockets;
    using Windows.Storage.Streams;
    using System.Linq;
    using System.Text;
    using System.Threading.Tasks;
    using System.Runtime.CompilerServices;
    using libShared;
    using libShared.Interfaces;

    public class Async_TCP_StreamSocketServer : IEthernetAsync
    {
        public event NotifyTextDelegate NotifyTextEvent;
        public event NotifyexceptionDelegate NotifyexceptionEvent;
        public event NotifyMessageReceivedDelegate NotifyMessageReceivedEvent;

        string ip;
        string port;
        private bool isConnected = false;
        List<string> hostNames = new List<string>();

        public Async_TCP_StreamSocketServer()
        {
            List<string> hostNames = new List<string>();
            var h = NetworkInformation.GetHostNames().Where(x => x.IPInformation != null && (x.IPInformation.NetworkAdapter.IanaInterfaceType == 71 || x.IPInformation.NetworkAdapter.IanaInterfaceType == 6));

            //1   Some other type of network interface.
            //6   An Ethernet network interface.
            //9   A token ring network interface.
            //23  A PPP network interface.
            //24  A software loopback network interface.
            //37  An ATM network interface.
            //71  An IEEE 802.11 wireless network interface.
            //131 A tunnel type encapsulation network interface.
            //144 An IEEE 1394 (Firewire) high performance serial bus network interface.

            foreach (var hn in h)
            {
                this.HostNames.Add(hn.DisplayName);
                this.HostNameOrIp = hn.DisplayName;
            }
        }

        public bool IsConnected
        {
            get { return isConnected; }
            internal set { isConnected = value; }
        }

        /// <summary>
        /// localHost Name / Local Ip (Name/IP des Servers)
        /// </summary>
        public string HostNameOrIp
        {
            get { return ip; }
            set { ip = value; }
        }

        /// <summary>
        /// localServiceName / Local Port (Servie-Name/Port des Servers)
        /// </summary>
        public string Port
        {
            get { return port; }
            set { port = value; }
        }

        public List<string> HostNames
        {
            get { return hostNames; }
            set { hostNames = value; }
        }

        private bool isRunning = false;
        private bool shouldStop = false;

        StreamSocketListener listener;

        List<Async_TCP_StreamSocketClient> serverClients = new List<Async_TCP_StreamSocketClient>();

        public async Task Start(string host, string localServiceName)
        {
            this.HostNameOrIp = host;
            this.Port = localServiceName;
            await Start();
        }

        /// <summary>
        /// StartListener
        /// </summary>
        /// <returns></returns>
        public async Task Start()
        {
            shouldStop = false;
            listener = new StreamSocketListener();

            HostName hostName;
            hostName = new HostName(this.HostNameOrIp);

            listener.ConnectionReceived += OnConnection;
            

            // If necessary, tweak the listener's control options before carrying out the bind operation.
            // These options will be automatically applied to the connected StreamSockets resulting from
            // incoming connections (i.e., those passed as arguments to the ConnectionReceived event handler).
            // Refer to the StreamSocketListenerControl class' MSDN documentation for the full list of control options.
            listener.Control.KeepAlive = false;

            // Don't limit traffic to an address or an adapter.
            //await listener.BindServiceNameAsync(localServiceName);
            await listener.BindEndpointAsync(hostName, this.Port);

            this.IsConnected = true;
        }

        public async Task Stop()
        {
            try
            {
                shouldStop = true;

                foreach (var client in this.serverClients)
                {
                    await client.Stop();
                }

                if (listener != null)
                {
                    await listener.CancelIOAsync();
                }

                this.IsConnected = false;
            }
            catch (Exception ex)
            {
                System.Exception exNew = new System.Exception(string.Format("Exception In: {0}", CallerName()), ex);
                this.Notifyexception(exNew);
                throw exNew;
            }
        }


        /// <summary>
        /// Invoked once a connection is accepted by StreamSocketListener.
        /// </summary>
        /// <param name="sender">The listener that accepted the connection.</param>
        /// <param name="args">Parameters associated with the accepted connection.</param>
        private void OnConnection(
            StreamSocketListener sender,
            StreamSocketListenerConnectionReceivedEventArgs args)
        {
            if (this.NotifyTextEvent != null)
            {
                this.NotifyTextEvent(this, string.Format("Recive connection from {0}", args.Socket.Information.RemoteHostName.DisplayName));
            }

            Async_TCP_StreamSocketClient client = new Async_TCP_StreamSocketClient(args.Socket);

            client.NotifyTextEvent += Client_NotifyTextEvent;
            client.NotifyexceptionEvent += Client_NotifyexceptionEvent;
            client.NotifyMessageReceivedEvent += Client_NotifyMessageReceivedEvent;
            client.NotifyConnectionClosedEvent += Client_NotifyConnectionClosedEvent;
            serverClients.Add(client);
            client.ReadAsync();
        }

        private void Client_NotifyConnectionClosedEvent(object sender)
        {
            var client = sender as Async_TCP_StreamSocketClient;

            if (this.NotifyTextEvent != null)
            {
                this.NotifyTextEvent(this, string.Format("Client connection closed {0}", client.HostNameOrIp));
            }

            if (serverClients.Contains(client))
            {
                serverClients.Remove(client);
            }

            
        }

        string CallerName([CallerMemberName]string caller = "")
        {
            return caller;
        }

        //async public Task<string> ReadAll()
        //{
        //    StringBuilder sb = new StringBuilder();

        //    try
        //    {
        //        foreach (var item in this.serverClients)
        //        {
        //            //string text = await item.ReadData();
        //            //sb.Append(text);//
        //            await item.ReadData();
        //        }
        //    }
        //    catch (Exception ex)
        //    {
        //        System.Exception exNew = new System.Exception(string.Format("Exception In: {0}", CallerName()), ex);
        //        this.Notifyexception(exNew);
        //        throw exNew;
        //    }

        //    return sb.ToString();
        //}

        /// <summary>
        /// SendTextToAll
        /// </summary>
        /// <param name="text"></param>
        async public Task SendText(string text)
        {
            try
            {
                if (string.IsNullOrEmpty(text))
                {
                    throw new Exception("Send value null or empty or lengt 0");
                }
                else
                {
                    foreach (var item in this.serverClients.Where(x => x.IsConnected))
                    {
                        await item.SendText(text);
                    }
                }
            }
            catch (Exception ex)
            {
                System.Exception exNew = new System.Exception(string.Format("Exception In: {0}", CallerName()), ex);
                this.Notifyexception(exNew);
                throw exNew;
            }
        }

        /// <summary>
        /// SendDataToAll
        /// </summary>
        /// <param name="data"></param>
        async public Task SendData(byte[] data)
        {
            try
            {
                if (data == null || data.Length == 0)
                {
                    throw new Exception("Send value null or empty or lengt 0");
                }
                else
                {
                    foreach (var item in this.serverClients.Where(x => x.IsConnected))
                    {
                        await item.SendData(data);
                    }
                }
            }
            catch (Exception ex)
            {
                System.Exception exNew = new System.Exception(string.Format("Exception In: {0}", CallerName()), ex);
                this.Notifyexception(exNew);
                throw exNew;
            }
        }

        private void Client_NotifyMessageReceivedEvent(object sender, byte[] data)
        {
            if (this.NotifyMessageReceivedEvent != null)
            {
                this.NotifyMessageReceivedEvent(sender, data);
            }
        }

        private void Notifyexception(Exception ex)
        {
            if (this.NotifyexceptionEvent != null)
            {
                this.NotifyexceptionEvent(this, ex);
            }
        }

        private void Client_NotifyexceptionEvent(object sender, Exception ex)
        {
            if (this.NotifyexceptionEvent != null)
            {
                this.NotifyexceptionEvent(sender, ex);
            }
        }

        private void Client_NotifyTextEvent(object sender, string text)
        {
            if (this.NotifyTextEvent != null)
            {
                this.NotifyTextEvent(sender, text);
            }
        }
    }
}
