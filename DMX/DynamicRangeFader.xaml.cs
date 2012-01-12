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
using System.ComponentModel;


namespace ThreeByte.DMX
{
    /// <summary>
    /// Interaction logic for DynamicRangeFader.xaml
    /// </summary>
    public partial class DynamicRangeFader : UserControl//, INotifyPropertyChanged
    {
        public DynamicRangeFader() {
            InitializeComponent();

            LayoutRoot.DataContext = this;
            _textBinding = ValueTextBox.GetBindingExpression(TextBox.TextProperty);

            //Just make this property OneWayToSource so we don't have to add another dependency property
            Binding enabledBinding = new Binding("ShowEdit") { Source = this, Mode = BindingMode.OneWayToSource };
            this.SetBinding(UserControl.IsEnabledProperty, enabledBinding);
        }

        private BindingExpression _textBinding;

        private double _maxValue = 65535;
        private double _minValue = 0;
        
        private bool _showEdit = true;
        public bool ShowEdit {
            get {
                return _showEdit;
            }
            set {
                _showEdit = value;
                if(!_showEdit) {
                    ValueTextBox.Visibility = Visibility.Hidden;
                }
            }
        }

        public static readonly DependencyProperty Show8BitProperty =
            DependencyProperty.Register("Show8Bit", typeof(bool), typeof(DynamicRangeFader));

        public bool Show8Bit {
            get {
                return (bool)GetValue(Show8BitProperty);
            }
            set {
                SetValue(Show8BitProperty, value);
            }
        }

        public static readonly DependencyProperty ValueProperty =
            DependencyProperty.Register("Value", typeof(int), typeof(DynamicRangeFader),
                                        new FrameworkPropertyMetadata(0, FrameworkPropertyMetadataOptions.BindsTwoWayByDefault,
                                        ValueChanged));

        public int Value {
            get {
                return (int)GetValue(ValueProperty);
            }
            set {
                SetValue(ValueProperty, value);
            }
        }

        private static void ValueChanged(DependencyObject obj, DependencyPropertyChangedEventArgs e) {
            DynamicRangeFader range = (DynamicRangeFader)obj;
            //Set Coarse and Fine values
            range.CoarseValue = (byte)(range.Value / 256);
            range.FineValue = (byte)(range.Value % 256);

        }

        public static readonly DependencyProperty CoarseValueProperty =
            DependencyProperty.Register("CoarseValue", typeof(byte), typeof(DynamicRangeFader));

        public byte CoarseValue {
            get {
                return (byte)GetValue(CoarseValueProperty);
            }
            set {
                SetValue(CoarseValueProperty, value);
            }
        }

        public static readonly DependencyProperty FineValueProperty =
            DependencyProperty.Register("FineValue", typeof(byte), typeof(DynamicRangeFader));

        public byte FineValue {
            get {
                return (byte)GetValue(FineValueProperty);
            }
            set {
                SetValue(FineValueProperty, value);
            }
        }

        public static readonly DependencyProperty ResolutionProperty = DependencyProperty.Register("Resolution", typeof(double), typeof(DynamicRangeFader),
            new FrameworkPropertyMetadata(16.0, ResolutionChanged));

        private static void ResolutionChanged(DependencyObject obj, DependencyPropertyChangedEventArgs e) {
            DynamicRangeFader range = (DynamicRangeFader)obj;
            range.UpdateRange();
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
            Fader.Maximum = Math.Min(Value + (int)Math.Pow(2, Resolution), _maxValue);
            Fader.Minimum = Math.Max(Value - (int)Math.Pow(2, Resolution), _minValue);
        }

        public static readonly DependencyProperty TitleProperty =
            DependencyProperty.Register("Title", typeof(string), typeof(DynamicRangeFader), new UIPropertyMetadata(string.Empty));

        public string Title {
            get {
                return (string)GetValue(TitleProperty);
            }
            set {
                SetValue(TitleProperty, value);
            }
        }

        private void UserControl_GotKeyboardFocus(object sender, KeyboardFocusChangedEventArgs e) {
            ValueTextBox.SelectAll();
        }

        private void ValueTextBox_KeyDown(object sender, KeyEventArgs e) {
            if(e.Key == Key.Enter || e.Key == Key.Return) {
                _textBinding.UpdateSource();
                _textBinding.UpdateTarget();
                ValueTextBox.SelectAll();
                e.Handled = true;
            } else if(e.Key == Key.Tab) {
                _textBinding.UpdateSource();
                _textBinding.UpdateTarget();
            }
        }

        private void ValueTextBox_MouseDown(object sender, MouseButtonEventArgs e) {
            ValueTextBox.Focus();
            ValueTextBox.SelectAll();
            e.Handled = true;
        }
    }
}
