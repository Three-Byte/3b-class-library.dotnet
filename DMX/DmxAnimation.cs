﻿using System;
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

        public static bool RandomizeOutput = false;
        public static int BitDepthReduction = 0;

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
                buildRandomizer(value[groupIdx]);
                foreach (int channel in value[groupIdx]) {
                    channelToGroupIdx[channel] = groupIdx;
                }
                groupIdxToChannelGroup[groupIdx] = new GroupCounter() { GroupSize = value[groupIdx].Count, SeenSoFar = 0 };
            }
        }

        static Dictionary<int, int> randomizer = new Dictionary<int, int>();

        private void buildRandomizer(List<int> list) {
            List<int> orig = new List<int>();
            orig.AddRange(list);
            Random rng = new Random(Seed: 1);

            int n = list.Count;
            while (n > 1) {
                n--;
                int k = rng.Next(n + 1);
                int value = list[k];
                list[k] = list[n];
                list[n] = value;
            }
            for (int i = 0; i < list.Count(); i++) {
                randomizer[orig[i]] = list[i];
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
                    double toValue = toValues[c] >> BitDepthReduction;
                    double fromValue = fromValues[c] >> BitDepthReduction;
                    int groupIdx = channelToGroupIdx[c];
                    //UnroundedNewVals is rounded to three decimal places to handle the problem of numerical instability 
                    //resulting in numbers like .0000001 which gets blown up in the Math.Ceiling call
                    double unroundedNewVal = Math.Round(((1.0 - progress) * fromValue) + (progress * toValue), 3);

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
                int idx = c;
                if (randomizer.ContainsKey(c) && RandomizeOutput) {
                    idx = randomizer[c];
                }
                returnValue[idx] = newValue << BitDepthReduction;
                channelsSet.Add(c);
            }

            foreach(int c in toValues.Keys) {
                if(!channelsSet.Contains(c)) {
                    if(!channelToGroupIdx.ContainsKey(c)) {
                        continue;
                    }
                    //This handles the case of a toValue without a fromValue

                    double toValue = toValues[c] >> BitDepthReduction;
                    int groupIdx = channelToGroupIdx[c];

                    double unroundedNewVal = Math.Round(progress * toValue, 3);

                    double roundedNewVal = Math.Floor(unroundedNewVal);

                    double decimalComponent = unroundedNewVal - roundedNewVal;
                    double threshold = groupIdxToChannelGroup[groupIdx].SeenSoFar++ / groupIdxToChannelGroup[groupIdx].GroupSize;
                    if(decimalComponent > threshold) {
                        returnValue[c] = (int)Math.Ceiling(unroundedNewVal) << BitDepthReduction;
                    } else {
                        returnValue[c] = (int)(roundedNewVal) << BitDepthReduction;
                    }
                    if(returnValue[c] > 65535) {
                        returnValue[c] = 65535;
                    }
                }
                channelsSet.Add(c);
            }
            return returnValue;
        }

        public static int ComputeNewValue(double progress, int initialVal, int targetVal) {
            int newValue = initialVal;
            //Basic linear interpolation
            newValue = (int)Math.Round(((1.0 - progress) * initialVal) + (progress * targetVal));
            return newValue;
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

                int targetValue = fromValues[c];

                if (toValues.ContainsKey(c)) {
                    targetValue = ComputeNewValue(progress, fromValues[c], toValues[c]);
                }

                returnValue[c] = targetValue;
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
