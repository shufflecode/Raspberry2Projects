using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using libCore.IOevalBoard;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.Devices.Gpio;
using Windows.Devices.Spi;

using libShared.HardwareNah;
using System.Text.RegularExpressions;
using Windows.Devices.Enumeration;
using Windows.UI.Xaml;
using libSharedProject.ProtolV1Commands;


namespace libCore.IOevalBoard
{
    public class IoDemoBoard
    {
        //DispatcherTimer LEDdriverRefresh;
        //private void LEDrefresh_Tick(object sender, object e)
        //{
        //    GSLEDdriver.RefreshPWMcounter();
        //}

        //DispatcherTimer StatusRefresh;
        //const int StatusLEDrefreshCycle = 100;
        //bool InstantRGBsetIsActive = false;

        //private void StatusRefresh_Tick(object sender, object e)
        //{
        //    btn_SetRGBvalues(sender, null);
        //}

        //DispatcherTimer AnalogRefresh;
        //const int AnalogCycle = 100;
        //bool InstantAnalogTimerIsActive = false;

        //int AnalogState = 0;
        //private void AnalogRefresh_Tick(object sender, object e)
        //{
        //    switch (AnalogState)
        //    {
        //        case 0:
        //            btn_SetDACvalues(sender, null);
        //            AnalogState++;
        //            break;

        //        case 1:
        //            btn_ReadADCvalues(sender, null);
        //            AnalogState = 0;
        //            break;

        //        default:
        //            AnalogState = 0;
        //            break;
        //    }
        //}



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


        public IoDemoBoard()
        {

        }

        public async void InitAll()
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
                // Beim Build 10586 (WinIoT-Version auf Raspi) ist die verwendung der zweiten SPI-Schnittstelle nicht vorgesehen
                //throw new Exception("SPI Initialization Failed", ex);
                StatusLEDInterface = null;
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
            //LEDdriverRefresh = new DispatcherTimer();
            //LEDdriverRefresh.Interval = TimeSpan.FromMilliseconds(5); // 2^12/1E6 = 
            //LEDdriverRefresh.Tick += LEDrefresh_Tick;

            //StatusRefresh = new DispatcherTimer();
            //StatusRefresh.Interval = TimeSpan.FromMilliseconds(StatusLEDrefreshCycle);
            //StatusRefresh.Tick += StatusRefresh_Tick;

            //AnalogRefresh = new DispatcherTimer();
            //AnalogRefresh.Interval = TimeSpan.FromMilliseconds(AnalogCycle);
            //AnalogRefresh.Tick += AnalogRefresh_Tick;
        }

        IoDemoAdc adc = new IoDemoAdc();

        /// <summary>
        /// Polls ADC-Data from Slave and returns Values
        /// </summary>
        /// <returns></returns>
        public IoDemoAdc GetAdc()
        {
            short[] tempResults = new short[8];

            libSharedProject.ProtolV1Commands.IoDemoAdc adc = new libSharedProject.ProtolV1Commands.IoDemoAdc();

            ADCslave.GetSingleChannel(0, out tempResults[0]);
            ADCslave.GetSingleChannel(1, out tempResults[1]);
            ADCslave.GetSingleChannel(2, out tempResults[2]);
            ADCslave.GetSingleChannel(3, out tempResults[3]);
            ADCslave.GetSingleChannel(4, out tempResults[4]);
            ADCslave.GetSingleChannel(5, out tempResults[5]);
            ADCslave.GetSingleChannel(6, out tempResults[6]);
            ADCslave.GetSingleChannel(7, out tempResults[7]);

            adc.Adc0 = tempResults[0];
            adc.Adc1 = tempResults[1];
            adc.Adc2 = tempResults[2];
            adc.Adc3 = tempResults[3];
            adc.Adc4 = tempResults[4];
            adc.Adc5 = tempResults[5];
            adc.Adc6 = tempResults[6];
            adc.Adc7 = tempResults[7];

            return this.adc;
        }

        IoDemoDac dac = new IoDemoDac();

