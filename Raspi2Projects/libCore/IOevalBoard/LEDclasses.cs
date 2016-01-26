namespace libCore.IOevalBoard
{
    using System;
    using System.Collections.Generic;
    using System.Text;
    using Windows.Devices.Gpio;
    using Windows.Devices.Spi;
    //using RaspiIOhelpers;
    //using RaspiSlaves;
    using System.Collections;
    using libCore.IOevalBoard;
    using libShared.HardwareNah;


    /// <summary>
    /// LED-Driver class for TLC5925
    /// Texas Instruments Driver with 16 binary Ports. Configurable in daisy-chain supported.
    /// </summary>
    public class LEDD_TLC5925 : GenreicLEDslave
    {
        /* Device info
        // Driver SCLK-Idle-State is 0
        // Driver samples serial input at rising edge / MOSI has to be set on falling edge
        // Driver sets serial output on rising edge / MOSI has to be sampled on falling edge

        // Protocol:
        // Send port data (binary, for each port) to SP-interface (MSbyte first) (total length 16 bits)
        // Execute high-level Pulse on latch enable pin (LE-Pin) to latch the port values to internal register

        // LED-driver can be used in daisy-chain. Therefor LED port data for last driver in the daisy-chain 
        // has to be transmitted first.
        // Data can be latched at all LED drivers simultaneously
        // Output enable (/OE, low active) has to be low to enable LED outputs. LEDs can be blanked by setting 
        // the /OE pin high
        */

        /// <summary>
        /// Handle for latch enable pin (to latch previously transmitted data into output register)
        /// </summary>
        GpioPin LEpin;
        /// <summary>
        /// Handle for output enable pin
        /// </summary>
        GpioPin OEpin;

        /// <summary>
        /// SPI-Specs for LED-driver
        /// </summary>
        readonly static SPIHardwareConstrains InterfaceConstrains = new SPIHardwareConstrains
        {
            DemandedSPIMode = SpiMode.Mode0,
            MaxSPIclock = 30000000,
            MinSPIclock = 0,
            //@todo in allen Klassen, 8-bit-Modus als soll eintragen
        };

        /// <summary>
        /// Specs for LED-driver
        /// </summary>
        readonly static LEDdriverSpecificDefinitions DriverDefines = new LEDdriverSpecificDefinitions
        {
            BlankOptions = LEDdriverSpecificDefinitions.eLEDBlankOptions.OnlyByNewDataSet, // Not Mandatory on PCB, Has to be enabled explicitly
            GrayValWidth = 1,
            GainValueWidth = 0,
            NumChannels = 16,
            IsDaisyChainSupported = true,
        };

        /// <summary>
        /// LED daisy-chain object
        /// </summary>
        List<UInt16> PortData;

        /// <summary>
        /// Constructor for TLC_5925
        /// </summary>
        /// <param name="spiInterface"> Defines the SP-interface on RasPi board</param>
        /// <param name="spiAdr">Defines the CS-address combination for addressing the slave</param>
        /// <param name="lePin">Latch enable pin (LE-Pin) to latch the port values to internal register</param>
        /// <param name="oePin">Output enable pin (optional) disables LED-driver when high, accepts null if not connected</param>
        public LEDD_TLC5925(SpiDevice spiInterface, SPIAddressObject spiAdr, GpioPin lePin, GpioPin oePin)
            : base(spiInterface, spiAdr, InterfaceConstrains, DriverDefines)
        {
            // Check if an SPI Object is valid
            if (lePin != null)
            {
                LEpin = lePin;
                if (oePin != null)
                {
                    // Set Blank Option to Blank Pin
                    OEpin = oePin;
                    LEDSpecifigdefines.BlankOptions = LEDdriverSpecificDefinitions.eLEDBlankOptions.BlankInput;
                    OEpin.Write(GpioPinValue.High); // Set Outputs in Safe State
                    OEpin.SetDriveMode(GpioPinDriveMode.Output);

                }
                LEpin.Write(GpioPinValue.Low);
                LEpin.SetDriveMode(GpioPinDriveMode.Output);

                PortData = new List<UInt16>();

                /// Add first LED to daisy-chain
                AddLED(0);
            }
            else
            {
                base.SPIhandle = null;
                throw new Exception("Missing Handle for LE-Pin");
            }
        }

        /// <summary>
        /// Add LED driver to daisy-chain
        /// </summary>
        /// <param name="startValues"></param>
        public void AddLED(UInt16 startValues)
        {
            PortData.Add(startValues);
            ConfigRunDone = false;
        }

        /// <summary>
        /// Set port data of single LED driver
        /// </summary>
        /// <param name="driverIndex"></param>
        /// <param name="Values"></param>
        public void SetLED(int driverIndex, UInt16 Values)
        {
            PortData[driverIndex] = Values;
        }

        /// <summary>
        /// Generates LED-driver data
        /// </summary>
        /// <param name="Send"></param>
        protected override void GenLEDStram(out byte[] Send)
        {
            UInt16 tVal;
            int tIdx;
            Send = new byte[PortData.Count * 2];

            // Build Data
            for (int idx = 0; idx < PortData.Count; idx++)
            {
                tVal = PortData[idx];
                tIdx = idx * 2;
                Send[tIdx] = (byte)(tVal >> 8);
                Send[tIdx + 1] = (byte)tVal;
            }
        }

        /// <summary>
        /// Set latch pin to latch transmitted data to outputs
        /// </summary>
        protected override void LatchData()
        {
            LEpin.Write(GpioPinValue.High);
            LEpin.Write(GpioPinValue.Low);
            //@todo Hier ide Pulszeit Messen: Mindestzeit ist 10 ns
        }

        /// <summary>
        /// Method to blank LED ports
        /// </summary>
        /// <param name="disalbeLEDs"></param>
        protected override void BlankLEDs(bool disalbeLEDs)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Resets all LED port data
        /// </summary>
        protected override void ResetLEDs()
        {
            // Reset PortData
            for (int idx = 0; idx < PortData.Count; idx++)
            {
                PortData[idx] = 0;
            }
            base.UpdateLEDs();
        }
    }




    /// <summary>
    /// LED-Driver Class for TLC5941
    /// Texas Instruments Driver with 16 binary Ports. Configurable in daisy-chain.
    /// </summary>
    public class LEDD_TLC5941 : GenreicLEDslave
    {
        /* Device info
        // SCLK-Idle-State is low
        // Driver samples serial input at rising edge / MOSI has to be set on falling edge
        // Driver sets serial Output at rising edge / MOSI has to be sampled on falling edge

        // Protocol:
        // First send with ModePin=high the configuration (96 bits / 12 Bytes). Theses are dot correction Values. 
        //      Latch data with short pulse on XLAT-pin.
        // Then send with ModePin=low  the port data gray values (192 bits / 24 Bytes). 
        //      Latch data with short pulse on XLAT-pin.
        // Data has to be clocked with MSByte (Out15 ... Out 0) first
        //      Set blank to low to enable outputs. 

        // LED-Driver can be used in daisy-chain. Therefor LED port data for last driver in the daisy-chain has to be transmitted first.
        // Data can be latched at all LED drivers simultaneously
        // Blank (high active) has to be low to enable LED outputs. LEDs can be blanked by setting Blank-pin high.
        */

        /// <summary>
        /// Handle for latch enable pin (to latch previously transmitted Data into output register). XLAT-pin is high-level triggered.
        /// </summary>
        GpioPin XLatPin;
        /// <summary>
        /// Mode pin defines the mode of data transmission (whether dot correction values are to be transmitted or gray values)
        /// </summary>
        GpioPin ModePin;
        /// <summary>
        /// Handle for blank pIn (high active). Blank has to be low to enable ports
        /// </summary>
        GpioPin BlankPin;

        /// <summary>
        /// SPI-Specs for LED-driver
        /// </summary>
        readonly static SPIHardwareConstrains InterfaceConstrains = new SPIHardwareConstrains
        {
            DemandedSPIMode = SpiMode.Mode0,
            MaxSPIclock = 30000000,
            MinSPIclock = 0
        };

        /// <summary>
        /// Specs for LED-Driver
        /// </summary>
        readonly static LEDdriverSpecificDefinitions DriverDefines = new LEDdriverSpecificDefinitions
        {
            BlankOptions = LEDdriverSpecificDefinitions.eLEDBlankOptions.OnlyByNewDataSet, // Not mandatory on PCB, has to be enabled explicitly
            GrayValWidth = 12,
            GainValueWidth = 0,
            NumChannels = 16,
            IsDaisyChainSupported = true,
        };

        /// <summary>
        /// LED daisy-chain object
        /// </summary>
        List<UInt16[]> PortData;

        /// <summary>
        /// Constructor for LEDD_TLC5941
        /// </summary>
        /// <param name="spiInterface"> Defines the SP-interface on RasPi board</param>
        /// <param name="spiAdr">Defines the CS-address combination for addressing the slave</param>
        /// <param name="xlatPin">Latch enable pin to latch the port-values to internal register</param>
        /// <param name="modePin">Mode pin defines whether the correction or gray-values are written</param>
        /// <param name="blankPin">Output enable pin (optional). Disables LED-driver when high, accepts null if not connected</param>
        public LEDD_TLC5941(SpiDevice spiInterface, SPIAddressObject spiAdr, GpioPin xlatPin, GpioPin modePin, GpioPin blankPin)
            : base(spiInterface, spiAdr, InterfaceConstrains, DriverDefines)
        {
            /// Check if an SPI object is valid
            if ((xlatPin != null) || (modePin != null))
            {
                XLatPin = xlatPin;
                ModePin = modePin;

                if (blankPin != null)
                {
                    /// Set Blank Option to Blank Pin
                    BlankPin = blankPin;
                    LEDSpecifigdefines.BlankOptions = LEDdriverSpecificDefinitions.eLEDBlankOptions.BlankInput;
                    blankPin.Write(GpioPinValue.High); // Set Outputs in Safe State
                    blankPin.SetDriveMode(GpioPinDriveMode.Output);
                }
                XLatPin.Write(GpioPinValue.Low);
                XLatPin.SetDriveMode(GpioPinDriveMode.Output);
                ModePin.Write(GpioPinValue.Low);
                ModePin.SetDriveMode(GpioPinDriveMode.Output);

                PortData = new List<UInt16[]>();

                /// Add first LED Driver to daisy-chain
                UInt16[] firstData = new ushort[1] { 0 };
                AddLED(firstData);
            }
            else
            {
                base.SPIhandle = null;
                throw new Exception("Missing Handle for XLAT-Pin and/or Mode-Pin");
            }
        }

        /// <summary>
        /// Adds new LED driver to daisy-chain
        /// </summary>
        /// <param name="startValues"></param>
        public void AddLED(UInt16[] startValues)
        {
            UInt16[] Values;
            prepareLEDdata(startValues, out Values);
            PortData.Add(Values);
            ConfigRunDone = false;
        }

        /// <summary>
        /// Set port data of single LED driver
        /// </summary>
        /// <param name="driverIndex"></param>
        /// <param name="Values"></param>
        public void SetLED(int driverIndex, UInt16[] newValues)
        {
            UInt16[] Values;
            prepareLEDdata(newValues, out Values);
            PortData[driverIndex] = Values;
        }

        /// <summary>
        /// Get port data of certain LED-driver
        /// </summary>
        /// <param name="driverIndex"></param>
        /// <param name="Values"></param>
        public void GetLED(int driverIndex, out UInt16[] Values)
        {
            Values = new UInt16[base.LEDSpecifigdefines.NumChannels];
            if (driverIndex < PortData.Count)
            {
                UInt16[] tVal = PortData[driverIndex];
                for (int idx = 0; idx < base.LEDSpecifigdefines.NumChannels; idx++)
                {
                    Values[idx] = (UInt16)(tVal[idx] << 4);
                }
            }
        }

        /// <summary>
        /// Ether cuts given data to fit in existing slots or expands the given data/pattern to write it in all slots
        /// </summary>
        /// <param name="newData"></param>
        /// <param name="preparedData"></param>
        void prepareLEDdata(UInt16[] newData, out UInt16[] preparedData)
        {
            int defineLength;
            if (base.LEDSpecifigdefines.GrayValWidth == 1)
            {
                defineLength = base.LEDSpecifigdefines.NumChannels / 16;
                //@todo Achtung hier BUg: Aufrunden nicht vergessen
            }
            else
            {
                defineLength = base.LEDSpecifigdefines.NumChannels;
            }
            preparedData = new UInt16[defineLength];
            int index = 0;

            for (int cntr = 0; cntr < defineLength; cntr++)
            {
                preparedData[cntr] = (UInt16)(newData[index] >> 4);
                //@todo Hier Shift Konstante mit etwsa Sysmtemweiteren ersetzetn
                index++;
                if (index >= newData.Length)
                {
                    index = 0;
                }
            }
        }

        /// <summary>
        /// Generates LED-driver data
        /// </summary>
        /// <param name="Send"></param>
        protected override void GenLEDStram(out byte[] Send)
        {
            UInt16[] tArray;
            UInt32 tempVal;
            int streamLen = PortData.Count * (DriverDefines.NumChannels * DriverDefines.GrayValWidth) / 8;
            int sendIdx = streamLen - 1;
            Send = new byte[streamLen];
            //@todo Achtung hier Bug: Aufrunden nicht vergessen

            /// Build send stream beginning from the last Element, 
            /// because the latch signal can only be set after each Byte correctly
            for (int dIdx = PortData.Count; dIdx > 0; dIdx--)
            {
                tArray = PortData[dIdx - 1];
                for (int idx = 0; idx < tArray.Length; idx += 2)
                {
                    tempVal = 0;
                    tempVal |= (UInt32)(((UInt32)tArray[idx] & 0x0FFF) << 12);
                    tempVal |= (UInt32)tArray[idx + 1] & 0x0FFF;

                    Send[sendIdx] = (byte)((tempVal >> 0) & 0xFF);
                    sendIdx--;
                    Send[sendIdx] = (byte)((tempVal >> 8) & 0xFF);
                    sendIdx--;
                    Send[sendIdx] = (byte)((tempVal >> 16) & 0xFF);
                    sendIdx--;
                }
            }
        }

        //@todo ConfigRun fertig machen
        protected override void ConfigRun()
        {
            byte[] Send = new byte[12];

            ModePin.Write(GpioPinValue.High);
            for (int idx = 0; idx > Send.Length; idx++)
            {
                Send[idx] = 0xFF;
            }

            base.SendByteStram(Send);

            ModePin.Write(GpioPinValue.Low);
            BlankPin.Write(GpioPinValue.Low);

            LatchData();
        }

        /// <summary>
        /// Set latch pin to latch transmitted data to outputs
        /// </summary>
        protected override void LatchData()
        {
            XLatPin.Write(GpioPinValue.High);
            XLatPin.Write(GpioPinValue.Low);
            //@todo Hier ide Pulszeit Messen: Mindestzeit ist 20 ns
        }

        /// <summary>
        /// Method to blank LED ports
        /// </summary>
        /// <param name="disalbeLEDs"></param>
        protected override void BlankLEDs(bool disalbeLEDs)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Resets all LED port data
        /// </summary>
        protected override void ResetLEDs()
        {
            UInt16[] tArray;
            // Reset PortData
            for (int idx = 0; idx < PortData.Count; idx++)
            {
                tArray = PortData[idx];
                for (int aIdx = 0; aIdx < tArray.Length; aIdx++)
                {
                    tArray[idx] = 0;
                }
                PortData[idx] = tArray;
            }
            base.UpdateLEDs();
        }

        /// <summary>
        /// Refreshes the intrinsic PWM-counter of the Gray scale LED driver
        /// </summary>
        public void RefreshPWMcounter()
        {
            BlankPin.Write(GpioPinValue.High);
            BlankPin.Write(GpioPinValue.Low);
        }
    }


    /// <summary>
    /// REG-LED with integrated synchronous serial interface
    /// </summary>
    public class LED_APA102 : GenreicLEDslave
    {
        /* Device info
        // SCLK-Idle-State is 0
        // Driver samples serial input at rising edge / MOSI has to be set on falling edge

        // The LEDs have a serial-data-input and a clock-Input for incoming LED-data
        // The LEDs have a serial-data-output and a cLock output for following LEDs (daisy-chain)
        // Each LED-value has a width of 32-Bit
        //  Start: 0x00000000
        //  Color:   8bit:      [111iiiii] Drive current (5 bit value)
        //              8bit:   [BBBBBBBB] Blue gray Value
        //              8bit:   [GGGGGGGG] Green gray Value
        //              8bit:   [RRRRRRRR] Red gray value
        //  Stop: 0xFFFFFFFF

        // Protocol:
        // Send start sequence.
        // Send color data to first LED in daisy-chain
        // Send color data for the following LEDs
        // Send end sequence

        // LED can be used in daisy-chain. LED data for first LED is transmitted first.
        */

        /// <summary>
        /// SPI-Specs for LED-driver
        /// </summary>
        readonly static SPIHardwareConstrains InterfaceConstrains = new SPIHardwareConstrains
        {
            DemandedSPIMode = SpiMode.Mode0,
            MaxSPIclock = int.MaxValue, // Value is unknown
            MinSPIclock = 0
        };

        /// <summary>
        /// Specs for LED-driver
        /// </summary>
        readonly static LEDdriverSpecificDefinitions DriverDefines = new LEDdriverSpecificDefinitions
        {
            BlankOptions = LEDdriverSpecificDefinitions.eLEDBlankOptions.OnlyByNewDataSet,
            GrayValWidth = 8,
            GainValueWidth = 5,
            NumChannels = 3,
            IsDaisyChainSupported = true,
        };

        private readonly byte[] StartSeq = new byte[4] { 0, 0, 0, 0 };
        private readonly byte[] StopSeq = new byte[4] { 0xFF, 0xFF, 0xFF, 0xFF };

        List<RGBset> LEDs;

        /// <summary>
        /// Constructor for LED_APA102
        /// </summary>
        /// <param name="spiInterface"> Defines the SP-interface on Raspi board</param>
        /// <param name="spiAdr">Defines the CS-address combination for addressing the slave</param>
        public LED_APA102(SpiDevice spiInterface, SPIAddressObject spiAdr)
            : base(spiInterface, spiAdr, InterfaceConstrains, DriverDefines)
        {
            LEDs = new List<RGBset>(); //@todo hier ggf. auf etwas Ressourcenschonenderes umsteigen

            AddLED(RGBDefines.Black);
        }

        /// <summary>
        /// Adds new RGB-LED to daisy-chain
        /// </summary>
        /// <param name="color"></param>
        public void AddLED(RGBValue startColor)
        {
            RGBset tLED = new RGBset();
            tLED.SetRGBvalue(startColor);
            LEDs.Add(tLED);
        }

        /// <summary>
        /// Sets LED intensity and color by generic RGBvalue of a certain LED in the daisy-chain
        /// </summary>
        /// <param name="index">Index of LED (starting with 0)</param>
        /// <param name="color">Color value to be set on chosen Index</param>
        public void SetLED(int index, RGBValue color)
        {
            if (index < LEDs.Count)
            {
                RGBset tLED = new RGBset();
                tLED.SetRGBvalue(color);
                LEDs[index] = tLED;
            }
        }

        /// <summary>
        /// Sets all LEDs with according to given Array.
        /// </summary>
        /// <param name="colors">Defines the new Colors for the array. 
        /// The array-length has to meet actual number of added LEDs</param>
        public void SetAllLEDs(RGBValue[] colors)
        {
            if (colors.Length == LEDs.Count)
            {
                RGBset tLED = new RGBset();
                for (int idx = 0; idx < LEDs.Count; idx++)
                {
                    tLED.SetRGBvalue(colors[idx]);
                    LEDs[idx] = tLED;
                }

            }
        }

        /// <summary>
        /// Sets LED intensity and color by local LED-Set of a certain LED in the daisy-chain
        /// </summary>
        /// <param name="index"></param>
        /// <param name="led"></param>
        public void SetLED(int index, RGBset led)
        {
            if (index < LEDs.Count)
            {
                LEDs[index] = led;
            }
        }

        /// <summary>
        /// Get LED Object
        /// </summary>
        /// <param name="index"></param>
        /// <param name="led"></param>
        public void GetLED(int index, out RGBset led)
        {
            if (index < LEDs.Count)
            {
                led = LEDs[index];
            }
            else
            {
                led = new RGBset();
            }
        }

        /// <summary>
        /// Get color of LED object
        /// </summary>
        /// <param name="index"></param>
        /// <param name="color"></param>
        public void GetLEDvalue(int index, out RGBValue color)
        {
            if (index < LEDs.Count)
            {
                LEDs[index].GetRGBvalue(out color);
            }
            else
            {
                color = new RGBValue();
            }
        }

        /// <summary>
        /// Generates LED-Driver data
        /// </summary>
        /// <param name="Send"></param>
        protected override void GenLEDStram(out byte[] Send)
        {
            int streamLen = RGBset.LEDValByteWidth * (LEDs.Count + 2);
            byte[] part;
            int idx;
            Send = new byte[streamLen];

            StartSeq.CopyTo(Send, 0);

            for (idx = 0; idx < LEDs.Count; idx++)
            {
                LEDs[idx].GenValueStram(out part);
                part.CopyTo(Send, (1 + idx) * RGBset.LEDValByteWidth);
            }

            StopSeq.CopyTo(Send, (1 + idx) * RGBset.LEDValByteWidth);
        }

        /// <summary>
        /// Resets LED values
        /// </summary>
        protected override void ResetLEDs()
        {
            for (int idx = 0; idx < LEDs.Count; idx++)
            {
                LEDs[idx].SetRGBvalue(0, 0, 0, 0);
            }
        }

        /// <summary>
        /// Method to blank LED ports
        /// </summary>
        /// <param name="disalbeLEDs"></param>
        protected override void BlankLEDs(bool disalbeLEDs)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// RGB-LED-settings for color channels and drive current for single RGB-LED
        /// </summary>
        public struct RGBset
        {
            // Each Color Value is 32 bit Value with following structure:
            //  II BB GG RR
            //  Color:  MS 8bit:   [111iiiii] Driver current (with a Width of 5 bit)
            //             8bit:   [BBBBBBBB] Blue gray value
            //             8bit:   [GGGGGGGG] Green gray value
            //          LS 8bit:   [RRRRRRRR] Red gray value

            public const int ColorChannelWidth = 8;
            public const int IntensitiyWidth = 5;
            public const int MinRGBStepValue = 256;
            public const int MinIntensStepValue = 2048;
            public const int LEDValueWidt = 32;
            public const int LEDValByteWidth = 4;
            // @todo diese Definitionen in LED-Beschreibung einfüren

            /// <summary>
            /// Binary LED-value of RGB-LED
            /// </summary>
            private UInt32 LEDValue { get; set; }

            /// <summary>
            /// Generates byte stream to set one single RGB-LED
            /// </summary>
            /// <param name="stream"></param>
            public void GenValueStram(out byte[] stream)
            {
                stream = new byte[4];
                stream[0] = (byte)(LEDValue >> 24);
                stream[1] = (byte)(LEDValue >> 16);
                stream[2] = (byte)(LEDValue >> 8);
                stream[3] = (byte)LEDValue;
            }

            /// <summary>
            /// Setter-method to set color and intensity of RGB-LED
            /// </summary>
            /// <param name="intens">Driving current for each LED color channel</param>
            /// <param name="red">Red channel</param>
            /// <param name="green">Green channel</param>
            /// <param name="blue">Blue channel</param>
            public void SetRGBvalue(byte intens, byte red, byte green, byte blue)
            {
                LEDValue = ((UInt32)(intens | 0xE0) << 24) | (UInt32)blue << 16 | (UInt32)green << 8 | (UInt32)red;
            }

            /// <summary>
            /// Setter-method to set color and intensity of RGB-LED
            /// </summary>
            /// <param name="color"></param>
            public void SetRGBvalue(RGBValue color)
            {
                byte intens = (byte)(color.Intensity >> 11);
                byte red = (byte)(color.Red >> 8);
                byte green = (byte)(color.Green >> 8);
                byte blue = (byte)(color.Blue >> 8);
                this.SetRGBvalue(intens, red, green, blue);
            }

            /// <summary>
            /// Getter-method for brightness information for each channel
            /// </summary>
            /// <param name="intens"></param>
            /// <param name="red"></param>
            /// <param name="green"></param>
            /// <param name="blue"></param>
            public void GetRGBValue(out byte intens, out byte red, out byte green, out byte blue)
            {
                red = (byte)LEDValue;
                green = (byte)(LEDValue >> 8);
                blue = (byte)(LEDValue >> 16);
                intens = (byte)(LEDValue >> 24);
            }

            /// <summary>
            /// Getter-method for RGB-Color value
            /// </summary>
            /// <param name="startColor"></param>
            public void GetRGBvalue(out RGBValue color)
            {
                byte intens;
                byte red;
                byte green;
                byte blue;

                RGBValue tVal = new RGBValue();
                this.GetRGBValue(out intens, out red, out green, out blue);

                tVal.Intensity = (UInt16)((UInt16)intens << 11);
                tVal.Red = (UInt16)((UInt16)red << 8);
                tVal.Green = (UInt16)((UInt16)green << 8);
                tVal.Blue = (UInt16)((UInt16)blue << 8);
                color = tVal;
            }
        }
    }
}
