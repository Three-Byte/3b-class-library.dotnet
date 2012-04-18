using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Data;
using ThreeByte.Media;

namespace ThreeByte.Converters {


    public class DimensionToBoundaryConverter : IMultiValueConverter {
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
                    double width = System.Convert.ToDouble(values[0]);
                    double height = System.Convert.ToDouble(values[1]);


                    return new Boundary() { MinX = 0, MinY = 0, MaxX = width, MaxY = height };

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
