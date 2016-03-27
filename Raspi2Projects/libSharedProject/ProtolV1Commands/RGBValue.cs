using System;
using System.Collections.Generic;
using System.Text;

namespace libSharedProject.ProtolV1Commands
{
    /// <summary>
    /// RGB-LED-Settings for color channels and drive current for one LED
    /// </summary>
    public class RGBValue : ProtocolV1Base
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

        public static RGBValue operator *(RGBValue value, float factor)
        {
            value.red = (UInt16)((float)value.red * factor);
            value.green = (UInt16)((float)value.green * factor);
            value.blue = (UInt16)((float)value.blue * factor);
            return (value);
        }

        public static RGBValue operator +(RGBValue value1, RGBValue value2)
        {
            value1.red += value2.red;
            value1.green += value2.green;
            value1.blue += value2.blue;
            return (value1);
        }

        public static RGBValue operator -(RGBValue value1, RGBValue value2)
        {
            value1.red -= value2.red;
            value1.green -= value2.green;
            value1.blue -= value2.blue;
            return (value1);
        }
    }
}
