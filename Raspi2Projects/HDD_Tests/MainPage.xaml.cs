using System;
using System.Collections.ObjectModel;

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
using System.ComponentModel;
using Windows.UI;

using libCore.IOevalBoard;
using libShared.HardwareNah;
using System.Text.RegularExpressions;
using libShared;


/// <summary>
/// Programm zum Testen von Halldrucktastern
/// </summary>
/// Es sollen verschiedene Halldrucktaster- und LED-Beschaltungen erfasst werden können.
/// Halldrucktaster Output: 1k2 Widerstand, 2mA, 8mA
/// LED-Beschaltung +Schaltend, GND-Schaltend
/// 

namespace HDD_Tests
{



    // Die Vorlage "Leere Seite" ist unter http://go.microsoft.com/fwlink/?LinkId=402352&clcid=0x409 dokumentiert.


    // @todo Bindings Zwischen Oberfläche und Lokalen Variablen einbauen
    // @todo Styles in WPF einführen ... normales Windows vorgehen funktioniert hier nicht

    /// <summary>
    /// Eine leere Seite, die eigenständig verwendet werden kann oder auf die innerhalb eines Rahmens navigiert werden kann.
    /// </summary>
    public sealed partial class MainPage : Page, INotifyPropertyChanged
    {

        #region Constants
        private const int HW_CSAdrSelction_A0 = 18;
        private const int HW_CSAdrSelction_A1 = 22;
        private const int HW_CSAdrSelction_A2 = 23;

        private const int HW_ADC_CSadr = 1;
        private const int HW_DAC_CSadr = 2;
        private const int HW_GPIO_CSadr = 3;

        private const string HW_SPI_IO_Controller = "SPI0";
        private const int HW_SPI_CS_Line = 0;
        private const string HW_SPI_LED_Controller = "SPI1";

        private const int HW_DebugOut = 16;
        #endregion

        #region private Attributes

        GpioController GPIOinterface;
        SpiDevice SPIOInterface;
        // SpiDevice StatusLEDInterface; // Im Build nicht mehr verfügbar
        GpioPin CSAdrSelectionA0;
        GpioPin CSAdrSelectionA1;
        GpioPin CSAdrSelcttionA2;
        GpioPin DebugOutput;

        GpioPin[] CSAdrSelection;

        SPIAddressObject CSadrADC;
        SPIAddressObject CSadrDAC;
        SPIAddressObject CSadrGPIO;
        SPIAddressObject CSadrLED;

        //LED_APA102 StatusLED;
        ADC_MCP3208 ADCslave;
        DAC_MCP4922 DACslave;
        GPIO_MCP23S17 GPIOslave;
        int _StatusRedCh = 0;
        #endregion

        /// <summary>
        /// Command Objekt (MVVM): Empfängt Commands von der Oberfläche
        /// </summary>
        public System.Windows.Input.ICommand Command { get; protected set; }
        private ObservableCollection<Scenario> pageCollection = new ObservableCollection<Scenario>();
        public ObservableCollection<Scenario> PageCollection
        {
            get { return pageCollection; }
        }
        private Page displayPage;
        public Page DisplayPage
        {
            get
            {
                return displayPage;
            }
            set
            {
                if (displayPage == value)
                {
                    return;
                }

                this.displayPage = value;
                OnPropertyChanged();
            }
        }
        private int displayPageIndex;
        public int DisplayPageIndex
        {
            get { return displayPageIndex; }
            set
            {
                displayPageIndex = value;
                this.DisplayPage = PageCollection[displayPageIndex].PageObj;
                this.OnPropertyChanged();
            }
        }

        public MainPage()
        {
            this.InitializeComponent();
            Unloaded += MainPage_Unloaded;

            this.Command = new RelayCommand(this.CommandReceived);

            InitAll();
            this.DataContext = this;

        }

        private async void InitAll()
        {
            InitGpio();
            await InitSpi();

            InitIOModule();

            InitDemo();

            InitHardware();

            this.PageCollection.Add(new Scenario() { Title = "Debug-View", PageObj = new DebugView(this.Command) });
            this.PageCollection.Add(new Scenario() { Title = "Working-View", PageObj = new WorkPage(this.Command) });

        }

        private void InitHardware()
        {
            byte[] ValStream = new byte[2];
            ValStream[0] = 0xFF;
            ValStream[1] = 0xFF;

            GPIOslave.SetDirection(ValStream);
        }

        private void MainPage_Unloaded(object sender, object args)
        {
            // Cleanup
            // HardwareRessourcen bereinigen
            // SPIinterface.Dispose();
        }

        private void InitGpio()
        {
            GPIOinterface = GpioController.GetDefault(); /* Get the default GPIO controller on the system */
            if (GPIOinterface == null)
            {
                throw new Exception("GPIO does not exist on the current system.");
            }
            CSAdrSelectionA0 = GPIOinterface.OpenPin(HW_CSAdrSelction_A0);
            CSAdrSelectionA1 = GPIOinterface.OpenPin(HW_CSAdrSelction_A1);
            CSAdrSelcttionA2 = GPIOinterface.OpenPin(HW_CSAdrSelction_A2);
            DebugOutput = GPIOinterface.OpenPin(HW_DebugOut);
            DebugOutput.SetDriveMode(GpioPinDriveMode.Output);
            DebugOutput.Write(GpioPinValue.Low);

            /// @todo Reihenfolge prüfen
            CSAdrSelection = new GpioPin[3] { CSAdrSelectionA0, CSAdrSelectionA1, CSAdrSelcttionA2 };
        }

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
        }

