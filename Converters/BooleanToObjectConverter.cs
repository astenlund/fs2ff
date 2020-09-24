using System;
using System.Globalization;
using System.Windows.Data;

namespace fs2ff.Converters
{
    public class BooleanToObjectConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (!(value is bool val))
                throw new InvalidOperationException($"{nameof(value)} must be a {nameof(Boolean)}");

            if (!(parameter is Array objs))
                throw new InvalidOperationException($"{nameof(parameter)} must be an array");

            if (objs.Length != 2)
                throw new InvalidOperationException($"{nameof(parameter)} array must contain exactly two items");

            return val
                ? objs.GetValue(0)!
                : objs.GetValue(1)!;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
