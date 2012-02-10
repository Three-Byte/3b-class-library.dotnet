using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Diagnostics;
using System.Threading;
using System.ComponentModel;

namespace ThreeByte.DMX
{
    public class StrobeController : IDisposable, INotifyPropertyChanged
    {
        private Stopwatch Watch = Stopwatch.StartNew();
        private Thread PulseThread;
        private static readonly bool HIGH_PRIORITY = false;

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

        public event EventHandler<StrobeEventArgs> Pulse;

        private int _phaseShift = 0;
        public int PhaseShift {
            get {
                return _phaseShift;
            }
            set {
                _phaseShift = value;
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

        private object FrequencyLock = new object();
        private double _frequency = 0.0;
        public double Frequency {
            get {
                return _frequency;
            }
            set {
                lock(FrequencyLock) {
                    _frequency = value;
                    if(NextPulseTime == -1) {
                        NextPulseTime = 0;
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

        private long NextPulseTime;
        private long SyncPulseTime;

        public void SetNextPulse(int offsetMillis) {
            SyncPulseTime = Watch.ElapsedMilliseconds + offsetMillis;
            Console.WriteLine("SyncPulse Time: {0}", SyncPulseTime);
        }



        private void RunPulse() {

            while(true) {

                if(SyncPulseTime > 0){
                    long offset = SyncPulseTime - NextPulseTime;
                    SyncOffset = (int)(offset);
                    if(offset < 0 && SyncNudge) {
                        NextPulseTime = NextPulseTime - 10;  //Nudge the pulse back to where it should be
                        //Only push the pulse time sooner
                    }
                    Console.WriteLine("Sync Pulse Offset: {0}", offset);
                    SyncPulseTime = 0;
                }

                if((Watch.ElapsedMilliseconds > NextPulseTime + PhaseShift)
                    && (NextPulseTime > -1)) {

                    double freq;
                    lock(FrequencyLock) {
                        freq = Frequency;
                    }
                    if(freq != 0.0) {
                        int interval = (int)Math.Round(1000.0 / (freq * 2.0));
                        NextPulseTime = Watch.ElapsedMilliseconds + interval - PhaseShift;//Frequency calculation
                    } else {
                        Pulse(this, new StrobeEventArgs(true));  //Turn back to on
                        NextPulseTime = -1;
                    }

                    IsPulseOn = !IsPulseOn;
                    if(Pulse != null) {
                        Pulse(this, new StrobeEventArgs(IsPulseOn));
                    }
                }

                if(_disposed) {
                    break;
                }
                if(!HIGH_PRIORITY) {
                    Thread.Sleep(1);
                }
                
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


    public class StrobeEventArgs : EventArgs
    {
        public bool On { get; private set; }

        public StrobeEventArgs(bool on) {
            On = on;
        }
    }
}
