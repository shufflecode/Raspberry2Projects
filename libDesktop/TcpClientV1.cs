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
    public class TcpClientV1 : INotifyPropertyChanged, IDisposable, IEthernetSync
    {
        public event NotifyTextDelegate NotifyTextEvent;
        public event NotifyexceptionDelegate NotifyexceptionEvent;
        public event NotifyMessageReceivedDelegate NotifyMessageReceivedEvent;
        public event NotifyConnectionClosed NotifyConnectionClosedEvent;

        //private bool isConnected = false;

        private string remoteIP = Dns.GetHostEntry(Dns.GetHostName()).AddressList.FirstOrDefault(ip => ip.AddressFamily == AddressFamily.InterNetwork).ToString();
        private string remotePort = "27200";

        private bool shouldStop = false;
        TcpClient client;

        public bool IsConnected
        {
            get
            {
                return this.client != null && this.client.Connected;
            }
        }

        public TcpClient Client
        {
            get { return client; }
            internal set
            {
                client = value;
                this.OnPropertyChanged();
            }
        }

        /// <summary>
        /// Remote IP
        /// </summary>
        public string HostNameOrIp
        {
            get
            {
                return remoteIP;
            }

            set
            {
                remoteIP = value;
                this.OnPropertyChanged();
            }
        }


        /// <summary>
        /// Remote Port
        /// </summary>
        public string Port
        {
            get
            {
                return remotePort;
            }

            set
            {
                remotePort = value;
                this.OnPropertyChanged();
            }
        }



        public TcpClientV1()
        {
            this.Client = new TcpClient(AddressFamily.InterNetwork);

            //// Empfangs Buffer anlegen
            buffer = new byte[Client.ReceiveBufferSize];
        }

        public TcpClientV1(TcpClient _client)
        {
            this.Client = _client;

            //// Empfangs Buffer anlegen
            buffer = new byte[Client.ReceiveBufferSize];

            //// Da der Client schon verbunden ist wird sofort ein Lesevorgang gestartet (Start und ConnectCallback wird übersprungen)
            NetworkStream objStream = this.Client.GetStream();
            objStream.BeginRead(buffer, 0, buffer.Length, ReadCallback, objStream);
        }

        public void Start()
        {
            if (IsConnected)
            {
                this.NotifyTextEvent(this, string.Format("could not start: client connected"));
                return;
            }

            if (this.NotifyTextEvent != null)
            {
                this.NotifyTextEvent(this, string.Format("start connection"));
            }

            if (this.Client == null)
            {
                this.Client = new TcpClient(AddressFamily.InterNetwork);

                //// Empfangs Buffer anlegen
                buffer = new byte[Client.ReceiveBufferSize];
            }

            shouldStop = false;
            this.Client.BeginConnect(this.HostNameOrIp, Convert.ToInt32(this.Port), new AsyncCallback(ConnectCallback), this.Client);
        }

        public void Stop()
        {
            if (IsConnected == false)
            {
                this.NotifyTextEvent(this, string.Format("could not stop: client not connected"));
                return;
            }

            if (this.NotifyTextEvent != null)
            {
                this.NotifyTextEvent(this, string.Format("stop connection"));
            }

            if (NotifyConnectionClosedEvent != null)
            {
                NotifyConnectionClosedEvent(this);
            }

            shouldStop = true;
            this.Dispose();
            this.Client = null;
        }

        public void Dispose()
        {
            if (this.Client != null)
            {
                this.Client.Close();
                this.Client.Client.Dispose();
            }
        }

        byte[] buffer;

        private void ConnectCallback(IAsyncResult result)
        {
            if (shouldStop) return;

            try
            {
                if (this.NotifyTextEvent != null)
                {
                    this.NotifyTextEvent(this, string.Format("TCP Server Connected", Client.Client.LocalEndPoint));
                    this.NotifyTextEvent(this, string.Format("Local  Endpoint: {0}", Client.Client.LocalEndPoint));
                    this.NotifyTextEvent(this, string.Format("Remote Endpoint: {0}", Client.Client.RemoteEndPoint));
                }

                NetworkStream objStream = this.Client.GetStream();
                objStream.BeginRead(buffer, 0, buffer.Length, ReadCallback, objStream);
            }
            catch (Exception ex)
            {
                System.Exception exNew = new System.Exception(string.Format("Exception In: {0}", System.Reflection.MethodBase.GetCurrentMethod().Name), ex);
                this.Notifyexception(exNew);
            }
        }

        /// Callback for Read operation
        private void ReadCallback(IAsyncResult result)
        {
            try
            {
                if (result.AsyncState != null && this.Client.Connected)
                {
                    NetworkStream objStream = this.Client.GetStream();
                    int length = objStream.EndRead(result);

                    byte[] newData = new byte[length];
                    Buffer.BlockCopy(this.buffer, 0, newData, 0, length);

                    if (this.NotifyMessageReceivedEvent != null)
                    {
                        this.NotifyMessageReceivedEvent(this, newData);
                    }

                    objStream.BeginRead(buffer, 0, buffer.Length, ReadCallback, buffer);
                }
                //else
                //{
                //    throw new Exception("Error result.AsyncState = 0");
                //}
            }
            catch (Exception ex)
            {
                System.Exception exNew = new System.Exception(string.Format("Exception In: {0}", System.Reflection.MethodBase.GetCurrentMethod().Name), ex);
                this.Notifyexception(exNew);
                this.Stop();
            }
        }

        public void SendText(string text)
        {
            try
            {
                if (IsConnected == false)
                {
                    this.NotifyTextEvent(this, string.Format("could not send: client not connected"));
                    return;
                }

                if (string.IsNullOrEmpty(text))
                {
                    throw new Exception("Send value null or empty or lengt 0");
                }
                else
                {
                    this.Client.Client.Send(Encoding.UTF8.GetBytes(text));
                }

            }
            catch (Exception ex)
            {
                System.Exception exNew = new System.Exception(string.Format("Exception In: {0}", System.Reflection.MethodBase.GetCurrentMethod().Name), ex);
                this.Notifyexception(exNew);
                throw exNew;
            }
        }

        public void SendData(byte[] data)
        {
            try
            {
                if (IsConnected == false)
                {
                    this.NotifyTextEvent(this, string.Format("could not send: client not connected"));
                    return;
                }

                if (data == null || data.Length == 0)
                {
                    throw new Exception("Send value null or empty or lengt 0");
                }
                else
                {
                    this.Client.Client.Send(data, 0, data.Length, SocketFlags.None);
                }


            }
            catch (Exception ex)
            {
                System.Exception exNew = new System.Exception(string.Format("Exception In: {0}", System.Reflection.MethodBase.GetCurrentMethod().Name), ex);
                this.Notifyexception(exNew);
                throw exNew;
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
