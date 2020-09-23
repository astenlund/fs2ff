using System;
using System.Globalization;
using System.Net;
using System.Windows.Data;

namespace fs2ff.Converters
{
    public class IpAddressToStringConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (!(value is IPAddress ip))
                return "";

            return ip.ToString();
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (!(value is string str))
                return Binding.DoNothing;

            if (str.Split('.').Length != 4)
                return Binding.DoNothing;

            return IPAddress.TryParse(str, out var result)
                ? result
                : Binding.DoNothing;
        }
    }
}
