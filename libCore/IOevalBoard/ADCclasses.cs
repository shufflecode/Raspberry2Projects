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
    using libCore.IOevalBoard;

    public class ADC_MCP3208 : GenreicADCslave
    {
        /* Device info
        // SCLK-Idle-state is low
        // Device samples serial input at rising edge / MOSI has to be set on falling edge
        // Device sets serial output on falling edge / MOSI has to be sampled on rising edge

        // Protocol:
        // Pull CS-line
        // Send 5 bit command byte 
        // Send min 14 clock pulses to poll the required data
        // Release CS-line

        //	Control byte format (the command byte is dislocated to LSBit to get a left justified ADC Value)
        //	Bit	Name    Description
        //	7	0 	    
        //	6	0
        //	5	Start   =1 command start
        //	4	S/D     =1 for single conversion =0 for differential conversion
        //	3	Adr2    AdressSelection bit 2
        //	2	Adr1    AdressSelection bit 1
        //	1	Adr0    AdressSelection bit 0
        //	0	PD0

        // Protocol
        // SingleDataUnit [Byte]
        // Send -> [Command] 	[00]    	[00]
        //  Rec <- [00]			[MSB]		[LSB]
        // ReseulValue is 16 bit left Justified [0,B9,B8,B7,B6,B5,B4,B3],[B2,B1,B0, S1 S0, S1,S2,S3] // last 3 bits have to be masked


        // ADC Module supports only one conversion per command byte
        // The next AD-conversion can only start after release of the CS-Line
        // To get a right justified ADC value the command has to start after the 5th clock pulse
        // To get a left justified Value the command byte hast to be send after 2nd clock pulse, the last 3 bits of Conversion result has to be masked
        
            Note: 
            - The differential mode is not implemented yet
            */

        /// <summary>
        /// Command frame for AD-conversion
        /// The AD-converter has to send left justified values. Hence the command has to start after the second clock pulse
        /// [0,0, start-bit, single-bit, Adrbit2, Adrbit1, Adrbit0, 0]
        /// </summary>
        const byte CommandFrame = 0x20;
        /// <summary>
        /// Bit for single conversion Mode
        /// </summary>
        const byte SingleConversionBit = 0x10;

        /// <summary>
        /// SPI-specs for ADC
        /// </summary>
        readonly static SPIHardwareConstrains InterfaceConstrains = new SPIHardwareConstrains
        {
            DemandedSPIMode = SpiMode.Mode0,
            MaxSPIclock = 2000000,
            MinSPIclock = 500000,
        };

        /// <summary>
        /// Specs for ADC-slave
        /// </summary>
        readonly static ADCspecificDefinitions ConverterDefines = new ADCspecificDefinitions
        {
            ADCResolution = 12,
            NumOfADCchannels = 8,
            SingleTransmissionLength = 3,
            FullTransmissionLength = 3,
            FullConvMode = ADCspecificDefinitions.eFullConversionMode.OnePortPerTraceiveCycle,
            ReferenceMode = ADCspecificDefinitions.eChannelConversionMode.GNDreference,
        };

        /// <summary>
        /// Constructor for MCP_3208
        /// </summary>
        /// <param name="spiInterface"> Defines the SP-interface on RasPi board</param>
        /// <param name="spiAdr">Defines the CS-address combination for addressing the slave</param>
        public ADC_MCP3208(SpiDevice spiInterface, SPIAddressObject spiAdr)
            : base(spiInterface, spiAdr, InterfaceConstrains, ConverterDefines)
        {
            /// Nothing to do here
        }

        /// <summary>
        /// Get single value for given ADC-channel
        /// </summary>
        /// <param name="chNum">Channel-number starting with 0 for first ADC-channel</param>
        /// <param name="adcVal"></param>
        /// <returns></returns>
        public override void GetSingleChannel(int chNum, out Int16 adcVal)
        {
            if (chNum < base.ADCdefines.NumOfADCchannels)
            {
                byte[] send = new byte[ADCdefines.SingleTransmissionLength];
                byte[] receive = new byte[send.Length];
                send[0] = getSendCommand(chNum, ADCspecificDefinitions.eChannelConversionMode.GNDreference);

                // Send Data
                base.TranceiveByteStram(send, receive);

                Int16 result = ExtractValue(receive);
                base.ADCresults[chNum] = result;
                ADCresults[chNum] = (Int16)result;
                adcVal = result;
            }
            else
            {
                throw new Exception("Requested ADC-Channel exceeds Number of existing Ports");
            }
        }

        /// <summary>
        /// Generates address for conversion command
        /// </summary>
        /// <param name="chNum"></param>
        /// <param name="cMode"></param>
        /// <returns></returns>
        byte getSendCommand(int chNum, ADCspecificDefinitions.eChannelConversionMode cMode)
        {
            byte command = CommandFrame;
            switch (cMode)
            {
                case ADCspecificDefinitions.eChannelConversionMode.GNDreference:
                    command |= (byte)((byte)SingleConversionBit | (byte)(chNum << 1));
                    break;

                case ADCspecificDefinitions.eChannelConversionMode.NextChannelReferenc:

                    throw new NotImplementedException();
                    break;
            }
            return command;
        }

        /// <summary>
        /// Extracts read value from byte-stream
        /// </summary>
        /// <param name="rec">Received byte-stream</param>
        /// <returns></returns>
        protected Int16 ExtractValue(byte[] rec)
        {
            int tempVal;
            tempVal = ((int)rec[1] << 8) | ((int)rec[2] & 0xF8);
            return (Int16)tempVal;
        }

        /// <summary>
        /// Get all values of given ADC-slave
        /// </summary>
        /// <param name="adcVals"></param>
        public override void GetAllChannels(out Int16[] adcVals)
        {
            switch (base.ADCdefines.ReferenceMode)
            {
                case ADCspecificDefinitions.eChannelConversionMode.GNDreference:

                    Int16 dummy;
                    for (int idx = 0; idx < ADCdefines.NumOfADCchannels; idx++)
                    {
                        GetSingleChannel(idx, out dummy);
                    }
                    break;

                case ADCspecificDefinitions.eChannelConversionMode.NextChannelReferenc:
                    throw new NotImplementedException();
                    break;
            }
            adcVals = base.ADCresults;
        }
    }
}
