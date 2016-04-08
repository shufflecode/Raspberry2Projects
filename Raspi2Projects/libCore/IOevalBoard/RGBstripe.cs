using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using libCore.IOevalBoard;
using System.ComponentModel;
using Windows.Devices.Gpio;
using Windows.Devices.Spi;

using libShared.HardwareNah;
using System.Text.RegularExpressions;
using Windows.Devices.Enumeration;
using Windows.UI.Xaml;
using libSharedProject.ProtolV1Commands;

namespace libCore.IOevalBoard
{

    public class RGBstripe
    {
        private const string SPI_DEMO_CONTROLLER_NAME = "SPI0";
        private const int SPI_CS_LINE = 0;

        // Interface Objects
        private GpioController GPIOvar;
        private SpiDevice SPIinterface_Demo;
        private DispatcherTimer StripeTimer;
        private DispatcherTimer SweepTimer;

        // Objects for cyclic access
        SPIAddressObject CSadrLEDD;
        LED_APA102 ColorStripe;
        PatternGenerator StripePattern;

        const int StripeLen = 20;
        const int MaxSliderValue = 0xFF;

        const int refreshCycle = 25;

        
        /// <summary>
        /// Main-Page
        /// </summary>
        public RGBstripe()
        {
            InitAll();
        }

        /// <summary>
        /// TimerInitialisation
        /// </summary>
        private async void InitAll()
        {
            //StripeTimer = new DispatcherTimer();
            //StripeTimer.Interval = TimeSpan.FromMilliseconds(refreshCycle);
            //StripeTimer.Tick += StripeRefresh_Tick;

            //SweepTimer = new DispatcherTimer();
            //SweepTimer.Interval = TimeSpan.FromMilliseconds(3000);
            //SweepTimer.Tick += Sweep_Tick;

            GPIOvar = GpioController.GetDefault(); /* Get the default GPIO controller on the system */
            await InitSpi();        /* Initialize the SPI controller                */

            CSadrLEDD = new SPIAddressObject(SPIAddressObject.eCSadrMode.SPIdedicated, null, null, 0);
            ColorStripe = new LED_APA102(SPIinterface_Demo, CSadrLEDD);
            // Bei der Instanziierung wird erstes LED-Objekt erstellt.

            for (int i = 0; i < StripeLen - 1; i++)
            {
                ColorStripe.AddLED(RGBDefines.Black);
            }

            // LED-Demo
            StripePattern = new PatternGenerator(StripeLen, 1);
            StripePattern.AddCurve(PatternGenerator.eCurveType.Sine);
            StripePattern.AddCurve(PatternGenerator.eCurveType.Pulse);
            StripePattern.AddCurve(PatternGenerator.eCurveType.Cosine);
            StripePattern.AddCurve(PatternGenerator.eCurveType.Triangle);
            StripePattern.AddCurve(PatternGenerator.eCurveType.Sawtooth);

            StripeTimer.Start();
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


        //UInt16[] StripeRGB = new ushort[3] { 0, 0, 0 };
        //UInt16[] oldStripeRGB = new ushort[3] { 0, 0, 0 };
        //UInt16[] newStripeRGB = new ushort[3] { 0, 0, 0 };
        //UInt16[] RGBindex = new ushort[9] { 1, 2, 3, 1, 2, 3, 1, 2, 3 };

        //RGBValue[] newColors = new RGBValue[StripeLen];
        //private void StripeRefresh_Tick(object sender, object e)
        //{
        //    StripePattern.RefreshData(newColors);
        //    ColorStripe.SetAllLEDs(newColors);
        //    ColorStripe.UpdateLEDs();
        //}

        //int nextMode = 0;
        //int colorCycle = 0;
        //int patternCycle = 0;
        //private void Sweep_Tick(object sender, object e)
        //{
        //    if (nextMode == 0)
        //    {
        //        nextMode = 1;
        //        colorCycle++;
        //        if (colorCycle >= 7)
        //        {
        //            colorCycle = 1;
        //        }

        //        UInt16[] tempRGB = new ushort[3] { 0, 0, 0 };
        //        for (int i = 0; i < 3; i++)
        //        {
        //            if ((colorCycle & (1 << i)) != 0)
        //            {
        //                tempRGB[i] = RGBValue.MaxValue;
        //            }
        //        }
        //        RGBValue tempVal = new RGBValue() { Intensity = RGBValue.MaxValue, Red = tempRGB[0], Green = tempRGB[1], Blue = tempRGB[2] };

        //        StripePattern.InitColorChange(tempVal);

        //    }
        //    else
        //    {
        //        nextMode = 0;
        //        patternCycle++;
        //        if (colorCycle >= StripePattern.Curves.Count)
        //        {
        //            patternCycle = 0;
        //        }

        //        StripePattern.InitCurveChange(patternCycle);
        //    }
        //}


        libSharedProject.ProtolV1Commands.RGBstripeColor StripeColor = new libSharedProject.ProtolV1Commands.RGBstripeColor();

        public libSharedProject.ProtolV1Commands.RGBstripeColor GetSingleColor()
        {
            return this.StripeColor;
        }

        public void SetSingleColor(libSharedProject.ProtolV1Commands.RGBstripeColor myStripe)
        {
            StripeColor.StripeSingleColor = myStripe.StripeSingleColor;

            RGBValue tLEDval = new RGBValue(StripeColor.StripeSingleColor.Red, StripeColor.StripeSingleColor.Green, StripeColor.StripeSingleColor.Blue, StripeColor.StripeSingleColor.Intensity);
            ColorStripe.SetLED(0, tLEDval);
            ColorStripe.UpdateLEDs();

        }

    }
}





