using NeeLaboratory.ComponentModel;
using NeeView.Windows.Property;
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

        public PanelsConfig Panels { get; set; } = new PanelsConfig();

        public BookshelfPanelConfig Bookshelf { get; set; } = new BookshelfPanelConfig();

        public BookmarkPanelConfig Bookmark { get; set; } = new BookmarkPanelConfig();

        public InformationPanelConfig Information { get; set; } = new InformationPanelConfig();
    }


    public class InformationPanelConfig : BindableBase
    {
        private bool _isVisibleBitsPerPixel;
        private bool _isVisibleLoader;
        private bool _isVisibleFilePath;

        [PropertyMember("@ParamFileInformationIsVisibleBitsPerPixel")]
        public bool IsVisibleBitsPerPixel
        {
            get { return _isVisibleBitsPerPixel; }
            set { if (_isVisibleBitsPerPixel != value) { _isVisibleBitsPerPixel = value; RaisePropertyChanged(); } }
        }

        [PropertyMember("@ParamFileInformationIsVisibleLoader")]
        public bool IsVisibleLoader
        {
            get { return _isVisibleLoader; }
            set { if (_isVisibleLoader != value) { _isVisibleLoader = value; RaisePropertyChanged(); } }
        }

        [PropertyMember("@ParamFileInformationIsVisibleFilePath")]
        public bool IsVisibleFilePath
        {
            get { return _isVisibleFilePath; }
            set { if (_isVisibleFilePath != value) { _isVisibleFilePath = value; RaisePropertyChanged(); } }
        }
    }
}


