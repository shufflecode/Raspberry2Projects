using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using libShared;
using libShared.Interfaces;
using System.Collections.ObjectModel;
using System.ComponentModel;

namespace libDesktop.UserControls
{
    /// <summary>
    /// Interaktionslogik für UcSerial.xaml
    /// </summary>
    public partial class UcSerial : UserControl, INotifyPropertyChanged
    {
        ISerialAsync serial;
        private string selectedPort = "COM1";
        private uint selectedBaudRate = 9600;
        private SerialParity selectedParity = SerialParity.None;
        private SerialStopBitCount selectedStopBits = SerialStopBitCount.One;
        private SerialDataBits selectedDataBits = SerialDataBits.Eight;

        public ObservableCollection<string> CollectionPorts { get; set; }
        public ObservableCollection<uint> CollectionBaudrates { get; set; }
        public ObservableCollection<SerialParity> CollectionParities { get; set; }
        public ObservableCollection<SerialStopBitCount> CollectionStoptBits { get; set; }
        public ObservableCollection<SerialDataBits> CollectionDataBits { get; set; }

        private string valueSendText;
        ObservableCollection<byte> valueSendData = new ObservableCollection<byte>();

        public string SelectedPort
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

        public SerialParity SelectedParity
        {
            get { return selectedParity; }
            set
            {
                selectedParity = value;
                this.OnPropertyChanged();
            }
        }

        public SerialStopBitCount SelectedStopBits
        {
            get { return selectedStopBits; }
            set
            {
                selectedStopBits = value;
                this.OnPropertyChanged();
            }
        }

        public SerialDataBits SelectedDataBits
        {
            get { return selectedDataBits; }
            set
            {
                selectedDataBits = value;
                this.OnPropertyChanged();
            }
        }

        public ISerialAsync Serial
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

        #region AutoScroll

        /// <summary>
        /// Hilfs Varibale
        /// </summary>
        private bool autoScroll = true;

        /// <summary>
        /// Hilfs Methode für den ScrollViewr, damit dieser Automatisch an das Ende scrollt.
        /// </summary>
        /// <param name="sender">Sender objcect</param>
        /// <param name="e">ScrollChanged EventArgs</param>
        private void ScrollViewer_ScrollChanged(object sender, ScrollChangedEventArgs e)
        {
            try
            {
                //// User scroll event : set or unset autoscroll mode
                if (e.ExtentHeightChange == 0)
                {
                    //// Content unchanged : user scroll event
                    if (((ScrollViewer)sender).VerticalOffset == ((ScrollViewer)sender).ScrollableHeight)
                    {
                        //// Scroll bar is in bottom
                        //// Set autoscroll mode
                        this.autoScroll = true;
                    }
                    else
                    {   //// Scroll bar isn't in bottom
                        //// Unset autoscroll mode
                        this.autoScroll = false;
                    }
                }

                //// Content scroll event : autoscroll eventually
                if (this.autoScroll && e.ExtentHeightChange != 0)
                {
                    //// Content changed and autoscroll mode set
                    //// Autoscroll
                    ((ScrollViewer)sender).ScrollToVerticalOffset(((ScrollViewer)sender).ExtentHeight);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ExceptionHandling.GetExceptionText(new System.Exception(string.Format("Exception In: {0}", System.Reflection.MethodBase.GetCurrentMethod().Name), ex)));
            }
        }

        #endregion

        #region InfoTextList

        // Konstruktor: this.DispatcherObject = System.Windows.Threading.Dispatcher.CurrentDispatcher;

        //<ScrollViewer DockPanel.Dock="Bottom" ScrollChanged= "ScrollViewer_ScrollChanged" ScrollViewer.HorizontalScrollBarVisibility= "Auto" ScrollViewer.VerticalScrollBarVisibility= "Auto" >
        //    < ListBox ItemsSource= "{Binding InfoTextList}" BorderThickness= "0" />
        //</ ScrollViewer >

        /// <summary>
        /// Multi Threading Hilfs Objekt.
        /// </summary>
        private object lockThis = new object();

        /// <summary>
        /// Dispatcher Hilf Objekt.
        /// Stellt Dienste zum Verwalten der Warteschlange von Arbeitsaufgaben für einen Thread bereit.
        /// </summary>
        public virtual System.Windows.Threading.Dispatcher DispatcherObject { get; protected set; }

