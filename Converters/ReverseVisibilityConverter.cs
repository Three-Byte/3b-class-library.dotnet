using System;
using System.Windows.Data;
using System.Windows;


namespace ThreeByte.Converters
{
    public class ReverseVisibilityConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture) {
            Visibility? vis = value as Visibility?;
            return ((vis ?? Visibility.Visible) == Visibility.Visible ? Visibility.Hidden : Visibility.Visible);
        }

        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture) {
            return Convert(value, targetType, parameter, culture);
        }
    }
}
