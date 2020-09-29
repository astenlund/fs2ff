using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;

namespace fs2ff.Converters
{
    public class BooleanToVisibilityConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return value is bool visible && visible
                ? Visibility.Visible
                : parameter is Visibility vis
                    ? vis
                    : Visibility.Hidden;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return value is Visibility vis
                ? vis == Visibility.Visible
                : Binding.DoNothing;
        }
    }
}
