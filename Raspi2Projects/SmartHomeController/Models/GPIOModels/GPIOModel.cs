using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Windows.Devices.Enumeration;
using Windows.Devices.Gpio;
using Windows.Devices.Spi;
using Windows.Foundation.Metadata;
using Windows.System.Threading;
using Windows.UI.Xaml;
using libCore.IOevalBoard;
using libShared.ApiModels;
using libShared.HardwareNah;

namespace WebServer.Models
{
    class GPIOModel
    {

        #region Constants
        private const int HW_PowerOut1_Pin = 4;
        private const int HW_PowerOut2_Pin = 27;
        private const int HW_LEDDriver_GSMode = 6;
        private const int HW_LEDDriver_DCLatch = 12;
        private const int HW_LEDDriver_GSLatch = 13;
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
        ThreadPoolTimer StatusRefresh;
        const int StatusLEDrefreshCycle = 100;
        bool InstantRGBsetIsActive = false;
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
               _StatusIntensitiy = value;
            }
        }
        #endregion


        public GPIOModel()
        {
            InitAll();
        }

        private async void InitAll()
        {
            InitGpio();
            await InitSpi();
            InitIOModule();
            InitDemo();
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

            GSLEDdriver = new LEDD_TLC5941(SPIOInterface, CSadrLEDD, GSLEDdriverLatch, GSLEDdriverMode, null);
            DCLEDDriver = new LEDD_TLC5925(SPIOInterface, CSadrLEDD, DCLEDdriverLatch, null);
            StatusLED = new LED_APA102(StatusLEDInterface, CSadrLEDD);
            DACslave = new DAC_MCP4922(SPIOInterface, CSadrDAC, null, null);
            ADCslave = new ADC_MCP3208(SPIOInterface, CSadrADC);
            GPIOslave = new GPIO_MCP23S17(SPIOInterface, CSadrGPIO, null, null, 0);
        }


        private void InitDemo()
        {
            StatusRefresh = ThreadPoolTimer.CreatePeriodicTimer(StatusRefresh_Tick, TimeSpan.FromMilliseconds(StatusLEDrefreshCycle));
          
        }

        private void StatusRefresh_Tick(ThreadPoolTimer timer)
        {
            int tempRed = (int)RGBValue.MaxValue * 50 / MaxSliderValue;
            int tempGreen = (int)RGBValue.MaxValue * 50 / MaxSliderValue;
            int tempBlue = (int)RGBValue.MaxValue * 0 / MaxSliderValue;
            int tempIntens = (int)RGBValue.MaxValue * 10 / MaxSliderValue;
            RGBValue newLEDval = new RGBValue { Red = (byte)tempRed, Green = (byte)tempGreen, Blue = (byte)tempBlue, Intensity = (byte)tempIntens };

            StatusLED.SetLED(0, newLEDval);
            StatusLED.UpdateLEDs();
        }

        public static void SetGpio(GPIOStatus status)
        {
            
        }

        public GPIOStatus GetStatus()
        {
            byte[] status = new byte[100];
            GPIOslave.GetPorts(out status);

            var gpiostsatus = new GPIOStatus();

            //status.Ports.Add(new GPIOPort(1,GPIOPort.Portstatus.high));
            //status.Ports.Add(new GPIOPort(2, GPIOPort.Portstatus.high));
            //status.Ports.Add(new GPIOPort(3, GPIOPort.Portstatus.high));
            //status.Ports.Add(new GPIOPort(4, GPIOPort.Portstatus.low));
            return gpiostsatus;
        }
    }
}
