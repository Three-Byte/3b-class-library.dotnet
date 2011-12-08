using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;

namespace ThreeByte.DMX
{
    public class ChannelSplitter : DependencyObject
    {
        private readonly DMXControl _controller;
        private readonly int _coarseChannel;
        private readonly int _fineChannel;

        public static readonly DependencyProperty ValueProperty = DependencyProperty.Register("Value", typeof(int), typeof(ChannelSplitter),
                                                                                    new FrameworkPropertyMetadata(0, ValueChanged));

        public int Value {
            get {
                return (int)GetValue(ValueProperty);
            }
            set {
                SetValue(ValueProperty, value);
            }
        }
        
        private static void ValueChanged(DependencyObject obj, DependencyPropertyChangedEventArgs e) {
            ChannelSplitter splitter = (ChannelSplitter)obj;
            int newValue = (int)(e.NewValue);

            Dictionary<int, byte> splitValues = new Dictionary<int, byte>();
            splitValues[splitter._coarseChannel] = (byte)(newValue / 256);
            splitValues[splitter._fineChannel] = (byte)(newValue % 256);
            splitter._controller.SetValues(splitValues);
        }

        public ChannelSplitter(DMXControl controller, int coarseChannel, int fineChannel){
            _controller = controller;
            _coarseChannel = coarseChannel;
            _fineChannel = fineChannel;
        }


    }
}
