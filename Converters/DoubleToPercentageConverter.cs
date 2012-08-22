using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Data;

namespace ThreeByte.Converters {
    /// <summary>
    /// Takes a double (0.20349) and converts it to a readable percentage (20.35%)
    /// </summary>
    public class DoubleToPercentageConverter : IValueConverter {
        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture) {
            double ratio = 0.0f;
            if(!double.TryParse(value.ToString(), out ratio)) {
                return 0.0f;
            }

            return Math.Round(ratio * 100.0f, 2).ToString() + " %";
        }

        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture) {
            throw new NotImplementedException();
        }
    }
}
