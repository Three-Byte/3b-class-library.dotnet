using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Data;
using System.Data.Linq;
using System.Windows.Media.Imaging;
using System.IO;
using System.Windows;

namespace ThreeByte.Converters
{
    public class IncrementConverter : IValueConverter
    {
        public int Increment { get; set; }

        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture) {
            return System.Convert.ToInt32(value) + Increment;
        }

        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture) {
            return System.Convert.ToInt32(value) - Increment;
        }
    }
}
