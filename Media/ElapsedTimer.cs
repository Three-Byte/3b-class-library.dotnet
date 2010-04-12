using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.ComponentModel;
using System.Threading;


namespace ThreeByte.Media
{
    public class ElapsedTimer : INotifyPropertyChanged
    {
        private static readonly TimeSpan INTERVAL = TimeSpan.FromMilliseconds(500);
        private static readonly TimeSpan INFINITE = TimeSpan.FromMilliseconds(-1);

        public event PropertyChangedEventHandler PropertyChanged;

        protected void NotifyPropertyChanged(string property) {
            if(PropertyChanged != null) {
                PropertyChanged(this, new PropertyChangedEventArgs(property));
            }
        }

        public string StringValue {
            get {
                TimeSpan t = ElapsedValue;
                return string.Format("{0:00}d{1:00}:{2:00}:{3:00}", t.Days, t.Hours, t.Minutes, t.Seconds);
            }
        }

        public TimeSpan ElapsedValue {
            get {
                TimeSpan t = (DateTime.Now - _startTime);
                return (t > TimeSpan.Zero ? t : TimeSpan.Zero);
            }
        }

        public ElapsedTimer() {
            _timer = new Timer(new TimerCallback(Tick));
        }

        private readonly Timer _timer;
        private DateTime _startTime = DateTime.MaxValue;

        private void Tick(object state) {
            NotifyPropertyChanged("ElapsedValue");
            NotifyPropertyChanged("StringValue");
        }

        public void Start() {
            if(_startTime == DateTime.MaxValue) {
                _startTime = DateTime.Now;
            }
            _timer.Change(INTERVAL, INTERVAL);
        }

        public void Stop() {
            _timer.Change(INFINITE, INFINITE);
        }

        public void Reset() {
            _startTime = DateTime.Now;
        }


    }
}
