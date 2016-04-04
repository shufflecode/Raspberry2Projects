using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace libShared
{

    //
    // Zusammenfassung:
    //    RGBA
    public class SharedColor
    {
       
        /// <summary>
        /// Defines the Maximum Value that can be set
        /// </summary>
        public const int NormValueWidt = 16;
        public const int MaxValue = byte.MaxValue;

        private byte red = 10;
        /// <summary>
        /// Red Value
        /// </summary>
        public byte Red
        {
            get { return red; }
            set { red = value; }
        }

        private byte green = 0;
        /// <summary>
        /// Green Value
        /// </summary>
        public byte Green
        {
            get { return green; }
            set { green = value; }
        }

        private byte blue = 0;
        /// <summary>
        /// Blue Value
        /// </summary>
        public byte Blue
        {
            get { return blue; }
            set { blue = value; }
        }

        private byte intensity = 0;
        /// <summary>
        /// Intensity Value
        /// </summary>
        public byte Intensity
        {
            get { return intensity; }
            set { intensity = value; }
        }

        //public static SharedColor operator *(SharedColor value, float factor)
        //{
        //    value.red = (byte)((float)value.red * factor);
        //    value.green = (byte)((float)value.green * factor);
        //    value.blue = (byte)((float)value.blue * factor);
        //    return (value);
        //}

        //public static SharedColor operator +(SharedColor value1, SharedColor value2)
        //{
        //    value1.red += value2.red;
        //    value1.green += value2.green;
        //    value1.blue += value2.blue;
        //    return (value1);
        //}

        //public static SharedColor operator -(SharedColor value1, SharedColor value2)
        //{
        //    value1.red -= value2.red;
        //    value1.green -= value2.green;
        //    value1.blue -= value2.blue;
        //    return (value1);
        //}
    }

}
