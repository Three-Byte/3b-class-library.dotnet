using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Media;

namespace ThreeByte.Media
{
    public class TemplateRegion
    {
        public TemplateRegion(int x, int y, int w, int h, Color color) {
            X = x;
            Y = y;
            W = w;
            H = h;
            Color = color;
        }

        public int X { get; private set; }
        public int Y { get; private set; }
        public int W { get; private set; }
        public int H { get; private set; }
        public Color Color { get; private set; }
    }
}
