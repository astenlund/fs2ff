using System;
using System.Globalization;
using System.Net;
using System.Text.RegularExpressions;
using System.Windows.Data;
using System.Windows.Markup;

namespace fs2ff.Converters
{
    public class IpAddressToStringConverter : MarkupExtension, IValueConverter
    {
        private readonly Regex _regex = new Regex(@"^(?:(?:25[0-5]|2[0-4][0-9]|[01]?[0-9][0-9]?)\.){3}(?:25[0-5]|2[0-4][0-9]|[01]?[0-9][0-9]?)$");

        public object Convert(object? value, Type targetType, object parameter, CultureInfo culture)
        {
            if (!(value is IPAddress ip))
                return "";

            return Equals(ip, IPAddress.Any)
                ? ""
                : ip.ToString();
        }

        public object? ConvertBack(object? value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value == null)
                return null;

            if (!(value is string str))
                return Binding.DoNothing;

            if (str == "")
                return null;

            if (!_regex.IsMatch(str))
                return Binding.DoNothing;

            return IPAddress.TryParse(str, out var result)
                ? result
                : Binding.DoNothing;
        }

        public override object ProvideValue(IServiceProvider serviceProvider)
        {
            return this;
        }
    }
}
