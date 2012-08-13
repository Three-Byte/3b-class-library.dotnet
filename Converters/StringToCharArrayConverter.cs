using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Data;
using System.Data.Linq;
using System.Windows.Media.Imaging;
using System.IO;
using System.Windows;
using System.Windows.Media;

namespace ThreeByte.Converters
{
    public class StringToCharArrayConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture) {
            string s = value as string;
            if(string.IsNullOrWhiteSpace(s)) {
                return new char[0];
            }
            return s.ToCharArray();
        }

        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture) {
            char[] chars = value as char[];
            if(chars == null) {
                return string.Empty;
            }
            StringBuilder sb = new StringBuilder();
            foreach(char c in chars) {
                sb.Append(c);
            }
            return sb.ToString();
        }
    }
}
