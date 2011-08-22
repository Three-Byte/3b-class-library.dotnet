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
    public class IsNullOrWhitespaceConverter : IValueConverter
    {
        public bool Opposite { get; set; }

        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture) {
            if(Opposite) {
                return !string.IsNullOrWhiteSpace(value as string);
            } else {
                return string.IsNullOrWhiteSpace(value as string);
            }
        }

        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture) {
            throw new Exception("The method or operation is not implemented.");
        }
    }
}
