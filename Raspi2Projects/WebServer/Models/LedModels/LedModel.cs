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
        private const int SPI_CS_LINE = 0;

        // Interface Objects
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

        public LED_APA102eval LedArray => LEDArray;

        public void StartTimer(int speed)
        {
            this.ArrayTimer = ThreadPoolTimer.CreatePeriodicTimer(ArrayTimer_Tick, TimeSpan.FromMilliseconds(refreshCycle * 10));
        }

        public void ArrayOff()
        {
            foreach (var led in LEDArray.LEDs)
            {
                led.SetRGBvalue(0,0,0);
            }
            LEDArray.UpdateLEDs();
            ArrayTimer.Cancel();
        }
        private async void InitAll()
        {
            await InitSpi();

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
            LEDArray.UpdateLEDs();

            this.ArrayTimer = ThreadPoolTimer.CreatePeriodicTimer(ArrayTimer_Tick, TimeSpan.FromMilliseconds(refreshCycle * 10));
        }
        private async Task InitSpi()
        {
            try
            {
                var settings = new SpiConnectionSettings(SPI_CS_LINE); /* Create SPI initialization settings                               */
                settings.ClockFrequency = 2000000;                             /* Datasheet specifies maximum SPI clock frequency of 10MHz         */
                settings.Mode = SpiMode.Mode0; // Bedeutet, dass CLK-Idle ist low, Sample bei Steigender Flank

                string spiAqs1 = SpiDevice.GetDeviceSelector(SPI_DEMO_CONTROLLER_NAME);       /* Find the selector string for the SPI bus controller          */
                var devicesInfo1 = await DeviceInformation.FindAllAsync(spiAqs1);         /* Find the SPI bus controller device with our selector string  */
                SPIinterface_Demo = await SpiDevice.FromIdAsync(devicesInfo1[0].Id, settings);  /* Create an SpiDevice with our bus controller and SPI settings */


            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine("Exception: {0}", ex);
            }
        }

        private void ArrayTimer_Tick(ThreadPoolTimer timer)
        {
            cycleCount++;

            switch (cycleCount)
            {
                case 1:
                    LEDArray.SetLED(8, LED_APA102eval.Colors.Dark);
                    LEDArray.SetLED(0, LED_APA102eval.Colors.Blue);
                    break;

                case 2:
                    LEDArray.SetLED(0, LED_APA102eval.Colors.Dark);
                    LEDArray.SetLED(1, LED_APA102eval.Colors.Blue);
                    break;

                case 3:
                    LEDArray.SetLED(1, LED_APA102eval.Colors.Dark);
                    LEDArray.SetLED(2, LED_APA102eval.Colors.Blue);
                    break;

                case 4:
                    LEDArray.SetLED(2, LED_APA102eval.Colors.Dark);
                    LEDArray.SetLED(3, LED_APA102eval.Colors.Blue);
                    break;

                case 5:
                    LEDArray.SetLED(3, LED_APA102eval.Colors.Dark);
                    LEDArray.SetLED(4, LED_APA102eval.Colors.Blue);
                    break;

                case 6:
                    LEDArray.SetLED(4, LED_APA102eval.Colors.Dark);
                    LEDArray.SetLED(5, LED_APA102eval.Colors.Blue);
                    break;

                case 7:
                    LEDArray.SetLED(5, LED_APA102eval.Colors.Dark);
                    LEDArray.SetLED(6, LED_APA102eval.Colors.Blue);
                    break;

                case 8:
                    LEDArray.SetLED(6, LED_APA102eval.Colors.Dark);
                    LEDArray.SetLED(7, LED_APA102eval.Colors.Blue);
                    break;

                case 9:
                    LEDArray.SetLED(7, LED_APA102eval.Colors.Dark);
                    LEDArray.SetLED(8, LED_APA102eval.Colors.Blue);
                    cycleCount = 0;
                    break;

                default:
                    cycleCount = 0;
                    break;
            }
            System.Diagnostics.Debug.WriteLine("TimerCycle: " +  cycleCount);
            LEDArray.UpdateLEDs();
        }
    }
}
