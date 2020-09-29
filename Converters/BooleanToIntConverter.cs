using System;
using System.Globalization;
using System.Linq;
using System.Windows.Data;

namespace fs2ff.Converters
{
    public class BooleanToIntConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (!(value is bool val))
                throw new InvalidOperationException($"{nameof(value)} must be a {nameof(Boolean)}");

            var ints = parameter.ToString()?.Split(',').Select(int.Parse).ToArray() ?? new int[0];

            if (ints.Length != 2)
                throw new InvalidOperationException($"{nameof(parameter)} must be two comma-separated integer values");

            return val
                ? ints[0]
                : ints[1];
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
