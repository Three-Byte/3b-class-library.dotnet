using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Controls;
using System.Windows;
using System.Windows.Input;
using System.Windows.Documents;
using log4net;
using ThreeByte.Media;


namespace ThreeByte.Controls
{
    public class DesignerItemsControl : ListBox
    {
        private static readonly ILog log = LogManager.GetLogger(typeof(DesignerItemsControl));

        public DesignerItemsControl() {

            DefaultStyleKey = typeof(DesignerItemsControl);

            this.KeyDown += new KeyEventHandler(DesignerItemsControl_KeyDown);
            //FocusManager.SetIsFocusScope(this, true);
            //this.Focusable = true;
            //this.PreviewMouseLeftButtonDown += new MouseButtonEventHandler(DesignerItemsControl_PreviewMouseLeftButtonDown);

            //this.PreviewMouseLeftButtonUp += new MouseButtonEventHandler(DesignerItemsControl_PreviewMouseLeftButtonUp);
            //this.MouseLeave += new MouseEventHandler(DesignerItemsControl_MouseLeave);
            //this.MouseMove += new MouseEventHandler(DesignerItemsControl_MouseMove);
        }

        public event EventHandler<ItemDeletedEventArgs> ItemDeleted;

        void DesignerItemsControl_KeyDown(object sender, KeyEventArgs e) {
            //See if something is selected
            if(e.Key == Key.Delete ||
                e.Key == Key.Back) {
                if(SelectedItem != null) {
                    object itemToDelete = SelectedItem;
                    Deselect();
                    if(ItemDeleted != null) {
                        ItemDeleted(this, new ItemDeletedEventArgs(itemToDelete));
                    }
                }
            } else if(e.Key == Key.Escape) {
                Deselect();
            }
        }

        private void Deselect() {
            SelectedItems.Clear();
        }

        //private AdornerLayer adornerLayer;

        //private bool _mouseIsDown;
        //private bool _isDragging;

        //public FrameworkElement SelectedItem;

        ////Drag testing
        //private Point _mouseStartPoint;
        //private Point _elementStartPosition;

        private Boundary _bounds;
        public Boundary Bounds {
            get {
                if(_bounds != null) {
                    return _bounds;
                }
                //Else construct bounds based on current size
                return new Boundary() { MinX = 0, MinY = 0, MaxX = ActualWidth, MaxY = ActualHeight };
            }
            set {
                _bounds = value;
            }
        }

        #region Attached Properties

        public static readonly DependencyProperty LockedProperty = DependencyProperty.RegisterAttached("Locked",
            typeof(bool), typeof(DesignerItemsControl), new UIPropertyMetadata(false));

        public static bool GetLocked(UIElement uiElement) {
            if(uiElement == null) {
                return false;
            }
            return (bool)(uiElement.GetValue(LockedProperty));
        }

        public static void SetLocked(UIElement uiElement, bool value) {
            if(uiElement != null) {
                uiElement.SetValue(LockedProperty, value);
            }
        }


        #endregion Attached Properties


        //protected override bool IsItemItsOwnContainerOverride(object item) {
        //    return (item is ListBoxItem);
        //}

        //protected override DependencyObject GetContainerForItemOverride() {
        //    return new ListBoxItem();
        //    //return new DesignerItem();
        //}

        //void DesignerItemsControl_MouseMove(object sender, MouseEventArgs e) {

        //    if(_mouseIsDown) {
        //        log.Debug("Mouse Down Move");
        //        if((_isDragging == false) &&
        //            ((Math.Abs(e.GetPosition(this).X - _mouseStartPoint.X) > SystemParameters.MinimumHorizontalDragDistance) ||
        //            (Math.Abs(e.GetPosition(this).Y - _mouseStartPoint.Y) > SystemParameters.MinimumVerticalDragDistance))) {
        //            log.Debug("Set Dragging = true");
        //            _isDragging = true;
        //        }

        //        if(_isDragging && SelectedItem != null) {

        //            Point newPosition = Mouse.GetPosition(this);
        //            Boundary bounds = Boundary.FromFrameworkElement(this);

