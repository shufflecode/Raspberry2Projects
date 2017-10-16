namespace libCore.IOevalBoard
{
    using System;
    using System.Collections.Generic;
    using System.Text;
    using Windows.Devices.Gpio;
    using Windows.Devices.Spi;
    //using RaspiIOhelpers;
    //using RaspiSlaves;
    using libCore.IOevalBoard;
    using System.Collections;

    public class DAC_MCP4922 : GenreicDACslave
    {
        /* Device info
        // SCLK-Idle-state is low
        // Device samples serial input at rising edge / MOSI has to be set on falling edge
        // Device sets serial output on falling edge / MOSI has to be sampled on rising edge

        // Protocol:
        // Pull CS-line
        // Send 4 bit command byte and attach 12-bit output value
        // Release CS-line

        //	Control byte format
        //	Bit	Name    Description
        //	7	Adr0 	Channel selection bit, 0= write channel0, 1= write channel1    
        //	6	Buff    Input-buffer control (for RefInput), 1=buffered, 0=unbuffered 
        //	5	Gain    Input-GainSelection 1=1xgain 0=2xgain
        //	4	SHDN    Output shutDown control. Set to 0 to shut down the DAC
        //	3	x       Output Data bit 11
        //	2	x       Output Data bit 10
        //	1	x       Output Data bit 9
        //	0	x       Output Data bit 8 ...

        // Protocol
        // SingleDataUnit [Byte]
        // Send -> [Command, MSBitsData] [LSBData]
        // Receive don't care

        // ADC module supports only one set-operation per cycle
        // The next set-operation can only be started after release of the CS-line
        */

        /// <summary>
        /// Synchronization input
        /// </summary>
        GpioPin LDACpin;

        /// <summary>
        /// Hardware-shut-sown input
        /// </summary>
        GpioPin SHDNpin;

        /// <summary>
        /// Command frame for DA-conversion
        /// DAC is set with enabled buffer, 1x gain and is active by default
        /// </summary>
        const byte CommandFrame = 0x70;

        //@todo Konfiguraiton in die Klasse umziehen

        /// <summary>
        /// Byte constant to enable the input buffer
        /// </summary>
        const byte InputBufferEnable = 0x40;

        /// <summary>
        /// Byte constant to set 1x gain
        /// </summary>
        const byte GainSelection = 0x20;

        /// <summary>
        /// Byte constant to set DAC active
        /// </summary>
        const byte ShutdonwnDAC = 0x10;


        /// <summary>
        /// SPI-specs for DAC
        /// </summary>
        readonly static SPIHardwareConstrains InterfaceConstrains = new SPIHardwareConstrains
        {
            DemandedSPIMode = SpiMode.Mode0,
            MaxSPIclock = 20000000,
            MinSPIclock = 0,
        };

        /// <summary>
        /// Specs of DAC
        /// </summary>
        readonly static DACspecificDefinitions ConverterDefines = new DACspecificDefinitions
        {
            DACResolution = 12,
            NumOfDACchannels = 2,
            SingleTransmissionLength = 2,
            FullTransmissionLength = 2,
        };

        /// <summary>
        /// Constructor for DAC_MCP4922
        /// </summary>
        /// <param name="spiInterface"> Defines the SP-interface on RasPi board</param>
        /// <param name="spiAdr">Defines the CS-address combination for addressing the slave</param>
        /// <param name="syncPin">Sync pin definition to (optional) sync both DAC ports together</param>
        /// <param name="shdownPin">Shutdown pin (optional) to shutdown DAC-slave </param>
        public DAC_MCP4922(SpiDevice spiInterface, SPIAddressObject spiAdr, GpioPin syncPin, GpioPin shdownPin)
            : base(spiInterface, spiAdr, InterfaceConstrains, ConverterDefines)
        {
            //@todo Hier noch berücksichtigen und was darauf machen
            LDACpin = syncPin;
            SHDNpin = shdownPin;
        }

        /// <summary>
        /// Set single Value for given DAC-channel
        /// </summary>
        /// <param name="chNum">Number of channel starting with 0 fist DAC-channel </param>
        /// <param name="dacVal"></param>
        /// <returns></returns>
        public override void SetSingleChannel(int chNum, Int16 dacVal)
        {
            if (chNum < DACdefines.NumOfDACchannels)
            {
                base.DACvalues[chNum] = dacVal;
                byte[] send = new byte[DACdefines.SingleTransmissionLength];
                byte[] receive = new byte[send.Length];

                send[0] = (byte)(CommandFrame | (byte)(chNum << 7) | (byte)(dacVal >> 11));
                send[1] = (byte)(dacVal >> 3);
                //@todo Allgemeingültige Berechnungen für ADC und DAC einführen die sich auf allgemeine und Spezielle Definitionen stützen
                // Send Data
                base.SendByteStram(send);
            }
            else
            {
                throw new Exception("Requested ADC-Channel exceeds Number of existing Ports");
            }
        }

        /// <summary>
        /// Set all values of given DAC-slave
        /// </summary>
        /// <param name="dacVals"></param>
        public override void SetAllChannels(Int16[] dacVals)
        {
            if (dacVals.Length == base.DACdefines.NumOfDACchannels)
            {
                dacVals.CopyTo(base.DACvalues, 0);
                for (int idx = 0; idx < DACdefines.NumOfDACchannels; idx++)
                {
                    SetSingleChannel(idx, dacVals[idx]);
                }
            }
            else
            {
                throw new Exception("Number of given Values don't meet number of implemented Ports");
            }
        }

        //@todo Uptdate Values hinzufügen
    }
}