        /// <summary>
        /// View: String Collection in der Informationen Angezeigt werden können.
        /// </summary>
        public System.Collections.ObjectModel.ObservableCollection<object> InfoTextList { get; internal set; }

        /// <summary>
        /// Fühgt der Infotext Liste einen weiteren Eintrag hinzu
        /// </summary>      
        public void AddInfoTextLine(string line)
        {
            this.AddInfoTextLine(null, line);
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

        /// <summary>
        /// Fühgt der Infotext Liste einen weiteren Eintrag hinzu
        /// </summary>      
        public void AddInfoTextLine(object sender, string line)
        {
            try
            {
                if (this.DispatcherObject.Thread != System.Threading.Thread.CurrentThread)
                {
                    this.DispatcherObject.Invoke(new Action(() => this.AddInfoTextLine(sender, line)));
                }
                else
                {
                    lock (this.lockThis)
                    {
                        System.Windows.Controls.TextBlock block = new System.Windows.Controls.TextBlock();
                        block.Text = line;
                        this.InfoTextList.Add(block);
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ExceptionHandling.GetExceptionText(new System.Exception(string.Format("Exception In: {0}", System.Reflection.MethodBase.GetCurrentMethod().Name), ex)));
            }
        }

        #endregion

        #region Command_Handling

        //// Konstukor: this.Command = new Lib_Port_SharedCode.RelayCommand(this.CommandReceived);

        /// <summary>
        /// Command Objekt (MVVM): Empfängt Commands von der Oberfläche
        /// </summary>
        public System.Windows.Input.ICommand Command { get; protected set; }



        /// <summary>
        /// Empfängt Commands von der Oberfläche
        /// </summary>
        /// <param name="param">Empfangenes Command</param>
        public async void CommandReceived(object param)
        {
            try
            {
                switch (param as string)
                {
                    case "Start":

                        if (this.Serial.IsConnected == false)
                        {
                            this.Serial.BaudRate = this.SelectedBaudRate;
                            this.Serial.Port = this.SelectedPort;
                            this.Serial.Parity = this.SelectedParity;
                            this.Serial.DataBits = this.SelectedDataBits;
                            this.Serial.StopBits = this.SelectedStopBits;

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
                        GetComPorts();
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
                MessageBox.Show(ExceptionHandling.GetExceptionText(new System.Exception(string.Format("Exception In: {0}", System.Reflection.MethodBase.GetCurrentMethod().Name), ex)));
            }
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
        protected virtual void OnPropertyChanged([System.Runtime.CompilerServices.CallerMemberName] string propertyName = null)
        {
            System.ComponentModel.PropertyChangedEventHandler handler = this.PropertyChanged;
            if (handler != null)
            {
                handler(this, new System.ComponentModel.PropertyChangedEventArgs(propertyName));
            }
        }

        #endregion

        public UcSerial()
        {
            InfoTextList = new ObservableCollection<object>();
            this.Command = new RelayCommand(this.CommandReceived);
            this.DispatcherObject = System.Windows.Threading.Dispatcher.CurrentDispatcher;

            this.CollectionParities = new ObservableCollection<SerialParity>(Enum.GetValues(typeof(SerialParity)).Cast<SerialParity>());
            this.CollectionStoptBits = new ObservableCollection<SerialStopBitCount>(Enum.GetValues(typeof(SerialStopBitCount)).Cast<SerialStopBitCount>());
            this.CollectionDataBits = new ObservableCollection<SerialDataBits>(Enum.GetValues(typeof(SerialDataBits)).Cast<SerialDataBits>());
            this.CollectionBaudrates = new ObservableCollection<uint>();
            this.CollectionBaudrates.Add(9600);
            this.CollectionBaudrates.Add(19200);
            this.CollectionBaudrates.Add(115200);
            this.CollectionPorts = new ObservableCollection<string>();
            this.GetComPorts();

            InitializeComponent();
            this.DataContext = this;
        }

        public UcSerial(ISerialAsync _serial)
            : this()
        {
            this.Serial = _serial;
        }

        /// <summary>
        /// Ermmitelt alle verfühgbaren COM Ports und FTDI Dongels
        /// </summary>
        public void GetComPorts()
        {
            this.CollectionPorts.Clear();

            List<string> serialNrs = System.IO.Ports.SerialPort.GetPortNames().ToList();

            foreach (var item in serialNrs)
            {
                this.CollectionPorts.Add(item);
            }
        }
    }
}
