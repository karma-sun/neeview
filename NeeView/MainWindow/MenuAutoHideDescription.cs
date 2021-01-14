using System;
using System.Windows;
using System.Windows.Input;


namespace NeeView
{
    public class MenuAutoHideDescription : BasicAutoHideDescription
    {
        private readonly SidePanelFrameView _sidePanelFrame;

        public MenuAutoHideDescription(FrameworkElement target, SidePanelFrameView sidePanelFrame) : base(target)
        {
            if (sidePanelFrame is null) throw new ArgumentNullException(nameof(sidePanelFrame));

            _sidePanelFrame = sidePanelFrame;
        }

        public override bool IsIgnoreMouseOverAppendix()
        {
            var pos = Mouse.GetPosition(_sidePanelFrame);
            return _sidePanelFrame.IsPanelMouseOver() && pos.Y >= Config.Current.AutoHide.AutoHideConfrictMargin;
        }
    }
}
