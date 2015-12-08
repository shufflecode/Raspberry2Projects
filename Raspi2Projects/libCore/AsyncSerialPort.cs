namespace libCore
{
    using libShared;
    using libShared.Interfaces;
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using System.Threading;
    using System.Threading.Tasks;

    public class AsyncSerialPort : ISerialAsync
    {
        public event NotifyTextDelegate NotifyTextEvent;
        public event NotifyexceptionDelegate NotifyexceptionEvent;
        public event NotifyMessageReceivedDelegate NotifyMessageReceivedEvent;


        Windows.Devices.SerialCommunication.SerialDevice serialPort = null;
        Windows.Storage.Streams.DataWriter dataWriteObject = null;
        Windows.Storage.Streams.DataReader dataReaderObject = null;
        private CancellationTokenSource readCancellationTokenSource = new CancellationTokenSource();

        private string port = "COM1";
        private uint baudRate = 9600;
        private SerialParity parity = SerialParity.None;
        private SerialStopBitCount stopBits = SerialStopBitCount.One;
        private SerialDataBits dataBits = SerialDataBits.Eight;

        public CancellationTokenSource ReadCancellationTokenSource
        {
            get
            {
                return readCancellationTokenSource;
            }

            internal set
            {
                readCancellationTokenSource = value;
            }
        }

        public string Port
        {
            get { return port; }
            set { port = value; }
        }

        public uint BaudRate
        {
            get { return baudRate; }
            set { baudRate = value; }
        }

        public SerialParity Parity
        {
            get { return parity; }
            set { parity = value; }
        }

        public SerialStopBitCount StopBits
        {
            get { return stopBits; }
            set { stopBits = value; }
        }

        public SerialDataBits DataBits
        {
            get { return dataBits; }
            set { dataBits = value; }
        }

        public bool IsConnected
        {
            get
            {
                return this.serialPort != null;//&& this.serialPort.IsOpen;
            }
        }

        /// <summary>
        /// Öffnet die Serielle Verbindung und startet den Worker-Thread.
        /// </summary>
        public async Task Start()
        {
            try
            {
                //// Serielle Schnittstelle öffen
                if (this.IsConnected == false)
                {
                    serialPort = await Windows.Devices.SerialCommunication.SerialDevice.FromIdAsync(this.Port);
                    serialPort.WriteTimeout = TimeSpan.FromMilliseconds(1000);
                    serialPort.ReadTimeout = TimeSpan.FromMilliseconds(1000);
                    this.serialPort.BaudRate = this.BaudRate;
                    //this.serialPort.PortName = this.Port;
                    this.serialPort.DataBits = (ushort)this.DataBits;

                    switch (this.StopBits)
                    {
                        case SerialStopBitCount.One:
                            this.serialPort.StopBits = Windows.Devices.SerialCommunication.SerialStopBitCount.One;
                            break;
                        case SerialStopBitCount.OnePointFive:
                            this.serialPort.StopBits = Windows.Devices.SerialCommunication.SerialStopBitCount.OnePointFive;
                            break;
                        case SerialStopBitCount.Two:
                            this.serialPort.StopBits = Windows.Devices.SerialCommunication.SerialStopBitCount.Two;
                            break;
                        default:
                            throw new NotImplementedException();
                    }

                    switch (this.Parity)
                    {
                        case SerialParity.None:
                            this.serialPort.Parity = Windows.Devices.SerialCommunication.SerialParity.None;
                            break;
                        case SerialParity.Odd:
                            this.serialPort.Parity = Windows.Devices.SerialCommunication.SerialParity.Odd;
                            break;
                        case SerialParity.Even:
                            this.serialPort.Parity = Windows.Devices.SerialCommunication.SerialParity.Even;
                            break;
                        case SerialParity.Mark:
                            this.serialPort.Parity = Windows.Devices.SerialCommunication.SerialParity.Mark;
                            break;
                        case SerialParity.Space:
                            this.serialPort.Parity = Windows.Devices.SerialCommunication.SerialParity.Space;
                            break;
                        default:
                            throw new NotImplementedException();
                    }

                    //this.serialPort.Open();
                    ReadCancellationTokenSource = new CancellationTokenSource();

                    dataReaderObject = new Windows.Storage.Streams.DataReader(serialPort.InputStream);
                    dataWriteObject = new Windows.Storage.Streams.DataWriter(serialPort.OutputStream);
                    ////// Input Buffer überprüfen bzw. anlegen
                    //if (this.InputBuffer == null || this.InputBuffer.Count() != this.serialPort.ReadBufferSize)
                    //{
                    //    this.InputBuffer = new byte[this.serialPort.ReadBufferSize];
                    //}
                }
            }
            catch (Exception ex)
            {

                throw ex;
            }

        }

        private void CancelReadTask()
        {
            if (ReadCancellationTokenSource != null)
            {
                if (!ReadCancellationTokenSource.IsCancellationRequested)
                {
                    ReadCancellationTokenSource.Cancel();
                }
            }
        }

        /// <summary>
        /// Stopt den Worker-Thread und beendet die Serielle Verbindung.
        /// </summary>
        public async Task Stop()
        {
            try
            {
                CancelReadTask();

                if (serialPort != null)
                {
                    serialPort.Dispose();
                }
                serialPort = null;

                ////// Serielle Schnittstelle beenden
                //if (this.IsConnected)
                //{
                //    this.serialPort.Close();
                //}
            }

            catch (Exception ex)
            {
                throw ex;
            }
        }

        /// <summary>
        /// ReadAsync: Task that waits on data and reads asynchronously from the serial device InputStream
        /// </summary>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        public async Task<byte[]> Read()
        {
            Task<UInt32> loadAsyncTask;

            uint ReadBufferLength = 1024;

            // If task cancellation was requested, comply
            this.ReadCancellationTokenSource.Token.ThrowIfCancellationRequested();

            // Set InputStreamOptions to complete the asynchronous read operation when one or more bytes is available
            dataReaderObject.InputStreamOptions = Windows.Storage.Streams.InputStreamOptions.Partial;

            // Create a task object to wait for data on the serialPort.InputStream
            loadAsyncTask = dataReaderObject.LoadAsync(ReadBufferLength).AsTask(this.ReadCancellationTokenSource.Token);

            // Launch the task and wait
            UInt32 bytesRead = await loadAsyncTask;
            byte[] data = new byte[bytesRead];

            if (bytesRead > 0)
            {
                dataReaderObject.ReadBytes(data);
            }

            if (NotifyMessageReceivedEvent != null)
            {
                NotifyMessageReceivedEvent(this, data);
            }

            return data;
        }

        public async Task SendData(byte[] data)
        {
            Task<UInt32> storeAsyncTask;

            // Load the text from the sendText input text box to the dataWriter object
            dataWriteObject.WriteBytes(data);

            // Launch an async task to complete the write operation
            storeAsyncTask = dataWriteObject.StoreAsync().AsTask();

            UInt32 bytesWritten = await storeAsyncTask;
        }

        public async Task SendText(string text)
        {
            await SendData(System.Text.Encoding.UTF8.GetBytes(text));
        }


    }
}
