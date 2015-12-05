using libShared;
using libShared.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace libDesktop
{
    public class AsyncSerialPort : ISerialAsync
    {
        System.IO.Ports.SerialPort serialPort = new System.IO.Ports.SerialPort();
        private CancellationTokenSource readCancellationTokenSource = new CancellationTokenSource();
        private byte[] inputBuffer;

        public event NotifyTextDelegate NotifyTextEvent;
        public event NotifyexceptionDelegate NotifyexceptionEvent;
        public event NotifyMessageReceivedDelegate NotifyMessageReceivedEvent;

        private string port = "COM1";
        private uint baudRate = 9600;
        private SerialParity parity = SerialParity.None;
        private SerialStopBitCount stopBits = SerialStopBitCount.One;
        private SerialDataBits dataBits = SerialDataBits.Eight;

        Queue<byte[]> messagesQue = new Queue<byte[]>();
        List<byte> temp = new List<byte>();

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

        public byte[] InputBuffer
        {
            get
            {
                return inputBuffer;
            }

            internal set
            {
                inputBuffer = value;
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

        public async Task<byte[]> Read()
        {
            int bytesRead = await this.serialPort.BaseStream.ReadAsync(InputBuffer, 0, InputBuffer.Length, this.ReadCancellationTokenSource.Token);
            byte[] data = new byte[bytesRead];
            Array.Copy(this.InputBuffer, data, bytesRead);

            if (NotifyMessageReceivedEvent != null)
            {
                NotifyMessageReceivedEvent(this, data);
            }

            return data;
        }

        public async Task SendData(byte[] data)
        {
            await this.serialPort.BaseStream.WriteAsync(data, 0, data.Length);
        }

        public async Task SendText(string text)
        {
            await SendData(System.Text.Encoding.UTF8.GetBytes(text));
        }

        public AsyncSerialPort()
        {
        }

        public bool IsConnected
        {
            get
            {
                return this.serialPort != null && this.serialPort.IsOpen;
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

        /// <summary>
        /// Öffnet die Serielle Verbindung und startet den Worker-Thread.
        /// </summary>
        public async Task Start()
        {
            //// Serielle Schnittstelle öffen
            if (this.IsConnected == false)
            {
                this.serialPort.BaudRate = (int)this.BaudRate;
                this.serialPort.PortName = this.Port;
                this.serialPort.DataBits = (int)this.DataBits;

                switch (this.StopBits)
                {
                    case SerialStopBitCount.One:
                        this.serialPort.StopBits = System.IO.Ports.StopBits.One;
                        break;
                    case SerialStopBitCount.OnePointFive:
                        this.serialPort.StopBits = System.IO.Ports.StopBits.OnePointFive;
                        break;
                    case SerialStopBitCount.Two:
                        this.serialPort.StopBits = System.IO.Ports.StopBits.Two;
                        break;
                    default:
                        throw new NotImplementedException();
                }

                switch (this.Parity)
                {
                    case SerialParity.None:
                        this.serialPort.Parity = System.IO.Ports.Parity.None;
                        break;
                    case SerialParity.Odd:
                        this.serialPort.Parity = System.IO.Ports.Parity.Odd;
                        break;
                    case SerialParity.Even:
                        this.serialPort.Parity = System.IO.Ports.Parity.Even;
                        break;
                    case SerialParity.Mark:
                        this.serialPort.Parity = System.IO.Ports.Parity.Mark;
                        break;
                    case SerialParity.Space:
                        this.serialPort.Parity = System.IO.Ports.Parity.Space;
                        break;
                    default:
                        throw new NotImplementedException();
                }



                this.serialPort.Open();
                ReadCancellationTokenSource = new CancellationTokenSource();

                //// Input Buffer überprüfen bzw. anlegen
                if (this.InputBuffer == null || this.InputBuffer.Count() != this.serialPort.ReadBufferSize)
                {
                    this.InputBuffer = new byte[this.serialPort.ReadBufferSize];
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

                //// Serielle Schnittstelle beenden
                if (this.IsConnected)
                {
                    this.serialPort.Close();
                }
            }

            catch (Exception ex)
            {
                throw ex;
            }
        }
    }
}
