using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Data;
using System.Globalization;

namespace ThreeByte.Converters
{
    public class UppercaseConverter : IValueConverter
    {
        //If these are strings ro
        public object Convert(object value, Type tagerType, object parameter, CultureInfo culture) {
            return ((value == null) ? null : value.ToString().ToUpper());
        }

        public object ConvertBack(object value, Type tagerType, object parameter, CultureInfo culture) {
            throw new InvalidOperationException("Cannot convert back from UppercaseConverter!");
        }
        

    }
}
