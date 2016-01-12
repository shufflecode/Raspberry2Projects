namespace libCore.IOevalBoard
{
    using System;
    using System.Collections.Generic;
    using System.Text;
    using Windows.Devices.Gpio;
    using Windows.Devices.Spi;
    using Windows.Devices.Enumeration;
    //using RaspiSlaves;
    using System.Collections;
    //using RaspiIOhelpers;
    using libCore.IOevalBoard;
    using libShared.HardwareNah;

    /// <summary>
    /// Generic class for SPI-controled analog to digital converter.
    /// The class is designed for ADCs with a resolution up to 16 bit signed (or 15 bit unsigned).
    /// </summary>
    abstract public class GenreicADCslave : RaspiMultiSlave
    {
        /// <summary>
        /// Definitions for DACclasses
        /// </summary>
        readonly public static GenericADCdefinitions ADCclassDefines = new GenericADCdefinitions
        {
            ADCvalueWidt = 16,
            IsSigned = true,
            MaxADCvalue = Int16.MaxValue,
            MinADCvalue = Int16.MinValue,
        };

        /// <summary>
        /// Specifications of given ADC-Slave
        /// </summary>
        protected ADCspecificDefinitions ADCdefines = new ADCspecificDefinitions { };

        private Int16[] adcResults;
        /// <summary>
        /// Container for Conversion Results 
        /// </summary>
        public Int16[] ADCresults
        {
            get { return adcResults; }
            set { adcResults = value; }
        }

        /// <summary>
        /// Constructor for generic ADC
        /// </summary>
        /// <param name="spiInterface"> Defines the SP-interface on Raspi board</param>
        /// <param name="spiAdr">Defines the CS-address combination for addressing the slave</param>
        /// <param name="givenConstrains">Restriction for using the respective device</param>
        /// <param name="givenSpecs">Specifications of given ADC-Slave</param>
        public GenreicADCslave(SpiDevice spiInterface, SPIAddressObject spiAdr, SPIHardwareConstrains givenConstrains, ADCspecificDefinitions givenSpecs)
            : base(spiInterface, spiAdr, givenConstrains)
        {
            ADCdefines = givenSpecs;
            ADCresults = new Int16[ADCdefines.NumOfADCchannels];
        }

        /// <summary>
        /// Get single value for given ADC-channel
        /// </summary>
        /// <param name="chNum">Number of channel starting with 0 to (ADC-channels-1) </param>
        /// <param name="adcVal"></param>
        /// <returns></returns>
        public abstract void GetSingleChannel(int chNum, out Int16 adcVal);

        /// <summary>
        /// Get all values of given ADC-slave
        /// </summary>
        /// <param name="adcVals"></param>
        public abstract void GetAllChannels(out Int16[] adcVals);
    }

    /// <summary>
    /// Generic ADC definitions for ADC classes
    /// </summary>
    public struct GenericADCdefinitions
    {
        /// <summary>
        /// Widt of ADC-values. Actual ADC values will be scaled.
        /// </summary>
        public int ADCvalueWidt { get; set; }

        /// <summary>
        /// Fullscale ADC value.
        /// </summary>
        public int MaxADCvalue { get; set; }

        /// <summary>
        /// Minimum ADC value.
        /// </summary>
        public int MinADCvalue { get; set; }

        /// <summary>
        /// Defines if value is signed
        /// </summary>
        public bool IsSigned { get; set; }
    }

    /// <summary>
    /// Defeinitions for ADC-devices
    /// </summary>
    public struct ADCspecificDefinitions
    {
        /// <summary>
        /// Number of ADC-inputs whitch are implemented in the SPI-decice
        /// </summary>
        public int NumOfADCchannels { get; set; }

        /// <summary>
        /// Defines the ADC-resolution (which can be useful to determin minimum voltage steps and errors)
        /// </summary>
        public int ADCResolution { get; set; }

        /// <summary> 
        /// Property defines the minimum count of bytes to poll one single conversion value
        /// </summary>
        public int SingleTransmissionLength { get; set; }

        /// <summary>
        /// Property defines the minimum count of bytes to poll all conversion values
        /// </summary>
        public int FullTransmissionLength { get; set; }

        /// <summary>
        /// Defines conversion reference
        /// </summary>
        public eChannelConversionMode ReferenceMode { get; set; }

        /// <summary>
        /// Defines which full conversion mode
        /// </summary>
        public eFullConversionMode FullConvMode { get; set; }

        /// <summary>
        /// Defines for full conversion modes
        /// </summary>
        public enum eFullConversionMode
        {
            /// It is possible to poll only one value per trasmision
            OnePortPerTraceiveCycle,
            /// It is possible to poll all values at ones (during one transmission, without CS-idle-states)
            AllPortsAtONce
        }

        /// <summary>
        /// Defines whether conversion referece modes
        /// </summary>
        public enum eChannelConversionMode
        {
            /// Each Channel is converted to GND-reverence
            GNDreference,
            /// Each Channel is converted to next-channel-reference
            NextChannelReferenc,
        }
    }

    /// <summary>
    /// Helper methodes for ADC-devices and DAC-devices
    /// </summary>
    public class GenericADnumerics
    {
        /// <summary>
        /// Calculates the corresponting physical value to smapled integer code
        /// </summary>
        /// <param name="sampleValue">Smpled integer value</param>
        /// <param name="chConfig">Exerne port wirieing</param>
        /// <param name="sourceADC">Specificatoin of sourceADC </param>
        /// <returns></returns>
        public static float CalcValue(int sampleValue, AnaChConfig chConfig, GenericADCdefinitions sourceADC)
        {
            //@todo Hier alle Fälle abdecken
            //@todo Hier Klasse auf Generisch trimmen
            float measValue;
            // Calculate voltage on ADC-pin
            measValue = chConfig.RefVoltage * (float)sampleValue / (float)Math.Pow(2, sourceADC.ADCvalueWidt);
            // Scale voltage to port of input network
            measValue = measValue / chConfig.ResistorRatio;
            return measValue;
        }
    }

    /// <summary>
    /// Generic class for SPI-controled digital to analog converter 
    /// The class is designed for DACs with a resolution up to 16 bit signed (or 15 bit unsigned).
    /// </summary>
    abstract public class GenreicDACslave : RaspiMultiSlave
    {
        /// <summary>
        /// Definitions for DAC classes
        /// </summary>
        readonly public static GenericDACdefinitions DACclassDefines = new GenericDACdefinitions
        {
            ADCvalueWidt = 16,
            IsSigned = true,
            MaxADCvalue = Int16.MaxValue,
            MinADCvalue = Int16.MinValue,
        };

        /// <summary>
        /// Specifications of given DAC-slave
        /// </summary>
        protected DACspecificDefinitions DACdefines = new DACspecificDefinitions { };

        private Int16[] dacValues;
        /// <summary>
        /// Container for output values
        /// </summary>
        public Int16[] DACvalues
        {
            get { return dacValues; }
            set { dacValues = value; }
        }

        /// <summary>
        /// Constructor for generic DAC
        /// </summary>
        /// <param name="spiInterface"> Defines the SP-interface on Raspi board</param>
        /// <param name="spiAdr">Defines the CS-address combination for addressing the slave</param>
        /// <param name="givenConstrains">Restriction for using the respective device</param>
        /// <param name="givenSpecs">Specifications of given DAC-Slave</param>
        public GenreicDACslave(SpiDevice spiInterface, SPIAddressObject spiAdr, SPIHardwareConstrains givenConstrains, DACspecificDefinitions givenSpecs)
            : base(spiInterface, spiAdr, givenConstrains)
        {
            DACdefines = givenSpecs;
            DACvalues = new Int16[DACdefines.NumOfDACchannels];
        }

        /// <summary>
        /// Get single value for given DAC-channel
        /// </summary>
        /// <param name="chNum">Number of channel starting with 0 for first DAC-channel to (NumChannels-1)</param>
        /// <param name="dacVal"></param>
        /// <returns></returns>
        abstract public void SetSingleChannel(int chNum, Int16 dacVal);

        /// <summary>
        /// Sets all values of given DAC-slave
        /// </summary>
        /// <param name="dacVals"></param>
        abstract public void SetAllChannels(Int16[] dacVals);

        //@todo Methode zur Deaktivierung der DACs (abstimmen mit LED-Klasse)
    }

    /// <summary>
    /// Generic ADC definitions for ADC classes
    /// </summary>
    public struct GenericDACdefinitions
    {
        /// <summary>
        /// Widt of DAC-values. Actual DAC Values will be scaled.
        /// </summary>
        public int ADCvalueWidt { get; set; }

        /// <summary>
        /// Fullscale DAC value.
        /// </summary>
        public int MaxADCvalue { get; set; }

        /// <summary>
        /// Minimum DAC value.
        /// </summary>
        public int MinADCvalue { get; set; }

        /// <summary>
        /// Defines if Value is signed
        /// </summary>
        public bool IsSigned { get; set; }
    }

    /// <summary>
    /// Defeinitions for DAC-Devices
    /// </summary>
    public struct DACspecificDefinitions
    {
        /// <summary>
        /// Number of ADC-Inputs whitch are implemented in the SPI-Decice
        /// </summary>
        public int NumOfDACchannels { get; set; }

        /// <summary>
        /// Defines the ADC-Resolution (which can be usefull to determin minimum Voltage steps and errors)
        /// </summary>
        public int DACResolution { get; set; }

        /// <summary>
        /// Property defines the minimum count of bytes to set one single analog value
        /// </summary>
        public int SingleTransmissionLength { get; set; }

        /// <summary>
        /// Property defines the the minimum count of bytes to set all analog values
        /// </summary>
        public int FullTransmissionLength { get; set; }
    }


    /// <summary>
    /// Generic class for SPI-controled general-purpose-IO-expander 
    /// The class is designed for GPIOs with 8-bit-port-cluster.
    /// </summary>
    abstract public class GenreicGPIOslave : RaspiMultiSlave
    {
        /// <summary>
        /// Definitions for GPIOclasses
        /// </summary>
        readonly public static GenericGPIOdefinitions GPIOclassDefines = new GenericGPIOdefinitions
        {
            PortWidt = 8,
        };

        /// <summary>
        /// Specifications of given GPIO-slave
        /// </summary>
        protected GPIOspecificDefinitions GPIOdefines = new GPIOspecificDefinitions { };


        private byte[] portData;
        /// <summary>
        /// Container for port data 
        /// </summary>
        public byte[] PortData
        {
            get { return portData; }
            set { portData = value; }
        }

        private byte[] portDirection;
        /// <summary>
        /// Container for port configuration
        /// </summary>
        public byte[] PortDirection
        {
            get { return portData; }
            set { portData = value; }
        }

        private bool configRunDone = false;
        /// <summary>
        /// Shows whether the configuration run was already executed or not
        /// </summary>
        public bool ConfigRunDone
        {
            get { return configRunDone; }
            set { configRunDone = value; }
        }

        /// <summary>
        /// Constructor for generic GPIO
        /// </summary>
        /// <param name="spiInterface"> Defines the SP-interface on Raspi board</param>
        /// <param name="spiAdr">Defines the CS-address combination for addressing the slave</param>
        /// <param name="givenConstrains">Restriction for using the respective device</param>
        /// <param name="givenSpecs">Specifications of given GPIO-slave</param>
        public GenreicGPIOslave(SpiDevice spiInterface, SPIAddressObject spiAdr, SPIHardwareConstrains givenConstrains, GPIOspecificDefinitions givenSpecs)
            : base(spiInterface, spiAdr, givenConstrains)
        {
            GPIOdefines = givenSpecs;
            PortData = new byte[GPIOdefines.NumOfPorts];

            ExecuteConfigRun(eConfigRunMode.DeviceConfigOnly);
        }

        /// <summary>
        /// Set port data
        /// </summary>
        /// <param name="outputs"></param>
        abstract public void SetPorts(byte[] portData);

        /// <summary>
        /// Get port data
        /// </summary>
        /// <param name="portData"></param>
        abstract public void GetPorts(out byte[] portData);

        /// <summary>
        /// Set port direction
        /// </summary>
        /// <param name="dirData">1=input, 0=output</param>
        abstract public void SetDirection(byte[] dirData);

        /// <summary>
        /// Execute slave configuration
        /// </summary>
        abstract protected void ExecuteConfigRun(eConfigRunMode mode);

        /// <summary>
        /// Definition for configrun modes
        /// </summary>
        protected enum eConfigRunMode
        {
            /// Executes whole driver configuration
            //wholeConfiguration,
            /// Ececutes only IO-configuration
            IOconfigurationOnly,
            /// Ececutes only general-device-configuration
            DeviceConfigOnly,
        }

        /// <summary>
        /// Definition for messagetypes
        /// </summary>
        protected enum eMessageMode
        {
            /// Execute read
            read,
            /// Execute write
            write,
            /// Execute write and read concurrently
            //readAndWrite,
        }
    }

    /// <summary>
    /// Generic GPIO definitions for GPIO classes
    /// </summary>
    public struct GenericGPIOdefinitions
    {
        /// <summary>
        /// Widt of GPIO-port.
        /// </summary>
        public int PortWidt { get; set; }
    }

    /// <summary>
    /// Defeinitions for ADC-Devices
    /// </summary>
    public struct GPIOspecificDefinitions
    {
        /// <summary>
        /// Number of digital Ports whitch are implemented on the SPI-Decice
        /// </summary>
        public int NumOfPorts { get; set; }

        /// <summary>
        /// Property defines the minimum count of bytes to set one single port value
        /// </summary>
        public int SingleTransmissionLength { get; set; }

        /// <summary>
        /// Property defines the minimum count of bytes to set all port values
        /// </summary>
        public int FullTransmissionLength { get; set; }
    }



    /// <summary>
    /// Generic class for SPI-controled LED-driver or integrated LEDs
    /// This class is designed for unsigned 16 bit values
    /// </summary>
    abstract public class GenreicLEDslave : RaspiMultiSlave
    {
        //@todo LED-Klasse in drei Teile Splitten: binary, grayValues, RGB
        /// <summary>
        /// Definitions for LEDclasses
        /// </summary>
        readonly public static GenericLEDLibDefinitions LEDclassDefines = new GenericLEDLibDefinitions
        {
            NormValueWidth = 16,
            MaxLEDportValue = UInt16.MaxValue,
        };

        /// <summary>
        /// Specification of given LED-Driver
        /// </summary>
        protected LEDdriverSpecificDefinitions LEDSpecifigdefines = new LEDdriverSpecificDefinitions { };

        private UInt16 commonGainVal;
        /// <summary>
        /// Arrayglobal gain value
        /// </summary>
        public UInt16 CommonGainValue
        {
            get { return commonGainVal; }
            set { commonGainVal = value; }
        }

        /// <summary>
        /// Container for last message to restore blanked LED-values
        /// </summary>
        byte[] LastMessage;

        /// <summary>
        /// Constructor for LED-driver
        /// </summary>
        /// <param name="spiInterface"> Defines the SP-interface on Raspi board</param>
        /// <param name="spiAdr">Defines the CS-address combination for addressing the slave</param>
        /// <param name="givenConstrains">Restriction for using the respective device</param>
        /// <param name="givenSpecs">Specifications of given LED-Slave</param>
        public GenreicLEDslave(SpiDevice spiInterface, SPIAddressObject spiAdr, SPIHardwareConstrains givenConstrains, LEDdriverSpecificDefinitions givenSpecs)
            : base(spiInterface, spiAdr, givenConstrains)
        {
            LEDSpecifigdefines = givenSpecs;
        }

        private bool configRunDone = false;
        /// <summary>
        /// Schows if LED-driver have already been configured
        /// </summary>
        public bool ConfigRunDone
        {
            get { return configRunDone; }
            set { configRunDone = value; }
        }

        /// <summary>
        /// Sends new port values to LED-driver 
        /// </summary>
        public void UpdateLEDs()
        {
            byte[] Send;

            /// Execute configuration run (if necessary)
            if (ConfigRunDone == false)
            {
                ConfigRun();
                ConfigRunDone = true;
                //@todo ConfigRun immer Voraussetzen wenn neue LED hinzugefügt wird
            }

            /// Generate send-stram
            GenLEDStram(out Send);

            /// send data
            base.SendByteStram(Send);
            /// Latch data (if necessary)
            LatchData();

            /// Save current set for restore options
            /// //@todo Recovery Informationen einbauen
            //if(LastMessage.Length == 0)
            //{
            //    LastMessage = new byte[Send.Length];
            //}
            //Array.Copy(Send, LastMessage, Send.Length);
        }

        /// <summary>
        /// Prepare send-bytearray for transmission
        /// </summary>
        /// <param name="Send"></param>
        abstract protected void GenLEDStram(out byte[] Send);

        //@todo Ggf. diese auf auf Delegates umstellen
        /// <summary>
        /// Execute devicespecific operation to latch port data
        /// </summary>
        virtual protected void LatchData() { }

        /// <summary>
        /// Write device configuration
        /// </summary>
        virtual protected void ConfigRun() { }

        /// <summary>
        /// Blank LEDs (switch all Ports off)
        /// </summary>
        /// <param name="disalbeLEDs"></param>
        abstract protected void BlankLEDs(bool disalbeLEDs);

        /// <summary>
        /// Resets all stored gray values and sets the LED-driver in default reset configuration
        /// </summary>
        abstract protected void ResetLEDs();
    }

    //@todo Text abgleichen
    /// <summary>
    /// Definitions for LED-driver Base-class
    /// </summary>
    public struct GenericLEDLibDefinitions
    {
        /// <summary>
        /// Fullscale value regardless actual LED-Driver implementation.
        /// </summary>
        public int NormValueWidth { get; set; }

        /// <summary>
        /// Fullscale value Regardless actual LED-Driver implementation.
        /// </summary>
        public int MaxLEDportValue { get; set; }
    }

    /// <summary>
    /// Defeinitions for LED-Driver devices
    /// </summary>
    public struct LEDdriverSpecificDefinitions
    {
        /// <summary>
        /// Width of on gray- or color value
        /// </summary>
        public int GrayValWidth { get; set; }

        /// <summary>
        ///  Defines the maximum gray-value
        /// </summary>
        public int MaxGrayVal { get; set; }

        /// <summary>
        /// Number of LED or color channels
        /// </summary>
        public int NumChannels { get; set; }

        /// <summary>
        /// Definition for global LED-gain value for all channels of driver
        /// </summary>
        public int GainValueWidth { get; set; }

        /// <summary>
        ///  Defines the maximum gain-value
        /// </summary>
        public int MaxGainVal { get; set; }

        /// <summary>
        /// Defines whether the driver supports daisychain configuration or not
        /// </summary>
        public bool IsDaisyChainSupported { get; set; }

        /// <summary>
        /// LED-shutdown or blank options
        /// </summary>
        public eLEDBlankOptions BlankOptions { get; set; }

        /// <summary>
        /// Defines options to disable LED ports
        /// </summary>
        public enum eLEDBlankOptions
        {
            /// LED outputs can be only schut down bo Zweo Set of Output Data
            OnlyByNewDataSet,
            /// LEDs can additianally be blanked by dedicated Input
            BlankInput,
        }
    }

    /// <summary>
    /// Generic class for other complex SPI-driven periphery
    /// </summary>
    class GenericComplexDevice
    {

    }
}
