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
    using System.Collections.Generic;
    using libCore.IOevalBoard;

    public class GPIO_MCP23S17 : GenreicGPIOslave
    {
        /* Device Info
        // SCLK-Idle-State is low
        // Device samples serial input at rising edge / MOSI has to be set on falling edge
        // Device sets serial output on falling edge / MOSI has to be sampled on rising edge

        // Protocol:
        // Pull CS-line
        // Send 8 bit command byte attach 8-bit register-address
        // According to operation (Read or Write) either attach further data or poll demanded data
        // Release CS-line

        //	Control Byte Format
        //	Bit	Name    Description
        //	7	0 	    Constant
        //	6	1       Constant
        //	5	0       Constant
        //	4	0       Constant
        //	3	A2      Address bit A2
        //	2	A1      Address bit A1
        //	1	A0      Address bit A0
        //	0	R/W     Read/write-flag     Set 0 for write operation
        //                                  and 1 for read operation
        // Protocol
        // Single data unit [Byte]
        // Send     ->   [Command]  [Address] [Data] [Data] ...
        // Receive  <-   [xx]       [xx]      [Data] [Data] ...

        // The GPIO slave starts with following register address association*:
        // *An alternative association can be set (see documentation) but the table 
        // below shows the configuration which comes in handy for continuous operation (with auto incrementing index).

            Register    Address     default     Description
            IODIRA      00          1111 1111   Direction register port A: 
            IODIRB      01          1111 1111   ... port B: 1=input, 0=output
            IPOLA       02          0000 0000   Input polarity register port A: 
            IPOLB       03          0000 0000   ... port B: Set 1 for opposite logic
            GPINTENA    04          0000 0000   Interrupt on change register port A: 
            GPINTENB    05          0000 0000   ... port B: Set=1 to enable pin for interrupt-on-change event 
            DEFVALA     06          0000 0000   Default compare register port A:
            DEFVALB     07          0000 0000   ... port B: for interrupt on change register
            INTCONA     08          0000 0000   Interrupt control register port A:
            INTCONB     09          0000 0000   ... port B: Set 1 to compare with DefValx, set 0 for pin change interrupt
            IOCON       0A          0000 0000   Configuration register (see below)
            IOCON       0B          0000 0000   Same configuration register
            GPPUA       0C          0000 0000   Pull-up register port A
            GPPUB       0D          0000 0000   ... port B: Set=1 to enable 100kR pull-up on port
            INTFA       0E          0000 0000   Interrupt flag register on port A:
            INTFB       0F          0000 0000   ... port B: If 1 the corresponding pin caused an interrupt
            INTCAPA     10          0000 0000   Interrupt capture register port A:
            INTCAPB     11          0000 0000   ... port B: Captures the IO-data at interrupt event
            GPIOA       12          0000 0000   Port register port A
            GPIOB       13          0000 0000   ... port B: 
            OLATA       14          0000 0000   Output latch register port A
            OLATB       15          0000 0000   ... port B: 

            Configuration Register
            bit     Name    Description
            bit 7   BANK    Address association:    0=A/B-registers ar paired, 1=A/B registers a separated
            bit 6   MIRROR  Interrupt outputs:       Set=1 to connect IntA and IntB internally 
            bit 5   SEQOP   Sequential operation mode:  Set=1 to increment address-pointer automatically
            bit 4   DISSLW  Slew rate control:      Set=1 to activate slew-rate on SDA-output
            bit 3   HAEN    Hardware address enable: Set=1 to enable hardware address pins
            bit 2   ODR     Interrupt output:       Set=1 to change interrupt output from push-pull to open drain
            bit 1   INTPOL  Interrupt polarity:     1=active-high, 0=active-low
            bit 0   ---

        // GPIO Module supports also continuous Write/Read 
        */

        /// <summary>
        /// Reset input
        /// </summary>
        GpioPin Resetpin;

        /// <summary>
        /// Interrupt output A
        /// </summary>
        GpioPin IntApin;

        /// <summary>
        /// Interrupt output B
        /// </summary>
        GpioPin IntBpin;

        /// <summary>
        /// Address constants for bank Set = 0
        /// Because the "sequential operation mode" is activated at first configuration run, only sequential writes are implemented.
        /// Hence there are only start addresses for Port A needed in the Dictionary (port B follows always port a in this mode)
        /// </summary>
        readonly Dictionary<string, byte> AdrConstantsBank0 = new Dictionary<string, byte>
        {
            {"IODirA",0x00}, // Data direction
            {"IPolA",0x02}, // Input polarization
            {"GPIntEnA",0x04}, // Interrupt on change register
            {"DefValA",0x06}, // Compare register
            {"IntConA",0x08}, // Interrupt control register
            {"IOCon",0x0A}, // Configuration register
            {"GPPUA",0x0C}, // Pull-up register
            {"IntFA",0x0E}, // Interrupt flag register 
            {"IntCapA",0x10}, // Interrupt capture register
            {"GPIOA",0x12}, // Port register 
            {"OLatA",0x14}, // Latch register
        };

        /// <summary>
        /// Byte constant for default Slave configuration.
        /// Sequential operation mode and hardware address enable is set by default
        /// </summary>
        const byte DefaultGPIOslaveConfig = 0x28;

        /// <summary>
        /// Command frame for GPIO access
        /// First four bit are a constant value witch are given in the documentation
        /// </summary>
        const byte CommandFrame = 0x40;
        /// <summary>
        /// Byte constant to set read flag
        /// </summary>
        const byte ReadFlag = 0x01;

        /// <summary>
        /// SPI-Specs for IO-Expander
        /// </summary>
        readonly static SPIHardwareConstrains InterfaceConstrains = new SPIHardwareConstrains
        {
            DemandedSPIMode = SpiMode.Mode0,
            MaxSPIclock = 10000000,
            MinSPIclock = 0,
        };

        /// <summary>
        /// Specs for IO-Expander
        /// </summary>
        readonly static GPIOspecificDefinitions ConverterDefines = new GPIOspecificDefinitions
        {
            NumOfPorts = 2,
            SingleTransmissionLength = 3,
            FullTransmissionLength = 4,
        };

        /// <summary>
        /// Constructor for DAC_MCP23S17
        /// </summary>
        /// <param name="spiInterface"> Defines the SP-interface on RasPi board</param>
        /// <param name="spiAdr">Defines the CS-address combination for addressing the slave</param>
        /// <param name="rstPin">Rest pin (optional) to reset device</param>
        /// <param name="intPin">Interrupt Input pins (optional) for interrupt handling</param>
        /// <param name="spiBusAddress">Intrinsic SPI-address witch is to be send via SPI message</param>
        public GPIO_MCP23S17(SpiDevice spiInterface, SPIAddressObject spiAdr, GpioPin rstPin, GpioPin[] intPin, int spiBusAddress)
            : base(spiInterface, spiAdr, InterfaceConstrains, ConverterDefines)
        {
            //@todo Hier noch berücksichtigen
            Resetpin = rstPin;
            if (intPin != null)
            {
                if (intPin.Length >= 1)
                {
                    IntApin = intPin[0];
                }
                if (intPin.Length >= 2)
                {
                    IntBpin = intPin[1];
                }
            }
            base.SPIbusAddress = spiBusAddress;
        }

        /// <summary>
        /// Generates command frame for SPI byte stream
        /// </summary>
        /// <param name="mode"></param>
        /// <returns></returns>
        byte GenerateCommand(eMessageMode mode)
        {
            byte command = CommandFrame;
            // Add Device Address
            command |= (byte)(SPIbusAddress << 1);
            if (mode == eMessageMode.read)
            {
                command |= ReadFlag;
            }
            return command;

        }

        /// <summary>
        /// Set basic configuration on GPIO slave
        /// </summary>
        /// <param name="mode"></param>
        protected override void ExecuteConfigRun(eConfigRunMode mode)
        {
            byte[] send;

            if (mode == eConfigRunMode.DeviceConfigOnly)
            {
                send = new byte[3];
                send[0] = GenerateCommand(eMessageMode.write);
                send[1] = AdrConstantsBank0["IOCon"];
                send[2] = DefaultGPIOslaveConfig;

                base.SendByteStram(send);
                ConfigRunDone = true;
            }
        }

        /// <summary>
        /// Get GPIO port data for port A and B
        /// </summary>
        /// <param name="data"></param>
        public override void GetPorts(out byte[] data)
        {
            byte[] send;
            byte[] receive;

            send = new byte[4];
            receive = new byte[4];

            send[0] = GenerateCommand(eMessageMode.read);
            send[1] = AdrConstantsBank0["GPIOA"];

            TranceiveByteStram(send, receive);

            PortData[0] = receive[2];
            PortData[1] = receive[3];
            data = PortData;
        }

        /// <summary>
        /// Set GPIO port data for port A and B
        /// </summary>
        /// <param name="data"></param>
        public override void SetPorts(byte[] data)
        {
            byte[] send;
            if (data.Length == 2)
            {
                //@todo Hier ggf. noch Auf Plausibilität mit Direction bringen
                //@todo Ggf noch eine DataContainer für OutputSet mitbringen

                send = new byte[4];

                send[0] = GenerateCommand(eMessageMode.write);
                send[1] = AdrConstantsBank0["OLatA"]; // @todo Testen ob nicht GPIOA besser wäre 
                send[2] = data[0];
                send[3] = data[1];

                SendByteStram(send);
            }
        }

        /// <summary>
        /// Set GPIO port-direction for port A and B
        /// </summary>
        /// <param name="data"></param>
        public override void SetDirection(byte[] data)
        {
            byte[] send;
            if (data.Length == 2)
            {
                data.CopyTo(PortDirection, 0);
                send = new byte[4];

                send[0] = GenerateCommand(eMessageMode.write);
                send[1] = AdrConstantsBank0["IODirA"];
                send[2] = PortDirection[0];
                send[3] = PortDirection[1];

                SendByteStram(send);
            }
        }
        //@todo Uptdate Values hinzufügen

        /// <summary>
        /// Set GPIO pull-ups for Port A and B
        /// </summary>
        /// <param name="data"></param>
        public void SetPullUps(byte[] data)
        {
            byte[] send;
            if (data.Length == 2)
            {
                //@todo Hier ggf. noch Auf Plausibilität mit Direction bringen

                send = new byte[4];

                send[0] = GenerateCommand(eMessageMode.write);
                send[1] = AdrConstantsBank0["GPPUA"];
                send[2] = data[0];
                send[3] = data[1];

                SendByteStram(send);
            }
        }
        /// <summary>
        /// Set GPIO input logic for Port A and B
        /// </summary>
        /// <param name="data">1 reverses the input logic</param>
        public void SetInputLogic(byte[] data)
        {
            byte[] send;
            if (data.Length == 2)
            {
                send = new byte[4];

                send[0] = GenerateCommand(eMessageMode.write);
                send[1] = AdrConstantsBank0["IPolA"];
                send[2] = data[0];
                send[3] = data[1];

                SendByteStram(send);
            }
        }

    }
}
