using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Data;
using System.Windows;

namespace ThreeByte.Converters
{
    public class PercentToGridLengthConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture) {
            double percent = ((value as int?) ?? 0) / 100.0;
            percent = Math.Min(1.0, Math.Max(0.0, percent));

            if(parameter == null) {
                return new GridLength(percent, GridUnitType.Star);
            } else if(parameter as string == "half"){
				return new GridLength((1.0 - percent) / 2.0, GridUnitType.Star);
			} else {
                return new GridLength(1.0 - percent, GridUnitType.Star);
            }
        }

        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture) {
            throw new Exception("Cannot convert back from PercentToGridLengthConverter");
        }
    }
   
}
