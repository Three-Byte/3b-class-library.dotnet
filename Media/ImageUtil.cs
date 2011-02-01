using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.IO;
using System.Data.Linq;

namespace ThreeByte.Media
{
    public static class ImageUtil
    {

        public static void SaveToBitmap(FrameworkElement surface, string filename) {
            Transform xform = surface.LayoutTransform;
            surface.LayoutTransform = null;


            int width = (int)surface.Width;
            int height = (int)surface.Height;

            Size sSize = new Size(width, height);
            surface.Measure(sSize);
            surface.Arrange(new Rect(sSize));

            RenderTargetBitmap renderBitmap = new RenderTargetBitmap(width, height, 96, 96, PixelFormats.Pbgra32);
            renderBitmap.Render(surface);

            string dir = System.IO.Path.GetDirectoryName(filename);
            if(dir.Trim() != string.Empty) {
                Directory.CreateDirectory(System.IO.Path.GetDirectoryName(filename));
            }

            using(FileStream fStream = new FileStream(filename, FileMode.Create)) {
                PngBitmapEncoder pngEncoder = new PngBitmapEncoder();
                pngEncoder.Frames.Add(BitmapFrame.Create(renderBitmap));
                pngEncoder.Save(fStream);
            }

            surface.LayoutTransform = xform;
        }

        public static string SerializeBinary(Binary bits) {
                StringBuilder sb = new StringBuilder(bits.Length * 2);
                foreach(byte b in bits.ToArray()) {
                    sb.Append(String.Format("{0:x2}", b));
                }
                return sb.ToString();
        }

        public static Binary UnserializeBinary(string chars) {

            byte[] byteArray = new byte[chars.Length / 2];
            for(int i = 0; i < chars.Length; i = i+2) {
                byteArray[i/2] = Convert.ToByte(chars.Substring(i, 2), 16);
            }
            return new Binary(byteArray);
        }


        public static string EncodeBitmapImage(BitmapImage image, BitmapEncoder encoder = null) {
            //Default to PNG Encoding
            if(encoder == null) {
                encoder = new PngBitmapEncoder();
            }

            FormatConvertedBitmap formattedIcon = new FormatConvertedBitmap();
            formattedIcon.BeginInit();
            formattedIcon.Source = image;
            formattedIcon.DestinationFormat = PixelFormats.Bgra32;//.Rgb24;
            formattedIcon.EndInit();
            BitmapFrame frame = BitmapFrame.Create(formattedIcon);

            encoder.Frames.Add(frame);
            MemoryStream mem = new MemoryStream();
            encoder.Save(mem);

            byte[] graphicBytes = mem.GetBuffer();
            mem.Dispose();

            return Convert.ToBase64String(graphicBytes, Base64FormattingOptions.InsertLineBreaks);
        }


        public static BitmapImage DecodeBitmapImage(string encoded) {

            byte[] graphicBytes = Convert.FromBase64String(encoded);
            
            BitmapImage image = new BitmapImage();
            using(MemoryStream memStream = new MemoryStream(graphicBytes)) {
                image.BeginInit();
                image.CacheOption = BitmapCacheOption.OnLoad;
                image.StreamSource = memStream;
                image.EndInit();
                image.Freeze();
            }

            return image;
        }

    
    }
}
