using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;
using Windows.Devices.Gpio;
using Windows.Devices.Spi;
using System.Threading.Tasks;
using Windows.Devices.Enumeration;

namespace libCore.IOevalBoard
{
    /// <summary>
    /// REG-LED with integrated synchronous serial interface
    /// </summary>
    public class LED_APA102eval
    {
        // The LEDs have a Serial-Data-Input and a Clock-Input for incoming LED-Data
        // The LEDs have a Serial-Data-Output and a CLock Output for following LEDs (Daisy-Chain)
        // Each LED-Value has a Width of 32-Bit
        //  Start: 0x00000000
        //  Color:   8bit:      [111iiiii] Global Intensity (Wert ist 5 bit-Breit)
        //              8bit:   [BBBBBBBB] Blue GrayValue
        //              8bit:   [GGGGGGGG] Green GrayValue
        //              8bit:   [RRRRRRRR] Red GrayValue
        //  Stop: 0xFFFFFFFF

        SpiDevice SPI_Handle;
        //GpioPin CSpin;

        const UInt32 StartVal = 0x00000000;
        const UInt32 EndVal = 0xFFFFFFFF;
        List<RGB_Val> LEDs = new List<RGB_Val>();

        /// <summary>
        /// Constructor for LED_APA102 class
        /// </summary>
        /// <param name="spiInterface"></param>
        public LED_APA102eval(SpiDevice spiInterface)
        {
            // Check if an SPI Object is valid
            if (spiInterface != null)
            {
                // Temp-Configuration Object 
                SpiConnectionSettings tempSet = spiInterface.ConnectionSettings;
                // SPI-Configuration has to meet the specifications of the ADC-Module
                if (tempSet.Mode == SpiMode.Mode0)
                {
                    // Safe Objects locally
                    SPI_Handle = spiInterface;
                }
                else
                {
                    SPI_Handle = null;
                    throw new Exception("Demanded SPI Mode deiffers from Configuration");
                }
            }
            else
            {
                SPI_Handle = null;
                //CSpin = null;
                throw new Exception("Missing SPI-Interface-Handle or CS-Pin-Handle");
            }
        }

        public void AddLED(Colors color)
        {
            LEDs.Add(new RGB_Val { LEDValue = (UInt32)(Intensitiy.Medium) << 24 | (UInt32)color });
        }

        /// <summary>
        /// Sets LED intensity color by predefined enum constants
        /// </summary>
        /// <param name="index">Index of LED (starting with 0)</param>
        /// <param name="color">Colorvalue to be set on chosen Index</param>
        public void SetLED(int index, Colors color)
        {
            LEDs[index] = new RGB_Val { LEDValue = (UInt32)(Intensitiy.Medium) << 24 | (UInt32)color };
        }

        public void SetLED(int index, byte intens, byte red, byte green, byte blue)
        {
            // @todo hier noch eine Count-Abfang-Routine
            LEDs[index].SetRGBvalue(intens, red, green, blue);
        }
        public void SetLED(int index, RGB_Val led)
        {
            // @todo hier noch eine Count-Abfang-Routine
            LEDs[index] = led;
        }

        public RGB_Val GetLEDobj(int index)
        {
            return LEDs[index];
        }

        /// <summary>
        /// Send current LED-Configuration to LED-Array over SPI-Interface
        /// </summary>
        public void UpdateLEDs()
        {
            byte[] tempLEDval = new byte[0];
            byte[] sendArray = new byte[0];
            // Activate CS-Pin 
            //CSpin.Write(GpioPinValue.Low);

            // Send Start-Command
            tempLEDval = BitConverter.GetBytes(StartVal);
            //SPI_Handle.Write(tempLEDval);
            sendArray = sendArray.Concat(tempLEDval).ToArray();

            // Send LED-Values
            for (int idx = 0; idx < LEDs.Count; idx++)
            {
                // Transform LED-Value into Byte array 
                tempLEDval = BitConverter.GetBytes(LEDs[idx].LEDValue);
                Array.Reverse(tempLEDval);
                sendArray = sendArray.Concat(tempLEDval).ToArray();
                //SPI_Handle.Write(tempLEDval);
            }
            // Send Stop Command
            tempLEDval = BitConverter.GetBytes(EndVal);
            //SPI_Handle.Write(tempLEDval);
            sendArray = sendArray.Concat(tempLEDval).ToArray();

            SPI_Handle.Write(sendArray);
            // Deactivate CS-Pin
            //CSpin.Write(GpioPinValue.High);
        }


