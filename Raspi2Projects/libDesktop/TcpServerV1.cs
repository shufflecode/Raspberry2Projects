using libShared;
using libShared.Interfaces;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace libDesktop
{
    public class TcpServerV1 : IDisposable, INotifyPropertyChanged, IEthernet
    {
        public event NotifyTextDelegate NotifyTextEvent;
        public event NotifyexceptionDelegate NotifyexceptionEvent;
        public event NotifyMessageReceivedDelegate NotifyMessageReceivedEvent;

        private bool isRunning = false;
        private bool shouldStop = false;

        private TcpListener listener;

        List<TcpClientV1> serverClients = new List<TcpClientV1>();

        public bool IsConnected
        {
            get
            {
                return isRunning;
            }

            internal set
            {
                isRunning = value;
                this.OnPropertyChanged();
            }
        }



        private string localPort = "27200";

        /// <summary>
        /// Local Port
        /// </summary>
        public string Port
        {
            get { return localPort; }
            set
            {
                localPort = value;
                this.OnPropertyChanged();
            }
        }

        private string localIP = Dns.GetHostEntry(Dns.GetHostName()).AddressList.FirstOrDefault(ip => ip.AddressFamily == AddressFamily.InterNetwork).ToString();

        /// <summary>
        /// Local IP
        /// </summary>
        public string HostNameOrIp
        {
            get { return localIP; }
            set
            {
                localIP = value;
                this.OnPropertyChanged();
            }
        }

        public TcpServerV1()
        {
        }

        /// <summary>
        /// Finalizes an instance of the <see cref="TcpServerV1"/> class.
        /// </summary>
        ~TcpServerV1()
        {
            //// Do not re-create Dispose clean-up code here.
            //// Calling Dispose(false) is optimal in terms of
            //// readability and maintainability.
            this.Dispose(false);
        }

        /// <summary>
        /// Führt anwendungsspezifische Aufgaben durch, die mit der Freigabe, der Zurückgabe oder dem Zurücksetzen von nicht verwalteten Ressourcen zusammenhängen.
        /// </summary>
        public void Dispose()
        {
            this.Dispose(true);

            //// This object will be cleaned up by the Dispose method.
            //// Therefore, you should call GC.SupressFinalize to
            //// take this object off the finalization queue
            //// and prevent finalization code for this object
            //// from executing a second time.
            GC.SuppressFinalize(this);
        }

        /// <summary>
        /// Releases unmanaged and - optionally - managed resources.
        /// Dispose(bool disposing) executes in two distinct scenarios.
        /// If disposing equals true, the method has been called directly
        /// or indirectly by a user's code. Managed and unmanaged resources
        /// can be disposed.
        /// If disposing equals false, the method has been called by the
        /// runtime from inside the finalizer and you should not reference
        /// other objects. Only unmanaged resources can be disposed.
        /// </summary>
        /// <param name="disposing"><c>true</c> to release both managed and unmanaged resources; <c>false</c> to release only unmanaged resources.</param>
        protected virtual void Dispose(bool disposing)
        {
            if (disposing)
            {
                //// dispose managed resources  
                //this.MessageReceivedEvent = null;
                this.DisConnect();
            }

            //// free native resources
        }



        public void DisConnect()
        {
            //if (m_TcpListener != null)
            //{
            //    m_TcpListener.Stop();
            //    m_TcpListener = null;
            //}

            // Clients.Clear();
        }

        public void Start()
        {


            shouldStop = false;
            this.listener = new TcpListener(IPAddress.Parse(this.HostNameOrIp), Convert.ToInt32(this.Port));
            this.listener.Start();

            accept_connection();
        }

        public void Stop()
        {
            if (this.NotifyTextEvent != null)
            {
                this.NotifyTextEvent(this, string.Format("stop connection"));
            }

            shouldStop = true;

            this.listener.Stop();
        }

        private void accept_connection()
        {
            if (shouldStop) return;

            if (this.NotifyTextEvent != null)
            {
                this.NotifyTextEvent(this, string.Format("start TcpListener"));
            }

            this.listener.BeginAcceptTcpClient(handle_connection, null);  //this is called asynchronously and will run in a different thread
        }

        private void handle_connection(IAsyncResult result)  //the parameter is a delegate, used to communicate between threads
        {

            if (shouldStop) return;
            accept_connection();  //once again, checking for any other incoming connections

            TcpClient client = this.listener.EndAcceptTcpClient(result);  //creates the TcpClient

            if (this.NotifyTextEvent != null)
            {
                this.NotifyTextEvent(this, string.Format("Tcp Client Connected"));
                this.NotifyTextEvent(this, string.Format("Local  Endpoint: {0}", client.Client.LocalEndPoint));
                this.NotifyTextEvent(this, string.Format("Remote Endpoint: {0}", client.Client.RemoteEndPoint));
            }

            TcpClientV1 v1Client = new TcpClientV1(client);
            v1Client.NotifyexceptionEvent += this.Client_NotifyexceptionEvent;
            v1Client.NotifyTextEvent += this.Client_NotifyTextEvent;
            v1Client.NotifyMessageReceivedEvent += this.Client_NotifyMessageReceivedEvent;
            serverClients.Add(v1Client);

            //NetworkStream ns = client.GetStream();
            /* here you can add the code to send/receive data */
        }

        /// <summary>
        /// SendTextToAll
        /// </summary>
        /// <param name="text"></param>
        public void SendText(string text)
        {
            try
            {
                if (string.IsNullOrEmpty(text))
                {
                    throw new Exception("Send value null or empty or lengt 0");
                }
                else
                {
                    foreach (var item in this.serverClients)
                    {
                        item.SendText(text);
                    }
                }
            }
            catch (Exception ex)
            {
                System.Exception exNew = new System.Exception(string.Format("Exception In: {0}", System.Reflection.MethodBase.GetCurrentMethod().Name), ex);
                this.Notifyexception(exNew);
                throw exNew;
            }
        }

        /// <summary>
        /// SendDataToAll
        /// </summary>
        /// <param name="data"></param>
        public void SendData(byte[] data)
        {
            try
            {
                if (data == null || data.Length == 0)
                {
                    throw new Exception("Send value null or empty or lengt 0");
                }
                else
                {
                    foreach (var item in this.serverClients)
                    {
                        item.SendData(data);
                    }
                }


            }
            catch (Exception ex)
            {
                System.Exception exNew = new System.Exception(string.Format("Exception In: {0}", System.Reflection.MethodBase.GetCurrentMethod().Name), ex);
                this.Notifyexception(exNew);
                throw exNew;
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

        #region INotifyPropertyChanged

        /// <summary>
        /// Occurs when [property changed].
        /// </summary>
        public event System.ComponentModel.PropertyChangedEventHandler PropertyChanged;

        /// <summary>
        /// Wird manuell Aufgerufen wenn sich eine Property ändert, dammit alle Elemente die an diese Property gebunden sind (UI-Elemente) aktualisiert werden.
        /// </summary>
        /// <param name="propertyname">Name der Property welche sich geändert hat.</param>
        protected virtual void OnPropertyChanged([System.Runtime.CompilerServices.CallerMemberName] string propertyName = null)
        {
            System.ComponentModel.PropertyChangedEventHandler handler = this.PropertyChanged;
            if (handler != null)
            {
                handler(this, new System.ComponentModel.PropertyChangedEventArgs(propertyName));
            }
        }

        #endregion

    }
}

