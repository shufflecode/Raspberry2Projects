namespace libCore.IOevalBoard
{
    using System;
    using System.Collections.Generic;
    using System.Text;
    using Windows.Devices.Gpio;
    using Windows.Devices.Spi;
    using Windows.Devices.Enumeration;
    using libCore.IOevalBoard;
    //using RaspiSlaves;

    /// <summary>
    /// Base-Class for SPI-addressed peripheral devices.
    /// This base class is designed for the RasPi IO-module which has different SPI-controlled slaves populated.
    /// All IO-Slaves are controlled by the the same SP-interface the chip selection is achieved either by 
    /// a 3 bit CS-demultiplexer and/or dedicated control lines.
    /// </summary>
    abstract public class RaspiMultiSlave
    {
        private SpiDevice _SPIhandle = null;
        /// <summary>
        /// Assigned SP-interface for SPI-device
        /// </summary>
        public SpiDevice SPIhandle
        {
            get
            {
                return _SPIhandle;
            }

            set
            {
                _SPIhandle = value;
            }
        }

        private SPIAddressObject _CSadr = null;
        /// <summary>
        /// Assigned hardware-address 
        /// </summary>
        public SPIAddressObject CSadr
        {
            get
            {
                return _CSadr;
            }

            set
            {
                _CSadr = value;
            }
        }

        private int _SPIbusAddress = 0;
        /// <summary>
        /// Intrinsic SPI-device address witch has to be transmitted via SPI.
        /// </summary>
        public int SPIbusAddress
        {
            get { return _SPIbusAddress; }
            set { _SPIbusAddress = value; }
        }

        /// <summary>
        /// Hardware constrains defines the restriction for using the respective device.
        /// The given restrictions are verified with the actual SPI configuration of given SPI handle during initialization.
        /// </summary>
        protected SPIHardwareConstrains SPIconstrains = new SPIHardwareConstrains
        {
            //DemandedSPIMode
            //MaxSPIclock
            //MinSPIclock
        };

        /// <summary>
        /// Constructor for RasPiMultiSlave
        /// </summary>
        /// <param name="spiInterface"> Defines the SP-interface on RasPi board</param>
        /// <param name="spiAdr">Defines the CS-address combination (optional) to address the slave during transmission of data,
        /// can be null if no address definition needed</param>
        /// <param name="givenConstrains">Restriction for the respective device</param>
        public RaspiMultiSlave(SpiDevice spiInterface, SPIAddressObject spiAdr, SPIHardwareConstrains givenConstrains)
        {
            /// check if the SP-interface is defined
            if (spiInterface != null)
            {
                SpiConnectionSettings tempSet = spiInterface.ConnectionSettings;
                SPIconstrains = givenConstrains;

                /// Check whether the SPI-configuration meets demanded configuration
                //@todo Prüfen ob die Abfrage so überhaupt funktioniert
                if (tempSet.ClockFrequency < SPIconstrains.MinSPIclock)
                {
                    spiInterface = null;
                    throw new Exception("SPI-Clock doesn't meet the Specification: Clock was set to low");
                }
                if (tempSet.ClockFrequency > SPIconstrains.MaxSPIclock)
                {
                    spiInterface = null;
                    throw new Exception("SPI-Clock doesn't meet the Specification: Clock was set to high");
                }
                if (tempSet.Mode != SPIconstrains.DemandedSPIMode)
                {
                    //@todo ggf. hier die Restriktion herausnehmen, wenn SPI on the Fly umkonfiguriert werden kann
                    spiInterface = null;
                    throw new Exception("SPI-Mode doesned meet the Specification");
                }

                if (spiAdr != null)
                {
                    switch (spiAdr.CSmode)
                    {
                        //@todo Hier prüfen was bei dem Kopieren passiert
                        case SPIAddressObject.eCSadrMode.NoCSPort:
                        case SPIAddressObject.eCSadrMode.SPIdedicated:
                            // no further treatment necessary
                            break;

                        case SPIAddressObject.eCSadrMode.SPIwithGPIO:
                            if (spiAdr.GpioCSpin != null)
                            {
                                throw new Exception("GPIO-Pin for CS use (SPIAddressObject.gpioCSpin) is not defined");
                            }
                            // Set the CS-Pin high by default
                            spiAdr.GpioCSpin.SetDriveMode(GpioPinDriveMode.Output);
                            this.resetCSsignal();
                            break;

                        case SPIAddressObject.eCSadrMode.SPIwithCSdemux:
                            if (spiAdr.CSadrPins.Length == 0)
                            {
                                throw new Exception("There are no Address-Pins for CS-Demux specified");
                            }
                            if (spiAdr.GpioCSpin != null)
                            {
                                spiAdr.GpioCSpin.SetDriveMode(GpioPinDriveMode.Output);
                            }
                            for (int idx = 0; idx < spiAdr.CSadrPins.Length; idx++)
                            {
                                spiAdr.CSadrPins[idx].SetDriveMode(GpioPinDriveMode.Output);
                            }
                            this.resetCSsignal();
                            break;

                        default:
                            throw new Exception("Some crazy shit just happend. The switch parameter is an enum hence this state should be impossible");
                    }
                    CSadr = spiAdr;
                }
                else
                {
                    //@todo hier ein Objekt definieren, dass modus ohne CS-Leitung wählt
                    CSadr = new SPIAddressObject(SPIAddressObject.eCSadrMode.NoCSPort, null, null, 0);
                }
                /// Memorize SPI-Handle for further usage
                SPIhandle = spiInterface;
            }
            else
            {
                throw new Exception("Missing SP-interface-definition");
            }
        }

        /// <summary>
        /// Prepares the CS-signal
        /// </summary>
        public void setCSsignal()
        {
            /// CS-address-object has to be defined
            if (CSadr != null)
            {
                switch (CSadr.CSmode)
                {
                    case SPIAddressObject.eCSadrMode.NoCSPort:
                    case SPIAddressObject.eCSadrMode.SPIdedicated:
                        break;

                    case SPIAddressObject.eCSadrMode.SPIwithGPIO:
                        // Set GPIO-CS-pin active
                        CSadr.GpioCSpin.Write(GpioPinValue.Low);
                        break;
                    case SPIAddressObject.eCSadrMode.SPIwithCSdemux:
                        // Set  CS-address on GIPO-Pins
                        for (int idx = 0; idx < CSadr.CSadrPins.Length; idx++)
                        {
                            if ((CSadr.CSdemuxAdr & (1 << idx)) == 0)
                            {
                                CSadr.CSadrPins[idx].Write(GpioPinValue.Low);
                            }
                            else
                            {
                                CSadr.CSadrPins[idx].Write(GpioPinValue.High);
                            }
                        }

                        if (CSadr.GpioCSpin != null)
                        {
                            CSadr.GpioCSpin.Write(GpioPinValue.High);
                        }
                        break;

                    default:
                        break;
                }
            }
        }

        /// <summary>
        /// Resets CS-configuration
        /// </summary>
        public void resetCSsignal()
        {
            if (CSadr != null)
            {
                switch (CSadr.CSmode)
                {
                    case SPIAddressObject.eCSadrMode.NoCSPort:
                    case SPIAddressObject.eCSadrMode.SPIdedicated:
                        break;

                    case SPIAddressObject.eCSadrMode.SPIwithGPIO:
                        // Set GPIO-CS-pin passive
                        CSadr.GpioCSpin.Write(GpioPinValue.High);
                        break;

                    case SPIAddressObject.eCSadrMode.SPIwithCSdemux:
                        // Reset to default CS-address
                        for (int idx = 0; idx < CSadr.CSadrPins.Length; idx++)
                        {
                            if ((SPIAddressObject.defaultCSAdress & (1 << idx)) == 0)
                            {
                                CSadr.CSadrPins[idx].Write(GpioPinValue.Low);
                            }
                            else
                            {
                                CSadr.CSadrPins[idx].Write(GpioPinValue.High);
                            }
                        }

                        if (CSadr.GpioCSpin != null)
                        {
                            CSadr.GpioCSpin.Write(GpioPinValue.High);
                        }
                        break;

                    default:
                        break;
                }
            }
        }

        /// <summary>
        /// Send data to SPI-slave
        /// </summary>
        /// <param name="sendData">Byte-Array for transmission</param>
        protected void SendByteStram(byte[] sendData)
        {
            if (SPIhandle != null)
            {
                /// Activate CS-signal and CS-address, if necessary
                setCSsignal();
                SPIhandle.Write(sendData);
                // Resets CS-signal and CS-address
                resetCSsignal();
            }
        }

        /// <summary>
        /// Get data from SPI-slave
        /// </summary>
        /// <param name="recData">ByteArray for polled data</param>
        protected void GetByteStream(byte[] recData)
        {
            if (SPIhandle != null)
            {
                // Activate CS-signal and CS-address, if necessary
                setCSsignal();
                SPIhandle.Read(recData);
                // Resets CS-signal and CS-address
                resetCSsignal();
            }
        }

        /// <summary>
        /// Transceive data to/from SPI-slave
        /// </summary>
        /// <param name="sendData">Byte-Array for data transmission</param>
        /// <param name="recData">ByteArray for polled data</param>
        protected void TranceiveByteStram(byte[] sendData, byte[] recData)
        {
            if (SPIhandle != null)
            {
                // Activate CS-signal and CS-address, if necessary
                setCSsignal();
                SPIhandle.TransferFullDuplex(sendData, recData);
                // Resets CS-signal and CS-address
                resetCSsignal();
            }
        }
    }

    /// <summary>
    /// Address definition Class.
    /// The IO Slave can be addressed in several different ways.
    /// 1. The slave needs no CS-signal (chip-select-signal):
    ///     In that case all Pin-References and the CS-adress can be Null
    /// 2. The slave needs only a CS-signal, while a SP-interface dedicated CS-port can be used:
    ///     In that case all Pin-References and the CS-address can be Null
    /// 3. The slave needs only a CS-Signal but the CS-port has to be one of the GPIO-ports:
    ///     In that case the csPin has to be defined with a valid port object
    /// 4. The slave needs only a CS-Signal witch is provided by a CS-demultiplexer
    ///     In that case the respective AdrPins and the csAdr have to be provided
    ///     The csPin can also be provided if a GPIO pin shall be used for chip selection
    /// </summary>
    public class SPIAddressObject
    {
        /// <summary>
        /// Default CS-address
        /// </summary>
        public const int defaultCSAdress = 0;

        /// <summary>
        /// Chip-Select pin if a GPIO pin has to be used for chip selection
        /// </summary>
        public GpioPin GpioCSpin = null;

        /// <summary>
        /// PinArray for address selection pins on CS-demultiplexer
        /// </summary>
        public GpioPin[] CSadrPins = null;

        private int _CSdemuxAdr = defaultCSAdress;
        /// <summary>
        /// Numeric definition of CS-demultiplexer-address for SPIwithCSdemux-mode
        /// </summary>
        public int CSdemuxAdr
        {
            get
            {
                return _CSdemuxAdr;
            }

            set
            {
                _CSdemuxAdr = value;
            }
        }

        /// <summary>
        /// Constructor for SPIAddressObject
        /// </summary>
        /// <param name="mode">Chip selection mode</param>
        /// <param name="csPin">GPIO (depending on mode/ optional) pin for chip selection</param>
        /// <param name="AdrPins">GPIO (depending on mode) pin for address selection on CS-demultiplexer</param>
        /// <param name="csAdr">CS-demultiplexer address for slave</param>
        public SPIAddressObject(eCSadrMode mode, GpioPin csPin, GpioPin[] AdrPins, int csAdr)
        {
            switch (mode)
            {
                case eCSadrMode.NoCSPort:
                case eCSadrMode.SPIdedicated:
                    CSmode = mode;
                    break;
                case eCSadrMode.SPIwithGPIO:
                    if (csPin != null)
                    {
                        CSmode = mode;
                        GpioCSpin = csPin;
                    }
                    else
                    {
                        throw new Exception("Missing CS-Pin Definition");
                    }
                    break;

                case eCSadrMode.SPIwithCSdemux:
                    if (AdrPins.Length > 0)
                    {
                        CSadrPins = AdrPins;
                        CSmode = mode;
                        CSdemuxAdr = csAdr;
                    }
                    else
                    {
                        throw new Exception("Missing AdressPinDefinitions");
                    }
                    if (csPin != null)
                    {
                        GpioCSpin = csPin;
                    }
                    break;

                default:
                    break;
            }
        }

        eCSadrMode _CSmode = eCSadrMode.NoCSPort;
        /// <summary>
        /// Defines assigned chip-select mode 
        /// </summary>
        public eCSadrMode CSmode
        {
            get
            {
                return _CSmode;
            }

            set
            {
                _CSmode = value;
            }
        }

        /// <summary>
        /// Enumerator for chip-select-mode definitions
        /// </summary>
        public enum eCSadrMode
        {
            /// If there is no CS-port definition necessary
            NoCSPort,
            /// Use the chip-select pin witch is assigned to SP-interface as dedicated hardware-pin
            SPIdedicated,
            /// Use chip-select pin from GPIO pins
            SPIwithGPIO,
            /// Use SPI-Port with dedicated chip-select pin and several GPIO pins for addressing the demultiplexer
            SPIwithCSdemux,
        }
    }

    /// <summary>
    /// Hardware constrains SPI-Slaves
    /// </summary>
    public struct SPIHardwareConstrains
    {
        /// <summary>
        /// SPI-clock-restrictions for using the device
        /// </summary>
        public int MinSPIclock { get; set; }
        /// <summary>
        /// SPI-clock-restrictions for using the device 
        /// </summary>
        public int MaxSPIclock { get; set; }
        /// <summary>
        /// SPI-mode for transmission
        /// </summary>
        public SpiMode DemandedSPIMode { get; set; }
    }
}
