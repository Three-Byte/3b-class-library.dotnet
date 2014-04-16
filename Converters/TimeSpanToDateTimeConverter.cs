using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Data;

namespace ThreeByte.Converters {
    public class TimeSpanToDateTimeConverter : IValueConverter {

        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture) {
            TimeSpan timespan = (TimeSpan)value;
            DateTime datetime = DateTime.Now;
            if(timespan.Ticks >= 0) {
                datetime = new DateTime(timespan.Ticks);
            }
            return datetime;
        }

        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture) {
            if(value == null) {
                return TimeSpan.Zero;
            }

            DateTime datetime = DateTime.Parse(value.ToString());
            TimeSpan timespan = TimeSpan.FromTicks(datetime.TimeOfDay.Ticks);
            return timespan;
        }
    }
}
