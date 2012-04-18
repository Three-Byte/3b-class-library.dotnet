using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Data;

namespace ThreeByte.Converters {


        public class ParallaxOffsetConverter : IMultiValueConverter {
            //public static readonly DependencyProperty AvailableColorsProperty = DependencyProperty.Register("AvailableColors",
            //            typeof(Color[]), typeof(CategoryToColorMultiConverter), new PropertyMetadata(new Color[0]));

            //public Color[] AvailableColors {
            //    get {
            //        return (Color[])GetValue(AvailableColorsProperty);
            //    }
            //    set {
            //        SetValue(AvailableColorsProperty, value);
            //    }
            //}

            //BasePosition
            //Offset
            //Factor
            public object Convert(object[] values, Type targetType, object parameter, System.Globalization.CultureInfo culture) {

                try {
                    double basePosition = System.Convert.ToDouble(values[0]);
                    double offset = System.Convert.ToDouble(values[1]);
                    double factor = System.Convert.ToDouble(values[2]);

                    return (basePosition + (offset * factor / 100.0));

                } catch {
                    //Don't throw in a converter because it messes with the design-time editor
                }
                return null;
            }

            public object[] ConvertBack(object value, Type[] targetType, object parameter, System.Globalization.CultureInfo culture) {
                throw new Exception("Cannot convert back from ParallaxOffsetConverter");
            }
        }


    
}
