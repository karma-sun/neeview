using NeeLaboratory.ComponentModel;

namespace NeeView
{
    public class LayoutConfig : BindableBase
    {
        public ThemeConfig Theme { get; set; } = new ThemeConfig();

        public AutoHideConfig AutoHide { get; set; } = new AutoHideConfig();

        public PanelsConfig Panels { get; set; } = new PanelsConfig();
    }

    public class PanelsConfig : BindableBase
    {
    }
}