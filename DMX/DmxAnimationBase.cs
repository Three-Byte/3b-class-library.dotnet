using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Media.Animation;

namespace ThreeByte.DMX
{
    public abstract class DmxAnimationBase : AnimationTimeline
    {

        public override Type TargetPropertyType {
            get { return typeof(Dictionary<int, int>); }
        }

        public sealed override object GetCurrentValue(object defaultOriginValue, object defaultDestinationValue, AnimationClock animationClock) {
            //Validate default origin and destination types
            return GetCurrentValueCore(defaultOriginValue as Dictionary<int, int>, defaultDestinationValue as Dictionary<int, int>, animationClock);
        }

        protected abstract Dictionary<int, int> GetCurrentValueCore(Dictionary<int, int> defaultOriginValue, Dictionary<int, int> defaultDestinationValue, AnimationClock animationClock);

    }
}
