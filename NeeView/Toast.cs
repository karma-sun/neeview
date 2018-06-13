using System;

namespace NeeView
{
    public class Toast
    {
        public Toast(string message)
        {
            Message = message;
        }

        public Toast(string message, string caption, string buttonContext)
        {
            Message = message;
            Caption = caption;
            ButtonContent = buttonContext;
        }

        public event EventHandler Confirmed;

        public string Caption { get; private set; }
        public string Message { get; private set; }
        public string ButtonContent { get; private set; }
        public TimeSpan DisplayTime { get; private set; } = new TimeSpan(0, 0, 10);
        public bool IsCanceled { get; private set; }

        public void Cancel()
        {
            IsCanceled = true;
            ToastService.Current.Update();
        }

        public void RaiseConfirmedEvent()
        {
            if (IsCanceled) return;
            Confirmed?.Invoke(this, null);
        }
    }
}
