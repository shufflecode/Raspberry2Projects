//using System;
//using System.Collections.Generic;
//using System.Linq;
//using System.Text;
//using System.Threading.Tasks;
//using libSharedProject.ProtolV1Commands;

//namespace libCore.IOevalBoard
//{
//    public class IOevalBoard
//    {
//        IoDemoAdc adc = new IoDemoAdc();

//        public IoDemoAdc GetAdc()
//        {           
//            // ADC lesen ...

//            //adc.Adc0 = 0;
//            //adc.Adc2 = 2;
//            //adc.Adc3 = 3;
//            //adc.Adc4 = 4;
//            //adc.Adc5 = 5;
//            //adc.Adc6 = 6;
//            //adc.Adc7 = 7;

//            return this.adc;
//        }

//        IoDemoDac dac = new IoDemoDac();

//        public void SetDac(IoDemoDac _dac)
//        {
//            dac.Dac0 = _dac.Dac0;
//            dac.Dac1 = _dac.Dac1;
//        }

//        public IoDemoDac GetDac()
//        {
//            return this.dac;
//        }

//        IoDemoPowerState powerState = new IoDemoPowerState();

//        public void SetPowerState(IoDemoPowerState _powerState)
//        {
//            powerState.Power1State = _powerState.Power1State;
//            powerState.Power1State = _powerState.Power1State;

//            // Powerstate setzen
//        }

//        public IoDemoPowerState GetPowerState()
//        {
//            return this.powerState;
//        }

//        IoDemoGpio gpio = new IoDemoGpio();

//        public void SetGpio(IoDemoGpio _gpio)
//        {
//            gpio.GpioDirection = _gpio.GpioDirection;
//            gpio.GpioValue = _gpio.GpioValue;
//        }

//        public IoDemoGpio GetGpio()
//        {
//            return this.gpio;
//        }

//        IoDemoRgb rgb = new IoDemoRgb();

//        public void SetRgb(IoDemoRgb _rgb)
//        {
//            this.rgb.MyCol = _rgb.MyCol;
//        }

//        public IoDemoRgb GetRgb()
//        {
//            return this.rgb;
//        }

//        IoDemoState state = new IoDemoState();

//        public void SetState(IoDemoState _state)
//        {
//            //state.x = _state.x
//        }

//        public IoDemoState GetState()
//        {
//            return this.state;
//        }
//    }
//}
