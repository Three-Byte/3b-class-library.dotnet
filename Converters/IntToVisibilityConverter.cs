using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Data;
using System.Windows;

namespace ThreeByte.Converters
{
    public class IntToVisibilityConverter : IValueConverter
    {

        public int IntValue { get; set; }
        public Visibility DefaultVisibility { get; set; }
        public Visibility HighlightVisibility { get; set; }

        public IntToVisibilityConverter() {
            IntValue = 0;
            DefaultVisibility = Visibility.Visible;
            HighlightVisibility = Visibility.Hidden;
        }

        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture) {
            int intVal = System.Convert.ToInt32(value);
            return (intVal == IntValue ? HighlightVisibility : DefaultVisibility);
        }

        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture) {
            throw new Exception("Cannot convert back from IntToVisibilityConverter");
        }
    }
   
}