        /// <summary>
        /// Sets ADC-Slave according to Client Request
        /// </summary>
        /// <param name="_dac"></param>
        public void SetDac(IoDemoDac _dac)
        {
            dac.Dac0 = _dac.Dac0;
            dac.Dac1 = _dac.Dac1;

            DACslave.SetSingleChannel(0, _dac.Dac0);
            DACslave.SetSingleChannel(1, _dac.Dac1);
        }

        /// <summary>
        /// Return current DAC set
        /// </summary>
        /// <returns></returns>
        public IoDemoDac GetDac()
        {
            return this.dac;
        }

        IoDemoPowerState powerState = new IoDemoPowerState();

        /// <summary>
        /// Sets PowerOutputs according to Client Request
        /// </summary>
        /// <param name="_powerState"></param>
        public void SetPowerState(IoDemoPowerState _powerState)
        {
            powerState.Power1State = _powerState.Power1State;
            powerState.Power1State = _powerState.Power1State;

            if (powerState.Power1State == true)
            {
                PowerOut1.Write(GpioPinValue.High);
            }
            else
            {
                PowerOut1.Write(GpioPinValue.Low);
            }

            if (powerState.Power1State == true)
            {
                PowerOut2.Write(GpioPinValue.High);
            }
            else
            {
                PowerOut2.Write(GpioPinValue.Low);
            }
        }

        /// <summary>
        /// Return current PowerPin set
        /// </summary>
        /// <returns></returns>
        public IoDemoPowerState GetPowerState()
        {
            return this.powerState;
        }

        IoDemoGpio gpio = new IoDemoGpio();

        /// <summary>
        /// Sets GPIO COnfiguration and Outputs according to Client Request
        /// </summary>
        /// <param name="_gpio"></param>
        public void SetGpio(IoDemoGpio _gpio)
        {
            gpio.GpioDirection = _gpio.GpioDirection;
            gpio.GpioValue = _gpio.GpioValue;

            byte[] ValStream = new byte[2]; // temporary Byte Stream

            // Refresh / Change GPIO-Slave Configuration if necessary 
            if (_gpio.ModifyCOnfig == true)
            {
                ValStream[0] = (byte)gpio.GpioDirection;
                ValStream[1] = (byte)(gpio.GpioDirection >> 8);

                GPIOslave.SetDirection(ValStream);
                GPIOslave.SetPullUps(ValStream);
                GPIOslave.SetInputLogic(new byte[] { 0xFF, 0xFF });
            }

            // Write Output-Data on Slave
            ValStream[0] = (byte)gpio.GpioValue;
            ValStream[1] = (byte)(gpio.GpioValue >> 8);

            GPIOslave.SetPorts(ValStream);

        }

        /// <summary>
        /// Polls GPIO-Data from Slave and returns Values
        /// </summary>
        /// <returns></returns>
        public IoDemoGpio GetGpio()
        {
            byte[] ValStream = new byte[2];
            GPIOslave.GetPorts(out ValStream);

            this.gpio.GpioValue = (ushort)((ushort)ValStream[0] | ((ushort)ValStream[1] << 8));
            return this.gpio;
        }

        IoDemoRgb rgb = new IoDemoRgb();

        /// <summary>
        /// Sets Status LED according to Client Request
        /// </summary>
        /// <param name="_rgb"></param>
        public void SetRgb(IoDemoRgb _rgb)
        {
            this.rgb.MyCol = _rgb.MyCol;

            RGBValue tLEDval = new RGBValue(this.rgb.MyCol.Red, this.rgb.MyCol.Green, this.rgb.MyCol.Blue, this.rgb.MyCol.Intensity);
            StatusLED.SetLED(0, tLEDval);
            StatusLED.UpdateLEDs();
        }

        /// <summary>
        /// Return current RGB set
        /// </summary>
        /// <returns></returns>
        public IoDemoRgb GetRgb()
        {
            return this.rgb;
        }

        IoDemoState state = new IoDemoState();

        public void SetState(IoDemoState _state)
        {
            //state.x = _state.x

            //@todo Hier müssen wir morgen über ein paar grundlegende Betriebsarten reden
            // Bisher braucht man hier aber noch nichts zu implementieren.
        }

        public IoDemoState GetState()
        {
            return this.state;
        }

        // @todo LED-Objekt fehlt auf Seiten der IO-Modul-Klasse
    }
}
