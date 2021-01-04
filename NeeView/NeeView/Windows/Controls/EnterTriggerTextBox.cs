using System;
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
        private int _wheel;


        public event EventHandler<ValueDeltaEventArgs> MouseWheelChanged;


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
                    MouseWheelChanged?.Invoke(this, new ValueDeltaEventArgs(-1));
                }
                else if (e.Key == Key.Down)
                {
                    MouseWheelChanged?.Invoke(this, new ValueDeltaEventArgs(+1));
                }
            }
        }


        protected override void OnPreviewMouseWheel(MouseWheelEventArgs e)
        {
            if (!this.IsKeyboardFocused) return;

            _wheel += e.Delta;

            var delta = _wheel / 120;
            if (delta != 0)
            {
                MouseWheelChanged?.Invoke(this, new ValueDeltaEventArgs(-delta));
            }

            _wheel = _wheel % 120;
            e.Handled = true;
        }

    }
}
