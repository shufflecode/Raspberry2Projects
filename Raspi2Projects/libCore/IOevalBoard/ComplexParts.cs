namespace libCore.IOevalBoard
{
    using System;
    using System.Collections.Generic;
    using System.Text;
    using Windows.Devices.Gpio;
    using Windows.Devices.Spi;

    /// <summary>
    /// LED-Objekt
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
        /// Konstruktor für RGB-LED-Objekt. Reserviert Hardwarepins und definiert Schaltlogik
        /// </summary>
        /// <param name="gpio">GPIO-Objekt von IO-Controller</param>
        /// <param name="redPin">PinNummer für RotKanal</param>
        /// <param name="greenPin">PinNummer für GrünKanal</param>
        /// <param name="bluePin">PinNummer für BlauKanal</param>
        /// <param name="ledType">Schaltlogik der LED pins</param>
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
            // Setzen der Pins als Output
            this.red.SetDriveMode(GpioPinDriveMode.Output);
            this.green.SetDriveMode(GpioPinDriveMode.Output);
            this.blue.SetDriveMode(GpioPinDriveMode.Output);
        }

        /// <summary>
        /// Setzten der LED-Ausgangspins
        /// </summary>
        /// <param name="red">Rot-Kanal</param>
        /// <param name="green">Grün-Kanal</param>
        /// <param name="blue">Blau-Kanal</param>
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
        /// LED-PinRessourcen freigeben
        /// </summary>
        public void DisposePins()
        {
            this.red.Dispose();
            this.green.Dispose();
            this.blue.Dispose();
        }
        /// <summary>
        /// Gibt den RGB-LED-Typen an (Common Anode oder Common Catode)
        /// </summary>
        public enum E_RGB_LEDType
        {
            LowActive, // Wenn Common Anode
            HighActive, // Wenn Common Catode
        }
    }
}
