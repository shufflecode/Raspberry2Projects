using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Windows.Devices.Enumeration;
using Windows.Devices.Gpio;
using libCore.IOevalBoard;
using Windows.Devices.Spi;
using Windows.System.Threading;


namespace WebServer.Models.LedModels
{
    class LEDDemo
    {
        private const string SPI_DEMO_CONTROLLER_NAME = "SPI0";
        private const string SPI_STATUS_CONTROLLER_NAME = "SPI1";

        private const int SPI_CS_LINE = 0;

        // Interface Objects
        private GpioController GPIOvar;
        private SpiDevice SPIinterface_Status;
        private SpiDevice SPIinterface_Demo;
        private ThreadPoolTimer StatusTimer;
        private ThreadPoolTimer ArrayTimer;

        // Objects for cyclic access
        private LED_APA102eval StatusLED;
        private LED_APA102eval LEDArray;

        int cycleCount = 0;
        int statusMachineCount = 0;
        int demoMachineCount = 0;

        const int refreshCycle = 5;

        private int currCount = 0;
        private bool currAccenting = true;
        
        public LEDDemo()
        {
            InitAll();
        }

        public void Start()
        {
        }
        private async void InitAll()
        {
            GPIOvar = GpioController.GetDefault(); /* Get the default GPIO controller on the system */
            await InitSpi();        /* Initialize the SPI controller                */

            StatusLED = new LED_APA102eval(SPIinterface_Status);
            StatusLED.AddLED(LED_APA102eval.Colors.Dark);
            LEDArray = new LED_APA102eval(SPIinterface_Demo);
            LEDArray.AddLED(LED_APA102eval.Colors.Dark);
            LEDArray.AddLED(LED_APA102eval.Colors.Dark);
            LEDArray.AddLED(LED_APA102eval.Colors.Dark);
            LEDArray.AddLED(LED_APA102eval.Colors.Dark);
            LEDArray.AddLED(LED_APA102eval.Colors.Dark);
            LEDArray.AddLED(LED_APA102eval.Colors.Dark);
            LEDArray.AddLED(LED_APA102eval.Colors.Dark);
            LEDArray.AddLED(LED_APA102eval.Colors.Dark);
            LEDArray.AddLED(LED_APA102eval.Colors.Dark);

            this.StatusTimer = ThreadPoolTimer.CreateTimer(StatusTimer_Tick, TimeSpan.FromMilliseconds(refreshCycle));
            this.ArrayTimer = ThreadPoolTimer.CreateTimer(ArrayTimer_Tick, TimeSpan.FromMilliseconds(refreshCycle * 10));
        }

        private void ArrayTimer_Tick(ThreadPoolTimer timer)
        {
            LED_APA102eval.RGB_Val tempNewLEDval, tempOldLEDval;
            tempNewLEDval = StatusLED.GetLEDobj(0);
            cycleCount++;

            switch (cycleCount)
            {
                case 1:
                    LEDArray.SetLED(8, LED_APA102eval.Colors.Dark);
                    LEDArray.SetLED(0, tempNewLEDval);
                    break;

                case 2:
                    LEDArray.SetLED(0, LED_APA102eval.Colors.Dark);
                    LEDArray.SetLED(1, tempNewLEDval);
                    break;

                case 3:
                    LEDArray.SetLED(1, LED_APA102eval.Colors.Dark);
                    LEDArray.SetLED(2, tempNewLEDval);
                    break;

                case 4:
                    LEDArray.SetLED(2, LED_APA102eval.Colors.Dark);
                    LEDArray.SetLED(3, tempNewLEDval);
                    break;

                case 5:
                    LEDArray.SetLED(3, LED_APA102eval.Colors.Dark);
                    LEDArray.SetLED(4, tempNewLEDval);
                    break;

                case 6:
                    LEDArray.SetLED(4, LED_APA102eval.Colors.Dark);
                    LEDArray.SetLED(5, tempNewLEDval);
                    break;

                case 7:
                    LEDArray.SetLED(5, LED_APA102eval.Colors.Dark);
                    LEDArray.SetLED(6, tempNewLEDval);
                    break;

                case 8:
                    LEDArray.SetLED(6, LED_APA102eval.Colors.Dark);
                    LEDArray.SetLED(7, tempNewLEDval);
                    break;

                case 9:
                    LEDArray.SetLED(7, LED_APA102eval.Colors.Dark);
                    LEDArray.SetLED(8, tempNewLEDval);
                    cycleCount = 0;
                    break;

                default:
                    cycleCount = 0;
                    break;
            }
            LEDArray.UpdateLEDs();
        }

        private void StatusTimer_Tick(ThreadPoolTimer timer)
        {
            LED_APA102eval.RGB_Val tempLEDobj;
            tempLEDobj = StatusLED.GetLEDobj(0);

            if (currAccenting == true)
            {
                currCount += 5;
            }
            else
            {
                currCount -= 5;
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

            if ((currAccenting == true) && (currCount >= 100))
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
        
    }
}
