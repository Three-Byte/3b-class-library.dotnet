using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Documents;
using System.Windows.Controls.Primitives;
using System.Windows.Media;
using System.Windows;
using System.Windows.Input;
using System.Windows.Controls;
using System.Windows.Shapes;

namespace ThreeByte.Controls
{
    public class ResizingAdorner : Adorner
    {
        private Thumb topLeft, topRight, bottomLeft, bottomRight;
        private Rectangle overlay;

        VisualCollection visualChildren;

        public ResizingAdorner(UIElement adornedElement)
            : base(adornedElement) {

            visualChildren = new VisualCollection(this);

            topLeft = BuildAdorner(Cursors.SizeNWSE);
            topRight = BuildAdorner(Cursors.SizeNESW);
            bottomLeft = BuildAdorner(Cursors.SizeNESW);
            bottomRight = BuildAdorner(Cursors.SizeNWSE);

            topLeft.DragDelta += new DragDeltaEventHandler(topLeft_DragDelta);
            topRight.DragDelta += new DragDeltaEventHandler(topRight_DragDelta);
            bottomLeft.DragDelta += new DragDeltaEventHandler(bottomLeft_DragDelta);
            bottomRight.DragDelta += new DragDeltaEventHandler(bottomRight_DragDelta);

            overlay = new Rectangle();
            overlay.IsHitTestVisible = false;
            overlay.Fill = new SolidColorBrush(Color.FromArgb(50, 255, 0, 255));
            visualChildren.Add(overlay);

            this.Cursor = Cursors.SizeAll;//TODO: does this look right?
        }

        void topLeft_DragDelta(object sender, DragDeltaEventArgs e) {
            FrameworkElement adornedElement = AdornedElement as FrameworkElement;
            Thumb thisThumb = sender as Thumb;

            if(adornedElement == null || thisThumb == null) {
                return;
            }

            ConformSize(adornedElement);

            double oldWidth = adornedElement.Width;
            double newWidth = Math.Max(adornedElement.Width - e.HorizontalChange, thisThumb.DesiredSize.Width);
            newWidth = DesignerCanvas.ClampWidth(adornedElement, DesignerCanvas.DefaultScreen, newWidth);

            double oldHeight = adornedElement.Height;
            double newHeight = Math.Max(adornedElement.Height - e.VerticalChange, thisThumb.DesiredSize.Height);
            newHeight = DesignerCanvas.ClampHeight(adornedElement, DesignerCanvas.DefaultScreen, newHeight);

            adornedElement.Width = newWidth;
            adornedElement.Height = newHeight;

            double oldLeft = Canvas.GetLeft(adornedElement);
            double newLeft = oldLeft - (newWidth - oldWidth);
            newLeft = DesignerCanvas.ClampLeft(adornedElement, DesignerCanvas.DefaultScreen, newLeft);
            Canvas.SetLeft(adornedElement, newLeft);

            double oldTop = Canvas.GetTop(adornedElement);
            double newTop = oldTop - (newHeight - oldHeight);
            newTop = DesignerCanvas.ClampTop(adornedElement, DesignerCanvas.DefaultScreen, newTop);
            Canvas.SetTop(adornedElement, newTop);
        }

        void topRight_DragDelta(object sender, DragDeltaEventArgs e) {
            FrameworkElement adornedElement = AdornedElement as FrameworkElement;
            Thumb thisThumb = sender as Thumb;

            if(adornedElement == null || thisThumb == null) {
                return;
            }

            ConformSize(adornedElement);

            double oldWidth = adornedElement.Width;
            double newWidth = Math.Max(adornedElement.Width + e.HorizontalChange, thisThumb.DesiredSize.Width);
            newWidth = DesignerCanvas.ClampWidth(adornedElement, DesignerCanvas.DefaultScreen, newWidth);

            double oldHeight = adornedElement.Height;
            double newHeight = Math.Max(adornedElement.Height - e.VerticalChange, thisThumb.DesiredSize.Height);
            newHeight = DesignerCanvas.ClampHeight(adornedElement, DesignerCanvas.DefaultScreen, newHeight);

            adornedElement.Width = newWidth;
            adornedElement.Height = newHeight;

            double oldTop = Canvas.GetTop(adornedElement);
            double newTop = oldTop - (newHeight - oldHeight);
            newTop = DesignerCanvas.ClampTop(adornedElement, DesignerCanvas.DefaultScreen, newTop);
            Canvas.SetTop(adornedElement, newTop);
        }
        
