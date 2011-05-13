using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;

namespace ThreeByte.Media
{

    public class Boundary
    {
        public double MinX { get; set; }
        public double MaxX { get; set; }
        public double MinY { get; set; }
        public double MaxY { get; set; }

        public Boundary() { }
        public Boundary(TemplateRegion region) {
            MinX = region.X;
            MaxX = region.X + region.W;
            MinY = region.Y;
            MaxY = region.Y + region.H;
        }

        public static Boundary FromFrameworkElement(FrameworkElement element) {
            Boundary newBoundary = new Boundary();
            newBoundary.MaxX = element.ActualWidth;
            newBoundary.MaxY = element.ActualHeight;
            newBoundary.MinX = 0;
            newBoundary.MinY = 0;
            return newBoundary;
        }

        public static Boundary Universe = new Boundary()
        {
            MinX = double.NegativeInfinity,
            MaxX = double.PositiveInfinity,
            MinY = double.NegativeInfinity,
            MaxY = double.PositiveInfinity,
        };
    }
}
