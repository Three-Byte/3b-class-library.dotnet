using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.IO;

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

    }
}
