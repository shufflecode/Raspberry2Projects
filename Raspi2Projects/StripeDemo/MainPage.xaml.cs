using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;
using Windows.Devices.Gpio;
using Windows.Devices.Spi;
using System.Threading.Tasks;
using Windows.Devices.Enumeration;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using Windows.UI.Core;
using Windows.UI;
using Windows.Networking;
using Windows.Networking.Connectivity;
using Windows.Networking.Sockets;
using System.Threading;


using libCore.IOevalBoard;
using libCore.ValueConverters;
using libShared.HardwareNah;
using libShared.Interfaces;
using libShared.ApiModels;
using libShared;
using libCore;


/// <summary>
///  Achtung Das Projekt taugt noch nicht als Vorlage!!
/// </summary>

/*
Demo-Projekt für RGB-LED-Streifen mit 20 APA102 LEDs

    Der LED-Streifen wechselt Farben und Muster durch (in einem Zyklus das Muster, im nächsten die Farbe)
    Der Wechsel ist geschmeidig durch eine art Kreuzblende realisiert
    Auf dem GUI kann aber auch eine feste Farbe eingestellt werden

    Es ist auch eine Ethernet-Schnittstelle implementiert, diese ist aber noch nicht getestet.
    */

namespace StripeDemo
{
    public sealed partial class MainPage : Page, INotifyPropertyChanged
    {
        Windows.UI.Core.CoreDispatcher dispatcher = Windows.UI.Core.CoreWindow.GetForCurrentThread().Dispatcher;

        #region Ethernet_Mohn
        // Create an event to signal the timeout count threshold in the
        // timer callback.
        AutoResetEvent autoEvent = new AutoResetEvent(false);
        Timer cylictimer;

        /// <summary>
        /// Command Objekt (MVVM): Empfängt Commands von der Oberfläche
        /// </summary>
        public System.Windows.Input.ICommand Command { get; protected set; }

        private ObservableCollection<string> infoboxCollection = new ObservableCollection<string>();

        private string infoText = string.Empty;

        public string InfoText
        {
            get
            {
                return infoText;
            }

            set
            {
                infoText = value;
                this.OnPropertyChanged();
            }
        }

        /// <summary>
        /// View: String Collection in der Informationen Angezeigt werden können.
        /// </summary>
        public System.Collections.ObjectModel.ObservableCollection<string> InfoTextList { get; internal set; }

        libShared.Interfaces.IEthernetAsync server;
        public IEthernetAsync Server
        {
            get
            {
                return server;
            }

            internal set
            {
                server = value;
            }
        }

        string port = "27200";
        public string Port
        {
            get { return port; }
            set
            {
                port = value;
                this.OnPropertyChanged();
            }
        }

        string host = "localhost";
        public string Host
        {
            get { return host; }
            set
            {
                host = value;
                this.OnPropertyChanged();
            }
        }

        ObservableCollection<string> hostNames = new ObservableCollection<string>();
        public ObservableCollection<string> HostNames
        {
            get
            {
                return hostNames;
            }

            set
            {
                hostNames = value;
            }
        }

        private string valueSendText = String.Empty;
        public string ValueSendText
        {
            get { return valueSendText; }
            set
            {
                valueSendText = value;
                this.OnPropertyChanged();
            }
        }

        ObservableCollection<byte> valueSendData = new ObservableCollection<byte>();
        public ObservableCollection<byte> ValueSendData
        {
            get { return valueSendData; }
            set
            {
                valueSendData = value;
                this.OnPropertyChanged();
            }
        }

        public void SendText(string text)
        {
            try
            {
                if (this.Server != null)
                {
                    this.Server.SendText(text);
                }
                else
                {
                    this.AddInfoTextLine("Server Not Running");
                }
            }
            catch (Exception ex)
            {
                AddInfoTextLine(ExceptionHandling.GetExceptionText(new System.Exception(string.Format("Exception In: {0}", CallerName()), ex)));
            }
        }

        public void SendData(byte[] data)
        {
            try
            {
                if (this.Server != null)
                {
                    this.Server.SendData(data);
                }
                else
                {
                    this.AddInfoTextLine("Server Not Running");
                }
            }
            catch (Exception ex)
            {
                AddInfoTextLine(ExceptionHandling.GetExceptionText(new System.Exception(string.Format("Exception In: {0}", CallerName()), ex)));
            }
        }

        private Windows.UI.Color logBackground;
        public Windows.UI.Color LogBackground
        {
            get { return logBackground; }
            set
            {
                logBackground = value;
                this.OnPropertyChanged();
            }
        }


