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
    public class FillToStretchConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture) {
            return (System.Convert.ToBoolean(value) ? Stretch.UniformToFill : Stretch.Uniform);
        }

        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture) {
            Stretch stretch = (value as Stretch?) ?? Stretch.Uniform;
            return (stretch == Stretch.UniformToFill);
        }
    }
}
