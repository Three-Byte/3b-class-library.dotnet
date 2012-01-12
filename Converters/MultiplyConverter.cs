using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Data;
using System.Windows.Media;
using System.Windows;

namespace ThreeByte.Converters
{
    public class MultiplyConverter : IValueConverter
    {
        public double Factor {
            get;
            set;
        }

        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture) {
            double number = System.Convert.ToDouble(value);
            return (number * Factor);
        }

        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture) {
            double number = System.Convert.ToDouble(value);
            return (Factor == 0 ? double.NaN : number / Factor);
        }
    }

}
