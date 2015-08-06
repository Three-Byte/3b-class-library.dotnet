using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Diagnostics;
using System.Threading;
using System.ComponentModel;
using log4net;

namespace ThreeByte.DMX {
    public class StrobeController : IDisposable, INotifyPropertyChanged {

        private static readonly ILog log = LogManager.GetLogger(typeof(StrobeController));

        private Stopwatch Watch = Stopwatch.StartNew();
        private Thread PulseThread;
        private static readonly bool HIGH_PRIORITY = true;

        public StrobeController() {
            PulseThread = new Thread(new ThreadStart(RunPulse));
            PulseThread.Start();
        }

        public event PropertyChangedEventHandler PropertyChanged;
        private void NotifyPropertyChanged(string propertyName) {
            if(PropertyChanged != null) {
                PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
            }
        }

        private int _phaseShift = 0;
        public int PhaseShift {
            get {
                return _phaseShift;
            }
            set {
                double freq = _frequency;
                if(freq > 0.0) {
                    _phaseShift = (int)Math.Round(Math.Min(value, (1000.0 / (_frequency * 2.0))));
                } else {
                    _phaseShift = 0;
                }
                NotifyPropertyChanged("PhaseShift");
            }
        }

        private bool _syncNudge = true;
        public bool SyncNudge {
            get {
                return _syncNudge;
            }
            set {
                _syncNudge = value;
                NotifyPropertyChanged("SyncNudge");
            }
        }

        private readonly object FrequencyLock = new object();
        private double _frequency = 0.0;
        public double Frequency {
            get {
                return _frequency;
            }
            set {
                lock(FrequencyLock) {
                    _frequency = value;
                    if(NextPulseOnTime == -1) {
                        NextPulseOnTime = 0;
                    }
                }
            }
        }

        private int _syncOffset = 0;
        public int SyncOffset {
            get {
                return _syncOffset;
            }
            private set {
                _syncOffset = value;
                NotifyPropertyChanged("SyncOffset");
            }
        }

        public bool IsPulseOn { get; private set; }

        private long NextPulseOnTime;
        private long NextPulseOffTime;
        private long SyncPulseTime;

        public void SetNextPulse(int offsetMillis) {
            SyncPulseTime = Watch.ElapsedMilliseconds + offsetMillis;
        }

        private void RunPulse() {

            while(true) {
                Stopwatch sw = Stopwatch.StartNew();
                double freq;
                lock(FrequencyLock) {
                    Debug.Assert(NextPulseOnTime >= -1, string.Format("Next pulse should never be less than -1 ({0})", NextPulseOnTime));
                    freq = Frequency;

                    int period = 0;
                    if(freq != 0.0) {
                        //True period: 1/Hz
                        period = (int)Math.Round(1000.0 / freq);
                    }
                    if(period > 2000) {
                        //The period should never be more than 2 seconds
                        //log.Info(string.Format("Period compression: {0} --> 1000", period));
                        period = 2000;
                    }

                    //*********************************************
                    //Synchronization correction
                    //*********************************************
                    long nextSyncPulseTime = SyncPulseTime; //Avoid double read errors on different threads
                    if((nextSyncPulseTime > 0) && (freq != 0.0) && SyncNudge) {
                        long absOffset = (nextSyncPulseTime - NextPulseOnTime);
                        
                        while(absOffset < -period/2) {
                            // Ignore negative offsets temporarily
                            // look forward only to resync
                            absOffset += period;
                        }
                        while(absOffset > period/2) {
                            // Make sure the offset is within one period
                            absOffset -= period;
                        }
                        
                        //long relOffset = (long)(absOffset - (Math.Round(absOffset/(double)period) * (double)period));
                        long relOffset = absOffset;

                        SyncOffset = (int)(absOffset);
                        //log.Info(string.Format("Checking for sync correction, relOffset: {0}, SyncNudge: {1}, Period: {2}, NextPulseOnTime: {3}", Math.Abs(relOffset), SyncNudge, period, NextPulseOnTime));

                        // Take Action
                        if(Math.Abs(relOffset) > 0) {
                            int nudge = (int)Math.Min(period / 16, Math.Abs(relOffset));
                            if(relOffset > 0) {
                                // NOTE: Logging actually causes this loop to take too long and glitch, so don't do it here
                                //log.Info(string.Format("push --> {2}: {0}/{1}, nextSync: {3}, currentTime: {4}", absOffset, relOffset, nudge, nextSyncPulseTime, Watch.ElapsedMilliseconds));
                                NextPulseOnTime = NextPulseOnTime + nudge;  //Nudge the pulse back to where it should be (10ms)
                            } else {
                                //log.Info(string.Format("push <-- {2}: {0}/{1}, nextSync: {3}, currentTime: {4}", absOffset, relOffset, nudge, nextSyncPulseTime, Watch.ElapsedMilliseconds));
                                NextPulseOnTime = Math.Max(NextPulseOnTime - nudge, 0);  //Nudge the pulse back to where it should be (10ms), but never less than 0
                            }
                            SyncPulseTime = 0;
                        }
                    }

                    //***********************************
                    // Triggered Pulses
                    //***********************************
                    long currentTime = Watch.ElapsedMilliseconds;
                    if((currentTime > NextPulseOnTime + PhaseShift) && (NextPulseOnTime > -1)) {
                        if(freq != 0.0) {
                            if(currentTime > (NextPulseOnTime + PhaseShift + period)) {
                                log.WarnFormat("Missed pulse. Freq: {0}", freq);
                            }
                            _phaseShift = (period / 2);
                            NextPulseOnTime = Math.Max(currentTime + period - PhaseShift, 0);  //Ensure this isn't ever < 0 in (very early) corner cases
                            NextPulseOffTime = Math.Max(currentTime + (period / 2) - PhaseShift, 0);

                            IsPulseOn = true; //Explicitly Pulse On
                            RaisePulse(IsPulseOn);
                        } else {
                            // Frequency is 0
                            if(!IsPulseOn) {
                                IsPulseOn = true;
                                RaisePulse(IsPulseOn); //Turn back to on
                            }
                            NextPulseOnTime = -1;
                            NextPulseOffTime = -1;
                        }
                    }

                    if((currentTime > NextPulseOffTime + PhaseShift) && (NextPulseOffTime > -1)) {
                        IsPulseOn = false;  //Explicitly Pulse Off
                        RaisePulse(IsPulseOn);
                        NextPulseOffTime = -1; // Only send one off pulse
                    }
                }  // lock(FrequencyLock)
                sw.Stop();
                if(sw.Elapsed > TimeSpan.FromMilliseconds(50)) {
                    log.InfoFormat("Long pulse calculation >> Elapsed time: {0}", sw.Elapsed);
                }
                if(_disposed) {
                    break;
                }
                if(!HIGH_PRIORITY) {
                    Thread.Sleep(1);
                }
            }
        }

        public event EventHandler<StrobeEventArgs> Pulse;
        private void RaisePulse(bool on) {
            var handler = Pulse;
            if(handler != null) {
                handler(this, new StrobeEventArgs(on));
            }
        }

        private volatile bool _disposed = false;
        public void Dispose() {
            if(_disposed) {
                throw new ObjectDisposedException("StrobeController already disposed");
            }
            _disposed = true;
            //Wait for the pulse thread to exit
            PulseThread.Join(TimeSpan.FromSeconds(3));
        }
    }

    public class StrobeEventArgs : EventArgs {
        public bool On { get; private set; }

        public StrobeEventArgs(bool on) {
            On = on;
        }
    }
}
