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

namespace ThreeByte.Controls {
    /// <summary>
    /// Interaction logic for Encoder.xaml
    /// </summary>
    public partial class Encoder : UserControl {
        public class EncoderTurnedEventArgs : EventArgs {
            public double Value { get; set; }
            public EncoderTurnedEventArgs(double value) { this.Value = value; }
        }

        public event EncoderStartedEventHandler EncoderStarted;
        public delegate void EncoderStartedEventHandler(Object sender, EncoderTurnedEventArgs e);

        public event EncoderCompletedEventHandler EncoderCompleted;
        public delegate void EncoderCompletedEventHandler(Object sender, EncoderTurnedEventArgs e);

        public event EncoderTurnedEventHandler EncoderTurned;
        public delegate void EncoderTurnedEventHandler(Object sender, EncoderTurnedEventArgs e);

        private double _maxValue = 65535;
        private double _minValue = 0;

        public double InitialRotation { get; set; }
        protected RotateTransform encoderRotateTransform;

        public double EncoderValue { get; set; }
        public double ValuePreview { get; set; }
        public double ValueMinimum { get; set; }
        public double ValueMaximum { get; set; }
        public double PreviousValue { get; set; }
        public double EncoderSensitivityAngle { get; set; }
        public double EncoderSensitivityValue { get; set; }

        public Encoder() {
            EncoderSensitivityAngle = 1;
            EncoderSensitivityValue = 3;
            EncoderValue = 0;
            ValuePreview = 0;
            ValueMaximum = 360;
            ValueMinimum = 1;
            PreviousValue = ValueMinimum;
            InitializeComponent();
        }

        public static readonly DependencyProperty ResolutionProperty = DependencyProperty.Register("Resolution", typeof(double), typeof(Encoder),
            new FrameworkPropertyMetadata(16.0, ResolutionChanged));

        private static void ResolutionChanged(DependencyObject obj, DependencyPropertyChangedEventArgs e) {
            Encoder encoder = (Encoder)obj;
            encoder.UpdateRange();
        }

        public double Resolution {
            get {
                return (double)GetValue(ResolutionProperty);
            }
            set {
                SetValue(ResolutionProperty, value);
            }
        }

        private void UpdateRange() {
            this.ValueMaximum = Math.Min(EncoderValue + (int)Math.Pow(2, Resolution), _maxValue);
            this.ValueMinimum = Math.Max(EncoderValue - (int)Math.Pow(2, Resolution), _minValue);
        }

        private void encThumb_DragStarted(object sender, System.Windows.Controls.Primitives.DragStartedEventArgs e) {
            EncoderTurnedEventArgs ete = new EncoderTurnedEventArgs(0);
            if(EncoderStarted != null) {
                EncoderStarted(this, ete);
            }
        }

        private void encThumb_DragCompleted(object sender, System.Windows.Controls.Primitives.DragCompletedEventArgs e) {
            InitialRotation = encoderRotateTransform.Angle;
            EncoderValue = ValuePreview;
            EncoderTurnedEventArgs ete = new EncoderTurnedEventArgs(EncoderValue);
            if(EncoderCompleted != null) {
                EncoderCompleted(this, ete);
            }
        }

        private void encThumb_DragDelta(object sender, System.Windows.Controls.Primitives.DragDeltaEventArgs e) {
            encoderRotateTransform.Angle = (e.VerticalChange) * EncoderSensitivityAngle + InitialRotation;
            ValuePreview = EncoderValue + e.VerticalChange * EncoderSensitivityValue;
            EncoderTurnedEventArgs ete = new EncoderTurnedEventArgs(ValuePreview);
            if(EncoderTurned != null) {
                EncoderTurned(this, ete);
            }
        }

        private void enc_Loaded(object sender, RoutedEventArgs e) {
            encoderRotateTransform = new RotateTransform(InitialRotation);
            enc.RenderTransform = encoderRotateTransform;
        }
    }
}
