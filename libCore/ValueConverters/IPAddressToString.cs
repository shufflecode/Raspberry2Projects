using System;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using Windows.UI.Xaml.Data;

namespace libCore.ValueConverters
{
    public class IPAddressToString : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, string language)
        {
            return value.ToString();
        }

        public object ConvertBack(object value, Type targetType, object parameter, string language)
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
