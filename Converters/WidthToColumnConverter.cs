using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Data;

namespace ThreeByte.Converters {
    public class WidthToColumnConverter : IValueConverter {
        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture) {
            float width = float.Parse(value.ToString());
            float columnWidth = float.Parse(parameter.ToString());

            //pass in the column width
            float columns = width / columnWidth;

            return columns;
        }

        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture) {
            throw new NotImplementedException();
        }
    }
}