        private void InitIOModule()
        {
            CSadrADC = new SPIAddressObject(SPIAddressObject.eCSadrMode.SPIwithCSdemux, null, CSAdrSelection, HW_ADC_CSadr);
            CSadrDAC = new SPIAddressObject(SPIAddressObject.eCSadrMode.SPIwithCSdemux, null, CSAdrSelection, HW_DAC_CSadr);
            CSadrGPIO = new SPIAddressObject(SPIAddressObject.eCSadrMode.SPIwithCSdemux, null, CSAdrSelection, HW_GPIO_CSadr);
            DACslave = new DAC_MCP4922(SPIOInterface, CSadrDAC, null, null);
            ADCslave = new ADC_MCP3208(SPIOInterface, CSadrADC);
            GPIOslave = new GPIO_MCP23S17(SPIOInterface, CSadrGPIO, null, null, 0);
        }

        private void InitDemo()
        {
            StatusRefresh = new DispatcherTimer();
            StatusRefresh.Interval = TimeSpan.FromMilliseconds(StatusLEDrefreshCycle);
            StatusRefresh.Tick += StatusRefresh_Tick;

            TestMachine = new DispatcherTimer();
            TestMachine.Interval = TimeSpan.FromMilliseconds(TestMachineInterval);
            TestMachine.Tick += TestHDD_Tick;
        }

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

        DispatcherTimer StatusRefresh;
        const int StatusLEDrefreshCycle = 200;
        bool InstantRGBsetIsActive = false;
        RGBValue AppStatus = RGBDefines.Black;



        string txt_currentconsumption;
        public string Txt_currentconsumption
        {
            get { return txt_currentconsumption; }
            set { txt_currentconsumption = value; OnPropertyChanged(); }
        }

        string txt_currentOut1;
        public string Txt_currentOut1
        {
            get { return txt_currentOut1; }
            set { txt_currentOut1 = value; OnPropertyChanged(); }
        }
        string txt_currentOut2;
        public string Txt_currentOut2
        {
            get { return txt_currentOut2; }
            set { txt_currentOut2 = value; OnPropertyChanged(); }
        }
        string txt_LED1;
        public string Txt_LED1
        {
            get { return txt_LED1; }
            set { txt_LED1 = value; OnPropertyChanged(); }
        }
        string txt_LED2;
        public string Txt_LED2
        {
            get { return txt_LED2; }
            set { txt_LED2 = value; OnPropertyChanged(); }
        }

        SolidColorBrush colorOut1Text = new SolidColorBrush(Colors.Gray);
        public SolidColorBrush ColorOut1Text
        {
            get { return colorOut1Text; }
            set { colorOut1Text = value; OnPropertyChanged(); }
        }

        SolidColorBrush colorOut2Text = new SolidColorBrush(Colors.Gray);
        public SolidColorBrush ColorOut2Text
        {
            get { return colorOut2Text; }
            set { colorOut2Text = value; OnPropertyChanged(); }
        }


        const int LowerLEDonThreshhold = (int)(0.2 / 5 * Int16.MaxValue); // 0.2/5 * 2^15
        const int UpperLEDonThreshhold = (Int16.MaxValue - LowerLEDonThreshhold);

        private void StatusRefresh_Tick(object sender, object e)
        {
            short shuntVoltage;
            ADCslave.GetSingleChannel(1, out shuntVoltage);
            float current = (float)shuntVoltage / Int16.MaxValue * 5 / (6 * 50) * 1000;
            Txt_currentconsumption = Convert.ToString(current) + " mA";
            // I = D/2^15 5 /(R*50) = 546

            short outCurrent;
            ADCslave.GetSingleChannel(2, out outCurrent);
            float out1current = (float)outCurrent / Int16.MaxValue * 5 / (10 * 50) * 1000;
            Txt_currentOut1 = Convert.ToString(out1current) + " mA";

            ADCslave.GetSingleChannel(3, out outCurrent);
            float out2current = (float)outCurrent / Int16.MaxValue * 5 / (10 * 50) * 1000;
            Txt_currentOut2 = Convert.ToString(out2current) + " mA";

            short ledVoltage;
            ADCslave.GetSingleChannel(4, out ledVoltage);
            float LED1voltage = (float)ledVoltage / Int16.MaxValue * 5;
            Txt_LED1 = Convert.ToString(LED1voltage) + " V";

            ADCslave.GetSingleChannel(5, out ledVoltage);
            float LED2voltage = (float)ledVoltage / Int16.MaxValue * 5;
            Txt_LED2 = Convert.ToString(LED2voltage) + " V";


            byte[] ValStream = new byte[2];
            GPIOslave.GetPorts(out ValStream);
            byte inputs = ValStream[0];

            if ((inputs & 0x01) == 0x01)
            {
                ColorOut1Text = new SolidColorBrush(Colors.Green);
            }
            else
            {
                ColorOut1Text = new SolidColorBrush(Colors.Gray);
            }

            if ((inputs & 0x02) == 0x02)
            {
                ColorOut2Text = new SolidColorBrush(Colors.Green);
            }
            else
            {
                ColorOut2Text = new SolidColorBrush(Colors.Gray);
            }
        }

        DispatcherTimer TestMachine;
        const int TestMachineInterval = 1;
        bool TestMachineIsActive = false;

        private SolidColorBrush borderColor = new SolidColorBrush(Colors.LightGray);
        public SolidColorBrush BorderColor
        {
            get { return borderColor; }
            set { borderColor = value; OnPropertyChanged(); }
        }


