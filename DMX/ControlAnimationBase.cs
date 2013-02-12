using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Media.Animation;

namespace ThreeByte.DMX
{
    public abstract class ControlAnimationBase : AnimationTimeline
    {

        public override Type TargetPropertyType {
            get { return typeof(bool); }
        }

        public sealed override object GetCurrentValue(object defaultOriginValue, object defaultDestinationValue, AnimationClock animationClock) {
            //Validate default origin and destination types
            return GetCurrentValueCore(defaultOriginValue as bool?, defaultDestinationValue as bool?, animationClock);
        }

        protected abstract bool GetCurrentValueCore(bool? defaultOriginValue, bool? defaultDestinationValue, AnimationClock animationClock);

    }
}