        private void Server_NotifyMessageReceivedEvent(object sender, byte[] data)
        {
            try
            {
                string ret = System.Text.Encoding.UTF8.GetString(data);

                ProcessIncomingString(ret);
                this.AddInfoTextLine("Text:" + ret + " Data:" + Converters.ConvertByteArrayToHexString(data, " "));
            }
            catch (Exception ex)
            {
                ShowMessageBox(ExceptionHandling.GetExceptionText(new System.Exception(string.Format("Exception In: {0}", CallerName()), ex)));
            }

        }

        private void TimerProc(object state)
        {
            //// The state object is the Timer object.
            //Timer t = (Timer)state;
            //t.Dispose();
            ////Console.WriteLine("The timer callback executes.");

            string text = DateTime.Now.ToString();

            AddInfoTextLine(text);

            if (this.Server != null)
            {
                this.Server.SendText(text);
            }
        }

        private void Server_NotifyTextEvent(object sender, string text)
        {
            AddInfoTextLine(string.Format("Info from Server: {0}", text));
        }

        private void Server_NotifyexceptionEvent(object sender, Exception ex)
        {
            AddInfoTextLine(string.Format("Exception from Server: {0}", libShared.ExceptionHandling.GetExceptionText(ex)));
        }

        public void StartTimer()
        {
            try
            {
                AddInfoTextLine("Start Timer");
                cylictimer.Change(100, 100);
                //autoEvent.Reset();
            }
            catch (Exception ex)
            {
                ShowMessageBox(ExceptionHandling.GetExceptionText(new System.Exception(string.Format("Exception In: {0}", CallerName()), ex)));
            }
        }

        public void StopTimer()
        {
            try
            {
                AddInfoTextLine("Stop Timer");
                cylictimer.Change(0, 0);
                //autoEvent.Set();
            }
            catch (Exception ex)
            {
                ShowMessageBox(ExceptionHandling.GetExceptionText(new System.Exception(string.Format("Exception In: {0}", CallerName()), ex)));
            }
        }


        /// <summary>
        /// Fügt der Infotext Liste einen weiteren Eintrag hinzu
        /// </summary>      
        public void AddInfoTextLine(string line)
        {
            this.AddInfoTextLine(null, line);
        }

        /// <summary>
        /// Fügt der Infotext Liste einen weiteren Eintrag hinzu
        /// </summary>      
        public async void AddInfoTextLine(object sender, string line)
        {
            try
            {
                await this.dispatcher.RunAsync(CoreDispatcherPriority.Normal, () =>
                {
                    //this.InfoTextList.Add(line);
                    //TextBlockText1 += string.Format("{0}\n", line);

                    if (this.InfoText.Length > 1000)
                    {
                        this.InfoText = line + Environment.NewLine;
                    }
                    else
                    {
                        this.InfoText += line + Environment.NewLine;
                    }

                    Scoll1.ChangeView(null, Scoll1.ScrollableHeight, null);

                    //this.InfoTextList.Add(line);                    
                });
            }
            catch (Exception ex)
            {
                ShowMessageBox(ExceptionHandling.GetExceptionText(new System.Exception(string.Format("Exception In: {0}", CallerName()), ex)));
            }
        }

        private async void ShowMessageBox(string msg)
        {
            var msgDlg = new Windows.UI.Popups.MessageDialog(msg);
            msgDlg.DefaultCommandIndex = 1;
            await msgDlg.ShowAsync();
        }

        string CallerName([CallerMemberName]string caller = "")
        {
            return caller;
        }

        /// <summary>
        /// PropertyChanged EventHandler
        /// </summary>
        public event PropertyChangedEventHandler PropertyChanged;

        /// <summary>
        /// Wird manuell Aufgerufen wenn sich eine Property ändert, dammit alle Elemente die an diese Property gebunden sind (UI-Elemente) aktualisiert werden.
        /// </summary>
        /// <param name="propertyName">Name der Property welche sich geändert hat.</param>
        private void OnPropertyChanged([System.Runtime.CompilerServices.CallerMemberName] string propertyName = null)
        {
            PropertyChangedEventHandler handler = this.PropertyChanged;
            if (handler != null)
            {
                handler(this, new PropertyChangedEventArgs(propertyName));
            }
        }

        private void Page_Loaded(object sender, RoutedEventArgs e)
        {
        }

        private void ScrollViewer_ViewChanged(object sender, ScrollViewerViewChangedEventArgs e)
        {
            //var s = sender as ScrollViewer;
            //s.ChangeView(null, s.ScrollableHeight, null);
        }
        #endregion





        private const string SPI_DEMO_CONTROLLER_NAME = "SPI0";
        private const int SPI_CS_LINE = 0;

        // Interface Objects
        private GpioController GPIOvar;
        private SpiDevice SPIinterface_Demo;
        private DispatcherTimer StripeTimer;
        private DispatcherTimer SweepTimer;


        // Objects for cyclic access
        SPIAddressObject CSadrLEDD;
        LED_APA102 RGBstripe;
        PatternGenerator StripePattern;