        private void btn_StartClick(object sender, RoutedEventArgs e)
        {
            if (DebugModeIsActive == true)
            {
                DebugModeIsActive = false;
                btn_StartDebug.Background = new SolidColorBrush(Colors.Gray);
                StatusRefresh.Stop();
            }
            if (TestMachineIsActive == false)
            {
                TestMachineIsActive = true;
                btn_StartTest.Background = new SolidColorBrush(Colors.LightGreen);
                TestMachine.Start();
                TestMachineState = eHDDtestState.InitTests;
                this.DisplayPageIndex = 1;

            }
            else
            {
                TestMachineIsActive = false;
                btn_StartTest.Background = new SolidColorBrush(Colors.LightGray);
                BorderColor = new SolidColorBrush(Colors.LightGray);
                TestMachine.Stop();
            }


        }

        bool DebugModeIsActive = false;
        private void btn_StartDebugClick(object sender, RoutedEventArgs e)
        {
            if (TestMachineIsActive == true)
            {
                TestMachineIsActive = false;
                btn_StartTest.Background = new SolidColorBrush(Colors.LightGray);
                TestMachine.Stop();
            }

            if (DebugModeIsActive == false)
            {
                DebugModeIsActive = true;
                btn_StartDebug.Background = new SolidColorBrush(Colors.LightGreen);
                StatusRefresh.Start();
                DisplayPage = PageCollection[0].PageObj;
                this.DisplayPageIndex = 0;

                // Teste Output 1 oder Output 2
                GPIOslave.SetDirection(new byte[2] { 0xFF, 0xFC });
                GPIOslave.SetPorts(new byte[2] { 0xFF, 0xFC });
            }
            else
            {
                DebugModeIsActive = false;
                btn_StartDebug.Background = new SolidColorBrush(Colors.Gray);
                StatusRefresh.Stop();
            }

        }

        private void btn_ResetClick(object sender, RoutedEventArgs e)
        {

        }

        private void btn_StartServerClick(object sender, RoutedEventArgs e)
        {

        }

        const int ledTetsTime = 20;

        // Verschaltung 
        // Analog 0 für 5-V-Referenz
        // Analog 1 für Spannung nach Shunt -> Stromabfall
        // Analog 2 für Stromaufnahme Out1
        // Analog 3 für Stromaufnahme Out2
        // Analog 4 für Stromaufnahme LED1
        // Analog 5 für Stromaufnahme LED2
        // GPIO[0] sind Eingänge
        // GPIO[1] sind Ausgänge

        HDDcharecteristics HDDdut;
        byte[] InputHistory = new byte[5];
        int TestsWaitcycles = 0;
        bool RedundantDetected;
        float idleCurrentConsumption = 0.0f;

        private string txt_HDDstatus;
        public string Txt_HDDstatus
        {
            get { return txt_HDDstatus; }
            set { txt_HDDstatus = value; OnPropertyChanged(); }
        }

        private string txt_outCount;
        public string Txt_outCount
        {
            get { return txt_outCount; }
            set { txt_outCount = value; OnPropertyChanged(); }
        }

        private string txt_outCurrent;
        public string Txt_outCurrent
        {
            get { return txt_outCurrent; }
            set { txt_outCurrent = value; OnPropertyChanged(); }
        }

        private string txt_switchDifference;
        public string Txt_switchDifference
        {
            get { return txt_switchDifference; }
            set { txt_switchDifference = value; OnPropertyChanged(); }
        }

        private string txt_LED_Count;
        public string Txt_LED_Count
        {
            get { return txt_LED_Count; }
            set { txt_LED_Count = value; OnPropertyChanged(); }
        }

        private string txt_LEDdirection;
        public string Txt_LEDdirection
        {
            get { return txt_LEDdirection; }
            set { txt_LEDdirection = value; OnPropertyChanged(); }
        }

        private string txt_HD_StatusInformation;
        public string Txt_HD_StatusInformation
        {
            get { return txt_HD_StatusInformation; }
            set { txt_HD_StatusInformation = value; OnPropertyChanged(); }
        }

        byte savedInput = 0;

