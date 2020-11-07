using NeeView.Runtime.LayoutPanel;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace NeeView
{
    static class LayoutPanelFactory
    {
        public static List<LayoutPanel> CreatePanels(IEnumerable<IPanel> panels)
        {
            return panels.Select(e => Create(e)).ToList();
        }

        public static LayoutPanel Create(IPanel panel)
        {
            var ghost = new Border()
            {
                Width = 32,
                Height = 32,
                Child = new Image()
                {
                    Source = panel.Icon,
                    Margin = panel.IconMargin,
                }
            };
            var size = new Size(ghost.Width, ghost.Height);
            ghost.Measure(size);
            ghost.Arrange(new Rect(size));
            //ghost.UpdateLayout(); ... メインウィンドウ生成前での実行だと例外になる

            return new LayoutPanel(panel.TypeCode)
            {
                Title = panel.IconTips,
                Content = panel.View,
                DragGhost = ghost,
            };
        }
    }

}
