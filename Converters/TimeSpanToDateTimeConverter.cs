using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Data;

namespace ThreeByte.Converters {
    public class TimeSpanToDateTimeConverter : IValueConverter {

        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture) {
            TimeSpan timespan = (TimeSpan)value;
            DateTime datetime = new DateTime(timespan.Ticks);
            return datetime;
        }

        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture) {
            DateTime datetime = (DateTime)value;
            TimeSpan timespan = TimeSpan.FromTicks(datetime.Ticks);
            return timespan;
        }
    }
}