        eHDDtestState TestMachineState = eHDDtestState.InitTests;
        private void TestHDD_Tick(object sender, object e)
        {
            DebugOutput.Write(GpioPinValue.High);

            byte[] ValStream = new byte[2];
            GPIOslave.GetPorts(out ValStream);
            byte inputs = ValStream[0];

            switch (TestMachineState)
            {

                // Initial Zustand vor Prüfung
                case eHDDtestState.InitTests:
                    {
                        BorderColor = new SolidColorBrush(Colors.Yellow);
                        Txt_HDDstatus = "Prüfling anschließen";
                        HDDdut = new HDDcharecteristics()
                        {
                            HDDstatus = eDUTstatus.underTest,
                            CurrentConsumption = 0,
                            Output1Logic = eOutLogic.none,
                            Output2Logic = eOutLogic.none,
                            OutputCount = 0,
                            Output1Current = 0,
                            Output2Current = 0,
                            SwitchDifference = 0,
                            ReleaseDifference = 0,
                            LEDcount = 0,
                            LEDlogic = eLEDlogic.none,
                            BadDiagnosis = "",
                        };
                        TestMachineState = eHDDtestState.WaitforConnect;
                        RedundantDetected = false;
                        idleCurrentConsumption = 0f;

                        Txt_outCount = "...";
                        Txt_outCurrent = "...";
                        Txt_switchDifference = "...";
                        Txt_LEDdirection = "...";
                        Txt_LED_Count = "...";
                        Txt_HD_StatusInformation = "Im Test";

                        GPIOslave.SetDirection(new byte[2] { 0xFF, 0xFC });
                        GPIOslave.SetPorts(new byte[2] { 0xFF, 0xFC });
                    }
                    break;

                // Warte bis Taster angeschlossen wird
                case eHDDtestState.WaitforConnect:
                    {
                        Txt_HD_StatusInformation = "Warte auf Anschluss";

                        short shuntVoltage;
                        ADCslave.GetSingleChannel(1, out shuntVoltage);
                        float current = (float)shuntVoltage / Int16.MaxValue * 5 / (6 * 50) * 1000;
                        Txt_currentconsumption = Convert.ToString(current);

                        // Wenn mehr als 1 mA Strom aufgenommen wird
                        if ((current) >= 1.0)
                        {
                            TestsWaitcycles++;
                        }
                        else
                        {
                            TestsWaitcycles = 0;
                        }
                        // Entprellzeit abwarten. 
                        if (TestsWaitcycles >= 50)
                        {
                            AppStatus = RGBDefines.Yellow;
                            Txt_HDDstatus = "Halldrucktaster betätigen";
                            idleCurrentConsumption = current;

                            TestsWaitcycles = 0;
                            // Fehler feststellen
                            if ((inputs & 0x03) == 0x03)
                            {
                                HDDdut.HDDstatus = eDUTstatus.testedBad;
                                HDDdut.BadDiagnosis += "Beide Ausgänge im unbetätigtem Zustand aktiv \n";
                            }

                            InputHistory[0] = (byte)(inputs & 0x03);
                            TestMachineState = eHDDtestState.WaitForInit;
                        }
                    }
                    break;

                // Warte auf erste Flanke
                case eHDDtestState.WaitForInit:
                    {
                        Txt_HD_StatusInformation = "Warte auf Betätigung";
                        if ((byte)(inputs & 0x03) != InputHistory[0])
                        {
                            InputHistory[1] = (byte)(inputs & 0x03);
                            // Prüfen ob beide Inputs gewechselt sind
                            if (((inputs ^ 0x03) & 0x03) == (InputHistory[0]))
                            {
                                InputHistory[2] = (byte)(inputs & 0x03);
                                HDDdut.SwitchDifference = 0;
                                TestMachineState = eHDDtestState.TestOutput1;
                                RedundantDetected = true;
                            }
                            else
                            {
                                TestMachineState = eHDDtestState.WaitForNextInit;
                            }
                        }

                        // Abbruchbedingung: Wenn Drucktaster entfernt wird
                        short shuntVoltage;
                        ADCslave.GetSingleChannel(1, out shuntVoltage);
                        float current = (float)shuntVoltage / Int16.MaxValue * 5 / (6 * 50) * 1000;
                        // Wenn mehr als 1 mA Strom aufgenommen wird
                        if ((current) < 0.5)
                        {
                            TestMachineState = eHDDtestState.CleanUp;
                        }
                        TestsWaitcycles = 0;
                    }
                    break;

                // Warte auf nächste Flanke
                case eHDDtestState.WaitForNextInit:
                    {
                        Txt_HD_StatusInformation = "Warte ggf. auf zweiten Sensor";
                        // Warte kurz auf zweite Flanke
                        TestsWaitcycles++;
                        if (TestsWaitcycles >= 100)
                        {
                            HDDdut.SwitchDifference = -1;
                            TestsWaitcycles = 0;
                            InputHistory[2] = 0x80;
                            RedundantDetected = false;
                            TestMachineState = eHDDtestState.TestOutput1;
                        }
                        else
                        {
                            // Der Zustand muss wirklich neu sein.
                            if (((byte)(inputs & 0x03) != InputHistory[0]) && ((byte)(inputs & 0x03) != InputHistory[1]))
                            {
                                HDDdut.SwitchDifference = TestsWaitcycles;

                                TestsWaitcycles = 0;
                                InputHistory[2] = (byte)(inputs & 0x03);
                                RedundantDetected = true;
                                TestMachineState = eHDDtestState.TestOutput1;
                            }
                        }

                    }
                    break;

                // Teste Ersten Output
                case eHDDtestState.TestOutput1:
                    {
                        TestsWaitcycles++;
                        if (TestsWaitcycles == 1)
                        {
                            savedInput = inputs;
                            Txt_HD_StatusInformation = "Teste ersten Ausgang";
                            // Teste Output 1 oder Output 2
                            GPIOslave.SetDirection(new byte[2] { 0xFF, 0xFC });
                            GPIOslave.SetPorts(new byte[2] { 0xFF, 0xFF });
                        }
                        else
                        {
                            short tempCurrent;
                            ADCslave.GetSingleChannel(1, out tempCurrent);

                            if ((savedInput & 0x03) == 0x01)
                            {
                                ADCslave.GetSingleChannel(2, out tempCurrent);
                                float out1current = (float)tempCurrent / Int16.MaxValue * 5 / (10 * 50) * 1000;
                                Txt_currentOut1 = Convert.ToString(out1current);
                                HDDdut.Output1Current = out1current;
                            }
                            else if ((savedInput & 0x03) == 0x02)
                            {
                                ADCslave.GetSingleChannel(3, out tempCurrent);
                                float out2current = (float)tempCurrent / Int16.MaxValue * 5 / (10 * 50) * 1000;
                                Txt_currentOut2 = Convert.ToString(out2current);
                                HDDdut.Output2Current = out2current;
                            }
                            else
                            {
                                HDDdut.HDDstatus = eDUTstatus.testedBad;
                                HDDdut.BadDiagnosis += "Kein Ausgang im betätigen Zustand aktiv => Taster entweder verpolt, oder defekt \n";
                            }

                            GPIOslave.SetPorts(new byte[2] { 0xFF, 0xFC });
                            Txt_HDDstatus = "Halldrucktaster lösen";
                            TestMachineState = eHDDtestState.WaitForRelease;
                            TestsWaitcycles = 0;
                        }
                        
                    }
                    break;

                case eHDDtestState.WaitForRelease:
                    {
                        Txt_HD_StatusInformation = "Warte auf Lösung";
                        // Warte wieder auf erste Flanke
                        if ((byte)(inputs & 0x03) != InputHistory[2])
                        {
                            InputHistory[3] = inputs;
                            // Prüfen ob beide Inputs gewechselt sind
                            if (((inputs ^ 0x03) & 0x03) == (InputHistory[2]))
                            {
                                InputHistory[4] = (byte)(inputs & 0x03);
                                HDDdut.ReleaseDifference = 0;
                                TestMachineState = eHDDtestState.TestOutput2;
                            }
                            else
                            {
                                if (RedundantDetected == true)
                                {
                                    TestMachineState = eHDDtestState.WaitForNextRelease;
                                }
                                else
                                {
                                    TestMachineState = eHDDtestState.TestOutput2;
                                }
                            }
                        }

                        // Abbruchbedingung: Wenn Drucktaster entfernt wird
                        short shuntVoltage;
                        ADCslave.GetSingleChannel(1, out shuntVoltage);
                        float current = (float)shuntVoltage / Int16.MaxValue * 5 / (6 * 50) * 1000;
                        // Wenn mehr als 1 mA Strom aufgenommen wird
                        if ((current) < 0.5)
                        {
                            TestMachineState = eHDDtestState.CleanUp;
                        }
                        TestsWaitcycles = 0;
                    }
                    break;
                // @todo Hinweis für Abbruchbedingung nach 3 s

                case eHDDtestState.WaitForNextRelease:
                    {
                        Txt_HD_StatusInformation = "Warte ggf. auf zweiten Sensor";
                        // Warte kurz auf zweite Flanke
                        TestsWaitcycles++;
                        if (TestsWaitcycles >= 100)
                        {
                            HDDdut.ReleaseDifference = -1;
                            TestsWaitcycles = 0;
                            InputHistory[4] = 0x80;
                            HDDdut.HDDstatus = eDUTstatus.testedBad;
                            HDDdut.BadDiagnosis += "Zweiter Ausgang wechselt nicht (rechtzeitig) in Grundzustand \n";

                            TestMachineState = eHDDtestState.TestOutput2;
                        }
                        else
                        {
                            if (((byte)(inputs & 0x03) != InputHistory[2]) && ((byte)(inputs & 0x03) != InputHistory[3]))
                            {
                                HDDdut.ReleaseDifference = TestsWaitcycles;
                                TestsWaitcycles = 0;
                                InputHistory[4] = (byte)(inputs & 0x03);
                                TestMachineState = eHDDtestState.TestOutput2;
                            }
                        }

                    }
                    break;

                case eHDDtestState.TestOutput2:
                    {
                        TestsWaitcycles++;
                        if (TestsWaitcycles == 1)
                        {
                            savedInput = inputs;
                            Txt_HD_StatusInformation = "Teste zweiten Ausgang";
                            // Teste Output 1 oder Output 2
                            GPIOslave.SetDirection(new byte[2] { 0xFF, 0xFC });
                            GPIOslave.SetPorts(new byte[2] { 0xFF, 0xFF });
                        }
                        else
                        {
                            short tempCurrent;
                            ADCslave.GetSingleChannel(1, out tempCurrent);

                            short outCurrent;
                            if ((savedInput & 0x03) == 0x01)
                            {
                                ADCslave.GetSingleChannel(2, out outCurrent);
                                float out1current = (float)outCurrent / Int16.MaxValue * 5 / (10 * 50) * 1000;
                                Txt_currentOut1 = Convert.ToString(out1current);
                                HDDdut.Output1Current = out1current;
                            }
                            else if ((savedInput & 0x03) == 0x02)
                            {
                                ADCslave.GetSingleChannel(3, out outCurrent);
                                float out2current = (float)outCurrent / Int16.MaxValue * 5 / (10 * 50) * 1000;
                                Txt_currentOut2 = Convert.ToString(out2current);
                                HDDdut.Output2Current = out2current;
                            }
                            else
                            {
                                if (RedundantDetected == true)
                                {
                                    HDDdut.HDDstatus = eDUTstatus.testedBad;
                                    HDDdut.BadDiagnosis += "Zweiter Ausgang lässt sich nicht testen. Beide Ausgänge im unbetätigten Zustand deaktiviert \n";
                                }
                            }
                            TestMachineState = eHDDtestState.TestLED1high;
                            GPIOslave.SetPorts(new byte[2] { 0xFF, 0xFC });
                            TestsWaitcycles = 0;
                        }
                    }
                    break;


                // Wenn mehr als 2 mA durch die LED fließt, gilt diese als bestückt und Funktionsfähig
                // Testen der LEDs Zuerst LED1 mit positiver Logik
                case eHDDtestState.TestLED1high:
                    {
                        Txt_HDDstatus = "Testen der LEDs";
                        Txt_HD_StatusInformation = "Teste erste LED auf positive Logik";
                        short preLEDvolt;

                        TestsWaitcycles++;
                        // Mit verschiedenen Logiken
                        if (TestsWaitcycles == 1)
                        {
                            GPIOslave.SetPorts(new byte[2] { 0xFF, 0xFF });
                            GPIOslave.SetDirection(new byte[2] { 0xFF, 0xF8 });
                            GPIOslave.SetPorts(new byte[2] { 0xFF, 0xFC });
                        }
                        if (TestsWaitcycles >= ledTetsTime)
                        {
                            TestsWaitcycles = 0;
                            // Prüfen ob LED +Schaltend ist
                            ADCslave.GetSingleChannel(4, out preLEDvolt);
                            if ((preLEDvolt) < UpperLEDonThreshhold)
                            {
                                HDDdut.LEDcount++;
                                HDDdut.LEDlogic = eLEDlogic.Vdriven;
                            }
                            TestMachineState = eHDDtestState.TestLED1low;
                        }

                    }
                    break;

                // Teste LED1 mit negativer Logik
                case eHDDtestState.TestLED1low:
                    {
                        Txt_HD_StatusInformation = "Teste erste LED auf negative Logik";


                        short preLEDvolt;
                        TestsWaitcycles++;
                        // Mit verschiedenen Logiken
                        if (TestsWaitcycles == 1)
                        {
                            GPIOslave.SetPorts(new byte[2] { 0xFF, 0xF8 });
                        }
                        if (TestsWaitcycles >= ledTetsTime)
                        {
                            TestsWaitcycles = 0;
                            // Prüfen ob LED +GND-Schaltend ist

                            ADCslave.GetSingleChannel(4, out preLEDvolt);
                            if ((preLEDvolt) > LowerLEDonThreshhold)
                            {
                                HDDdut.LEDcount++;
                                if (HDDdut.LEDlogic == eLEDlogic.Vdriven)
                                {
                                    HDDdut.HDDstatus = eDUTstatus.testedBad;
                                    HDDdut.BadDiagnosis += "LED wahrscheinlich kurzgeschlossen";
                                }
                                HDDdut.LEDlogic = eLEDlogic.GNDdriven;
                            }
                            TestMachineState = eHDDtestState.TestLED2high;
                        }

                    }
                    break;

                // Teste LED2 mit positiver Logik
                case eHDDtestState.TestLED2high:
                    {
                        Txt_HD_StatusInformation = "Teste zweite LED auf positive Logik";

                        short preLEDvolt;
                        TestsWaitcycles++;
                        // Mit verschiedenen Logiken
                        if (TestsWaitcycles == 1)
                        {
                            GPIOslave.SetDirection(new byte[2] { 0xFF, 0xF4 });
                            GPIOslave.SetPorts(new byte[2] { 0xFF, 0xFC });
                        }
                        if (TestsWaitcycles >= ledTetsTime)
                        {
                            TestsWaitcycles = 0;
                            // Prüfen ob LED +Schaltend ist
                            ADCslave.GetSingleChannel(5, out preLEDvolt);
                            if ((preLEDvolt) < UpperLEDonThreshhold)
                            {
                                HDDdut.LEDcount++;
                                if (HDDdut.LEDlogic != eLEDlogic.Vdriven)
                                {
                                    HDDdut.HDDstatus = eDUTstatus.testedBad;
                                    HDDdut.BadDiagnosis += "LED Schaltlogik, nicht einheitlich";
                                }
                            }

                            TestMachineState = eHDDtestState.TestLED2low;
                        }
                    }
                    break;

                // Teste LED2 mit negativer Logik
                case eHDDtestState.TestLED2low:
                    {
                        Txt_HD_StatusInformation = "Teste zweite LED auf negative Logik";

                        short preLEDvolt;
                        TestsWaitcycles++;
                        // Mit verschiedenen Logiken
                        if (TestsWaitcycles == 1)
                        {
                            GPIOslave.SetPorts(new byte[2] { 0xFF, 0xF4 });
                        }
                        if (TestsWaitcycles >= ledTetsTime)
                        {
                            TestsWaitcycles = 0;
                            // Prüfen ob LED GND-Schaltend ist
                            GPIOslave.SetPorts(new byte[2] { 0xFF, 0xF4 });
                            ADCslave.GetSingleChannel(5, out preLEDvolt);
                            if ((preLEDvolt) > LowerLEDonThreshhold) //@todo hier korrekten Wert eintragen
                            {
                                HDDdut.LEDcount++;
                                if (HDDdut.LEDlogic != eLEDlogic.GNDdriven)
                                {
                                    HDDdut.HDDstatus = eDUTstatus.testedBad;
                                    HDDdut.BadDiagnosis += "LED Schaltlogik, nicht einheitlich";
                                }
                            }
                            TestMachineState = eHDDtestState.TestResult;

                            GPIOslave.SetDirection(new byte[2] { 0xFF, 0xFC });
                        }
                    }
                    break;

                // Testergebnisse zusammenstellen
                case eHDDtestState.TestResult:
                    {
                        Txt_HD_StatusInformation = "Ergebnisse zusammenstellen";
                        if ((HDDdut.LEDcount != 2) && (HDDdut.LEDcount != 0))
                        {
                            HDDdut.HDDstatus = eDUTstatus.testedBad;
                            HDDdut.BadDiagnosis += "Fehler bei LED-Beschaltung  \n";
                            if (HDDdut.LEDcount > 2)
                            {
                                HDDdut.HDDstatus = eDUTstatus.testedBad;
                                HDDdut.BadDiagnosis += "LED schaltet in beide Richtungen \n";
                            }
                            if (HDDdut.LEDcount == 1)
                            {
                                HDDdut.HDDstatus = eDUTstatus.testedBad;
                                HDDdut.BadDiagnosis += "Es funktioniert nur eine LED (wahrscheinlich Problem bei der Verdrahtung) \n";
                            }
                        }
                        else
                        {
                            Txt_LED_Count = Convert.ToString(HDDdut.LEDcount);
                        }
                        if (HDDdut.LEDlogic == eLEDlogic.Vdriven)
                        {
                            Txt_LEDdirection = "+Schaltend";
                        }
                        else if (HDDdut.LEDlogic == eLEDlogic.GNDdriven)
                        {
                            Txt_LEDdirection = "GND-Schaltend";
                        }
                        else if (HDDdut.LEDlogic == eLEDlogic.inSeries)
                        {
                            Txt_LEDdirection = "Durchverdrahtet";
                        }
                        else
                        {
                            Txt_LEDdirection = "...";
                        }

                        if (RedundantDetected == true)
                        {
                            Txt_outCount = "Redundant";
                            Txt_outCurrent = HDDdut.Output1Current + " mA / " + HDDdut.Output2Current + " mA";
                        }
                        else
                        {
                            Txt_outCount = "Single";
                            Txt_outCurrent = Math.Max(HDDdut.Output1Current, HDDdut.Output2Current) + " mA";
                        }

                        // Überprüfen der Schaltlogiken
                        if (RedundantDetected == true)
                        {// Bei redundanter Belegung
                            if ((InputHistory[0] == 0x02) && (InputHistory[2] == 0x01) && (InputHistory[4] == 0x02))
                            {
                                HDDdut.Output1Logic = eOutLogic.OneIsNO;
                                HDDdut.Output2Logic = eOutLogic.OneIsNC;
                            }
                            else if ((InputHistory[0] == 0x01) && (InputHistory[2] == 0x02) && (InputHistory[4] == 0x01))
                            {
                                HDDdut.Output1Logic = eOutLogic.OneIsNC;
                                HDDdut.Output2Logic = eOutLogic.OneIsNO;
                                HDDdut.HDDstatus = eDUTstatus.testedBad;
                                HDDdut.BadDiagnosis += "Polarität der Ausgänge verdreht (entweder Magnete, oder Leitungen) \n";
                            }
                            else
                            {
                                HDDdut.HDDstatus = eDUTstatus.testedBad;
                                HDDdut.BadDiagnosis += "Redundanter Drucktaster erkannt, jedoch fehlerhaft \n";
                            }

                            if (HDDdut.ReleaseDifference == -1)
                            {
                                HDDdut.HDDstatus = eDUTstatus.testedBad;
                                HDDdut.BadDiagnosis += "Beim Lösen verzögerte der zweite Sensor zu stark \n";
                            }
                        }
                        else
                        {// Bei einfacher Belegung
                            if ((InputHistory[0] == 0x00) && (InputHistory[2] == 0x01) && (InputHistory[4] == 0x00))
                            {
                                HDDdut.Output1Logic = eOutLogic.OneIsNO;
                                HDDdut.Output2Logic = eOutLogic.none;
                            }
                            else if ((InputHistory[0] == 0x01) && (InputHistory[2] == 0x00) && (InputHistory[4] == 0x01))
                            {
                                HDDdut.Output1Logic = eOutLogic.OneIsNC;
                                HDDdut.Output2Logic = eOutLogic.none;
                                HDDdut.HDDstatus = eDUTstatus.testedBad;
                                HDDdut.BadDiagnosis += "Polarität des Ausgangs verdreht (wahrscheinlich Magnete) \n";
                            }
                            else if ((InputHistory[0] == 0x00) && (InputHistory[2] == 0x02) && (InputHistory[4] == 0x00))
                            {
                                HDDdut.Output1Logic = eOutLogic.OneIsNC;
                                HDDdut.Output2Logic = eOutLogic.none;
                                HDDdut.HDDstatus = eDUTstatus.testedBad;
                                HDDdut.BadDiagnosis += "Polarität der Ausgänge verdreht (Leitung falsch angeschlossen) \n";
                            }
                            else if ((InputHistory[0] == 0x02) && (InputHistory[2] == 0x00) && (InputHistory[4] == 0x02))
                            {
                                HDDdut.Output1Logic = eOutLogic.OneIsNC;
                                HDDdut.Output2Logic = eOutLogic.none;
                                HDDdut.HDDstatus = eDUTstatus.testedBad;
                                HDDdut.BadDiagnosis += "Polarität der Ausgänge verdreht (Leitung und Magnete vertauscht) \n";
                            }
                            else
                            {
                                HDDdut.HDDstatus = eDUTstatus.testedBad;
                                HDDdut.BadDiagnosis += "Redundanter Drucktaster erkannt, jedoch fehlerhaft \n";
                            }
                        }

                        // Überprüfen der Schaltdifferenz (nur für redundante Taster 
                        const int maxSwitchDifference = 40;
                        if (RedundantDetected == true)
                        {
                            Txt_switchDifference = HDDdut.SwitchDifference + " ms / " + HDDdut.ReleaseDifference + " ms";
                            if ((HDDdut.SwitchDifference > maxSwitchDifference) || (HDDdut.ReleaseDifference > maxSwitchDifference))
                            {
                                HDDdut.HDDstatus = eDUTstatus.testedBad;
                                HDDdut.BadDiagnosis += "Zeitdifferenz beim Betätigen oder beim Lösen zu hoch. (Zulässige Dauer beträgt: " + maxSwitchDifference + " ms)\n";
                            }
                        }

                        if (HDDdut.HDDstatus == eDUTstatus.testedBad)
                        {
                            BorderColor = new SolidColorBrush(Colors.Red);
                            Txt_HD_StatusInformation = HDDdut.BadDiagnosis;
                            Txt_HDDstatus = "Halldrucktaster nicht i.O.";
                        }
                        else
                        {
                            BorderColor = new SolidColorBrush(Colors.Green);
                            Txt_HDDstatus = "Halldrucktaster i.O.";
                        }



                        for (int i = 0; i < InputHistory.Length; i++)
                        {
                            InputHistory[i] = 0;
                        }
                        TestMachineState = eHDDtestState.WaitforDisconnect;
                        Txt_HD_StatusInformation += "\n Warte auf Abschluss";
                    }
                    break;

                case eHDDtestState.WaitforDisconnect:
                    {


                        short shuntVoltage;
                        ADCslave.GetSingleChannel(1, out shuntVoltage);
                        float current = (float)shuntVoltage / Int16.MaxValue * 5 / (6 * 50) * 1000;
                        // Wenn mehr als 1 mA Strom aufgenommen wird
                        if ((current) < 0.5)
                        {
                            TestsWaitcycles++;
                        }
                        if (TestsWaitcycles >= 50)
                        {
                            Txt_HDDstatus = "Halldrucktaster betätigen";
                            HDDdut.CurrentConsumption = 0;

                            TestsWaitcycles = 0;

                            TestMachineState = eHDDtestState.InitTests;
                        }
                    }
                    break;

                case eHDDtestState.CleanUp:
                    {

                        Txt_HDDstatus = "Halldrucktaster betätigen";
                        HDDdut.CurrentConsumption = 0; //@todo hier korrekten Wert eintragen
                        TestsWaitcycles = 0;

                        TestMachineState = eHDDtestState.InitTests;
                    }
                    break;


                default:
                    break;
            }

            if ((inputs & 0x01) == 0x01)
            {
                ColorOut1Text = new SolidColorBrush(Colors.Green);
            }
            else
            {
                ColorOut1Text = new SolidColorBrush(Colors.Gray);
            }

            if ((inputs & 0x02) == 0x02)
            {
                ColorOut2Text = new SolidColorBrush(Colors.Green);
            }
            else
            {
                ColorOut2Text = new SolidColorBrush(Colors.Gray);
            }
            DebugOutput.Write(GpioPinValue.Low);

        }