        void bottomLeft_DragDelta(object sender, DragDeltaEventArgs e) {
            FrameworkElement adornedElement = AdornedElement as FrameworkElement;
            Thumb thisThumb = sender as Thumb;

            if(adornedElement == null || thisThumb == null) {
                return;
            }

            ConformSize(adornedElement);

            double oldWidth = adornedElement.Width;
            double newWidth = Math.Max(adornedElement.Width - e.HorizontalChange, thisThumb.DesiredSize.Width);
            newWidth = DesignerCanvas.ClampWidth(adornedElement, DesignerCanvas.DefaultScreen, newWidth);

            double oldHeight = adornedElement.Height;
            double newHeight = Math.Max(adornedElement.Height + e.VerticalChange, thisThumb.DesiredSize.Height);
            newHeight = DesignerCanvas.ClampHeight(adornedElement, DesignerCanvas.DefaultScreen, newHeight);

            adornedElement.Width = newWidth;
            adornedElement.Height = newHeight;

            double oldLeft = Canvas.GetLeft(adornedElement);
            double newLeft = oldLeft - (newWidth - oldWidth);
            newLeft = DesignerCanvas.ClampLeft(adornedElement, DesignerCanvas.DefaultScreen, newLeft);
            Canvas.SetLeft(adornedElement, newLeft);
        }

        void bottomRight_DragDelta(object sender, DragDeltaEventArgs e) {
            FrameworkElement adornedElement = AdornedElement as FrameworkElement;
            Thumb thisThumb = sender as Thumb;

            if(adornedElement == null || thisThumb == null) {
                return;
            }

            ConformSize(adornedElement);

            double oldWidth = adornedElement.Width;
            double newWidth = Math.Max(adornedElement.Width + e.HorizontalChange, thisThumb.DesiredSize.Width);
            newWidth = DesignerCanvas.ClampWidth(adornedElement, DesignerCanvas.DefaultScreen, newWidth);

            double oldHeight = adornedElement.Height;
            double newHeight = Math.Max(adornedElement.Height + e.VerticalChange, thisThumb.DesiredSize.Height);
            newHeight = DesignerCanvas.ClampHeight(adornedElement, DesignerCanvas.DefaultScreen, newHeight);

            adornedElement.Width = newWidth;
            adornedElement.Height = newHeight;
        }

        private Thumb BuildAdorner(Cursor cursor) {

            Thumb newThumb = new Thumb();

            newThumb.Cursor = cursor;
            newThumb.Height = newThumb.Width = 10;
            newThumb.Opacity = .5;
            newThumb.Background = new SolidColorBrush(Colors.LightBlue);
            newThumb.BorderBrush = new SolidColorBrush(Colors.MediumBlue);

            visualChildren.Add(newThumb);

            return newThumb;
        }

        private void ConformSize(FrameworkElement adornedElement) {
            if(adornedElement.Width.Equals(Double.NaN)) {
                adornedElement.Width = adornedElement.DesiredSize.Width;
            }
            if(adornedElement.Height.Equals(Double.NaN)) {
                adornedElement.Height = adornedElement.DesiredSize.Height;
            }

            FrameworkElement parent = adornedElement.Parent as FrameworkElement;
            if(parent != null) {
                adornedElement.MaxHeight = parent.ActualHeight;
                adornedElement.MaxWidth = parent.ActualWidth;
            }

        }


        //Layout and rendering overrides

        protected override Size ArrangeOverride(Size finalSize) {

            double desiredWidth = AdornedElement.DesiredSize.Width;
            double desiredHeight = AdornedElement.DesiredSize.Height;
            // adornerWidth & adornerHeight are used for placement as well.
            double adornerWidth = this.DesiredSize.Width;
            double adornerHeight = this.DesiredSize.Height;

            topLeft.Arrange(new Rect(-adornerWidth / 2, -adornerHeight / 2, adornerWidth, adornerHeight));
            topRight.Arrange(new Rect(desiredWidth - adornerWidth / 2, -adornerHeight / 2, adornerWidth, adornerHeight));
            bottomLeft.Arrange(new Rect(-adornerWidth / 2, desiredHeight - adornerHeight / 2, adornerWidth, adornerHeight));
            bottomRight.Arrange(new Rect(desiredWidth - adornerWidth / 2, desiredHeight - adornerHeight / 2, adornerWidth, adornerHeight));

            overlay.Arrange(new Rect(0, 0, desiredWidth, desiredHeight));

            //return finalSize;
            return base.ArrangeOverride(finalSize);
        }

        protected override int VisualChildrenCount { get { return visualChildren.Count; } }
        protected override Visual GetVisualChild(int index) { return visualChildren[index]; }

    }
}
