using System;
using System.Collections.Generic;
using System.Text;

namespace libSharedProject.ProtolV1Commands
{
    public class IoDemoAdc : ProtocolV1Base
    {
        public IoDemoAdc()
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

    public class IoDemoDac : ProtocolV1Base
    {
        public short Dac0 { get; set; }
        public short Dac1 { get; set; }
    }

    public class IoDemoState : ProtocolV1Base
    {
        //public ushort GpioDirection { get; set; }
        //public ushort GpioValue { get; set; }
    }


    public class IoDemoGpio : ProtocolV1Base
    {
        public ushort GpioDirection { get; set; }
        public ushort GpioValue { get; set; }
    }

    public class IoDemoPowerState : ProtocolV1Base
    {
        public bool Power1State { get; set; }
        public bool Power2State { get; set; }
    }

    public class IoDemoRgb : ProtocolV1Base
    {
#if WINDOWS_UWP
#else        
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
