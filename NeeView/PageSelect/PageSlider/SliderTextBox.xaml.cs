using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
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
using System.Diagnostics;
using System.Globalization;

namespace NeeView
{
    /// <summary>
    /// SliderTextBox.xaml の相互作用ロジック
    /// </summary>
    public partial class SliderTextBox : UserControl, INotifyPropertyChanged
    {
        #region INotifyPropertyChanged Support

        public event PropertyChangedEventHandler PropertyChanged;

        protected bool SetProperty<T>(ref T storage, T value, [System.Runtime.CompilerServices.CallerMemberName] string propertyName = null)
        {
            if (object.Equals(storage, value)) return false;
            storage = value;
            this.RaisePropertyChanged(propertyName);
            return true;
        }

        protected void RaisePropertyChanged([System.Runtime.CompilerServices.CallerMemberName] string name = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
        }

        public void AddPropertyChanged(string propertyName, PropertyChangedEventHandler handler)
        {
            PropertyChanged += (s, e) => { if (string.IsNullOrEmpty(e.PropertyName) || e.PropertyName == propertyName) handler?.Invoke(s, e); };
        }

        #endregion

        private MouseWheelDelta _mouseWheelDelta = new MouseWheelDelta();
        private string _dispText;

        public SliderTextBox()
        {
            InitializeComponent();
            this.Root.DataContext = this;
            this.TextBlock.SizeChanged += TextBlock_SizeChanged;
        }


        public event EventHandler ValueChanged;


        #region DependencyProperties

        public VideoSlider Target
        {
            get { return (VideoSlider)GetValue(TargetProperty); }
            set { SetValue(TargetProperty, value); }
        }

        public static readonly DependencyProperty TargetProperty =
            DependencyProperty.Register("Target", typeof(VideoSlider), typeof(SliderTextBox), new PropertyMetadata(null, TargetPropertyChanged));

        private static void TargetPropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            (d as SliderTextBox)?.UpdateTarget();
        }

        #endregion

        #region Properties

        public string DispText
        {
            get { return _dispText; }
            set { if (_dispText != value) { _dispText = value; RaisePropertyChanged(); } }
        }

        #endregion


        private void TextBlock_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            var control = (FrameworkElement)sender;
            if (e.WidthChanged && e.NewSize.Width > control.MinWidth)
            {
                SetWidth(e.NewSize.Width);
            }
        }

        private void UpdateTarget()
        {
            if (Target == null) return;

            // 注意：リークします
            DependencyPropertyDescriptor.FromProperty(VideoSlider.MaximumProperty, typeof(VideoSlider))
                .AddValueChanged(this.Target, (s, e) => Update());
            DependencyPropertyDescriptor.FromProperty(VideoSlider.ValueProperty, typeof(VideoSlider))
                .AddValueChanged(Target, (s, e) => UpdateDispText());

            Update();
        }

        private void Update()
        {
            int length = (int)Math.Log10(this.Target.Maximum) + 1;
            var width = (length * 2 + 1) * 7 + 20;
            SetWidth(width);

            UpdateDispText();
        }

        private void SetWidth(double width)
        {
            this.TextBlock.MinWidth = width;
            this.TextBox.Width = width;
        }


        private void UpdateDispText()
        {
            this.DispText = Target != null ? $"{Target.Value + 1} / {Target.Maximum + 1}" : "";
        }


        private void SliderTextBox_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            this.TextBox.Visibility = Visibility.Visible;
            this.TextBox.Focus();
        }

        private void SliderTextBox_GotFocus(object sender, RoutedEventArgs e)
        {
            this.TextBox.Visibility = Visibility.Visible;
            this.TextBox.Focus();
        }

        private void SliderTextBox_KeyDown(object sender, KeyEventArgs e)
        {
            e.Handled = true;
        }

        private void TextBox_LostFocus(object sender, RoutedEventArgs e)
        {
            UpdateSource();
            this.TextBox.Visibility = Visibility.Hidden;
        }

        private void TextBox_GotFocus(object sender, RoutedEventArgs e)
        {
            this.TextBox.SelectAll();
        }

        private void TextBox_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            switch (e.Key)
            {
                case Key.Escape:
                    MainWindowModel.Current.FocusMainView();
                    e.Handled = true;
                    break;

                case Key.Return:
                    UpdateSource();
                    this.TextBox.SelectAll();
                    e.Handled = true;
                    break;
            }
        }

        private void UpdateSource()
        {
            this.TextBox.GetBindingExpression(TextBox.TextProperty).UpdateSource();
            ValueChanged?.Invoke(this, null);
        }

        private void TextBox_MouseWheel(object sender, MouseWheelEventArgs e)
        {
            int turn = _mouseWheelDelta.NotchCount(e);
            if (turn != 0)
            {
                this.Target.Value = this.Target.Value - turn;
                ValueChanged?.Invoke(this, null);
                this.TextBox.SelectAll();
            }
            e.Handled = true;
        }
    }

    public class SliderValueConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return (double)value + 1;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (double.TryParse((string)value, out double result))
            {
                if (result > int.MaxValue)
                {
                    result = int.MaxValue;
                }
                else if (result < 1)
                {
                    result = 1;
                }

                return result - 1;
            }
            else
            {
                return DependencyProperty.UnsetValue;
            }
        }
    }

}
