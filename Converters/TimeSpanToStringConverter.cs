using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Data;

namespace ThreeByte.Converters {
    public class TimeSpanToStringConverter : IValueConverter {
        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture) {
            TimeSpan time = (TimeSpan)value;

            string neg = @"\ ";
            if(time < TimeSpan.Zero) {
                neg = @"\-";
            }

            return time.ToString(neg + @"hh\:mm\:ss\.f");
        }

        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture) {
            throw new NotImplementedException();
        }
    }
}
