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
using Windows.Networking.Connectivity;
using Windows.UI.Core;
using libCore.IOevalBoard;
using libShared.HardwareNah;

// Die Vorlage "Leere Seite" ist unter http://go.microsoft.com/fwlink/?LinkId=402352&clcid=0x409 dokumentiert.

namespace RGB_Demo
{

    /// <summary>
    /// Eine leere Seite, die eigenständig verwendet werden kann oder auf die innerhalb eines Rahmens navigiert werden kann.
    /// </summary>
    public sealed partial class MainPage : Page
    {
        Windows.UI.Core.CoreDispatcher dispatcher = Windows.UI.Core.CoreWindow.GetForCurrentThread().Dispatcher;

        private const string SPI_DEMO_CONTROLLER_NAME = "SPI0";
        private const string SPI_STATUS_CONTROLLER_NAME = "SPI1";

        private const int SPI_CS_LINE = 0;

        // Interface Objects
        private GpioController GPIOvar;
        private SpiDevice SPIinterface_Status;
        private SpiDevice SPIinterface_Demo;
        private DispatcherTimer StatusTimer;
        private DispatcherTimer ArrayTimer;

        // Objects for cyclic access
        SPIAddressObject CSadrLEDD;
        LED_APA102 StatusLED;
        LED_APA102 LEDArray;

        int cycleCount = 0;
        int statusMachineCount = 0;
        int demoMachineCount = 0;

        const int refreshCycle = 5;

        /// <summary>
        /// Main-Page
        /// </summary>
        public MainPage()
        {
            this.InitializeComponent();
            InitAll();

            this.DataContext = this;
        }


        /// <summary>
        /// TimerInitialisation
        /// </summary>
        private async void InitAll()
        {
            StatusTimer = new DispatcherTimer();
            StatusTimer.Interval = TimeSpan.FromMilliseconds(refreshCycle);
            StatusTimer.Tick += StatusTimer_Tick;
            ArrayTimer = new DispatcherTimer();
            ArrayTimer.Interval = TimeSpan.FromMilliseconds(refreshCycle * 10);
            ArrayTimer.Tick += ArrayTimer_Tick;

            GPIOvar = GpioController.GetDefault(); /* Get the default GPIO controller on the system */
            await InitSpi();        /* Initialize the SPI controller                */



            CSadrLEDD = new SPIAddressObject(SPIAddressObject.eCSadrMode.SPIdedicated, null, null, 0);
            StatusLED = new LED_APA102(SPIinterface_Status, CSadrLEDD);
            // Bei der Instanziierung wird erstes LED-Objekt erstellt.
            LEDArray = new LED_APA102(SPIinterface_Demo, CSadrLEDD);
            // Bei der Instanziierung wird erstes LED-Objekt erstellt.


            LEDArray.AddLED(RGBDefines.Black);
            LEDArray.AddLED(RGBDefines.Black);
            LEDArray.AddLED(RGBDefines.Black);
            LEDArray.AddLED(RGBDefines.Black);
            LEDArray.AddLED(RGBDefines.Black);
            LEDArray.AddLED(RGBDefines.Black);
            LEDArray.AddLED(RGBDefines.Black);
            LEDArray.AddLED(RGBDefines.Black);

            // LED-Demo
            StatusTimer.Start();
            ArrayTimer.Start();
        }

        private async Task InitSpi()
        {
            try
            {
                var settings = new SpiConnectionSettings(SPI_CS_LINE); /* Create SPI initialization settings                               */
                settings.ClockFrequency = 2000000;                             /* Datasheet specifies maximum SPI clock frequency of 10MHz         */
                settings.Mode = SpiMode.Mode0; // Bedeutet, dass CLK-Idle ist low, Sample bei Steigender Flank

                string spiAqs0 = SpiDevice.GetDeviceSelector(SPI_STATUS_CONTROLLER_NAME);       /* Find the selector string for the SPI bus controller          */
                var devicesInfo0 = await DeviceInformation.FindAllAsync(spiAqs0);         /* Find the SPI bus controller device with our selector string  */
                SPIinterface_Status = await SpiDevice.FromIdAsync(devicesInfo0[0].Id, settings);  /* Create an SpiDevice with our bus controller and SPI settings */

                string spiAqs1 = SpiDevice.GetDeviceSelector(SPI_DEMO_CONTROLLER_NAME);       /* Find the selector string for the SPI bus controller          */
                var devicesInfo1 = await DeviceInformation.FindAllAsync(spiAqs1);         /* Find the SPI bus controller device with our selector string  */
                SPIinterface_Demo = await SpiDevice.FromIdAsync(devicesInfo1[0].Id, settings);  /* Create an SpiDevice with our bus controller and SPI settings */


            }
            /* If initialization fails, display the exception and stop running */
            catch (Exception ex)
            {
                throw new Exception("SPI Initialization Failed", ex);
            }
        }