        enum eHDDtestState
        {
            InitTests,
            WaitforConnect,
            WaitForInit,
            WaitForNextInit,
            TestOutput1,
            WaitForRelease,
            WaitForNextRelease,
            TestOutput2,
            TestLED1high,
            TestLED1low,
            TestLED2high,
            TestLED2low,
            TestResult,
            WaitforDisconnect,
            CleanUp,
        }

        enum eLEDlogic
        {
            none,
            Vdriven,
            GNDdriven,
            inSeries,
        }

        enum eOutLogic
        {
            OneIsNO,
            OneIsNC,
            none,
        }

        enum eDUTstatus
        {
            underTest,
            testedGood,
            testedBad,
        }

        struct HDDcharecteristics
        {
            public eDUTstatus HDDstatus;

            public eOutLogic Output1Logic;
            public eOutLogic Output2Logic;
            public float CurrentConsumption;
            public float Output1Current;
            public float Output2Current;
            public int OutputCount;
            public int SwitchDifference;
            public int ReleaseDifference;

            public int LEDcount;
            public eLEDlogic LEDlogic;
            public string BadDiagnosis;
        }





        bool TestOutput1IsTrue = false;
        bool TestOutput2IsTrue = false;

        private SolidColorBrush testOutput1ButtonColor = new SolidColorBrush(Colors.Gray);

