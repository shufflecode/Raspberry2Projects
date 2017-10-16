
namespace libCore.UserControls
{
   
    using System;
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using System.ComponentModel;
    using System.Linq;
    using System.Runtime.CompilerServices;
    using System.Runtime.InteropServices.WindowsRuntime;
    using System.Threading.Tasks;
    using Windows.Devices.Enumeration;
    using Windows.Devices.SerialCommunication;
    using Windows.Networking.Connectivity;
    using Windows.UI.Core;
    using Windows.UI.Xaml.Controls;
    using libShared;


    public sealed partial class UcSerial : UserControl, INotifyPropertyChanged
    {
        private string title = "Async GUI";
        Windows.UI.Core.CoreDispatcher dispatcher = Windows.UI.Core.CoreWindow.GetForCurrentThread().Dispatcher;

        libShared.Interfaces.ISerialAsync serial;
        private DeviceInformation selectedPort = null;
        private uint selectedBaudRate = 9600;
        private libShared.Interfaces.SerialParity selectedParity = libShared.Interfaces.SerialParity.None;
        private libShared.Interfaces.SerialStopBitCount selectedStopBits = libShared.Interfaces.SerialStopBitCount.One;
        private libShared.Interfaces.SerialDataBits selectedDataBits = libShared.Interfaces.SerialDataBits.Eight;

        public ObservableCollection<DeviceInformation> CollectionPorts { get; set; }
        public ObservableCollection<uint> CollectionBaudrates { get; set; }
        public ObservableCollection<libShared.Interfaces.SerialParity> CollectionParities { get; set; }
        public ObservableCollection<libShared.Interfaces.SerialStopBitCount> CollectionStoptBits { get; set; }
        public ObservableCollection<libShared.Interfaces.SerialDataBits> CollectionDataBits { get; set; }

        private string valueSendText;
        ObservableCollection<byte> valueSendData = new ObservableCollection<byte>();

        public DeviceInformation SelectedPort
        {
            get { return selectedPort; }
            set
            {
                selectedPort = value;
                this.OnPropertyChanged();
            }
        }

        public uint SelectedBaudRate
        {
            get { return selectedBaudRate; }
            set
            {
                selectedBaudRate = value;
                this.OnPropertyChanged();
            }
        }

        public libShared.Interfaces.SerialParity SelectedParity
        {
            get { return selectedParity; }
            set
            {
                selectedParity = value;
                this.OnPropertyChanged();
            }
        }

        public libShared.Interfaces.SerialStopBitCount SelectedStopBits
        {
            get { return selectedStopBits; }
            set
            {
                selectedStopBits = value;
                this.OnPropertyChanged();
            }
        }

        public libShared.Interfaces.SerialDataBits SelectedDataBits
        {
            get { return selectedDataBits; }
            set
            {
                selectedDataBits = value;
                this.OnPropertyChanged();
            }
        }

        public libShared.Interfaces.ISerialAsync Serial
        {
            get { return serial; }
            set { serial = value; }
        }


        public string ValueSendText
        {
            get { return valueSendText; }
            set
            {
                valueSendText = value;
                this.OnPropertyChanged();
            }
        }

        public ObservableCollection<byte> ValueSendData
        {
            get { return valueSendData; }
            set
            {
                valueSendData = value;
                this.OnPropertyChanged();
            }
        }

        public string Title
        {
            get { return title; }
            set
            {
                title = value;
                this.OnPropertyChanged();
            }
        }

        public string TextBlockText1
        {
            get
            {
                return textBlockText1;
            }

            set
            {
                textBlockText1 = value;
                this.OnPropertyChanged();
            }
        }