        private int currCount = 0;
        private bool currAccenting = true;
        private void StatusTimer_Tick(object sender, object e)
        {
            RGBValue tempLEDobj;
            StatusLED.GetLEDvalue(0,out tempLEDobj);

            if (currAccenting == true)
            {
                currCount += 0x500;
            }
            else
            {
                currCount -= 0x500;
            }

            switch (statusMachineCount)
            {
                case 0: // fading red
                    tempLEDobj.Red = (byte)currCount;
                    break;
                case 1: // fading green
                    tempLEDobj.Green = (byte)currCount;
                    break;
                case 2: // fading blue
                    tempLEDobj.Blue = (byte)currCount;
                    break;
                case 3: // fading red
                    tempLEDobj.Red = (byte)currCount;
                    tempLEDobj.Green = (byte)currCount;
                    break;
                case 4: // fading red
                    tempLEDobj.Red = (byte)currCount;
                    tempLEDobj.Blue = (byte)currCount;
                    break;
                case 5: // fading red
                    tempLEDobj.Green = (byte)currCount;
                    tempLEDobj.Blue = (byte)currCount;
                    break;
                case 6: // fading red
                    tempLEDobj.Red = (byte)currCount;
                    tempLEDobj.Green = (byte)currCount;
                    tempLEDobj.Blue = (byte)currCount;
                    break;
                case 7: // fading red
                    tempLEDobj.Red = (byte)currCount;
                    statusMachineCount = 0;
                    break;

                default:
                    statusMachineCount = 0;
                    break;
            }
            tempLEDobj.Intensity = RGBValue.MaxValue;

            if ((currAccenting == true) && (currCount >= 0x6400))
            {
                currAccenting = false;
            }
            if ((currAccenting == false) && (currCount <= 0))
            {
                currAccenting = true;
                statusMachineCount++;
            }

            StatusLED.SetLED(0, tempLEDobj);
            StatusLED.UpdateLEDs();
        }

        private void ArrayTimer_Tick(object sender, object e)
        {
            RGBValue tempNewLEDval;
            StatusLED.GetLEDvalue(0, out tempNewLEDval);
            cycleCount++;

            switch (cycleCount)
            {
                case 1:
                    LEDArray.SetLED(8, RGBDefines.Black);
                    LEDArray.SetLED(0, tempNewLEDval);
                    break;

                case 2:
                    LEDArray.SetLED(0, RGBDefines.Black);
                    LEDArray.SetLED(1, tempNewLEDval);
                    break;

                case 3:
                    LEDArray.SetLED(1, RGBDefines.Black);
                    LEDArray.SetLED(2, tempNewLEDval);
                    break;

                case 4:
                    LEDArray.SetLED(2, RGBDefines.Black);
                    LEDArray.SetLED(3, tempNewLEDval);
                    break;

                case 5:
                    LEDArray.SetLED(3, RGBDefines.Black);
                    LEDArray.SetLED(4, tempNewLEDval);
                    break;

                case 6:
                    LEDArray.SetLED(4, RGBDefines.Black);
                    LEDArray.SetLED(5, tempNewLEDval);
                    break;

                case 7:
                    LEDArray.SetLED(5, RGBDefines.Black);
                    LEDArray.SetLED(6, tempNewLEDval);
                    break;

                case 8:
                    LEDArray.SetLED(6, RGBDefines.Black);
                    LEDArray.SetLED(7, tempNewLEDval);
                    break;

                case 9:
                    LEDArray.SetLED(7, RGBDefines.Black);
                    LEDArray.SetLED(8, tempNewLEDval);
                    cycleCount = 0;
                    break;

                default:
                    cycleCount = 0;
                    break;
            }
            LEDArray.UpdateLEDs();
        }
    }
}
