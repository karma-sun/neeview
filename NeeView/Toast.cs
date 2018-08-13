using System;

namespace NeeView
{
    public class Toast
    {
        public static TimeSpan DefaultDisplayTime { get; } = TimeSpan.FromSeconds(5);
        public static TimeSpan LongDisplayTime { get; } = TimeSpan.FromSeconds(10);

        public Toast(string message)
        {
            Message = message;
        }

        public Toast(string message, TimeSpan displayTime)
        {
            Message = message;
            DisplayTime = displayTime;
        }

        public Toast(string message, string buttonContext, Action buttonAction)
        {
            Message = message;
            DisplayTime = LongDisplayTime;
            ButtonContent = buttonContext;
            ButtonAction = buttonAction;
        }

        public string Caption { get; set; }
        public string Message { get; private set; }
        public string ButtonContent { get; private set; }
        public Action ButtonAction { get; private set; }
        public TimeSpan DisplayTime { get; private set; } = DefaultDisplayTime;

        public bool IsCanceled { get; private set; }


        public void Cancel()
        {
            IsCanceled = true;
            ToastService.Current.Update();
        }

        public void RaiseConfirmedEvent()
        {
            if (IsCanceled) return;
            ButtonAction?.Invoke();
        }
    }
}
