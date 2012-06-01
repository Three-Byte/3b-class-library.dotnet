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

        private double _maxValue = 65535;
        private double _minValue = 0;

        public event EncoderStartedEventHandler EncoderStarted;
        public delegate void EncoderStartedEventHandler(Object sender, EncoderTurnedEventArgs e);

        public event EncoderCompletedEventHandler EncoderCompleted;
        public delegate void EncoderCompletedEventHandler(Object sender, EncoderTurnedEventArgs e);

        public event EncoderTurnedEventHandler EncoderTurned;
        public delegate void EncoderTurnedEventHandler(Object sender, EncoderTurnedEventArgs e);

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
            EncoderSensitivityValue = 1;
            EncoderValue = 0;
            ValuePreview = 0;
            //ValueMaximum = 96;
            //ValueMinimum = 1;
            //PreviousValue = ValueMinimum;
            InitializeComponent();
        }

        private void encThumb_DragStarted(object sender, System.Windows.Controls.Primitives.DragStartedEventArgs e) {
            InitialRotation = encoderRotateTransform.Angle;
            EncoderTurnedEventArgs ete = new EncoderTurnedEventArgs(0);
            if(EncoderStarted != null) {
                EncoderStarted(this, ete);
            }
        }

        private void encThumb_DragCompleted(object sender, System.Windows.Controls.Primitives.DragCompletedEventArgs e) {
            //InitialRotation = encoderRotateTransform.Angle;
            EncoderValue = ValuePreview;
            EncoderTurnedEventArgs ete = new EncoderTurnedEventArgs(EncoderValue);
            if(EncoderCompleted != null) {
                EncoderCompleted(this, ete);
            }
        }

        private void encThumb_DragDelta(object sender, System.Windows.Controls.Primitives.DragDeltaEventArgs e) {
            
            encoderRotateTransform.Angle = ((-1 * (e.VerticalChange) * EncoderSensitivityAngle + InitialRotation) + 360) % 360;
            Console.WriteLine("Updated Angle: " + encoderRotateTransform.Angle);
            ValuePreview = EncoderValue + (-1 * e.VerticalChange * EncoderSensitivityValue);
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
