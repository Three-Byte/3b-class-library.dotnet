using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Data;
using System.Windows.Media;
using System.Windows;

namespace ThreeByte.Converters
{
    public class IntToColorConverter : DependencyObject, IValueConverter
    {

        public static readonly DependencyProperty AvailableColorsProperty = DependencyProperty.Register("AvailableColors",
                    typeof(Color[]), typeof(IntToColorConverter), new PropertyMetadata(new Color[0]));

        public Color[] AvailableColors {
            get {
                return (Color[])GetValue(AvailableColorsProperty);
            }
            set {
                SetValue(AvailableColorsProperty, value);
            }
        }
        
        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture) {
            try {
                int intVal = System.Convert.ToInt32(value);
                return AvailableColors[intVal % AvailableColors.Length];
            } catch(Exception) {
                return null;
            }
        }

        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture) {
            Color colorVal = (Color)value;

            for(int i = 0; i < AvailableColors.Length; i++) {
                if(AvailableColors[i] == colorVal) {
                    return i;
                }
            }

            throw new ArgumentOutOfRangeException("value", "Provided color was not in the list of AvailableColors");
        }
    }
   
}
