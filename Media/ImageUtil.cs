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
        public static readonly Size DefaultPortraitSize = new Size(768, 1024);
        public static readonly Size DefaultLanscapeSize = new Size(DefaultPortraitSize.Height, DefaultPortraitSize.Width);

        public static void SaveToBitmap(FrameworkElement surface, string filename, Size size = default(Size)) {
            Transform xform = surface.LayoutTransform;
            surface.LayoutTransform = null;

            if(size == default(Size)) {
                size = new Size(surface.ActualWidth, surface.ActualHeight);
            }
            //int width = (int)surface.Width;
            //int height = (int)surface.Height;

            //Size sSize = new Size(width, height);
            surface.Measure(size);
            surface.Arrange(new Rect(size));
            surface.UpdateLayout();

            //RenderTargetBitmap renderBitmap = new RenderTargetBitmap((int)(size.Width), (int)(size.Height), 96, 96, PixelFormats.Pbgra32);
            //renderBitmap.Render(surface);
            RenderTargetBitmap renderBitmap = new RenderTargetBitmap((int)(size.Width), (int)(size.Height), 96, 96, PixelFormats.Pbgra32);
            DrawingVisual dv = new DrawingVisual();
            using(DrawingContext ctx = dv.RenderOpen()) {
                VisualBrush vb = new VisualBrush(surface);
                ctx.DrawRectangle(vb, null, new Rect(new Point(), size));
            }
            renderBitmap.Render(dv);
            renderBitmap.Freeze();

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

        public static BitmapSource SaveToBitmapSource(FrameworkElement surface, Size size = default(Size)) {
            Transform xform = surface.LayoutTransform;
            surface.LayoutTransform = null;

            if(size == default(Size)) {
                size = new Size(surface.ActualWidth, surface.ActualHeight);
            }
            //int width = (int)surface.Width;
            //int height = (int)surface.Height;

            //Size sSize = new Size(width, height);
            surface.Measure(size);
            surface.Arrange(new Rect(size));

            RenderTargetBitmap renderBitmap = new RenderTargetBitmap((int)(size.Width), (int)(size.Height), 96, 96, PixelFormats.Pbgra32);
            DrawingVisual dv = new DrawingVisual();
            using(DrawingContext ctx = dv.RenderOpen()) {
                VisualBrush vb = new VisualBrush(surface);
                ctx.DrawRectangle(vb, null, new Rect(new Point(), size));
            }
            renderBitmap.Render(dv);
            renderBitmap.Freeze();

            surface.LayoutTransform = xform;
            return renderBitmap;
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


        public static string EncodeBitmapImage(BitmapSource image, BitmapEncoder encoder = null) {
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
