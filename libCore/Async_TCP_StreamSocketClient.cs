

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

    public class Async_TCP_StreamSocketClient : IEthernetAsync
    {
        public event NotifyTextDelegate NotifyTextEvent;
        public event NotifyexceptionDelegate NotifyexceptionEvent;
        public event NotifyMessageReceivedDelegate NotifyMessageReceivedEvent;
        public event NotifyConnectionClosed NotifyConnectionClosedEvent;

        string ip;
        string port;
        private bool isConnected = false;

        public bool IsConnected
        {
            get { return isConnected; }
            internal set { isConnected = value; }
        }

        /// <summary>
        /// Remote Ip (IP des Ziel Servers)
        /// </summary>
        public string HostNameOrIp
        {
            get { return ip; }
            set { ip = value; }
        }

        /// <summary>
        /// Remote Port (Port des Ziel Servers)
        /// </summary>
        public string Port
        {
            get { return port; }
            set { port = value; }
        }

        public StreamSocket Socket
        {
            get
            {
                return socket;
            }

            internal set
            {
                socket = value;
            }
        }

        StreamSocket socket;

        public Async_TCP_StreamSocketClient()
        {
        }

        public Async_TCP_StreamSocketClient(StreamSocket _socket)
        {            
            this.Socket = _socket;
            this.HostNameOrIp = this.Socket.Information.RemoteHostName.DisplayName;
            this.Port = this.Socket.Information.RemotePort;
                 
            this.IsConnected = true;
        }

        /// <summary>
        /// CONNECT TO SERVER
        /// </summary>
        /// <param name="ip">Host name/IP address</param>
        /// <param name="port">Port number</param>
        /// <param name="message">Message to server</param>
        /// <returns>Response from server</returns>
        public async Task Start()
        {
            if (IsConnected)
            {
                this.NotifyTextEvent(this, string.Format("could not start: client connected"));
                return;
            }

            HostName hostName = new HostName(HostNameOrIp);
            Socket = new StreamSocket();

            // Set NoDelay to false so that the Nagle algorithm is not disabled
            Socket.Control.NoDelay = false;

            try
            {
                // Connect to the server
                await Socket.ConnectAsync(hostName, Port);
                this.IsConnected = true;

                // Start Reading
                ReadAsync();
            }
            catch (Exception ex)
            {
                this.Notifyexception(new System.Exception(string.Format("Exception In: {0}", CallerName()), ex));
            }
        }

        public async Task Stop()
        {
            try
            {
                if (IsConnected == false)
                {
                    this.NotifyTextEvent(this, string.Format("could not stop: client not connected"));
                    return;
                }

                this.IsConnected = false;

                if (Socket != null)
                {
                    await Socket.CancelIOAsync();
                    Socket.Dispose();
                }

                if (NotifyConnectionClosedEvent != null)
                {
                    NotifyConnectionClosedEvent(this);
                }                               
            }
            catch (Exception ex)
            {
                System.Exception exNew = new System.Exception(string.Format("Exception In: {0}", CallerName()), ex);
                this.Notifyexception(exNew);
                throw exNew;
            }
        }

        async public Task ReadAsync()
        {
            try
            {
                //if (socket.Control.KeepAlive == false)
                //{
                //    return;
                //}

                using (DataReader reader = new DataReader(Socket.InputStream))
                {
                    // Set the DataReader to only wait for available data (so that we don't have to know the data size)
                    reader.InputStreamOptions = Windows.Storage.Streams.InputStreamOptions.Partial;

                    // The encoding and byte order need to match the settings of the writer we previously used.
                    reader.ByteOrder = Windows.Storage.Streams.ByteOrder.LittleEndian;

                    // Send the contents of the writer to the backing stream. 
                    // Get the size of the buffer that has not been read.
                    var readOperation = reader.LoadAsync(256);
                    var result = await readOperation;

                    if (result <= 0 || readOperation.Status != Windows.Foundation.AsyncStatus.Completed)
                    {
                        await this.Stop();
                        //cts.Cancel(); // never gets called, status is always Completed, result always > 0
                    }
                    else
                    {

                        byte[] newData = new byte[reader.UnconsumedBufferLength];
                        reader.ReadBytes(newData);

                        reader.DetachStream();

                        if (this.NotifyMessageReceivedEvent != null)
                        {
                            this.NotifyMessageReceivedEvent(this, newData);
                            await ReadAsync();
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                System.Exception exNew = new System.Exception(string.Format("Exception In: {0}", CallerName()), ex);
                this.Notifyexception(exNew);
                await this.Stop();
            }
        }

        string CallerName([CallerMemberName]string caller = "")
        {
            return caller;
        }

        async public Task SendText(string text)
        {
            await this.SendData(System.Text.Encoding.UTF8.GetBytes(text));
        }

        async public Task SendData(byte[] data)
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
                    // Create the data writer object backed by the in-memory stream. 
                    using (DataWriter writer = new DataWriter(Socket.OutputStream))
                    {
                        writer.ByteOrder = ByteOrder.LittleEndian;

                        // Schreibt ein Array von Bytewerten in den Ausgabestream.
                        writer.WriteBytes(data);

                        // Übergibt Daten im Puffer an einen Sicherungsspeicher.                        
                        await writer.StoreAsync();

                        // Liefert Daten asynchron.
                        await writer.FlushAsync();

                        // Trennt den Stream, der dem Datenschreiber zugeordnet ist.
                        writer.DetachStream();
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

        private void Notifyexception(Exception ex)
        {
            if (this.NotifyexceptionEvent != null)
            {
                this.NotifyexceptionEvent(this, ex);
            }
        }

    }
}
