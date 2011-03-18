using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Data;

namespace ThreeByte.Converters
{
    public class PercentClampConverter : IValueConverter
    {
        public int Minimum { get; set; }
        public int Maximum { get; set; }

        public PercentClampConverter() {
            Minimum = 24;
            Maximum = 72;  //Defaults
        }

        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture) {
            int intValue = (int)value;
			
			return Math.Min(Maximum, Math.Max(Minimum, intValue));
        }

        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture) {
            throw new Exception("Cannot convert back from PercentClampConverter");
        }
    }
   
}