        //            double newTop = Math.Max(0, Math.Min(ActualHeight, newPosition.Y - (_mouseStartPoint.Y - _elementStartPosition.Y)));
        //            double newLeft = Math.Max(0, Math.Min(ActualWidth, newPosition.X - (_mouseStartPoint.X - _elementStartPosition.X)));
        //            log.DebugFormat("Drag move: Left: {0} Top: {1}", newLeft, newTop);
        //            newTop = ClampTop(SelectedItem, bounds, newTop);
        //            newLeft = ClampLeft(SelectedItem, bounds, newLeft);

        //            Canvas.SetTop(SelectedItem, newTop);
        //            Canvas.SetLeft(SelectedItem, newLeft);
        //        }

        //    }
        //}

        //void DesignerItemsControl_MouseLeave(object sender, MouseEventArgs e) {
        //    StopDragging();
        //    e.Handled = true;
        //}

        //void DesignerItemsControl_PreviewMouseLeftButtonUp(object sender, MouseButtonEventArgs e) {
        //    StopDragging();
        //    e.Handled = true;
        //}

        //private void StopDragging() {
        //    if(_mouseIsDown) {
        //        log.Debug("Stop Dragging");
        //        _mouseIsDown = false;
        //        _isDragging = false;
        //    }
        //}

        //void DesignerItemsControl_PreviewMouseLeftButtonDown(object sender, MouseButtonEventArgs e) {
        //    log.Debug("Designer Set Focus: " + this.Focus());

        //    Deselect();

        //    //If an element other than the canvas is clicked and is not locked, then add
        //    //adorner to allow it to be resized/repositioned
        //    if(e.Source != this && !(GetLocked(e.Source as UIElement))) {
        //        _mouseIsDown = true;
        //        _mouseStartPoint = e.GetPosition(this);

        //        SelectedItem = e.Source as FrameworkElement;
        //        log.Debug("Selected Item: " + SelectedItem);

        //        _elementStartPosition = new Point(Canvas.GetLeft(SelectedItem), Canvas.GetTop(SelectedItem));

        //        adornerLayer = AdornerLayer.GetAdornerLayer(SelectedItem);
        //        //Create new Boundary
        //        adornerLayer.Add(new ResizingAdorner(SelectedItem, Bounds));
        //        //_isSelected = true;
        //        e.Handled = true;
        //    }
        //}

        //private void Deselect() {
        //    if(SelectedItem != null) {
        //        foreach(Adorner a in adornerLayer.GetAdorners(SelectedItem)) {
        //            adornerLayer.Remove(a);
        //        }
        //        SelectedItem = null;
        //    }
        //}


        //#region Boundary Clamping
        //public static double ClampTop(FrameworkElement uiElement, Boundary boundary, double top) {
        //    if(top <= boundary.MinY) {
        //        return boundary.MinY;
        //    } else if(top + uiElement.Height >= boundary.MaxY) {
        //        return boundary.MaxY - uiElement.Height;
        //    }
        //    //otherwise
        //    return top;
        //}

        //public static double ClampLeft(FrameworkElement uiElement, Boundary boundary, double left) {
        //    if(left <= boundary.MinX) {
        //        return boundary.MinX;
        //    } else if(left + uiElement.Width >= boundary.MaxX) {
        //        return boundary.MaxX - uiElement.Width;
        //    }
        //    //otherwise
        //    return left;
        //}

        //public static double ClampWidth(FrameworkElement uiElement, Boundary boundary, double width) {
        //    double left = Canvas.GetLeft(uiElement);

        //    if(width <= 0) {
        //        return 0;
        //    } else if(width + left >= boundary.MaxX) {
        //        return boundary.MaxX - left;
        //    }
        //    //otherwise
        //    return width;
        //}

        //public static double ClampHeight(FrameworkElement uiElement, Boundary boundary, double height) {
        //    double top = Canvas.GetTop(uiElement);

        //    if(height <= 0) {
        //        return 0;
        //    } else if(height + top >= boundary.MaxY) {
        //        return boundary.MaxY - top;
        //    }
        //    //otherwise
        //    return height;
        //}
        //#endregion //Boundary Clamping

    }
}