        public SolidColorBrush TestOutput1ButtonColor
        {
            get { return testOutput1ButtonColor; }
            set { testOutput1ButtonColor = value; OnPropertyChanged(); }
        }
        private SolidColorBrush testOutput2ButtonColor = new SolidColorBrush(Colors.Gray);

        public SolidColorBrush TestOutput2ButtonColor
        {
            get { return testOutput2ButtonColor; }
            set { testOutput2ButtonColor = value; OnPropertyChanged(); }
        }

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

                    case "TestOutput1Click":
                        if (TestOutput1IsTrue == true)
                        {
                            TestOutput1IsTrue = false;
                            TestOutput1ButtonColor = new SolidColorBrush(Colors.Gray);
                            GPIOslave.SetPorts(new byte[2] { 0xFF, 0xFC });
                        }
                        else
                        {
                            TestOutput1IsTrue = true;
                            TestOutput1ButtonColor = new SolidColorBrush(Colors.Green);
                            GPIOslave.SetPorts(new byte[2] { 0xFF, 0xFD });
                        }
                        break;

                    case "TestOutput2Click":
                        if (TestOutput2IsTrue == true)
                        {
                            TestOutput2IsTrue = false;
                            TestOutput2ButtonColor = new SolidColorBrush(Colors.Gray);
                            GPIOslave.SetPorts(new byte[2] { 0xFF, 0xFC });

                        }
                        else
                        {
                            TestOutput2IsTrue = true;
                            TestOutput2ButtonColor = new SolidColorBrush(Colors.Green);
                            GPIOslave.SetPorts(new byte[2] { 0xFF, 0xFE });
                        }
                        break;

