using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Media.Animation;

namespace ThreeByte.DMX {
    public class DmxAnimation : DmxAnimationBase {
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

        ///For every channel key, we get the amount of channels that are affected in parallel as defined 
        ///by the encoder page mappings.
        public void SetGroupIdxChannels(Dictionary<int, List<int>> value) {
            channelToGroupIdx.Clear();
            groupIdxToChannelGroup.Clear();
            foreach(int groupIdx in value.Keys) {
                foreach(int channel in value[groupIdx]) {
                    channelToGroupIdx[channel] = groupIdx;
                }
                groupIdxToChannelGroup[groupIdx] = new GroupCounter() { GroupSize = value[groupIdx].Count, SeenSoFar = 0 };
            }
        }

        private static Dictionary<int, int> channelToGroupIdx = new Dictionary<int, int>();
        private static Dictionary<int, GroupCounter> groupIdxToChannelGroup = new Dictionary<int, GroupCounter>();

        ///We need a dictionary that maps from a channel to a 
        ///Dictionary<GroupIdx, List<Channel>>
        ///Dictionary<channel, groupIDX>
        ///Dictionary<groupIDX, channelGroup>
        private Dictionary<int, int> fromValues;
        private Dictionary<int, int> toValues;
        private HashSet<int> channelsSet = new HashSet<int>();

        public static bool TemporalOffset { get; set; }

        private Dictionary<int, int> temporalOffest(Dictionary<int, int> defaultOriginValue,
                                                                    Dictionary<int, int> defaultDestinationValue,
                                                                    AnimationClock animationClock) {
            if((From == null || From.Count() == 0) && (To == null || To.Count() == 0)) {
                return new Dictionary<int, int>();
            }
            if(animationClock.CurrentProgress == null) {
                return null;
            }

            Dictionary<int, int> returnValue = flip ? returnValue1 : returnValue2;

            flip = !flip;
            returnValue.Clear();

            double progress = animationClock.CurrentProgress.Value;
            //Choose the correct From/To values
            fromValues = From ?? defaultOriginValue;
            toValues = To ?? defaultDestinationValue;

            if(progress == 1.0) {
                return toValues;
            }


            channelsSet.Clear();

            foreach(int groupIdx in groupIdxToChannelGroup.Keys) {
                groupIdxToChannelGroup[groupIdx].SeenSoFar = 0;
            }

            //Interpolate the values
            foreach(int c in fromValues.Keys) {
                int newValue = fromValues[c];
                if(toValues.ContainsKey(c) && channelToGroupIdx.ContainsKey(c) && toValues[c] != newValue) {
                    //This handles the case of a channel with a toValue and a fromValue
                    int toValue = toValues[c];

                    int groupIdx = channelToGroupIdx[c];
                    //UnroundedNewVals is rounded to three decimal places to handle the problem of numerical instability 
                    //resulting in numbers like .0000001 which gets blown up in the Math.Ceiling call
                    double unroundedNewVal = Math.Round(((1.0 - progress) * fromValues[c]) + (progress * toValues[c]), 3);

                    double roundedNewVal = Math.Floor(unroundedNewVal);

                    double decimalComponent = unroundedNewVal - roundedNewVal;
                    double threshold = groupIdxToChannelGroup[groupIdx].SeenSoFar++ / groupIdxToChannelGroup[groupIdx].GroupSize;
                    if(decimalComponent > threshold) {
                        newValue = (int)Math.Ceiling(unroundedNewVal);
                    } else {
                        newValue = (int)roundedNewVal;
                    }
                    if(newValue > 65535) {
                        newValue = 65535;
                    }
                }

                //This handles the case of a channel with a fromValue but not a toValue
                returnValue[c] = newValue;
                channelsSet.Add(c);
            }

            foreach(int c in toValues.Keys) {
                if(!channelsSet.Contains(c)) {
                    if(!channelToGroupIdx.ContainsKey(c)) {
                        continue;
                    }
                    //This handles the case of a toValue without a fromValue

                    int toValue = toValues[c];
                    int groupIdx = channelToGroupIdx[c];

                    double unroundedNewVal = Math.Round(progress * toValues[c], 3);

                    double roundedNewVal = Math.Floor(unroundedNewVal);

                    double decimalComponent = unroundedNewVal - roundedNewVal;
                    double threshold = groupIdxToChannelGroup[groupIdx].SeenSoFar++ / groupIdxToChannelGroup[groupIdx].GroupSize;
                    if(decimalComponent > threshold) {
                        returnValue[c] = (int)Math.Ceiling(unroundedNewVal);
                    } else {
                        returnValue[c] = (int)roundedNewVal;
                    }
                    if(returnValue[c] > 65535) {
                        returnValue[c] = 65535;
                    }
                }
                channelsSet.Add(c);
            }

            return returnValue;
        }

        private Dictionary<int, int> noTemporalOffest(Dictionary<int, int> defaultOriginValue,
                                                                    Dictionary<int, int> defaultDestinationValue,
                                                                    AnimationClock animationClock) {
            if(animationClock.CurrentProgress == null) {
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

        protected override Dictionary<int, int> GetCurrentValueCore(Dictionary<int, int> defaultOriginValue,
                                                                    Dictionary<int, int> defaultDestinationValue,
                                                                    AnimationClock animationClock) {


            if(TemporalOffset) {
                return temporalOffest(defaultOriginValue, defaultDestinationValue, animationClock);
            } else {
                return noTemporalOffest(defaultOriginValue, defaultDestinationValue, animationClock);
            }
        }




        public override bool IsDestinationDefault {
            get {
                return base.IsDestinationDefault;
            }
        }
    }

    public class GroupCounter {
        public GroupCounter() {
            this.GroupSize = 1.0;
            this.SeenSoFar = 0;
        }
        public double GroupSize { get; set; }
        public int SeenSoFar { get; set; }
    }
}