        const int StripeLen = 20;
        const int MaxSliderValue = 0xFF;

        const int refreshCycle = 25;

        /// <summary>
        /// Main-Page
        /// </summary>
        public MainPage()
        {
            this.InitializeComponent();
            InitAll();

            this.DataContext = this;
            cylictimer = new Timer(new TimerCallback(TimerProc), autoEvent, 0, 0);


            var h = NetworkInformation.GetHostNames().Where(x => x.IPInformation != null && (x.IPInformation.NetworkAdapter.IanaInterfaceType == 71 || x.IPInformation.NetworkAdapter.IanaInterfaceType == 6));

            foreach (var hn in h)
            {
                this.HostNames.Add(hn.DisplayName);
                this.Host = hn.DisplayName;
            }

            this.Server = new libCore.Async_TCP_StreamSocketServer();
            this.Server.NotifyTextEvent += Server_NotifyTextEvent;
            this.Server.NotifyexceptionEvent += Server_NotifyexceptionEvent;
            this.Server.NotifyMessageReceivedEvent += Server_NotifyMessageReceivedEvent;

            txt_StatusBar.Text = "Constructor successful"; 
        }

        /// <summary>
        /// TimerInitialisation
        /// </summary>
        private async void InitAll()
        {
            StripeTimer = new DispatcherTimer();
            StripeTimer.Interval = TimeSpan.FromMilliseconds(refreshCycle);
            StripeTimer.Tick += StripeRefresh_Tick;

            SweepTimer = new DispatcherTimer();
            SweepTimer.Interval = TimeSpan.FromMilliseconds(3000);
            SweepTimer.Tick += Sweep_Tick;

            GPIOvar = GpioController.GetDefault(); /* Get the default GPIO controller on the system */
            await InitSpi();        /* Initialize the SPI controller                */

            CSadrLEDD = new SPIAddressObject(SPIAddressObject.eCSadrMode.SPIdedicated, null, null, 0);
            RGBstripe = new LED_APA102(SPIinterface_Demo, CSadrLEDD);
            // Bei der Instanziierung wird erstes LED-Objekt erstellt.

            for (int i = 0; i < StripeLen - 1; i++)
            {
                RGBstripe.AddLED(RGBDefines.Black);
            }

            // LED-Demo
            StripePattern = new PatternGenerator(StripeLen, 1);
            StripePattern.AddCurve(PatternGenerator.eCurveType.Sine);
            StripePattern.AddCurve(PatternGenerator.eCurveType.Pulse);
            StripePattern.AddCurve(PatternGenerator.eCurveType.Cosine);
            StripePattern.AddCurve(PatternGenerator.eCurveType.Triangle);
            StripePattern.AddCurve(PatternGenerator.eCurveType.Sawtooth);

            StripeTimer.Start();
            btn_StartDemo(new object(), new RoutedEventArgs());
        }

    private async Task InitSpi()
        {
            var settings = new SpiConnectionSettings(SPI_CS_LINE); /* Create SPI initialization settings                               */
            settings.ClockFrequency = 8000000;                             /* Datasheet specifies maximum SPI clock frequency of 10MHz         */
            settings.Mode = SpiMode.Mode0; // Bedeutet, dass CLK-Idle ist low, Sample bei Steigender Flank

            string spiAqs1 = SpiDevice.GetDeviceSelector(SPI_DEMO_CONTROLLER_NAME);       /* Find the selector string for the SPI bus controller          */
            var devicesInfo1 = await DeviceInformation.FindAllAsync(spiAqs1);         /* Find the SPI bus controller device with our selector string  */
            SPIinterface_Demo = await SpiDevice.FromIdAsync(devicesInfo1[0].Id, settings);  /* Create an SpiDevice with our bus controller and SPI settings */
        }


        UInt16[] StripeRGB = new ushort[3] { 0, 0, 0 };
        UInt16[] oldStripeRGB = new ushort[3] { 0, 0, 0 };
        UInt16[] newStripeRGB = new ushort[3] { 0, 0, 0 };
        UInt16[] RGBindex = new ushort[9] { 1, 2, 3, 1, 2, 3, 1, 2, 3 };

        RGBValue[] newColors = new RGBValue[StripeLen];
        private void StripeRefresh_Tick(object sender, object e)
        {
            StripePattern.RefreshData(newColors);
            RGBstripe.SetAllLEDs(newColors);
            RGBstripe.UpdateLEDs();
        }

