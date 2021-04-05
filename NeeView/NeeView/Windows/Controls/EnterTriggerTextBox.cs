using NeeLaboratory;
using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace NeeView.Windows.Controls
{
    /// <summary>
    /// デルタ変化量を通知
    /// </summary>
    public class ValueDeltaEventArgs : EventArgs
    {
        public ValueDeltaEventArgs(int delta)
        {
            Delta = delta;
        }

        public int Delta { get; set; }
    }

    /// <summary>
    /// Enter キーで UpdateSource を発行する TextBox
    /// </summary>
    public class EnterTriggerTextBox : TextBox
    {
        private MouseWheelDelta _mouseWheelDelta = new MouseWheelDelta();


        public event EventHandler<ValueDeltaEventArgs> ValueDeltaChanged;


        public Slider Slider
        {
            get { return (Slider)GetValue(SliderProperty); }
            set { SetValue(SliderProperty, value); }
        }

        public static readonly DependencyProperty SliderProperty =
            DependencyProperty.Register("Slider", typeof(Slider), typeof(EnterTriggerTextBox), new PropertyMetadata(null));


        protected override void OnPreviewKeyDown(KeyEventArgs e)
        {
            base.OnPreviewKeyDown(e);

            if (Keyboard.Modifiers == ModifierKeys.None)
            {
                if (e.Key == Key.Enter)
                {
                    var expression = GetBindingExpression(TextBox.TextProperty);
                    expression?.UpdateSource();
                }
                else if (e.Key == Key.Up)
                {
                    ValueDeltaChanged?.Invoke(this, new ValueDeltaEventArgs(+1));
                }
                else if (e.Key == Key.Down)
                {
                    ValueDeltaChanged?.Invoke(this, new ValueDeltaEventArgs(-1));
                }
            }
        }

        protected override void OnPreviewMouseWheel(MouseWheelEventArgs e)
        {
            base.OnPreviewMouseWheel(e);

            if (!this.IsKeyboardFocusWithin) return;

            var delta = _mouseWheelDelta.NotchCount(e);
            if (delta != 0)
            {
                ValueDeltaChanged?.Invoke(this, new ValueDeltaEventArgs(delta));
            }

            if (this.Slider != null)
            {
                var frequency = this.Slider.TickFrequency > 0.0 ? this.Slider.TickFrequency : (this.Slider.Maximum - this.Slider.Minimum) * 0.01;
                this.Slider.Value = MathUtility.Clamp(this.Slider.Value + frequency * delta, this.Slider.Minimum, this.Slider.Maximum);
                e.Handled = true;
            }
        }
    }

}
