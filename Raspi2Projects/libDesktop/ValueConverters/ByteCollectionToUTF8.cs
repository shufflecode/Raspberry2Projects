using libShared;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Data;

namespace libDesktop.ValueConverters
{
    class ByteCollectionToUTF8 : IValueConverter
    {
        System.Text.Encoding defaultEncoder = System.Text.Encoding.GetEncoding(1252);
        System.Text.ASCIIEncoding asciiEncoder = new System.Text.ASCIIEncoding();

        /// <summary>
        /// List<byte> -> 1252 string
        /// </summary>
        /// <param name="value"></param>
        /// <param name="targetType"></param>
        /// <param name="parameter"></param>
        /// <param name="culture"></param>
        /// <returns></returns>
        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            string ret = null;
            ObservableCollection<byte> data = value as ObservableCollection<byte>;
            int padLeft = 1;

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
                if (padLeft <= 1)
                {
                    ret = defaultEncoder.GetString(data.ToArray()).Replace("\n", " ").Replace("\r", " ");
                }
                else
                {
                    string s = defaultEncoder.GetString(data.ToArray());

                    s = s.Replace("\n", " ").Replace("\r", " ");

                    char[] chars = s.ToCharArray();

                    ret = string.Join(" ", chars.Select(b => System.Convert.ToString(b).PadLeft(padLeft)));
                }
            }

            return ret;
        }

        /// <summary>
        /// string -> list
        /// </summary>
        /// <param name="value"></param>
        /// <param name="targetType"></param>
        /// <param name="parameter"></param>
        /// <param name="culture"></param>
        /// <returns></returns>
        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            byte[] ret = defaultEncoder.GetBytes(value.ToString());
            ObservableCollection<byte> temp = new ObservableCollection<byte>();

            foreach (var item in ret)
            {
                temp.Add(item);
            }

            return temp;
        }
    }
}
