using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;

namespace ThreeByte.TaskModel
{
    public class ConsoleProgressBar
    {

        public int Total { get; set; }
        public int Current { get; private set; }
        public double Percent { get; private set; }
        public TimeSpan Elapsed { get { return _stopwatch.Elapsed; } }
        public double TicksPerSecond {
            get {
                lock(TickQueue) {
                    if(TickQueue.Count < 2) {
                        return 0.0;
                    }
                    TickCount start = TickQueue.First();
                    TickCount end = TickQueue.Last();
                    return ((end.Ticks - start.Ticks) / (end.Timestamp - start.Timestamp).TotalSeconds);
                }
            }
        }


        private Queue<TickCount> TickQueue = new Queue<TickCount>();


        private Stopwatch _stopwatch;

        public void Init(int total) {
            Total = total;
            Current = 0;
            Percent = 0.0;
            //Console.CursorVisible = false;
            _stopwatch = Stopwatch.StartNew();
            lock(TickQueue) {
                TickQueue.Clear();
            }
            Draw();
        }

        public void Tick() {
            Tick(1);
        }

        public void TickTo(int current) {
            Current = current;
            Tick(0);
        }

        public void Tick(int increment) {
            Current += increment;
            if(Current >= Total) {
                Complete();
                return;
            }
            UpdateTickQueue(Current);
            Draw();
        }

        private void UpdateTickQueue(int ticks) {
            lock(TickQueue) {
                TickQueue.Enqueue(new TickCount(ticks));
                while(TickQueue.Count > 5) {
                    TickQueue.Dequeue();
                }
            }
        }

        public void Complete() {
            Console.WriteLine("\r[==================================================]\tDone.   ");
            //Console.CursorVisible = true;
        }

        private void Draw() {

            Percent = System.Math.Round((Current * 100.0) / (double)Total, 2);

            StringBuilder output = new StringBuilder();
            output.Append("\r[");
            int stepsToDraw = (int)(Percent / 2);
            for(int i = 0; i < 50; i++) {
                output.Append((i < stepsToDraw ? "=" : (i == stepsToDraw ? ">" : " ")));
            }
            output.AppendFormat("]\t{0:0.00}%\t{1:hh\\:mm\\:ss} {2:0.0}", Percent, Elapsed, TicksPerSecond);
            Console.Write(output.ToString());
        }

        private class TickCount
        {
            public int Ticks { get; private set; }
            public DateTime Timestamp { get; private set; }

            public TickCount(int ticks) {
                Ticks = ticks;
                Timestamp = DateTime.Now;
            }
        }

    }

    
}
