// Die Vorlage "Leere Seite" ist unter http://go.microsoft.com/fwlink/?LinkId=402352&clcid=0x409 dokumentiert.

namespace App_IO_Demo
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
    using System.Runtime.InteropServices.WindowsRuntime;
    using Windows.Foundation;
    using Windows.Foundation.Collections;
    using System.Collections.ObjectModel;
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
    using System.ComponentModel;
    using Windows.UI;

    using libCore.IOevalBoard;
    using libShared.HardwareNah;
    using System.Text.RegularExpressions;

    using libSharedProject.ProtolV1Commands;
    using libShared.Interfaces;


    // Die Vorlage "Leere Seite" ist unter http://go.microsoft.com/fwlink/?LinkId=402352&clcid=0x409 dokumentiert.


    // @todo Bindings Zwischen Oberfläche und Lokalen Variablen einbauen
    // @todo Styles in WPF einführen ... normales Windows vorgehen funktioniert hier nicht

    /// <summary>
    /// Eine leere Seite, die eigenständig verwendet werden kann oder auf die innerhalb eines Rahmens navigiert werden kann.
    /// </summary>
    public sealed partial class MainPage : Page, INotifyPropertyChanged
    {

        #region Constants
        private const int HW_PowerOut1_Pin = 4;
        private const int HW_PowerOut2_Pin = 27;
        private const int HW_LEDDriver_GSMode = 6;
        private const int HW_LEDDriver_DCLatch = 12;
        private const int HW_LEDDriver_GSLatch = 13;
        private const int HW_LEDDriver_GSBlank = 26;
        private const int HW_CSAdrSelction_A0 = 18;
        private const int HW_CSAdrSelction_A1 = 22;
        private const int HW_CSAdrSelction_A2 = 23;

        private const int HW_ADC_CSadr = 1;
        private const int HW_DAC_CSadr = 2;
        private const int HW_GPIO_CSadr = 3;
        private const int HW_Reset_CSadr = 7;

        private const int MaxSliderValue = 100;

        private const string HW_SPI_IO_Controller = "SPI0";
        private const int HW_SPI_CS_Line = 0;
        private const string HW_SPI_LED_Controller = "SPI1";
        #endregion

        #region private Attributes

        GpioController GPIOinterface;
        SpiDevice SPIOInterface;
        SpiDevice StatusLEDInterface;

        GpioPin PowerOut1;
        GpioPin PowerOut2;
        GpioPin GSLEDdriverMode;
        GpioPin GSLEDdriverLatch;
        GpioPin GSLEDdriverBlank;
        GpioPin DCLEDdriverLatch;
        GpioPin CSAdrSelectionA0;
        GpioPin CSAdrSelectionA1;
        GpioPin CSAdrSelcttionA2;

        GpioPin[] CSAdrSelection;

        SPIAddressObject CSadrADC;
        SPIAddressObject CSadrDAC;
        SPIAddressObject CSadrGPIO;
        SPIAddressObject CSadrLEDD;
        SPIAddressObject CSadrLED;

        LEDD_TLC5941 GSLEDdriver;
        LEDD_TLC5925 DCLEDDriver;
        LED_APA102 StatusLED;
        ADC_MCP3208 ADCslave;
        DAC_MCP4922 DACslave;
        GPIO_MCP23S17 GPIOslave;
        int _StatusRedCh = 0;
        #endregion

        #region Public Attributes

        public int StatusRedCh
        {
            get
            {
                return _StatusRedCh;
            }
            set
            {
                this.OnPropertyChanged();
                _StatusRedCh = value;
            }
        }


        int _StatusGreenCh = 0;
        public int StatusGreenCh
        {
            get
            {
                return _StatusGreenCh;
            }

            set
            {
                this.OnPropertyChanged();
                _StatusGreenCh = value;
            }
        }

        int _StatusBlueCh = 0;
        public int StatusBlueCh
        {
            get
            {
                return _StatusBlueCh;
            }

            set
            {
                this.OnPropertyChanged();
                _StatusBlueCh = value;
            }
        }

        int _StatusIntensitiy = 0;
        public int StatusIntensitiy
        {
            get
            {
                return _StatusIntensitiy;
            }

            set
            {
                this.OnPropertyChanged();
                _StatusIntensitiy = value;
            }
        }
        #endregion


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

        #region Initialization

        public MainPage()
        {
            this.InitializeComponent();
            Unloaded += MainPage_Unloaded;

            InitAll();
            this.DataContext = this;

        }

        private async void InitAll()
        {
            InitGpio();
            await InitSpi();

            InitIOModule();

            InitDemo();
        }

        /// <summary>
        /// Set Resources Free
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="args"></param>
        private void MainPage_Unloaded(object sender, object args)
        {
            // Cleanup
            // HardwareRessourcen bereinigen
            // SPIinterface.Dispose();
        }

        /// <summary>
        /// Init used GPIO Pins
        /// </summary>
        private void InitGpio()
        {
            GPIOinterface = GpioController.GetDefault(); /* Get the default GPIO controller on the system */
            if (GPIOinterface == null)
            {
                throw new Exception("GPIO does not exist on the current system.");
            }

            PowerOut1 = GPIOinterface.OpenPin(HW_PowerOut1_Pin);
            PowerOut2 = GPIOinterface.OpenPin(HW_PowerOut2_Pin);
            PowerOut1.SetDriveMode(GpioPinDriveMode.Output);
            PowerOut2.SetDriveMode(GpioPinDriveMode.Output);
            PowerOut1.Write(GpioPinValue.Low);
            PowerOut2.Write(GpioPinValue.Low);
            GSLEDdriverMode = GPIOinterface.OpenPin(HW_LEDDriver_GSMode);
            GSLEDdriverLatch = GPIOinterface.OpenPin(HW_LEDDriver_GSLatch);
            GSLEDdriverBlank = GPIOinterface.OpenPin(HW_LEDDriver_GSBlank); 
            DCLEDdriverLatch = GPIOinterface.OpenPin(HW_LEDDriver_DCLatch);
            CSAdrSelectionA0 = GPIOinterface.OpenPin(HW_CSAdrSelction_A0);
            CSAdrSelectionA1 = GPIOinterface.OpenPin(HW_CSAdrSelction_A1);
            CSAdrSelcttionA2 = GPIOinterface.OpenPin(HW_CSAdrSelction_A2);

            /// @todo Reihenfolge prüfen
            CSAdrSelection = new GpioPin[3] { CSAdrSelectionA0, CSAdrSelectionA1, CSAdrSelcttionA2 };
        }

        /// <summary>
        /// Init SP-interfaces
        /// </summary>
        /// <returns></returns>
        private async Task InitSpi()
        {
            try
            {
                var settings = new SpiConnectionSettings(HW_SPI_CS_Line);
                settings.ClockFrequency = 2000000;
                settings.Mode = SpiMode.Mode0; // CLK-Idle ist low, Dataset on Falling Edge, Sample on Rising Edge
                string spiAqs = SpiDevice.GetDeviceSelector(HW_SPI_IO_Controller);
                var devicesInfo = await DeviceInformation.FindAllAsync(spiAqs);
                SPIOInterface = await SpiDevice.FromIdAsync(devicesInfo[0].Id, settings);
            }
            /* If initialization fails, display the exception and stop running */
            catch (Exception ex)
            {
                throw new Exception("SPI Initialization Failed", ex);
            }
            try
            {
                var settings = new SpiConnectionSettings(HW_SPI_CS_Line);
                settings.ClockFrequency = 4000000;
                settings.Mode = SpiMode.Mode0; // CLK-Idle ist low, Dataset on Falling Edge, Sample on Rising Edge
                string spiAqs = SpiDevice.GetDeviceSelector(HW_SPI_LED_Controller);
                var devicesInfo = await DeviceInformation.FindAllAsync(spiAqs);
                StatusLEDInterface = await SpiDevice.FromIdAsync(devicesInfo[0].Id, settings);
            }
            /* If initialization fails, display the exception and stop running */
            catch (Exception ex)
            {
                throw new Exception("SPI Initialization Failed", ex);
            }
        }

        private void InitIOModule()
        {
            CSadrADC = new SPIAddressObject(SPIAddressObject.eCSadrMode.SPIwithCSdemux, null, CSAdrSelection, HW_ADC_CSadr);
            CSadrDAC = new SPIAddressObject(SPIAddressObject.eCSadrMode.SPIwithCSdemux, null, CSAdrSelection, HW_DAC_CSadr);
            CSadrGPIO = new SPIAddressObject(SPIAddressObject.eCSadrMode.SPIwithCSdemux, null, CSAdrSelection, HW_GPIO_CSadr);
            CSadrLEDD = new SPIAddressObject(SPIAddressObject.eCSadrMode.SPIdedicated, null, null, 0);

            GSLEDdriver = new LEDD_TLC5941(SPIOInterface, CSadrLEDD, GSLEDdriverLatch, GSLEDdriverMode, GSLEDdriverBlank);
            DCLEDDriver = new LEDD_TLC5925(SPIOInterface, CSadrLEDD, DCLEDdriverLatch, null);
            StatusLED = new LED_APA102(StatusLEDInterface, CSadrLEDD);
            DACslave = new DAC_MCP4922(SPIOInterface, CSadrDAC, null, null);
            ADCslave = new ADC_MCP3208(SPIOInterface, CSadrADC);
            GPIOslave = new GPIO_MCP23S17(SPIOInterface, CSadrGPIO, null, null, 0);
        }

        private void InitDemo()
        {
            LEDdriverRefresh = new DispatcherTimer();
            LEDdriverRefresh.Interval = TimeSpan.FromMilliseconds(5); // 2^12/1E6 = 
            LEDdriverRefresh.Tick += LEDrefresh_Tick;

            StatusRefresh = new DispatcherTimer();
            StatusRefresh.Interval = TimeSpan.FromMilliseconds(StatusLEDrefreshCycle);
            StatusRefresh.Tick += StatusRefresh_Tick;

            AnalogRefresh = new DispatcherTimer();
            AnalogRefresh.Interval = TimeSpan.FromMilliseconds(AnalogCycle);
            AnalogRefresh.Tick += AnalogRefresh_Tick;


        }

        #endregion // Initialization



        #region EthernetCommand Handling


        //static ProtocolV1Base ProtocolV1BaseObj = new ProtocolV1Base();
        //static string ProtocolV1Marker = nameof(ProtocolV1BaseObj.MyType);

        //App_IO_Demo.IoDemoBoard ioDemoBoard = new App_IO_Demo.IoDemoBoard();
        //libCore.IOevalBoard.IoDemoBoard ioDemoBoard = new libCore.IOevalBoard.IoDemoBoard();

        //private void Server_NotifyMessageReceivedEvent(object sender, byte[] data)
        //{
        //    try
        //    {
        //        var obj = libSharedProject.ProtolV1Commands.ProtocolV1Base.ConvertJsonStingToObj(System.Text.Encoding.UTF8.GetString(data));

        //        if (obj != null)
        //        {
        //            if (obj.GetType().Equals(typeof(libSharedProject.ProtolV1Commands.IoDemoGetRequest)))
        //            {
        //                switch (((libSharedProject.ProtolV1Commands.IoDemoGetRequest)obj).Key)
        //                {
        //                    case IoDemoGetRequest.CmdValue.Adc:

        //                        this.SendText(Newtonsoft.Json.JsonConvert.SerializeObject(ioDemoBoard.GetAdc()));

        //                        break;
        //                    case IoDemoGetRequest.CmdValue.Dac:

        //                        this.SendText(Newtonsoft.Json.JsonConvert.SerializeObject(ioDemoBoard.GetDac()));

        //                        break;
        //                    case IoDemoGetRequest.CmdValue.Gpio:

        //                        this.SendText(Newtonsoft.Json.JsonConvert.SerializeObject(ioDemoBoard.GetGpio()));

        //                        break;
        //                    case IoDemoGetRequest.CmdValue.Powerstate:

        //                        this.SendText(Newtonsoft.Json.JsonConvert.SerializeObject(ioDemoBoard.GetPowerState()));

        //                        break;
        //                    case IoDemoGetRequest.CmdValue.Rgb:

        //                        this.SendText(Newtonsoft.Json.JsonConvert.SerializeObject(ioDemoBoard.GetRgb()));

        //                        break;
        //                    case IoDemoGetRequest.CmdValue.State:

        //                        this.SendText(Newtonsoft.Json.JsonConvert.SerializeObject(ioDemoBoard.GetState()));

        //                        break;
        //                    default:
        //                        break;
        //                }
        //            }
        //            else if (obj.GetType().Equals(typeof(IoDemoDac)))
        //            {
        //                ioDemoBoard.SetDac((IoDemoDac)obj);
        //            }
        //            else if (obj.GetType().Equals(typeof(IoDemoGpio)))
        //            {
        //                ioDemoBoard.SetGpio((IoDemoGpio)obj);
        //            }
        //            else if (obj.GetType().Equals(typeof(IoDemoPowerState)))
        //            {
        //                ioDemoBoard.SetPowerState((IoDemoPowerState)obj);
        //            }
        //            else if (obj.GetType().Equals(typeof(IoDemoState)))
        //            {
        //                ioDemoBoard.SetState((IoDemoState)obj);
        //            }
        //            else if (obj.GetType().Equals(typeof(IoDemoRgb)))
        //            {
        //                ioDemoBoard.SetRgb((IoDemoRgb)obj);
        //            }
        //        }
        //        else
        //        {
        //            this.AddInfoTextLine("Text:" + System.Text.Encoding.UTF8.GetString(data) + " Data:" + Converters.ConvertByteArrayToHexString(data, " "));
        //        }
        //    }
        //    catch (Exception ex)
        //    {
        //        ShowMessageBox(ExceptionHandling.GetExceptionText(new System.Exception(string.Format("Exception In: {0}", CallerName()), ex)));
        //    }
        //}

        //libShared.Interfaces.IEthernetAsync server;

        //public IEthernetAsync Server
        //{
        //    get
        //    {
        //        return server;
        //    }

        //    internal set
        //    {
        //        server = value;
        //    }
        //}

        //string port = "27200";


        //public string Port
        //{
        //    get { return port; }
        //    set
        //    {
        //        port = value;
        //        this.OnPropertyChanged();
        //    }
        //}

        //string host = "localhost";

        //public string Host
        //{
        //    get { return host; }
        //    set
        //    {
        //        host = value;
        //        this.OnPropertyChanged();
        //    }
        //}

        //ObservableCollection<string> hostNames = new ObservableCollection<string>();

        //public ObservableCollection<string> HostNames
        //{
        //    get
        //    {
        //        return hostNames;
        //    }

        //    set
        //    {
        //        hostNames = value;
        //    }
        //}

        //private string valueSendText = "Test Cmd";

        //public string ValueSendText
        //{
        //    get { return valueSendText; }
        //    set
        //    {
        //        valueSendText = value;
        //        this.OnPropertyChanged();
        //    }
        //}

        //ObservableCollection<byte> valueSendData = new ObservableCollection<byte>();

        //public ObservableCollection<byte> ValueSendData
        //{
        //    get { return valueSendData; }
        //    set
        //    {
        //        valueSendData = value;
        //        this.OnPropertyChanged();
        //    }
        //}

        //public void SendText(string text)
        //{
        //    try
        //    {
        //        if (this.Server != null)
        //        {
        //            this.Server.SendText(text);
        //        }
        //        else
        //        {
        //            this.AddInfoTextLine("Server Not Running");
        //        }
        //    }
        //    catch (Exception ex)
        //    {
        //        //AddInfoTextLine(ExceptionHandling.GetExceptionText(new System.Exception(string.Format("Exception In: {0}", CallerName()), ex)));
        //    }
        //}

        //public void SendData(byte[] data)
        //{
        //    try
        //    {
        //        if (this.Server != null)
        //        {
        //            this.Server.SendData(data);
        //        }
        //        else
        //        {
        //            this.AddInfoTextLine("Server Not Running");
        //        }
        //    }
        //    catch (Exception ex)
        //    {
        //        //AddInfoTextLine(ExceptionHandling.GetExceptionText(new System.Exception(string.Format("Exception In: {0}", CallerName()), ex)));
        //    }
        //}

        #endregion
        #region Debug-Ausgaben

        ///// <summary>
        ///// Fühgt der Infotext Liste einen weiteren Eintrag hinzu
        ///// </summary>      
        //public void AddInfoTextLine(string line)
        //{
        //    this.AddInfoTextLine(null, line);
        //}

        ///// <summary>
        ///// ühgt der Infotext Liste einen weiteren Eintrag hinzu
        ///// </summary>      
        //public async void AddInfoTextLine(object sender, string line)
        //{
        //    //try
        //    //{
        //    //    //await this.dispatcher.RunAsync(CoreDispatcherPriority.Normal, () =>
        //    //    //{
        //    //    //    if (this.InfoText.Length > 1000)
        //    //    //    {
        //    //    //        this.InfoText = line + Environment.NewLine;
        //    //    //    }
        //    //    //    else
        //    //    //    {
        //    //    //        this.InfoText += line + Environment.NewLine;
        //    //    //    }

        //    //    //    Scoll1.ChangeView(null, Scoll1.ScrollableHeight, null);

        //    //        //this.InfoTextList.Add(line);                    
        //    //    //});
        //    //}
        //    //catch (Exception ex)
        //    //{
        //    //    //ShowMessageBox(ExceptionHandling.GetExceptionText(new System.Exception(string.Format("Exception In: {0}", CallerName()), ex)));
        //    //}
        //}

        #endregion




        private void btn_SetRGBvalues(object sender, RoutedEventArgs e)
        {
            //@todo hier auf Binding setzten

            int tempRed = (int)RGBValue.MaxValue * (int)RedChannel.Value / MaxSliderValue;
            int tempGreen = (int)RGBValue.MaxValue * (int)GreenChannel.Value / MaxSliderValue;
            int tempBlue = (int)RGBValue.MaxValue * (int)BlueChannel.Value / MaxSliderValue;
            int tempIntens = (int)RGBValue.MaxValue * (int)IntensitySet.Value / MaxSliderValue;
            RGBValue newLEDval = new RGBValue { Red = (ushort)tempRed, Green = (ushort)tempGreen, Blue = (ushort)tempBlue, Intensity = (ushort)tempIntens };

            StatusLED.SetLED(0, newLEDval);
            StatusLED.UpdateLEDs();
        }

        DispatcherTimer StatusRefresh;
        const int StatusLEDrefreshCycle = 100;
        bool InstantRGBsetIsActive = false;

        private void btn_InstantRGBrefresh(object sender, RoutedEventArgs e)
        {
            if (InstantRGBsetIsActive == false)
            {
                InstantRGBsetIsActive = true;
                btn_InstantRGB.Background = new SolidColorBrush(Colors.LightGreen);
                StatusRefresh.Start();
            }
            else
            {
                InstantRGBsetIsActive = false;
                btn_InstantRGB.Background = new SolidColorBrush(Colors.LightGray);
                StatusRefresh.Stop();
            }
        }

        private void StatusRefresh_Tick(object sender, object e)
        {
            btn_SetRGBvalues(sender, null);
        }

        private void btn_SetDACvalues(object sender, RoutedEventArgs e)
        {
            int tempCh0 = (int)DAC_MCP4922.DACclassDefines.MaxADCvalue * (int)DAC0Channel.Value / MaxSliderValue;
            int tempCh1 = (int)DAC_MCP4922.DACclassDefines.MaxADCvalue * (int)DAC1Channel.Value / MaxSliderValue;

            DACslave.SetSingleChannel(0, (short)tempCh0);
            DACslave.SetSingleChannel(1, (short)tempCh1);
            //@todo Alle Kanäle gleichzeitig
        }

        private void btn_ReadADCvalues(object sender, RoutedEventArgs e)
        {
            short[] tempResults = new short[8];

            ADCslave.GetSingleChannel(0, out tempResults[0]);
            ADCslave.GetSingleChannel(1, out tempResults[1]);
            ADCslave.GetSingleChannel(2, out tempResults[2]);
            ADCslave.GetSingleChannel(3, out tempResults[3]);
            ADCslave.GetSingleChannel(4, out tempResults[4]);
            ADCslave.GetSingleChannel(5, out tempResults[5]);
            ADCslave.GetSingleChannel(6, out tempResults[6]);
            ADCslave.GetSingleChannel(7, out tempResults[7]);
            //@todo Alle Kanäle gleichzeitig

            ADC0Channel.Value = MaxSliderValue * tempResults[0] / ADC_MCP3208.ADCclassDefines.MaxADCvalue;
            ADC1Channel.Value = MaxSliderValue * tempResults[1] / ADC_MCP3208.ADCclassDefines.MaxADCvalue;
            ADC2Channel.Value = MaxSliderValue * tempResults[2] / ADC_MCP3208.ADCclassDefines.MaxADCvalue;
            ADC3Channel.Value = MaxSliderValue * tempResults[3] / ADC_MCP3208.ADCclassDefines.MaxADCvalue;
            ADC4Channel.Value = MaxSliderValue * tempResults[4] / ADC_MCP3208.ADCclassDefines.MaxADCvalue;
            ADC5Channel.Value = MaxSliderValue * tempResults[5] / ADC_MCP3208.ADCclassDefines.MaxADCvalue;
            ADC6Channel.Value = MaxSliderValue * tempResults[6] / ADC_MCP3208.ADCclassDefines.MaxADCvalue;
            ADC7Channel.Value = MaxSliderValue * tempResults[7] / ADC_MCP3208.ADCclassDefines.MaxADCvalue;
        }

        DispatcherTimer AnalogRefresh;
        const int AnalogCycle = 100;
        bool InstantAnalogTimerIsActive = false;

        private void btn_InstantSetReadADC(object sender, RoutedEventArgs e)
        {
            if (InstantAnalogTimerIsActive == false)
            {
                InstantAnalogTimerIsActive = true;
                btn_InstantAnalog.Background = new SolidColorBrush(Colors.LightGreen);
                AnalogRefresh.Start();
            }
            else
            {
                InstantAnalogTimerIsActive = false;
                btn_InstantAnalog.Background = new SolidColorBrush(Colors.LightGray);
                AnalogRefresh.Stop();
            }
        }
        
        int AnalogState = 0;
        private void AnalogRefresh_Tick(object sender, object e)
        {
            switch (AnalogState)
            {
                case 0:
                    btn_SetDACvalues(sender, null);
                    AnalogState++;
                    break;

                case 1:
                    btn_ReadADCvalues(sender, null);
                    AnalogState = 0;
                    break;

                default:
                    AnalogState = 0;
                    break;
            }
        }

        private void btn_SetDirection(object sender, RoutedEventArgs e)
        {
            string tempDirSet = txt_SetDir.Text;
            tempDirSet = Regex.Replace(tempDirSet, "[^0-1]", "");
            tempDirSet = ExtractValidString(tempDirSet, 16, E_Justification.right);
            ushort Value = Convert.ToUInt16(tempDirSet, 2);

            byte[] ValStream = new byte[2];
            ValStream[0] = (byte)Value;
            ValStream[1] = (byte)(Value >> 8);

            GPIOslave.SetDirection(ValStream);
            GPIOslave.SetPullUps(ValStream);
            GPIOslave.SetInputLogic(new byte[] { 0xFF, 0xFF} );
        }

        private void btn_SetOutputs(object sender, RoutedEventArgs e)
        {
            string tempPortSet = txt_SetOut.Text;
            tempPortSet = Regex.Replace(tempPortSet, "[^0-1]", "");
            tempPortSet = ExtractValidString(tempPortSet, 16, E_Justification.right);
            ushort Value = Convert.ToUInt16(tempPortSet, 2);

            byte[] ValStream = new byte[2];
            ValStream[0] = (byte)Value;
            ValStream[1] = (byte)(Value >> 8);

            GPIOslave.SetPorts(ValStream);
        }

        private void btn_GetInputs(object sender, RoutedEventArgs e)
        {
            byte[] ValStream = new byte[2];
            GPIOslave.GetPorts(out ValStream);

            ushort Value = (ushort)((ushort)ValStream[0] | ((ushort)ValStream[1] << 8));
            string trempPortGet = Convert.ToString(Value, 2);

            txt_GetPort.Text = LooseString(trempPortGet.PadLeft(16, '0'), 4, " ");
        }


        private void btn_BinaryLEDdriverSet(object sender, RoutedEventArgs e)
        {
            string tempPortSet = txt_BinLEDDriver.Text;
            tempPortSet = Regex.Replace(tempPortSet, "[^0-1]", "");
            tempPortSet = ExtractValidString(tempPortSet, 16, E_Justification.right);
            ushort Value = Convert.ToUInt16(tempPortSet, 2);

            string userFeedback = Convert.ToString(Value, 2);
            txt_BinLEDDriver.Text = LooseString(userFeedback.PadLeft(16, '0'), 4, " ");

            DCLEDDriver.SetLED(0, Value);
            DCLEDDriver.UpdateLEDs();
        }


        DispatcherTimer LEDdriverRefresh;
        private void LEDrefresh_Tick(object sender, object e)
        {
            GSLEDdriver.RefreshPWMcounter();
        }


        /// <summary>
        /// Methode zum optischen aufteile von Zahlen zur besseren Lesbarkeit
        /// </summary>
        /// <param name="inString">String der geteilt werden soll</param>
        /// <param name="bSize">Größe der einzelnen Blöcke</param>
        /// <param name="separator">Trennzeichen zwischen den Blöcken</param>
        /// <returns></returns>
        static string LooseString(string inString, int bSize, string separator)
        {// @optToDo: Ggf. Hier die Eigenschaften bei der Instanziierung veränderbar machen.
            string outString = inString;
            int length = outString.Length;
            if (length >= bSize)
            {
                for (int i = length - bSize; i > 0; i -= bSize)
                {
                    outString = outString.Insert(i, separator);
                }
                //outString = outString.Insert(0, separator);
            }
            return outString;
        }

        /// <summary>
        /// Methode zum Extrahiern eines Teilstrings mit vorgegebener Maximallänge aus einem String\n
        /// </summary>
        /// <param name="sInput">InputString aus dem der Teilstring extrahiert werden soll</param>
        /// <param name="width">Zulässige Länge des Gesamtstrings</param>
        /// <param name="eJust">Ausrichtung des Teilstrings im Gesamtstring (Links oder Rechtsbündig)</param>
        /// <returns></returns>
        public static string ExtractValidString(string sInput, int width, E_Justification eJust)
        {
            string outString = sInput;
            // Restlänge auswerten
            int len = outString.Length;
            if (width <= len)
            {
                if (eJust == E_Justification.left)
                {
                    // Extrahiere den linken Teil des Strings mit maximal zulässiger Länge
                    outString = outString.Substring(0, Math.Min(len, width));
                }
                else
                {
                    // Extrahiere den rechten Teil des Strings mit maximal zulässiger Länge
                    outString = outString.Substring(len - Math.Min(len, width), Math.Min(len, width));
                }
            }
            return outString;
        }

        /// <summary>
        /// Definition der Ausrichtung, Links oder Rechtsbündig
        /// </summary>
        public enum E_Justification
        {
            left,
            right,
        }

        private void btn_SetGrayValues(object sender, RoutedEventArgs e)
        {
            ushort[] tempChVals = new ushort[16];

            tempChVals[0] = (ushort)((int)LEDD_TLC5941.LEDclassDefines.MaxLEDportValue * (int)GrayCh0.Value / MaxSliderValue);
            tempChVals[1] = (ushort)((int)LEDD_TLC5941.LEDclassDefines.MaxLEDportValue * (int)GrayCh1.Value / MaxSliderValue);
            tempChVals[2] = (ushort)((int)LEDD_TLC5941.LEDclassDefines.MaxLEDportValue * (int)GrayCh2.Value / MaxSliderValue);
            tempChVals[3] = (ushort)((int)LEDD_TLC5941.LEDclassDefines.MaxLEDportValue * (int)GrayCh3.Value / MaxSliderValue);
            tempChVals[4] = (ushort)((int)LEDD_TLC5941.LEDclassDefines.MaxLEDportValue * (int)GrayCh4.Value / MaxSliderValue);
            tempChVals[5] = (ushort)((int)LEDD_TLC5941.LEDclassDefines.MaxLEDportValue * (int)GrayCh5.Value / MaxSliderValue);
            tempChVals[6] = (ushort)((int)LEDD_TLC5941.LEDclassDefines.MaxLEDportValue * (int)GrayCh6.Value / MaxSliderValue);
            tempChVals[7] = (ushort)((int)LEDD_TLC5941.LEDclassDefines.MaxLEDportValue * (int)GrayCh7.Value / MaxSliderValue);
            tempChVals[8] = (ushort)((int)LEDD_TLC5941.LEDclassDefines.MaxLEDportValue * (int)GrayCh8.Value / MaxSliderValue);
            tempChVals[9] = (ushort)((int)LEDD_TLC5941.LEDclassDefines.MaxLEDportValue * (int)GrayCh9.Value / MaxSliderValue);
            tempChVals[10] = (ushort)((int)LEDD_TLC5941.LEDclassDefines.MaxLEDportValue * (int)GrayCh10.Value / MaxSliderValue);
            tempChVals[11] = (ushort)((int)LEDD_TLC5941.LEDclassDefines.MaxLEDportValue * (int)GrayCh11.Value / MaxSliderValue);
            tempChVals[12] = (ushort)((int)LEDD_TLC5941.LEDclassDefines.MaxLEDportValue * (int)GrayCh12.Value / MaxSliderValue);
            tempChVals[13] = (ushort)((int)LEDD_TLC5941.LEDclassDefines.MaxLEDportValue * (int)GrayCh13.Value / MaxSliderValue);
            tempChVals[14] = (ushort)((int)LEDD_TLC5941.LEDclassDefines.MaxLEDportValue * (int)GrayCh14.Value / MaxSliderValue);
            tempChVals[15] = (ushort)((int)LEDD_TLC5941.LEDclassDefines.MaxLEDportValue * (int)GrayCh15.Value / MaxSliderValue);

            GSLEDdriver.SetLED(0, tempChVals);
            GSLEDdriver.UpdateLEDs();
            if (LEDdriverRefresh.IsEnabled == false)
            {
                LEDdriverRefresh.Start();
            }
        }

        bool Power1State = false;
        bool Power2State = false;
        private void btn_SetPower1(object sender, RoutedEventArgs e)
        {
            if (Power1State == false)
            {
                Power1State = true;
                PowerOut1.Write(GpioPinValue.High);
            }
            else
            {
                Power1State = false;
                PowerOut1.Write(GpioPinValue.Low);
            }
        }

        private void btn_SetPower2(object sender, RoutedEventArgs e)
        {
            if (Power2State == false)
            {
                Power2State = true;
                PowerOut2.Write(GpioPinValue.High);
            }
            else
            {
                Power2State = false;
                PowerOut2.Write(GpioPinValue.Low);
            }
        }
    }


}
