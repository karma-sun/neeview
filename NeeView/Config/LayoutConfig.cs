using NeeLaboratory.ComponentModel;

namespace NeeView
{
    public class LayoutConfig : BindableBase
    {
        public ThemeConfig Theme { get; set; } = new ThemeConfig();

        public BackgroundConfig Background { get; set; } = new BackgroundConfig();

        public WindowTitleConfig WindowTittle { get; set; } = new WindowTitleConfig();

        public AutoHideConfig AutoHide { get; set; } = new AutoHideConfig();

        public NoticeConfig Notice { get; set; } = new NoticeConfig();

        public SidePanelsConfig SidePanels { get; set; } = new SidePanelsConfig();

        public MenuBarConfig MenuBar { get; set; } = new MenuBarConfig();

        public SliderConfig Slider { get; set; } = new SliderConfig();
    }
}


