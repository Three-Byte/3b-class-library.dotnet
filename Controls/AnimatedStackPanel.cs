using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Animation;

namespace ThreeByte.Controls
{
    public class AnimatedStackPanel : Panel
    {
        public TimeSpan AnimationDuration { get; set; }

        public AnimatedStackPanel() {
            AnimationDuration = TimeSpan.FromMilliseconds(500);  //Reasonable Default
        }

        protected override Size MeasureOverride(Size availableSize) {
            Size resultSize = new Size(0, 0);

            foreach(UIElement child in Children) {
                child.Measure(availableSize);
                resultSize.Width = Math.Max(resultSize.Width, child.DesiredSize.Width);
                resultSize.Height += child.DesiredSize.Height;
            }

            resultSize.Width = double.IsPositiveInfinity(availableSize.Width) ?
                resultSize.Width : availableSize.Width;

            resultSize.Height = double.IsPositiveInfinity(availableSize.Height) ?
                resultSize.Height : availableSize.Height;

            return resultSize;
        }

        private HashSet<UIElement> previousChildren = new HashSet<UIElement>();

        protected override Size ArrangeOverride(Size finalSize) {

            double curY = 0;
            TranslateTransform trans = null;
            HashSet<UIElement> currentChildren = new HashSet<UIElement>();

            foreach(UIElement child in Children) {

                trans = child.RenderTransform as TranslateTransform;
                if(trans == null) {
                    child.RenderTransformOrigin = new Point(0, 0);
                    trans = new TranslateTransform();
                    child.RenderTransform = trans;
                }

                if(!(previousChildren.Contains(child))) {
                    //Animate the opacity property
                    child.Arrange(new Rect(0, 0, finalSize.Width, child.DesiredSize.Height));
                    trans.BeginAnimation(TranslateTransform.YProperty, new DoubleAnimation(curY, TimeSpan.FromMilliseconds(0)), HandoffBehavior.Compose);
                    child.BeginAnimation(UIElement.OpacityProperty, new DoubleAnimation(0, 1, AnimationDuration), HandoffBehavior.Compose);
                } else {
                    child.Arrange(new Rect(0, 0, finalSize.Width, child.DesiredSize.Height));
                    trans.BeginAnimation(TranslateTransform.YProperty, new DoubleAnimation(curY, AnimationDuration), HandoffBehavior.Compose);
                }
                currentChildren.Add(child);

                curY += child.DesiredSize.Height;
            }

            previousChildren = currentChildren;

            return finalSize;
        }
    }
}
