using System;
using System.Globalization;
using System.Windows.Data;
using System.Windows.Markup;

namespace fs2ff.Converters
{
    public class UIntToDoubleConverter : MarkupExtension, IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return value is uint i
                ? (double)i
                : Binding.DoNothing;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return value is double d
                ? (int)Math.Round(d)
                : Binding.DoNothing;
        }

        public override object ProvideValue(IServiceProvider serviceProvider)
        {
            return this;
        }
    }
}
