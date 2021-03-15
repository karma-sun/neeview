using System;
using System.Windows;
using System.Windows.Input;


namespace NeeView
{
    public class StatusAutoHideDescription : BasicAutoHideDescription
    {
        private readonly SidePanelFrameView _sidePanelFrame;

        public StatusAutoHideDescription(FrameworkElement target, SidePanelFrameView sidePanelFrame) : base(target)
        {
            if (sidePanelFrame is null) throw new ArgumentNullException(nameof(sidePanelFrame));

            _sidePanelFrame = sidePanelFrame;
        }

        public override bool IsIgnoreMouseOverAppendix()
        {
            if (!_sidePanelFrame.IsPanelMouseOver())
            {
                return false;
            }

            switch (Config.Current.AutoHide.AutoHideConfrictBottomMargin)
            {
                default:
                case AutoHideConfrictMode.Allow:
                    return false;

                case AutoHideConfrictMode.AllowPixel:
                    var pos = Mouse.GetPosition(_sidePanelFrame);
                    return pos.Y < _sidePanelFrame.ActualHeight - 1.5;

                case AutoHideConfrictMode.Deny:
                    return true;
            }
        }
    }
}
