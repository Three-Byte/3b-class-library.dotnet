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

        public double InitialRotation { get; set; }
        protected RotateTransform encoderRotateTransform;

        public double EncoderValue { get; set; }
        public double ValuePreview { get; set; }
        public double ValueMinimum { get; set; }
        public double ValueMaximum { get; set; }
        public double PreviousValue { get; set; }
        public double EncoderSensitivityAngle { get; set; }
        public double EncoderSensitivityValue { get; set; }



        public SolidColorBrush FillColor {
            get { return (SolidColorBrush)GetValue(FillColorProperty); }
            set { SetValue(FillColorProperty, value); }
        }

        // Using a DependencyProperty as the backing store for FillColor.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty FillColorProperty =
            DependencyProperty.Register("FillColor", typeof(SolidColorBrush), typeof(Encoder), new PropertyMetadata(Brushes.Transparent));



        public Double StrokeWidth {
            get { return (Double)GetValue(StrokeWidthProperty); }
            set { SetValue(StrokeWidthProperty, value); }
        }

        // Using a DependencyProperty as the backing store for StrokeWidth.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty StrokeWidthProperty =
            DependencyProperty.Register("StrokeWidth", typeof(Double), typeof(Encoder), new PropertyMetadata(1.0));

        

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
           
            double newVal = EncoderValue + (-1 * e.VerticalChange * EncoderSensitivityValue);
            if (ValueMaximum != ValueMinimum) {
                ValuePreview = Math.Max(Math.Min(newVal, ValueMaximum), ValueMinimum);
            } else {
                ValuePreview = EncoderValue + (-1 * e.VerticalChange * EncoderSensitivityValue);
            }

            double newAngle = ((-1 * (e.VerticalChange) * EncoderSensitivityAngle + InitialRotation) + 360) % 360;
            //max and min are set, so fix the angle
            if (ValueMaximum != ValueMinimum && (ValuePreview >= ValueMaximum || ValuePreview <= ValueMinimum)) {
                return;
            } else {
                encoderRotateTransform.Angle = newAngle;
            }
            Console.WriteLine("New Angle: " + newAngle + ", Updated Angle: " + encoderRotateTransform.Angle);
            
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
