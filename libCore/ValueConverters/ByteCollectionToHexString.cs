using libShared;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.UI.Xaml.Data;

namespace libCore.ValueConverters
{
    public class ByteCollectionToHexString : IValueConverter
    {
        public static ObservableCollection<byte> StringToByteArrayFastest(string _hex)
        {
            string hex = _hex.Replace(" ", "");

            if (hex.Length % 2 == 1)
                throw new Exception("The binary key cannot have an odd number of digits");

            //byte[] arr = new byte[hex.Length >> 1];
            ObservableCollection<byte> temp = new ObservableCollection<byte>();

            for (int i = 0; i < hex.Length >> 1; ++i)
            {
                temp.Add((byte)((GetHexVal(hex[i << 1]) << 4) + (GetHexVal(hex[(i << 1) + 1]))));
                //arr[i] = (byte)((GetHexVal(hex[i << 1]) << 4) + (GetHexVal(hex[(i << 1) + 1])));
            }

            return temp;
            //return arr;
        }

        public static int GetHexVal(char hex)
        {
            int val = (int)hex;
            //For uppercase A-F letters:
            return val - (val < 58 ? 48 : 55);
            //For lowercase a-f letters:
            //return val - (val < 58 ? 48 : 87);
            //Or the two combined, but a bit slower:
            //return val - (val < 58 ? 48 : (val < 97 ? 55 : 87));
        }

        public object Convert(object value, Type targetType, object parameter, string language)
        {
            string ret = null;
            ObservableCollection<byte> data = value as ObservableCollection<byte>;
            int padLeft = 2;

            if (parameter != null && parameter.GetType().Equals(typeof(ConverterParameterHelper)))
            {
                padLeft = ((ConverterParameterHelper)parameter).PadLeft;
            }
            else if (parameter != null && parameter.GetType().Equals(typeof(int)))
            {
                padLeft = (int)parameter;
            }

            if (data != null)
            {
                ret = string.Join(" ", data.Select(b => System.Convert.ToString(b, 16).PadLeft(2, '0').PadLeft(padLeft))).ToUpper();
            }

            return ret;
        }

        public object ConvertBack(object value, Type targetType, object parameter, string language)
        {
            if (value == null)
            {
                return null;
            }

            return StringToByteArrayFastest(value.ToString());
        }
    }
}
