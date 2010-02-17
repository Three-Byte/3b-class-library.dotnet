using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Data;
using System.Data.Linq;
using System.Windows.Media.Imaging;
using System.IO;
using System.Windows.Media;

namespace ThreeByte.Converters
{
    public class IsEnabledBackgroundConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture) {
            if((value as bool?) ?? false) {
                return new SolidColorBrush(Colors.LightGreen);
            }
            return new SolidColorBrush(Colors.White);
        }

        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture) {
            throw new Exception("Cannot convert back from IsEnabledBackgroundConverter");
        }
    }
}
