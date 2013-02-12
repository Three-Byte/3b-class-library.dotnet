using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Media.Animation;

namespace ThreeByte.DMX {
    public class LoopPointAnimation : ControlAnimationBase {
        protected override Freezable CreateInstanceCore() {
            return new LoopPointAnimation();
        }

        public static readonly DependencyProperty FromProperty = DependencyProperty.Register("From", typeof(bool), typeof(LoopPointAnimation));

        public bool From {
            get { return (bool)GetValue(FromProperty); }
            set { SetValue(FromProperty, value); }
        }

        public static readonly DependencyProperty ToProperty = DependencyProperty.Register("To", typeof(bool), typeof(LoopPointAnimation));

        public bool To {
            get { return (bool)GetValue(ToProperty); }
            set { SetValue(ToProperty, value); }
        }

        protected override bool GetCurrentValueCore(bool? defaultOriginValue,
                                                    bool? defaultDestinationValue,
                                                    AnimationClock animationClock) {
            return true;
        }




        public override bool IsDestinationDefault {
            get {
                return base.IsDestinationDefault;
            }
        }
    }
 
}
