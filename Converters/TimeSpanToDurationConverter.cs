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
    public class TimeSpanToDurationConverter : IValueConverter
    {

        private static readonly TimeSpan DEFAULT_TIMESPAN = TimeSpan.FromSeconds(0);

        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture) {
            if(!(value is TimeSpan) || ((TimeSpan)value) < DEFAULT_TIMESPAN) {
                return new Duration(DEFAULT_TIMESPAN);
            }
            return new Duration((TimeSpan)value);
        }

        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture) {
            if(!(value is Duration)) {
                return DEFAULT_TIMESPAN;
            }
            Duration duration = (Duration)value;
            if(!(duration.HasTimeSpan)) {
                return DEFAULT_TIMESPAN;
            }
            return duration.TimeSpan;
        }
    }
}
