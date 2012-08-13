using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Data;
using System.Windows;

namespace ThreeByte.Converters
{
    public class ParameterEqualVisibilityConverter : IValueConverter
    {
        public Visibility DefaultVisibility { get; set; }
        public Visibility HighlightVisibility { get; set; }

        public ParameterEqualVisibilityConverter() {
            DefaultVisibility = Visibility.Visible;
            HighlightVisibility = Visibility.Hidden;
        }

        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture) {
            return (value == parameter ? HighlightVisibility : DefaultVisibility);
        }

        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture) {
            throw new Exception("Cannot convert back from ParameterEqualVisibilityConverter");
        }
    }
   
}
