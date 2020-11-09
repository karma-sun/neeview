using System;
using System.Windows;

namespace NeeView
{
    /// <summary>
    /// パネルのフォーカス機能の提供（未使用）
    /// </summary>
    public class FocusProvider
    {
        private UIElement _control;
        private bool _focusRequest;
        private Func<bool> _focusFunc;


        public FocusProvider(UIElement control, Func<bool> focusFunc)
        {
            if (control is null) throw new ArgumentNullException(nameof(control));

            _control = control;
            _control.IsVisibleChanged += Control_IsVisibleChanged;

            _focusFunc = focusFunc;
        }


        private void Control_IsVisibleChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            if (!_focusRequest) return;

            _control.Dispatcher.BeginInvoke((Action)delegate
            {
                Focus();
                _focusRequest = false;
            }, System.Windows.Threading.DispatcherPriority.Render);
        }

        public bool Focus()
        {
            if (_focusFunc != null)
            {
                return _focusFunc();
            }
            else
            {
                return _control.Focus();
            }
        }

        public void RequestFocus()
        {
            if (_control.IsVisible)
            {
                Focus();
            }
            else
            {
                _focusRequest = true;
            }
        }

    }
}
