using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Media.Animation;

namespace ThreeByte.DMX
{
    public class DmxAnimation : DmxAnimationBase
    {

        protected override Freezable CreateInstanceCore() {
            return new DmxAnimation();
        }

        public static readonly DependencyProperty FromProperty = DependencyProperty.Register("From", typeof(Dictionary<int, int>), typeof(DmxAnimation));

        public Dictionary<int, int> From {
            get { return (Dictionary<int, int>)GetValue(FromProperty); }
            set { SetValue(FromProperty, value); }
        }

        public static readonly DependencyProperty ToProperty = DependencyProperty.Register("To", typeof(Dictionary<int, int>), typeof(DmxAnimation));

        public Dictionary<int, int> To {
            get { return (Dictionary<int, int>)GetValue(ToProperty); }
            set { SetValue(ToProperty, value); }
        }


        //Allocated once to prevent memory allocation on each calculation
        Dictionary<int, int> returnValue1 = new Dictionary<int, int>();
        Dictionary<int, int> returnValue2 = new Dictionary<int, int>();
        bool flip = false;

        private Dictionary<int, int> fromValues;
        private Dictionary<int, int> toValues;
        private HashSet<int> channelsSet = new HashSet<int>();

        protected override Dictionary<int, int> GetCurrentValueCore(Dictionary<int, int> defaultOriginValue,
                                                                    Dictionary<int, int> defaultDestinationValue,
                                                                    AnimationClock animationClock) {

            if(animationClock.CurrentProgress == null){
                return null;
            }

            double progress = animationClock.CurrentProgress.Value;
            Dictionary<int, int> returnValue = flip ? returnValue1 : returnValue2;
            flip = !flip;
            returnValue.Clear();

            //Choose the correct From/To values
            fromValues = From ?? defaultOriginValue;
            toValues = To ?? defaultDestinationValue;

            channelsSet.Clear();

            //Interpolate the values
            foreach(int c in fromValues.Keys) {
                int newValue = fromValues[c];
                if(toValues.ContainsKey(c)) {
                    //Basic linear interpolation
                    newValue = (int)Math.Round(((1.0 - progress) * fromValues[c]) + (progress * toValues[c]));
                } else {
                    //This value doesn't change
                }
                returnValue[c] = newValue;
                channelsSet.Add(c);
            }

            foreach(int c in toValues.Keys) {
                if(!channelsSet.Contains(c)) {
                    returnValue[c] = (int)Math.Round(progress * toValues[c]);
                }
                channelsSet.Add(c);
            }

            return returnValue;
        }

        public override bool IsDestinationDefault {
            get {
                return base.IsDestinationDefault;
            }
        }
    }
}
