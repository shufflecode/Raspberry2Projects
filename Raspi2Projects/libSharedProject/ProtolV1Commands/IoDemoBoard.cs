using System;
using System.Collections.Generic;
using System.Text;


// THe Evaluation IO-Module has 
// - 8 ADC Input pins
// - 2 DAC Input pins
// - 16 GPIO pins
// - 16 binary LED-driver pins
// - 2 Power Outputs
// The analog data (ADCs and DACs) is left justified to signed short (16-bit) values
// (e.g. a 12 bit unsigned value will be shifted to represent 16 bit signed values i.e. short justifiedValue = (rawValue << 3) )
//
// Considering the Client Application on the PC as reference Point the following assignment is defined
// Input Data:  - 8 ADC
//              - 16 GPIO inputs (and current set output values)
// Output Data: - 2 DAC Outputs
//              - 16 GPIO outputs (pins set to input shall be ignored)
//              - 16 binary LED-driver Outputs
//              - 2 power outputs


namespace libSharedProject.ProtolV1Commands
{

    public class IoDemoState : ProtocolV1Base
    {
        /// <summary>
        /// Client Commands to IO-Server
        /// </summary>
        public enum eServerCommand
        {
            None,
            Stop,
            Init,
            Start,
            SetAcyclicMode, // Fernsteuerbetrieb durch Client
            SetCyclicMode, // Startet selbständige Datenerfassung
        }
        /// <summary>
        /// Io-Server status
        /// </summary>
        public enum eServerStatus
        {
            Stoped,
            AcyclicMode,
            CyclicMode,
            Error,
        }

        // @todo wohin mit den Konfigurationen 
        // Zykluszeit
        // Betreibsarten
        // Daten

        public eServerCommand Key { get; set; } = eServerCommand.None;
        public eServerStatus Status { get; set; } = eServerStatus.Stoped;
    }


    // Analog to digital entries
    // Values are left justified to Integer 16 bit 
    /// <summary>
    /// ADC-Values
    /// 8 ADC channels. Result is given left justified
    /// </summary>
    public class IoDemoAdc : ProtocolV1Base
    {
        public IoDemoAdc()// @todo braucht man den Konstruktor??
        {
        }

        public short Adc0 { get; set; } = 0;
        public short Adc1 { get; set; } = 0;
        public short Adc2 { get; set; } = 0;
        public short Adc3 { get; set; } = 0;
        public short Adc4 { get; set; } = 0;
        public short Adc5 { get; set; } = 0;
        public short Adc6 { get; set; } = 0;
        public short Adc7 { get; set; } = 0;        
    }

    // Digital to analog entries
    /// <summary>
    /// DAC-Values
    /// 2 DAC channels. Set value is understood left justified
    /// </summary>
    public class IoDemoDac : ProtocolV1Base
    {
        public short Dac0 { get; set; }
        public short Dac1 { get; set; }
    }


    public class IoDemoGpio : ProtocolV1Base
    {
        // Configuration entries
        /// <summary>
        /// Port configuration for GPIO slave
        /// </summary>
        public ushort GpioDirection { get; set; }
        // GPIO entries 
        // All entries are read in big endian: [Port B],[Port A]
        // hence Port B (MSByte) is transmitted first
        /// <summary>
        /// Output values for GPIO-Slave
        /// Inputs ignore output set
        /// </summary>
        public ushort GpioValue { get; set; }

        public bool ModifyCOnfig { get; set; } = false;
    }

#if WPF_TOOLKIT
     [System.ComponentModel.Description("Output values for power pins")]
#endif
    // Power Outputs
    /// <summary>
    /// Output values for power pins
    /// </summary>
    public class IoDemoPowerState : ProtocolV1Base
    {
#if WPF_TOOLKIT
     [System.ComponentModel.Description(" Output value for power pin 1")]
#endif
        public bool Power1State { get; set; }

#if WPF_TOOLKIT
     [System.ComponentModel.Description(" Output value for power pin 2")]
#endif
        public bool Power2State { get; set; }
    }

    public class IoDemoException : ProtocolV1Base
    {
       public Exception IoException { get; set; }
    }

    // LED entries
    /// <summary>
    /// Status RGB-LED
    /// </summary>
    //@todo LEDs nachtragen
    //public RGBValue Out_StatusLED = RGBDefines.Black;
    // Alternative Definition
    // public RGBValue Test = new RGBValue() { Intensity = 0xFFFF, Red = 0xFFFF, Green = 0xFFFF, Blue = 0xFFFF}



    public class IoDemoRgb : ProtocolV1Base
    {
#if WPF_TOOLKIT
        [System.ComponentModel.Editor(typeof(AppWpfToolkit.UcColorEditor), typeof(AppWpfToolkit.UcColorEditor))]
#endif
        public libShared.SharedColor MyCol { get; set; } = new libShared.SharedColor();
    }

    public class IoDemoGetRequest : ProtocolV1Base
    {
        public enum CmdValue
        {
            Adc = 0,
            Dac = 1,
            Gpio = 2,
            Powerstate = 3,
            Rgb = 4,
            State = 5,
        }

        public CmdValue Key { get; set; } = CmdValue.Adc;
    }

   
}