        public UcSerial()
        {


            InfoTextList = new ObservableCollection<object>();
            this.Command = new RelayCommand(this.CommandReceived);

            this.CollectionParities = new ObservableCollection<libShared.Interfaces.SerialParity>(Enum.GetValues(typeof(libShared.Interfaces.SerialParity)).Cast<libShared.Interfaces.SerialParity>());
            this.CollectionStoptBits = new ObservableCollection<libShared.Interfaces.SerialStopBitCount>(Enum.GetValues(typeof(libShared.Interfaces.SerialStopBitCount)).Cast<libShared.Interfaces.SerialStopBitCount>());
            this.CollectionDataBits = new ObservableCollection<libShared.Interfaces.SerialDataBits>(Enum.GetValues(typeof(libShared.Interfaces.SerialDataBits)).Cast<libShared.Interfaces.SerialDataBits>());
            this.CollectionBaudrates = new ObservableCollection<uint>();
            this.CollectionBaudrates.Add(9600);
            this.CollectionBaudrates.Add(19200);
            this.CollectionBaudrates.Add(115200);
            this.CollectionPorts = new ObservableCollection<DeviceInformation>();
            this.GetComPorts();


            InitializeComponent();
            this.DataContext = this;

            this.Serial = new AsyncSerialPort();
            this.Title = this.Serial.GetType().Name;
        }

        public UcSerial(libShared.Interfaces.ISerialAsync _serial)
            : this()
        {
            this.Serial = _serial;
            this.Title = this.Serial.GetType().Name;
        }

        /// <summary>
        /// Ermmitelt alle verfühgbaren COM Ports und FTDI Dongels
        /// </summary>
        public async Task GetComPorts()
        {
            try
            {
                this.CollectionPorts.Clear();
                string aqs = Windows.Devices.SerialCommunication.SerialDevice.GetDeviceSelector();
                var dis = await Windows.Devices.Enumeration.DeviceInformation.FindAllAsync(aqs);

                foreach (var item in dis)
                {
                    this.CollectionPorts.Add(item);
                    this.AddInfoTextLine(string.Format("{0} -> {1}", item.Name, item.Id));
                }

                if (this.CollectionPorts.Count() > 0 && this.SelectedPort != null)
                {
                    this.SelectedPort = this.CollectionPorts[0];
                }

                //DeviceListSource.Source = listOfDevices;
                //comPortInput.IsEnabled = true;
                //ConnectDevices.SelectedIndex = -1;
            }
            catch (Exception ex)
            {
                messageBox(ExceptionHandling.GetExceptionText(new System.Exception(string.Format("Exception In: {0}", CallerName()), ex)));
            }
        }

        public void SendText(string text)
        {
            try
            {
                if (this.Serial != null)
                {
                    this.Serial.SendText(text);
                }
                else
                {
                    this.AddInfoTextLine("TcpClient Not Running");
                }
            }
            catch (Exception ex)
            {
                messageBox(ExceptionHandling.GetExceptionText(new System.Exception(string.Format("Exception In: {0}", CallerName()), ex)));
            }
        }

        public void SendData(byte[] data)
        {
            try
            {
                if (this.Serial != null)
                {
                    this.Serial.SendData(data);
                }
                else
                {
                    this.AddInfoTextLine("TcpClient Not Running");
                }
            }
            catch (Exception ex)
            {
                messageBox(ExceptionHandling.GetExceptionText(new System.Exception(string.Format("Exception In: {0}", CallerName()), ex)));
            }
        }

        private void Ethernet_NotifyMessageReceivedEvent(object sender, byte[] data)
        {
            this.AddInfoTextLine("Text:" + System.Text.Encoding.UTF8.GetString(data));
            this.AddInfoTextLine("Data:" + Converters.ConvertByteArrayToHexString(data, " "));
        }

        private void Ethernet_NotifyexceptionEvent(object sender, Exception ex)
        {
            this.AddInfoTextLine(sender, ex.Message);
        }

        #region InfoTextList

        /// <summary>
        /// View: String Collection in der Informationen Angezeigt werden können.
        /// </summary>
        public System.Collections.ObjectModel.ObservableCollection<object> InfoTextList { get; internal set; }

        string textBlockText1 = string.Empty;

        /// <summary>
        /// ühgt der Infotext Liste einen weiteren Eintrag hinzu
        /// </summary>      
        public void AddInfoTextLine(string line)
        {
            this.AddInfoTextLine(null, line);
        }

