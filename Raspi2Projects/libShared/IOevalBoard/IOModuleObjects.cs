using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using libShared.HardwareNah;

namespace libShared.IOevalBoard
{
    /// <summary>
    /// Definition of IO-Evaluation-Board
    /// This Definition unites all Features of the Raspberry Pi based Evaluation Board
    /// </summary>
    class IOevalObject
    {
        // THe Evaluation IO-Module has 
        // - 8 ADC Input pins
        // - 2 DAC Input pins
        // - 16 GPIO pins
        // - 16 binary LED-driver pins
        // - 2 Power Outputs
        // The analog data (ADCs and DACs) is left justified to signed short (16-bit) values
        // (e.g. 12 bit unsigned values will be shifted to represent 16 bit signed values)
        //
        // Considering the Client Application on the PC as reference Point the following assignment is defined
        // Input Data:  - 8 ADC
        //              - 16 GPIO inputs (and current set output values)
        // Output Data: - 2 DAC Outputs
        //              - 16 GPIO outputs (pins set to input shall be ignored)
        //              - 16 binary LED-driver Outputs
        //              - 2 power outputs

        // Analog to digital entries
        // Values are left justified to Integer 16 bit 
        /// <summary>
        /// ADC-Values
        /// 8 ADC channels. Result is given left justified
        /// </summary>
        public Int16[] In_ADCvalues = new Int16[8] { 0, 0, 0, 0, 0, 0, 0, 0 };

        // Digital to analog entries
        /// <summary>
        /// DAC-Values
        /// 2 DAC channels. Set value is understood left justified
        /// </summary>
        public Int16[] Out_DACvalues = new Int16[2] { 0, 0 };

        // GPIO entries 
        // All entries are read in big endian: [Port B],[Port A]
        // hence Port B (MSByte) is transmitted first
        /// <summary>
        /// Output values for GPIO-Slave
        /// Inputs ignore output set
        /// </summary>
        public byte[] Out_OutputSet = new byte[2] { 0, 0 };
        
        /// <summary>
        /// Input values from GPIO-Slave
        /// Output values return current status
        /// </summary>
        public byte[] In_InputData = new byte[2] { 0, 0 };

        // LED entries
        /// <summary>
        /// Status RGB-LED
        /// </summary>
        public RGBValue Out_StatusLED = RGBDefines.Black;
        // Alternative Definition
        // public RGBValue Test = new RGBValue() { Intensity = 0xFFFF, Red = 0xFFFF, Green = 0xFFFF, Blue = 0xFFFF}
        
        /// <summary>
        /// Output values for binary LED-driver.
        /// </summary>
        public UInt16 Out_BinaryLEDdriver = 0;

        // Power Outputs
        /// <summary>
        /// Output values for power pins
        /// </summary>
        UInt16 PowerOutputs = 0;

        // Configuration entries
        /// <summary>
        /// Port configuration for GPIO slave
        /// </summary>
        public byte[] Config_PortDirection = new byte[2] { 0, 0 };

        /// <summary>
        /// Interval for cyclic operation
        /// if > 0 the server will run in cyclic message mode
        /// if = 0 the all data have to be polled
        /// </summary>
        public UInt16 Config_CyclicMessage = 0;

        /// <summary>
        /// Client Commands
        /// </summary>
        eServerCommand IOsercerCommand = eServerCommand.None;

            public enum eServerCommand
        {
            None,
            Stop,
            Start,
        }

        /// <summary>
        /// Server Status  
        /// </summary>
        eServerStatus IOserverStatus = eServerStatus.Stoped;

        public enum eServerStatus
        {
            Stoped,
            AcyclicMode,
            CyclicMode,
            Error,
        }

        /// OPTIONAL ... für nächste Ausbaustufe
        /// Code noch nicht getestet

        // IO-Server Flags - Optional für nächste Ausbaustufe
        // [ConfigBlock], [DataFlags]
        // => [x,x,x,x, x,x,x,ConfigConfirmed],[x,x,x,x x,x,NewIn,NewADCval]
        private UInt16 ioServerFlags = 0;
        const UInt16 newADCvalue = 0x01;
        const UInt16 newInputValues = 0x02;
        const UInt16 masterConfigConfirmed = 0x100;

        public bool NewADCvalues
        {
            get { return ((ioServerFlags & newADCvalue) == newADCvalue); }
            set { ioServerFlags |= value ? newADCvalue : (ushort)0x00; }
        }
        public bool NewInputValues
        {
            get { return ((ioServerFlags & newInputValues) == newInputValues); }
            set { ioServerFlags |= value ? newInputValues : (ushort)0x00; }
        }
        public bool NewConfigurationConfirmed
        {
            get { return ((ioServerFlags & masterConfigConfirmed) == masterConfigConfirmed); }
            set { ioServerFlags |= value ? masterConfigConfirmed : (ushort)0x00; }
        }

        // IO-Client Flags - Optional für nächste Ausbaustufe
        // [ConfigBlock], [DataFlags]
        // => [x,x,x,x, x,x,x,NewConfig],[x,x,x,NewPout NewColor,NewLED,NewOut,NewDACval]
        private UInt16 ioClientFlags = 0;
        const UInt16 newDACvalue = 0x01;
        const UInt16 NewOuptutValues = 0x02;
        const UInt16 newLEDOutputs = 0x04;
        const UInt16 newStatusColor = 0x08;
        const UInt16 newPowerOut = 0x10;
        const UInt16 newConfig = 0x100;

        public bool NewDACvalues
        {
            get { return ((ioClientFlags & newDACvalue) == newDACvalue); }
            set { ioServerFlags |= value ? newDACvalue : (ushort)0x00; }
        }
        public bool NewOutputvalues
        {
            get { return ((ioClientFlags & NewOuptutValues) == NewOuptutValues); }
            set { ioServerFlags |= value ? NewOuptutValues : (ushort)0x00; }
        }
        public bool NewLEDOutputVlaues
        {
            get { return ((ioClientFlags & newLEDOutputs) == newLEDOutputs); }
            set { ioServerFlags |= value ? newLEDOutputs : (ushort)0x00; }
        }
        public bool NewStatusLEDColor
        {
            get { return ((ioClientFlags & newStatusColor) == newStatusColor); }
            set { ioServerFlags |= value ? newStatusColor : (ushort)0x00; }
        }
        public bool NewPowerOutputs
        {
            get { return ((ioClientFlags & newPowerOut) == newPowerOut); }
            set { ioServerFlags |= value ? newPowerOut : (ushort)0x00; }
        }
        public bool NewServerConfig
        {
            get { return ((ioClientFlags & newConfig) == newConfig); }
            set { ioServerFlags |= value ? newConfig : (ushort)0x00; }
        }

    }
}
