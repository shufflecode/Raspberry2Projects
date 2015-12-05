using System.Linq;

namespace libShared
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
