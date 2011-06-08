using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Controls;
using System.Windows.Media.Imaging;
using System.Windows.Input;
using System.Windows;
using log4net;

//****
// With insight from Susan Warren: http://blogs.vertigo.com/personal/swarren/Blog/Lists/Posts/Post.aspx?ID=7
//****
namespace ThreeByte.Controls
{
    public class DraggableScrollViewer : ScrollViewer
    {
        private static readonly ILog log = LogManager.GetLogger(typeof(DraggableScrollViewer));

        public static readonly DependencyProperty OffsetXProperty = DependencyProperty.Register("OffsetX",
            typeof(int), typeof(DraggableScrollViewer), new FrameworkPropertyMetadata(0, FrameworkPropertyMetadataOptions.BindsTwoWayByDefault, OffsetXChanged));

        public int OffsetX {
            get {
                return (int)GetValue(OffsetXProperty);
            }
            set {
                SetValue(OffsetXProperty, value);
            }
        }

        public static void OffsetXChanged(DependencyObject obj, DependencyPropertyChangedEventArgs e) {
            DraggableScrollViewer viewer = (DraggableScrollViewer)obj;
            viewer.ScrollToHorizontalOffset((int)(e.NewValue));
        }

        public static readonly DependencyProperty OffsetYProperty = DependencyProperty.Register("OffsetY",
            typeof(int), typeof(DraggableScrollViewer), new FrameworkPropertyMetadata(0, FrameworkPropertyMetadataOptions.BindsTwoWayByDefault, OffsetYChanged));

        public int OffsetY {
            get {
                return (int)GetValue(OffsetYProperty);
            }
            set {
                SetValue(OffsetYProperty, value);
            }
        }

        public static void OffsetYChanged(DependencyObject obj, DependencyPropertyChangedEventArgs e) {
            DraggableScrollViewer viewer = (DraggableScrollViewer)obj;
            viewer.ScrollToVerticalOffset((int)(e.NewValue));
        }

        public static readonly DependencyProperty IsLockedToViewerProperty = DependencyProperty.RegisterAttached(
                                                                        "IsLockedToViewer",
                                                                        typeof(bool),
                                                                        typeof(DraggableScrollViewer),
                                                                        new FrameworkPropertyMetadata(false, FrameworkPropertyMetadataOptions.AffectsRender, IsLockedToViewerChanged));

        public static void IsLockedToViewerChanged(DependencyObject obj, DependencyPropertyChangedEventArgs e) {
            //FrameworkElement element = (FrameworkElement)obj;
            //if(GetIsInteractive(element)) {
            //    Binding visibilityBinding = new Binding("LayerVisibility");
            //    visibilityBinding.Source = Instance;
            //    element.SetBinding(FrameworkElement.VisibilityProperty, visibilityBinding);
            //} else {
            //    BindingOperations.ClearBinding(element, FrameworkElement.VisibilityProperty);
            //}
        }

        public static void SetIsLockedToViewer(FrameworkElement element, Boolean value) {
            element.SetValue(IsLockedToViewerProperty, value);
        }

        public static bool GetIsLockedToViewer(FrameworkElement element) {
            return (bool)element.GetValue(IsLockedToViewerProperty);
        }




        public DraggableScrollViewer() {

            this.HorizontalScrollBarVisibility = ScrollBarVisibility.Hidden;
            this.VerticalScrollBarVisibility = ScrollBarVisibility.Hidden;

            this.PreviewMouseLeftButtonDown += new MouseButtonEventHandler(DraggableScrollViewer_PreviewMouseLeftButtonDown);
            this.PreviewMouseMove += new MouseEventHandler(DraggableScrollViewer_PreviewMouseMove);
            this.PreviewMouseLeftButtonUp += new MouseButtonEventHandler(DraggableScrollViewer_PreviewMouseLeftButtonUp);
        }

        private Point mouseDownPoint;
        private Point scrollStartOffset;

        void DraggableScrollViewer_PreviewMouseLeftButtonDown(object sender, MouseButtonEventArgs e) {
            
            if((e.OriginalSource != this) && !GetIsLockedToViewer(e.Source as FrameworkElement)) {
                log.Debug("Mouse not clicked on the scroll viewer");
                return;
            }
            mouseDownPoint = e.GetPosition(this);
            scrollStartOffset.X = this.OffsetX;
            scrollStartOffset.Y = this.VerticalOffset;

            if((this.ExtentWidth > this.ViewportWidth) || (this.ExtentHeight > this.ViewportHeight)) {
                this.Cursor = Cursors.ScrollAll;
            } else {
                this.Cursor = null;
            }

            bool captured = this.CaptureMouse();
            log.DebugFormat("DraggableScrollViewer MouseCaptured: {0}", captured);
        }

        void DraggableScrollViewer_PreviewMouseMove(object sender, MouseEventArgs e) {

            if(this.IsMouseCaptured) {

                Point currentMousePosition = e.GetPosition(this);

                Point delta = new Point(mouseDownPoint.X - currentMousePosition.X,
                                        mouseDownPoint.Y - currentMousePosition.Y);

                OffsetX = (int)(Math.Round(scrollStartOffset.X + delta.X));
                OffsetY = (int)(Math.Round(scrollStartOffset.Y + delta.Y));

                //this.ScrollToHorizontalOffset(scrollStartOffset.X + delta.X);
                //this.ScrollToVerticalOffset(scrollStartOffset.Y + delta.Y);
                //log.DebugFormat("Scroller Delta {0} / {1}", delta.X, delta.Y);
            } 

            //Console.WriteLine("Scroller Extents {0} / {1}", this.ExtentHeight, this.ExtentWidth);
            //Console.WriteLine("Scroller Viewport {0} / {1}", this.ViewportHeight, this.ViewportWidth);
        }

        void DraggableScrollViewer_PreviewMouseLeftButtonUp(object sender, MouseButtonEventArgs e) {
            if(this.IsMouseCaptured) {
                this.Cursor = null;
                this.ReleaseMouseCapture();
            }
        }

    }
}
