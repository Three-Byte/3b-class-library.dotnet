using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Data;
using System.Data.Linq;
using System.Windows.Media.Imaging;
using System.IO;


namespace ThreeByte.Converters
{
    public class DateTimeConverter : IValueConverter
    {
        //TimeSpan --> DateTime
        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture) {
            if(value == null) {
                return DateTime.MinValue;
            }
            TimeSpan input = (TimeSpan)value;
            return new DateTime(Math.Max(input.Ticks, 0));
        }

        //DateTime --> TimeSpan
        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture) {
            if(value == null) {
                return TimeSpan.MinValue;
            }
            DateTime input = (DateTime)value;
            return input.TimeOfDay;
        }
    }
}
