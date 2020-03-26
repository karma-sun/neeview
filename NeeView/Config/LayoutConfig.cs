using NeeLaboratory.ComponentModel;
using System;

namespace NeeView
{
    public class LayoutConfig : BindableBase
    {
        public ThemeConfig Theme { get; set; } = new ThemeConfig();

        public BackgroundConfig Background { get; set; } = new BackgroundConfig();

        public WindowTitleConfig WindowTittle { get; set; } = new WindowTitleConfig();

        public AutoHideConfig AutoHide { get; set; } = new AutoHideConfig();

        public NoticeConfig Notice { get; set; } = new NoticeConfig();

        public MenuBarConfig MenuBar { get; set; } = new MenuBarConfig();

        public SliderConfig Slider { get; set; } = new SliderConfig();

        public FilmStripConfig FilmStrip { get; set; } = new FilmStripConfig();

        public PanelsConfig Panels { get; set; } = new PanelsConfig();

        public BookshelfPanelConfig Bookshelf { get; set; } = new BookshelfPanelConfig();

        public BookmarkPanelConfig Bookmark { get; set; } = new BookmarkPanelConfig();

        public InformationPanelConfig Information { get; set; } = new InformationPanelConfig();

        public PageListConfig PageList { get; set; } = new PageListConfig();
    }

    public class PageListConfig : BindableBase
    {
        private PanelListItemStyle _panelListItemStyle;
        private PageNameFormat _format = PageNameFormat.Smart;

        /// <summary>
        /// ページリストのリスト項目表示形式
        /// </summary>
        public PanelListItemStyle PanelListItemStyle
        {
            get { return _panelListItemStyle; }
            set { SetProperty(ref _panelListItemStyle, value); }
        }

        /// <summary>
        /// ページ名表示形式
        /// </summary>
        public PageNameFormat Format
        {
            get { return _format; }
            set { SetProperty(ref _format, value); }
        }
    }

}



