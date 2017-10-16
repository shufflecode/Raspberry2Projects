using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Data;

namespace libDesktop.ValueConverters
{
    public class IPAddressToString : IValueConverter
    {
        /// <summary>
        /// IP to Sting
        /// </summary>
        /// <param name="value"></param>
        /// <param name="targetType"></param>
        /// <param name="parameter"></param>
        /// <param name="culture"></param>
        /// <returns></returns>
        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            return value.ToString();
        }

        /// <summary>
        /// String to IP
        /// </summary>
        /// <param name="value"></param>
        /// <param name="targetType"></param>
        /// <param name="parameter"></param>
        /// <param name="culture"></param>
        /// <returns></returns>
        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            IPAddress ip = null;

            if (IPAddress.TryParse(value.ToString(), out ip))
            {
                return ip;
            }
            else
            {
                return IPAddress.Any;
            }
        }
    }
}
