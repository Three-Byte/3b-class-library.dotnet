using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Data;
using System.Data.Linq;
using System.Windows.Media.Imaging;
using System.Windows.Controls;
using System.Windows;
using System.IO;


namespace ThreeByte.Converters
{
    public class OppositeVisibilityConverter : IValueConverter
    {
        BooleanToVisibilityConverter _visConverter = new BooleanToVisibilityConverter();

        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture) {
            Visibility vis = ((Visibility?)_visConverter.Convert(value, targetType, parameter, culture) ?? Visibility.Hidden);
            return (vis == Visibility.Visible ? Visibility.Hidden : Visibility.Visible);
        }

        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture) {
            bool isVis = ((bool?)_visConverter.ConvertBack(value, targetType, parameter, culture) ?? false);
            return !isVis;
        }
    }
}