        /// <summary>
        /// ühgt der Infotext Liste einen weiteren Eintrag hinzu
        /// </summary>      
        async public void AddInfoTextLine(object sender, string line)
        {
            try
            {
                await this.dispatcher.RunAsync(CoreDispatcherPriority.Normal, () =>
                {
                    this.InfoTextList.Add(line);
                    TextBlockText1 += string.Format("{0}\n", line);
                });
            }
            catch (Exception ex)
            {
                messageBox(ExceptionHandling.GetExceptionText(new System.Exception(string.Format("Exception In: {0}", CallerName()), ex)));
            }
        }

        protected async void messageBox(string msg)
        {
            var msgDlg = new Windows.UI.Popups.MessageDialog(msg);
            msgDlg.DefaultCommandIndex = 1;
            await msgDlg.ShowAsync();
        }

        string CallerName([CallerMemberName]string caller = "")
        {
            return caller;
        }

        #endregion

        #region Command_Handling

        /// <summary>
        /// Command Objekt (MVVM): Empfängt Commands von der Oberfläche
        /// </summary>
        public System.Windows.Input.ICommand Command { get; protected set; }

        /// <summary>
        /// Empfängt Commands von der Oberfläche
        /// </summary>
        /// <param name="param">Empfangenes Command</param>
        async public void CommandReceived(object param)
        {
            try
            {
                switch (param as string)
                {
                    case "Start":

                        if (this.Serial.IsConnected == false)
                        {
                            this.Serial.BaudRate = this.SelectedBaudRate;

                            this.Serial.Parity = this.SelectedParity;
                            this.Serial.DataBits = this.SelectedDataBits;
                            this.Serial.StopBits = this.SelectedStopBits;
                            this.Serial.Port = this.SelectedPort.Id;

                            await this.Serial.Start();

                            this.Serial.NotifyTextEvent += this.AddInfoTextLine;
                            this.Serial.NotifyexceptionEvent += this.NotifyexceptionEvent;
                            this.Serial.NotifyMessageReceivedEvent += this.NotifyMessageReceivedEvent;
                        }
                        else
                        {
                            this.AddInfoTextLine("Die Verbindung ist bereits geöffnet.");
                        }

                        break;

                    case "Stop":

                        if (this.Serial.IsConnected)
                        {
                            await this.Serial.Stop();
                        }
                        else
                        {
                            this.AddInfoTextLine("Die Verbindung ist bereits beendet.");
                        }


                        break;

                    case "RefreshPorts":
                        await GetComPorts();
                        break;

                    case "Send TextBlock":
                        await this.Serial.SendText(this.ValueSendText);
                        break;

                    case "Send Data":
                        await this.Serial.SendData(this.ValueSendData.ToArray());
                        break;

                    case "Read":
                        byte[] data = await this.Serial.Read();
                        break;

                    default:
                        this.AddInfoTextLine(string.Format("Command {0} not Implemented", param.ToString()));
                        break;
                }

            }
            catch (Exception ex)
            {
                messageBox(ExceptionHandling.GetExceptionText(new System.Exception(string.Format("Exception In: {0}", CallerName()), ex)));
            }
        }

        private void NotifyMessageReceivedEvent(object sender, byte[] data)
        {
            this.AddInfoTextLine("Text:" + System.Text.Encoding.UTF8.GetString(data));
            this.AddInfoTextLine("Data:" + Converters.ConvertByteArrayToHexString(data, " "));
        }


        /// <summary>
        /// Fühgt der Infotext Liste einen weiteren Eintrag hinzu
        /// </summary>  
        private void NotifyexceptionEvent(object sender, Exception ex)
        {
            this.AddInfoTextLine(string.Format("Exception: {0}", ExceptionHandling.GetExceptionText(ex)));
        }

        #endregion

        #region INotifyPropertyChanged

        /// <summary>
        /// Occurs when [property changed].
        /// </summary>
        public event System.ComponentModel.PropertyChangedEventHandler PropertyChanged;

        /// <summary>
        /// Wird manuell Aufgerufen wenn sich eine Property ändert, dammit alle Elemente die an diese Property gebunden sind (UI-Elemente) aktualisiert werden.
        /// </summary>
        /// <param name="propertyname">Name der Property welche sich geändert hat.</param>
        private void OnPropertyChanged([System.Runtime.CompilerServices.CallerMemberName] string propertyName = null)
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
