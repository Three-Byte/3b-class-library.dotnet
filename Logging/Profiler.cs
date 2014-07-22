using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Diagnostics;

namespace ThreeByte.Logging {
	public class Profiler {
        public Profiler() {
			this.startVals = new Dictionary<string, TimeSpan>();
			this.durations = new Dictionary<string, TimeSpan>();
			this.longPoll = new Dictionary<string, LongPoll>();
		}
		Dictionary<string, TimeSpan> startVals;
		Dictionary<string, TimeSpan> durations;
		Dictionary<string, LongPoll> longPoll;
		public void Start(string name) {
			this.startVals[name] = DateTime.Now.TimeOfDay;
		}

        public void ResetAverages(string name) {
            if(this.longPoll.ContainsKey(name)) {
                this.longPoll[name].Reset();
            }
        }

		public TimeSpan Stop(string name) {
			TimeSpan startVal = this.startVals[name];
			TimeSpan duration = DateTime.Now.TimeOfDay - startVal;
			this.durations[name] = duration;
			if(this.longPoll.ContainsKey(name)) {
				var poll= longPoll[name];
				poll.Add(duration.Ticks);
			} else {
				longPoll[name] = new LongPoll(duration.Ticks);
			}
			//Debug.Print(duration.ToString());
			return duration;
		}

		public string Dump() {
			string output = "";
			foreach(var a in this.durations) {
				output += string.Format("Name: {0}, Duration: {1}", a.Key, a.Value);
			}
			return output;
		}

		public double Interval(string name) {
			double duration = 0;
			if(startVals.ContainsKey(name)) {
				duration = Stop(name).TotalMilliseconds;
				duration = TimeSpan.FromTicks(longPoll[name].Average()).TotalMilliseconds;
			}
			Start(name);
			return duration;
		}

        public TimeSpan GetLongPollAverage(string name) {
            if(this.longPoll.ContainsKey(name)) {
                return TimeSpan.FromTicks(this.longPoll[name].Average());
            } else {
                return TimeSpan.FromSeconds(0);
            }
        }

        ///// <summary>
        ///// Used to include a duration that has already transpired.  This helps in keeping track of collections of concurrent events.
        ///// </summary>
        //public void ReportConcludedDuration(string name, TimeSpan duration) {
        //    this.startVals[name] = DateTime.Now.TimeOfDay - duration;
        //    Stop(name);
        //}

        Dictionary<string, TimeSpan> timeLibrary = new Dictionary<string, TimeSpan>();
        public void BeginTimeLibraryEvent(string key) {
            this.timeLibrary[key] = DateTime.Now.TimeOfDay;
        }
        public void RemoveTimeLibraryEvent(string key) {
            this.timeLibrary.Remove(key);
        }

        /// <summary>
        /// Used to retreived a time from the time library and add the intervening interval to the profiler by calling ReportConcludedDuration with the found interval.
        /// </summary>
        /// <param name="key"></param>
        /// <param name="eventName"></param>
        /// <returns></returns>
        public TimeSpan ReportConcludedLibraryDuration(string key, string eventName) {
            TimeSpan? startTime = GetTimeFromLibrary(key);
            if(startTime != null) {
                this.startVals[eventName] = startTime.Value;
                RemoveTimeLibraryEvent(key);
                return Stop(eventName);
            } else {
                return TimeSpan.Zero;
            }
            //ReportConcludedDuration(eventName, interval);
        }

        public TimeSpan? GetTimeFromLibrary(string key) {
            if (this.timeLibrary.ContainsKey(key)) {
                return this.timeLibrary[key];
            } else {                
                return null;
            }
        }

	}

	public class LongPoll{
		public LongPoll() {
			Reset();
		}

		public void Reset() {
			this.Count = 0;
			this.Min = double.MaxValue;
			this.Max = double.MinValue;
			this.Sum = 0;
		}

		public LongPoll(long t) : this() {
			Add(t);
		}
		public void Add(long t) {
			if(Count % 200 == 0) {
				Reset();
			}
			this.Count++;
			this.Sum += t;
			if(t > Max) {
				Max = t;
			}else if(t < Min) {
				Min = t;
			}
		}

		public long Average() {
            if(Count > 0) {
			    return Sum / Count;
            } else {
                return 0;
            }
		}

		public double Min { get; set; }
		public double Max { get; set; }
		public int Count { get; set; }
		public long Sum { get; set; }

		public string GetTimeSpanStats() {
			return string.Format("PROFILER STATS:\nCount: {0}\nMin: {1}ms\nMax: {2}ms\nAverage: {3}ms",
				Count, TimeSpan.FromTicks((long)Min).TotalMilliseconds, TimeSpan.FromTicks((long)Max).TotalMilliseconds, 
				TimeSpan.FromTicks((long)Average()).TotalMilliseconds);
		}
	}

}
