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
    public class EqualityVisibilityConverter : IValueConverter
    {
        public bool Opposite { get; set; }
        public Visibility HideState { get; set; }
        public double Value { get; set; }

        public EqualityVisibilityConverter() {
            HideState = Visibility.Hidden;
            Value = 0.0;
        }

        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture) {
            double val = System.Convert.ToDouble(value);
            if(Opposite) {
                return (val == Value ? HideState : Visibility.Visible);
            } else {
                return (val == Value ? Visibility.Visible : HideState);
            }
        }

        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture) {
            Visibility? vis = value as Visibility?;
            if(Opposite) {
                return ((vis ?? Visibility.Visible) != Visibility.Visible);
            } else {
                return ((vis ?? Visibility.Visible) == Visibility.Visible);
            }
        }
    }
}
