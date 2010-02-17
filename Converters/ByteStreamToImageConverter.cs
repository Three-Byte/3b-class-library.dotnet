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
    public class ByteStreamToImageConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture) {
            Binary b = value as Binary;
            if(b == null) {
                return null;
            }

            BitmapImage newBitmap = new BitmapImage();
            newBitmap.BeginInit();
            newBitmap.CacheOption = BitmapCacheOption.OnLoad;
            MemoryStream newMemoryStream = new MemoryStream(b.ToArray());
            newBitmap.StreamSource = newMemoryStream;
            newBitmap.EndInit(); 
            return newBitmap;
        }

        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture) {
            throw new Exception("The method or operation is not implemented.");
        }
    }
}
