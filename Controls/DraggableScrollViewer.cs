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
            scrollStartOffset.X = this.HorizontalOffset;
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

                this.ScrollToHorizontalOffset(scrollStartOffset.X + delta.X);
                this.ScrollToVerticalOffset(scrollStartOffset.Y + delta.Y);
                log.DebugFormat("Scroller Delta {0} / {1}", delta.X, delta.Y);
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
