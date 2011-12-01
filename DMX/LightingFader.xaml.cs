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
    /// Interaction logic for LightingFader.xaml
    /// </summary>
    public partial class LightingFader : UserControl, INotifyPropertyChanged
    {
        public LightingFader() {
            InitializeComponent();

            DataContext = this;
            _textBinding = ValueTextBox.GetBindingExpression(TextBox.TextProperty);

            //Just make this property OneWayToSource so we don't have to add another dependency property
            Binding enabledBinding = new Binding("ShowEdit") { Source = this, Mode = BindingMode.OneWayToSource };
            this.SetBinding(UserControl.IsEnabledProperty, enabledBinding);
        }

        private BindingExpression _textBinding;

        public event PropertyChangedEventHandler PropertyChanged;

        private void NotifyPropertyChanged(string propertyName) {
            if(PropertyChanged != null) {
                PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
            }
        }

        private bool _showEdit = true;
        public bool ShowEdit {
            get {
                return _showEdit;
            }
            set {
                _showEdit = value;
                NotifyPropertyChanged("ShowEdit");
                if(!_showEdit) {
                    ValueTextBox.Visibility = Visibility.Hidden;
                }
            }
        }

        public static readonly DependencyProperty ByteValueProperty =
            DependencyProperty.Register("ByteValue", typeof(byte), typeof(LightingFader),
                                        new FrameworkPropertyMetadata((byte)0, FrameworkPropertyMetadataOptions.BindsTwoWayByDefault,
                                        ByteValueChanged));

        public byte ByteValue {
            get {
                return (byte)GetValue(ByteValueProperty);
            }
            set {
                SetValue(ByteValueProperty, value);
                NotifyPropertyChanged("ByteValue");
            }
        }

        private static void ByteValueChanged(DependencyObject obj, DependencyPropertyChangedEventArgs e) {
            LightingFader fader = (LightingFader)obj;
            fader.NotifyPropertyChanged("ByteValue");
        }

        public static readonly DependencyProperty TitleProperty =
            DependencyProperty.Register("Title", typeof(string), typeof(LightingFader), new UIPropertyMetadata(string.Empty));

        public string Title {
            get {
                return (string)GetValue(TitleProperty);
            }
            set {
                SetValue(TitleProperty, value);
                NotifyPropertyChanged("Title");
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