        int nextMode = 0;
        int colorCycle = 0;
        int patternCycle = 0;
        /// <summary>
        /// Leitet Farb-, oder Muster-Wechsel ein
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Sweep_Tick(object sender, object e)
        {
            // Wenn Farbwechsel ansteht
            if (nextMode == 0)
            {
                nextMode = 1;
                colorCycle++;
                if (colorCycle >= 7)
                {
                    colorCycle = 1;
                }

                Byte[] tempRGB = new Byte[3] { 0, 0, 0 };
                for (int i = 0; i < 3; i++)
                {
                    if ((colorCycle & (1 << i)) != 0)
                    {
                        tempRGB[i] = RGBValue.MaxValue;
                    }
                }
                RGBValue tempVal = new RGBValue() { Intensity = RGBValue.MaxValue, Red = tempRGB[0], Green = tempRGB[1], Blue = tempRGB[2]};

                StripePattern.InitColorChange(tempVal);

            }
            // Wenn Musterwechsel ansteht
            else
            {
                nextMode = 0;
                patternCycle++;
                if (colorCycle >= StripePattern.Curves.Count)
                {
                    patternCycle = 0;
                }

                StripePattern.InitCurveChange(patternCycle);
            }
        }

        async private void btn_ServerStart_Click(object sender, RoutedEventArgs e)
        {
            this.Server.HostNameOrIp = this.Host;
            this.Server.Port = this.Port;
            this.AddInfoTextLine(string.Format("Server Start Beginn -> Host: {0}, Port: {1}", this.Server.HostNameOrIp, this.Server.Port));
            await this.Server.Start();
            txt_StatusBar.Text = "Server started";
            btn_ServerStart.Background = new SolidColorBrush(Colors.LightGreen);
        }


        async private void btn_ServerStop_Click(object sender, RoutedEventArgs e)
        {
            txt_StatusBar.Text = "Stopping Server ...";

            await this.Server.Stop();
            btn_ServerStart.Background = new SolidColorBrush(Colors.LightGray);
        }

        private void btn_LocalColorSet(object sender, RoutedEventArgs e)
        {
            this.SendText("Color set locally");

            RGBValue tempVal = new RGBValue();
            tempVal.Red = (Byte)SliderRed.Value;
            tempVal.Green = (Byte)SliderGreen.Value;
            tempVal.Blue = (Byte)SliderBlue.Value;
            tempVal.Intensity = (Byte)SliderIntens.Value;

            StripePattern.InitColorChange(tempVal);

            string tempString = "";
            tempString += "I" + String.Format("{0,3:D}", tempVal.Intensity).Replace(' ', '0');
            tempString += "R" + String.Format("{0,3:D}", tempVal.Red).Replace(' ', '0');
            tempString += "G" + String.Format("{0,3:D}", tempVal.Green).Replace(' ', '0');
            tempString += "B" + String.Format("{0,3:D}", tempVal.Blue).Replace(' ', '0');
            this.SendText("Local Set:" + tempString);

            SweepTimer.Stop();
            btn_StartDemo(sender, e);
        }

        private void btn_StartDemo(object sender, RoutedEventArgs e)
        {
            if (SweepTimer.IsEnabled == false)
            {
                SweepTimer.Start();
                bnt_AutoSetColor.Background = new SolidColorBrush(Colors.LightGreen);
            }
            else
            {
                bnt_AutoSetColor.Background = new SolidColorBrush(Colors.LightGray);
            }
        }



        void ProcessIncomingString(string incoming)
        {
            // Echo zurückschicken
            this.SendText("Received :" + incoming);

            // Daten verarbeiten
            byte newIndensitiy = 255;
            byte newRed = 0;
            byte newGreen = 0;
            byte newBlue = 0;
            int Iidx = incoming.IndexOf('I');
            if (Iidx >= 0)
            {
                newIndensitiy = Convert.ToByte(incoming.Substring(Iidx, 3));
                this.AddInfoTextLine("New intensity : " + newIndensitiy);
            }
            Iidx = incoming.IndexOf('R');
            if (Iidx >= 0)
            {
                newRed = Convert.ToByte(incoming.Substring(Iidx, 3));
                this.AddInfoTextLine("New Red : " + newRed);
            }
            Iidx = incoming.IndexOf('G');
            if (Iidx >= 0)
            {
                newGreen = Convert.ToByte(incoming.Substring(Iidx, 3));
                this.AddInfoTextLine("New Green : " + newGreen);
            }
            Iidx = incoming.IndexOf('B');
            if (Iidx >= 0)
            {
                newBlue = Convert.ToByte(incoming.Substring(Iidx, 3));
                this.AddInfoTextLine("New Blue : " + newBlue);
            }

            RGBValue tempLEDobj = new RGBValue();
            tempLEDobj.Intensity = newIndensitiy;
            tempLEDobj.Red = newRed;
            tempLEDobj.Green = newGreen;
            tempLEDobj.Blue = newBlue;

            StripePattern.InitColorChange(tempLEDobj);

            this.SendText("Configuration done");
        }

    }
}