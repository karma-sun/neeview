using System.Windows;
using System.Windows.Controls;
using NeeView.Runtime.LayoutPanel;

namespace NeeView
{
    // サイドパネルアイコンのドラッグ設定
    public class SidePanelIconDescriptor : ISidePanelIconDescriptor
    {
        private SidePanelFrameViewModel _vm;

        public SidePanelIconDescriptor(SidePanelFrameViewModel vm)
        {
            _vm = vm;
        }

        public FrameworkElement CreateButtonContent(LayoutPanel panel)
        {
            var imageSource = _vm.MainLayoutPanelManager.PanelsSource[panel.Key].Icon;
            return new Image() { Source = imageSource };
        }

        public void DragBegin()
        {
            _vm.DragBegin(this, null);
        }

        public void DragEnd()
        {
            _vm.DragEnd(this, null);
        }

        public void ToggleLayoutPanel(LayoutPanel panel)
        {
            _vm.MainLayoutPanelManager.Toggle(panel);
        }
    }
}
