using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml.Linq;

namespace ThreeByte.Media
{
    public class PixelSpace
    {
        public int X;
        public int Y;
        public int W;
        public int H;

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
