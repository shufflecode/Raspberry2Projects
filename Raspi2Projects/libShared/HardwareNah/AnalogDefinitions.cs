using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace libShared.HardwareNah
{

    /// <summary>
    /// Defines the configuration Structure of an analog Channel
    /// </summary>
    public struct AnaChConfig
    {
        /// <summary>
        /// Defines the Ratio of the measuring Resistor to the whole Resisor
        /// </summary>
        public float ResistorRatio { get; set; }
        /// <summary>
        /// Defines the Reference Voltage of the Converter System
        /// </summary>
        public float RefVoltage { get; set; }
        /// <summary>
        /// Defines the Offset Value for Pseudo-Differential Measerment
        /// </summary>
        public float OffsetValue { get; set; }

        // @todo implement HelperClasses for setting Values little bit more idiote proof
    }
}
