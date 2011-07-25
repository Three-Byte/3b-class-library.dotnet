using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Data;
using System.Data.Linq;
using System.Windows.Media.Imaging;
using System.IO;


namespace ThreeByte.Converters
{
    public class FrozenImageConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture) {
            //Take a URI and 

            string path = System.Convert.ToString(value);
            if(string.IsNullOrEmpty(path)) {
                return null;
            }

            BitmapImage newBitmap = new BitmapImage();
            newBitmap.BeginInit();
            newBitmap.CacheOption = BitmapCacheOption.OnLoad;
            using(FileStream fs = new FileStream(path, FileMode.Open)) {
                newBitmap.StreamSource = fs;
                newBitmap.EndInit();
                newBitmap.Freeze();
            }
            return newBitmap;
        }

        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture) {
            throw new Exception("The method or operation is not implemented.");
        }
    }
}