                    case "SetLED1high":
                        GPIOslave.SetDirection(new byte[2] { 0xFF, 0xF8 });
                        GPIOslave.SetPorts(new byte[2] { 0xFF, 0xFC });
                        break;

                    case "SetLED1low":
                        GPIOslave.SetDirection(new byte[2] { 0xFF, 0xF8 });
                        GPIOslave.SetPorts(new byte[2] { 0xFF, 0xF8 });
                        break;

                    case "SetLED2high":
                        GPIOslave.SetDirection(new byte[2] { 0xFF, 0xF4 });
                        GPIOslave.SetPorts(new byte[2] { 0xFF, 0xFC });
                        break;

                    case "SetLED2low":
                        GPIOslave.SetDirection(new byte[2] { 0xFF, 0xF4 });
                        GPIOslave.SetPorts(new byte[2] { 0xFF, 0xF4 });
                        break;

                    case "ResetLEDs":
                        GPIOslave.SetDirection(new byte[2] { 0xFF, 0xFC });
                        GPIOslave.SetPorts(new byte[2] { 0xFF, 0xFC });
                        break;

                    default:
                        //@todo
                        break;
                }
            }
            catch (System.Exception ex)
            {
                //@todo
            }
        }
    }

    public class Scenario
    {
        public string Title { get; set; }

        public Windows.UI.Xaml.Controls.Page PageObj { get; set; }

        public override string ToString()
        {
            return Title;
        }
    }
}