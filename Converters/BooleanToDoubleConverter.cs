using System;
using System.Globalization;
using System.Linq;
using System.Windows.Data;
using System.Windows.Markup;

namespace fs2ff.Converters
{
    public class BooleanToDoubleConverter : MarkupExtension, IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (!(value is bool val))
                throw new InvalidOperationException($"{nameof(value)} must be a {nameof(Boolean)}");

            var doubles = parameter.ToString()?.Split(',').Select(double.Parse).ToArray() ?? new double[0];

            if (doubles.Length != 2)
                throw new InvalidOperationException($"{nameof(parameter)} must be two comma-separated integer values");

            return val
                ? doubles[0]
                : doubles[1];
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }

        public override object ProvideValue(IServiceProvider serviceProvider)
        {
            return this;
        }
    }
}
