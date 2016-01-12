using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace libShared.HardwareNah
{
    /// <summary>
    /// RGB-LED-Settings for color channels and drive current for one LED
    /// </summary>
    public struct RGBValue
    {
        /// <summary>
        /// Defines the Maximum Value that can be set
        /// </summary>
        public const int NormValueWidt = 16;
        public const int MaxValue = UInt16.MaxValue;

        private UInt16 red;
        /// <summary>
        /// Red Value
        /// </summary>
        public UInt16 Red
        {
            get { return red; }
            set { red = value; }
        }

        private UInt16 green;
        /// <summary>
        /// Green Value
        /// </summary>
        public UInt16 Green
        {
            get { return green; }
            set { green = value; }
        }

        private UInt16 blue;
        /// <summary>
        /// Blue Value
        /// </summary>
        public UInt16 Blue
        {
            get { return blue; }
            set { blue = value; }
        }

        private UInt16 intensity;
        /// <summary>
        /// Intensity Value
        /// </summary>
        public UInt16 Intensity
        {
            get { return intensity; }
            set { intensity = value; }
        }
    }

    /// <summary>
    /// Generic RGB-Color-Definitions
    /// </summary>
    public static class RGBDefines
    {
        const UInt16 MaxValue = UInt16.MaxValue;
        public readonly static RGBValue Black = new RGBValue { Intensity = MaxValue, Red = 0, Green = 0, Blue = 0 };
        public readonly static RGBValue Red = new RGBValue { Intensity = MaxValue, Red = MaxValue, Green = 0, Blue = 0 };
        public readonly static RGBValue Green = new RGBValue { Intensity = MaxValue, Red = 0, Green = MaxValue, Blue = 0 };
        public readonly static RGBValue Blue = new RGBValue { Intensity = MaxValue, Red = 0, Green = 0, Blue = MaxValue };
        public readonly static RGBValue Yellow = new RGBValue { Intensity = MaxValue, Red = MaxValue, Green = MaxValue, Blue = 0 };
        public readonly static RGBValue Cyan = new RGBValue { Intensity = MaxValue, Red = 0, Green = MaxValue, Blue = MaxValue };
        public readonly static RGBValue Magenta = new RGBValue { Intensity = MaxValue, Red = MaxValue, Green = 0, Blue = MaxValue };
        public readonly static RGBValue White = new RGBValue { Intensity = MaxValue, Red = MaxValue, Green = MaxValue, Blue = MaxValue };
    }
}