        /// <summary>
        /// RGB-LED-Settings for color channels and drive current for one LED
        /// </summary>
        public struct RGB_Val
        {
            private UInt32 _LEDValue;
            /// <summary>
            /// Current red Value
            /// </summary>
            public byte Red
            {
                get { return (byte)_LEDValue; }
                set
                {
                    _LEDValue &= 0xFFFFFF00;
                    _LEDValue |= (UInt32)value;
                }
            }
            /// <summary>
            /// Current green value
            /// </summary>
            public byte Green
            {
                get { return (byte)(_LEDValue >> 8); }
                set
                {
                    _LEDValue &= 0xFFFF00FF;
                    _LEDValue |= (UInt32)value << 8;
                }
            }
            /// <summary>
            /// Current blue Value
            /// </summary>
            public byte Blue
            {
                get { return (byte)(_LEDValue >> 16); }
                set
                {
                    _LEDValue &= 0xFF00FFFF;
                    _LEDValue |= (UInt32)value << 16;
                }
            }

            /// <summary>
            /// Current intensitiy (driver current)
            /// </summary>
            public byte Intensity
            {
                get { return (byte)(_LEDValue >> 24); }
                set
                {
                    _LEDValue &= 0x00FFFFFF;
                    _LEDValue |= (UInt32)value << 24;
                }
            }

            /// <summary>
            /// LED Value. Color and intensity are coden within this Value
            /// </summary>
            public UInt32 LEDValue
            {
                get { return _LEDValue; }
                set
                {
                    _LEDValue = value;
                    _LEDValue |= 0xE0000000;
                    Intensity = (byte)(value >> 24);
                    Blue = (byte)(value >> 16);
                    Green = (byte)(value >> 8);
                    Red = (byte)(value);
                }
            }

            /// <summary>
            /// Setter-Method to set color of RGB-LED
            /// </summary>
            /// <param name="red">Red channel</param>
            /// <param name="green">Green channel</param>
            /// <param name="blue">Blue channel</param>
            public void SetRGBvalue(byte red, byte green, byte blue)
            {
                SetRGBvalue((byte)LED_APA102eval.Intensitiy.Medium, red, green, blue);
            }
            /// <summary>
            /// Setter-Method to set color and intensity of RGB-LED
            /// </summary>
            /// <param name="intens">Driving current of each LED color channel</param>
            /// <param name="red">Red channel</param>
            /// <param name="green">Green channel</param>
            /// <param name="blue">Blue channel</param>
            public void SetRGBvalue(byte intens, byte red, byte green, byte blue)
            {
                _LEDValue = ((UInt32)(intens | 0xE0) << 24) | (UInt32)blue << 16 | (UInt32)green << 8 | (UInt32)blue;
            }
        }

        /// <summary>
        /// Raw values for LED-colors (without setting of drive current)
        /// </summary>
        public enum Colors
        {
            Dark = 0x000000,
            Red = 0x0000FF,
            Green = 0x00FF00,
            Blue = 0xFF0000,
            Yellow = 0x00FFFF,
            Cyan = 0xFFFF00,
            Magenta = 0xFF00FF,
            White = 0xFFFFFF,
        }

        /// <summary>
        /// Raw values for drive current
        /// </summary>
        public enum Intensitiy
        {
            Dark = 0xE0,
            Medium = 0xEF,
            Bright = 0xFF,
        }
    }
}
