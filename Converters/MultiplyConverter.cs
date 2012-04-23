using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Data;

namespace ThreeByte.Converters
{
    public class MultiplyConverter : IValueConverter
    {
        public MultiplyConverter() {
            Factor = 1.0;
        }

        public double Factor { get; set; }


        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture) {
            return System.Convert.ToDouble(value) * Factor;
        }

        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture) {
            return System.Convert.ToDouble(value) / Factor;
        }
    }
   
}
