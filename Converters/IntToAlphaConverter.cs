using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Data;

namespace ThreeByte.Converters
{
    public class IntToAlphaConverter : IValueConverter
    {
        public char _baseChar = 'A';
        public char BaseChar {
            get {
                return _baseChar;
            }
            set {
                _baseChar = value;
            }
        }

        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture) {
            int intVal = System.Convert.ToInt32(value);
            return string.Format("{0}", (char)(BaseChar + intVal));
        }

        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture) {
            throw new Exception("Cannot convert back from IntToAlphaConverter");
        }
    }
   
}
