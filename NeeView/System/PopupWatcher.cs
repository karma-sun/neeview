using System;
using System.Windows;

namespace NeeView
{
    /// <summary>
    /// Popup監視
    /// </summary>
    public static class PopupWatcher
    {
        public static event EventHandler<TargetElementChangedEventArgs> PopupElementChanged;

        public static UIElement PopupElement { get; private set; }

        public static void SetPopupElement(object sender, UIElement element)
        {
            if (PopupElement != element)
            {
                PopupElement = element;
                PopupElementChanged?.Invoke(sender, new TargetElementChangedEventArgs(element));
            }
        }
    }
}
