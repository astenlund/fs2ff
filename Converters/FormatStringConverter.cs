using System;
using System.Globalization;
using System.Windows.Data;

namespace fs2ff.Converters
{
    public class FormatStringConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object? parameter, CultureInfo culture)
        {
            return parameter is string format
                ? string.Format(format, value)
                : value;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
