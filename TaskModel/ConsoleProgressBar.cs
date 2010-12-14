using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ThreeByte.TaskModel
{
    public class ConsoleProgressBar
    {

        public int Total { get; set; }
        public int Current { get; private set; }
        public double Percent { get; private set; }

        public void Init(int total) {
            Total = total;
            Current = 0;
            Percent = 0.0;
            Console.CursorVisible = false;
            Draw();
        }

        public void Tick() {
            Tick(1);
        }

        public void Tick(int increment) {
            Current += increment;
            if(Current >= Total) {
                Complete();
                return;
            }
            Draw();
        }

        public void Complete() {
            Console.WriteLine("\r[==================================================]\tDone.   ");
            Console.CursorVisible = true;
        }

        private void Draw() {
            string output = string.Empty;
            Percent = System.Math.Round((Current * 100.0) / (double)Total, 2);

            output = "\r[";
            int stepsToDraw = (int)(Percent / 2);
            for(int i = 0; i < 50; i++) {
                output += (i < stepsToDraw ? "=" : (i == stepsToDraw ? ">" : " "));
            }
            output += string.Format("]\t{0:0.00}%\t", Percent);
            Console.Write(output);
        }


    }
}
