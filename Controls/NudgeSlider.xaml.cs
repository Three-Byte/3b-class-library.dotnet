using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Windows.Threading;

namespace ThreeByte.Controls
{
    /// <summary>
    /// Interaction logic for NudgeSlider.xaml
    /// </summary>
    public partial class NudgeSlider : UserControl
    {
        public NudgeSlider() {
            InitializeComponent();

            _updateTimer = new DispatcherTimer();
            _updateTimer.Interval = UpdateRate;
            _updateTimer.Tick += new EventHandler(_updateTimer_Tick);
        }

       

        private static readonly TimeSpan NEVER = TimeSpan.FromMilliseconds(-1);

        public static readonly DependencyProperty UpdateRateProperty = DependencyProperty.Register("UpdateRate", typeof(TimeSpan), typeof(NudgeSlider),
            new FrameworkPropertyMetadata(TimeSpan.FromMilliseconds(500)));

        private static void UpdateRateChanged(DependencyObject obj, DependencyPropertyChangedEventArgs e) {
            NudgeSlider slider = (NudgeSlider)obj;
            slider._updateTimer.Interval = (TimeSpan)(e.NewValue);
        }

        public TimeSpan UpdateRate {
            get {
                return (TimeSpan)GetValue(UpdateRateProperty);
            }
            set {
                SetValue(UpdateRateProperty, value);
            }
        }


        public static readonly DependencyProperty MagnitudeProperty = DependencyProperty.Register("Magnitude", typeof(double), typeof(NudgeSlider), new FrameworkPropertyMetadata(1.0));

        public double Magnitude {
            get {
                return (double)GetValue(MagnitudeProperty);
            }
            set {
                SetValue(MagnitudeProperty, value);
            }
        }

        public event EventHandler<NudgeEventArgs> Nudge;
        private DispatcherTimer _updateTimer;

        private void _updateTimer_Tick(object sender, EventArgs e) {
            if(Nudge != null) {
                Nudge(this, new NudgeEventArgs(Slider.Value)); 
            }
        }

        private void StartTimer() {
            lock(_updateTimer){
                _updateTimer.Start();
            }
        }

        private void StopTimer(){
            lock(_updateTimer){
                _updateTimer.Stop();
            }
        }

        private void SliderMouseDown(object sender, MouseButtonEventArgs e) {
            StartTimer();
        }

        private void SliderMouseUp(object sender, MouseButtonEventArgs e) {
            Slider.Value = 0;
            StopTimer();
        }

        private void SliderMouseLeave(object sender, MouseEventArgs e) {
            Slider.Value = 0;
            StopTimer();
        }


    }


    public class NudgeEventArgs : EventArgs
    {
        public double Amount { get; private set; }

        public NudgeEventArgs(double amount) {
            Amount = amount;
        }
    }
}
