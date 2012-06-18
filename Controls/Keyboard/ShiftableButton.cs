using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;

namespace ThreeByte.Controls.Keyboard
{
    public class ShiftableButton : Button
    {
        public string LowerContent { get; set; }
        public string UpperContent { get; set; }
        public bool Shift {
            get {
                return (bool)(this.GetValue(ShiftProperty));
            }
            set {
                this.SetValue(ShiftProperty, value);
            }
        }

        public static DependencyProperty ShiftProperty = DependencyProperty.Register(
            "Shift", typeof(bool), typeof(ShiftableButton),
                new FrameworkPropertyMetadata(false, OnShiftChanged));

        static void OnShiftChanged(DependencyObject sender, DependencyPropertyChangedEventArgs e) {
            ShiftableButton thisButton = (ShiftableButton)sender;
            thisButton.Content = ((bool)e.NewValue ? thisButton.UpperContent : thisButton.LowerContent);
            //Console.WriteLine("The Content: " + thisButton.Content);
            //thisButton.UpdateLayout();
        }

    }
}
