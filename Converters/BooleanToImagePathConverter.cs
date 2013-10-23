using System;
using System.Windows.Data;

//author: mike@3-byte.com

namespace ThreeByte.Converters
{
    public class BooleanToStringConverter : IValueConverter
    {
        public string TruePath { get; set; }
        public string FalsePath { get; set; }

        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture) {
            bool b = (bool)value;

            if (b) {
                return TruePath;
            } else {
                return FalsePath;
            }
        }

        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture) {
            throw new NotImplementedException();
        }
    }
}
