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
			return Sum / Count;
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
