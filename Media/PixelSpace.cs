using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml.Linq;
using System.ComponentModel;

namespace ThreeByte.Media
{
    public class PixelSpace : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;

        private void NotifyPropertyChanged(string propertyName) {
            if(PropertyChanged != null) {
                PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
            }
        }

        private int _x;
        public int X {
            get {
                return _x;
            }
            set {
                _x = value;
                NotifyPropertyChanged("X");
            }
        }

        private int _y;
        public int Y {
            get {
                return _y;
            }
            set {
                _y = value;
                NotifyPropertyChanged("Y");
            }
        }

        private int _w;
        public int W {
            get {
                return _w;
            }
            set {
                _w = value;
                NotifyPropertyChanged("W");
            }
        }

        private int _h;
        public int H {
            get {
                return _h;
            }
            set {
                _h = value;
                NotifyPropertyChanged("H");
            }
        }

        public PixelSpace() { }

        public PixelSpace(PixelSpace other) {
            X = other.X;
            Y = other.Y;
            W = other.W;
            H = other.H;
        }

        public XElement ToXml() {
            XElement node = new XElement("PixelSpace");
            node.Add(new XAttribute("Left", X)); 
            node.Add(new XAttribute("Top", Y));
            node.Add(new XAttribute("Width", W)); 
            node.Add(new XAttribute("Height", H));

            return node;
        }

        public override bool Equals(object obj) {
            PixelSpace other = obj as PixelSpace;
            if(other == null) {
                return false;
            }

            return (this.X == other.X
                    && this.Y == other.Y
                    && this.W == other.W
                    && this.H == other.H);
        }

        public override int GetHashCode() {
            return X + Y + W + H;
        }

        public override string ToString() {
            return ToXml().ToString();
        }

        public static PixelSpace GetContainingSpace(PixelSpace a, PixelSpace b) {
            PixelSpace container = new PixelSpace(a);

            container.X = Math.Min(a.X, b.X);
            container.Y = Math.Min(a.Y, b.Y);

            container.W = Math.Max(a.X + a.W, b.X + b.W) - container.X;
            container.H = Math.Max(a.Y + a.H, b.Y + b.H) - container.Y;

            return container;
        }

        public static double ClampTop(PixelSpace pos, Boundary boundary, double top) {
            if(top <= boundary.MinY) {
                return boundary.MinY;
            } else if(top + pos.H >= boundary.MaxY) {
                return boundary.MaxY - pos.H;
            }
            //otherwise
            return top;
        }

        public static double ClampLeft(PixelSpace pos, Boundary boundary, double left) {
            if(left <= boundary.MinX) {
                return boundary.MinX;
            } else if(left + pos.W >= boundary.MaxX) {
                return boundary.MaxX - pos.W;
            }
            //otherwise
            return left;
        }

        public static double ClampWidth(PixelSpace pos, Boundary boundary, double width) {
            if(width <= 0) {
                return 0;
            } else if(width + pos.X >= boundary.MaxX) {
                return boundary.MaxX - pos.X;
            }
            //otherwise
            return width;
        }

        public static double ClampHeight(PixelSpace pos, Boundary boundary, double height) {
            if(height <= 0) {
                return 0;
            } else if(height + pos.Y >= boundary.MaxY) {
                return boundary.MaxY - pos.Y;
            }
            //otherwise
            return height;
        }

    }

    public class PixelScaler
    {
        public double XScale;
        public double YScale;

        public PixelSpace Scale(PixelSpace input) {
            return new PixelSpace() {
                X = (int)Math.Round(input.X * XScale, MidpointRounding.ToEven),
                Y = (int)Math.Round(input.Y * YScale, MidpointRounding.ToEven),
                W = (int)Math.Round(input.W * XScale, MidpointRounding.ToEven),
                H = (int)Math.Round(input.H * YScale, MidpointRounding.ToEven)
            };
        }

        public PixelSpace ScaleBack(PixelSpace input) {
            return new PixelSpace() {
                X = (int)Math.Round(input.X / XScale, MidpointRounding.ToEven),
                Y = (int)Math.Round(input.Y / YScale, MidpointRounding.ToEven),
                W = (int)Math.Round(input.W / XScale, MidpointRounding.ToEven),
                H = (int)Math.Round(input.H / YScale, MidpointRounding.ToEven)
            };
        }
    }

    public class PixelTranslator
    {
        public int XOffset;
        public int YOffset;

        public PixelSpace Translate(PixelSpace input) {
            return new PixelSpace() {
                X = input.X + XOffset,
                Y = input.Y + YOffset,
                W = input.W,
                H = input.H
            };
        }

        public PixelSpace TranslateBack(PixelSpace input) {
            return new PixelSpace() {
                X = input.X - XOffset,
                Y = input.Y - YOffset,
                W = input.W,
                H = input.H
            };
        }
    }


}
