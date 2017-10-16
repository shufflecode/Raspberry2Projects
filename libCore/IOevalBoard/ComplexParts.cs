namespace libCore.IOevalBoard
{
    using System;
    using System.Collections.Generic;
    using System.Text;
    using Windows.Devices.Gpio;
    using Windows.Devices.Spi;

    /// <summary>
    /// LED-Object with tree dedicated binary pins of common logic
    /// </summary>
    struct RGB_LEDpins
    {
        private GpioPin red;
        private GpioPin blue;
        private GpioPin green;

        private GpioPinValue enableLEDpin;
        private GpioPinValue disableLEDpin;
        private E_RGB_LEDType LEDstat;

        /// <summary>
        /// Constructor for RGB-LED-object. Reserves hardware IO and defines pin-logic
        /// </summary>
        /// <param name="gpio">GPIO-object of IO-Controller</param>
        /// <param name="redPin">Pin number for red channel</param>
        /// <param name="greenPin">Pin number for green channel</param>
        /// <param name="bluePin">Pin number for blue channel</param>
        /// <param name="ledType">LED logic of channel-pins</param>
        public RGB_LEDpins(GpioController gpio, int redPin, int greenPin, int bluePin, E_RGB_LEDType ledType)
        {
            this.red = gpio.OpenPin(redPin);
            this.green = gpio.OpenPin(greenPin);
            this.blue = gpio.OpenPin(bluePin);
            LEDstat = ledType;
            if (LEDstat == E_RGB_LEDType.HighActive)
            {
                enableLEDpin = GpioPinValue.High;
                disableLEDpin = GpioPinValue.Low;
            }
            else
            {
                enableLEDpin = GpioPinValue.Low;
                disableLEDpin = GpioPinValue.High;
            }
            // Set associated pins as output
            this.red.SetDriveMode(GpioPinDriveMode.Output);
            this.green.SetDriveMode(GpioPinDriveMode.Output);
            this.blue.SetDriveMode(GpioPinDriveMode.Output);
        }

        /// <summary>
        /// Set LED output pins
        /// </summary>
        /// <param name="red">Red channel</param>
        /// <param name="green">Green channel</param>
        /// <param name="blue">Blue channel</param>
        public void SetLEDOutputs(bool red, bool green, bool blue)
        {
            if (red == true)
            {
                this.red.Write(enableLEDpin);
            }
            else
            {
                this.red.Write(disableLEDpin);
            }
            if (green == true)
            {
                this.green.Write(enableLEDpin);
            }
            else
            {
                this.green.Write(disableLEDpin);
            }
            if (blue == true)
            {
                this.blue.Write(enableLEDpin);
            }
            else
            {
                this.blue.Write(disableLEDpin);
            }
        }

        /// <summary>
        /// Dispose LED pins
        /// </summary>
        public void DisposePins()
        {
            this.red.Dispose();
            this.green.Dispose();
            this.blue.Dispose();
        }
        /// <summary>
        /// Defines LED driving logic (Common Anode oder Common Cathode)
        /// </summary>
        public enum E_RGB_LEDType
        {
            LowActive, // For Common Anode LEDs
            HighActive, // For Common Cathode LEDs
        }
    }
}
