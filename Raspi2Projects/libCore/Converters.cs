using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace libCore
{
    public static class Converters
    {
        /// <summary>
        /// Konvertiert ein Byte Array in einen Hex String
        /// </summary>       
        public static string ConvertByteArrayToHexString(byte[] data, string serperator)
        {
            return string.Join(serperator, data.Select(b => string.Format("{0:X2}", b)));
        }

    }
}
