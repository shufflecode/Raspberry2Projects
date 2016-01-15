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
using libShared.HardwareNah;


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
        private LED_APA102 StatusLED;
        private LED_APA102 LEDArray;
        SPIAddressObject CSadrLEDD;

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

        public LED_APA102 LedArray => LEDArray;

        public void StartTimer(int speed)
        {
            this.ArrayTimer = ThreadPoolTimer.CreatePeriodicTimer(ArrayTimer_Tick, TimeSpan.FromMilliseconds(refreshCycle * 10));
        }

        private void BlackoutArray()
        {
            for (int i = 0; i < 8; i++)
            {
                LEDArray.SetLED(i, RGBDefines.Black);
            }
            LEDArray.UpdateLEDs();
        }

        public void StopDemo()
        {
            BlackoutArray();
            ArrayTimer.Cancel();
        }
        private async void InitAll()
        {
            await InitSpi();


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
            LEDArray.AddLED(RGBDefines.Black);
            BlackoutArray();
            LEDArray.UpdateLEDs();

            this.ArrayTimer = ThreadPoolTimer.CreatePeriodicTimer(ArrayTimer_Tick, TimeSpan.FromMilliseconds(refreshCycle * 10));
            ArrayTimer.Cancel();
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
                    LEDArray.SetLED(8, RGBDefines.Black);
                    LEDArray.SetLED(0, RGBDefines.Cyan);
                    break;

                case 2:
                    LEDArray.SetLED(0, RGBDefines.Black);
                    LEDArray.SetLED(1,  RGBDefines.Cyan);
                    break;

                case 3:
                    LEDArray.SetLED(1, RGBDefines.Black);
                    LEDArray.SetLED(2,  RGBDefines.Cyan);
                    break;

                case 4:
                    LEDArray.SetLED(2, RGBDefines.Black);
                    LEDArray.SetLED(3,  RGBDefines.Cyan);
                    break;

                case 5:
                    LEDArray.SetLED(3, RGBDefines.Black);
                    LEDArray.SetLED(4,  RGBDefines.Cyan);
                    break;

                case 6:
                    LEDArray.SetLED(4, RGBDefines.Black);
                    LEDArray.SetLED(5,  RGBDefines.Cyan);
                    break;

                case 7:
                    LEDArray.SetLED(5, RGBDefines.Black);
                    LEDArray.SetLED(6,  RGBDefines.Cyan);
                    break;

                case 8:
                    LEDArray.SetLED(6, RGBDefines.Black);
                    LEDArray.SetLED(7,  RGBDefines.Cyan);
                    break;

                case 9:
                    LEDArray.SetLED(7, RGBDefines.Black);
                    LEDArray.SetLED(8,  RGBDefines.Cyan);
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
