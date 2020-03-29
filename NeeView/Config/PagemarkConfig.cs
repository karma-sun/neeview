using NeeLaboratory.ComponentModel;
using NeeView.Windows.Controls;
using NeeView.Windows.Property;

namespace NeeView
{
    public class PagemarkConfig : BindableBase
    {
        private PanelListItemStyle _panelListItemStyle;
        private bool _isSavePagemark = true;
        private string _pagemarkFilePath;
        private PagemarkOrder _pagemarkOrder;


        public PanelListItemStyle PanelListItemStyle
        {
            get { return _panelListItemStyle; }
            set { SetProperty(ref _panelListItemStyle, value); }
        }


        [PropertyMember("@ParamIsSavePagemark")]
        public bool IsSavePagemark
        {
            get { return _isSavePagemark; }
            set { SetProperty(ref _isSavePagemark, value); }
        }

        // ページマークの保存場所
        [PropertyPath("@ParamPagemarkFilePath", FileDialogType = FileDialogType.SaveFile, Filter = "XML|*.xml")]
        public string PagemarkFilePath
        {
            get => _pagemarkFilePath;
            set => _pagemarkFilePath = string.IsNullOrWhiteSpace(value) || value == SaveData.DefaultPagemarkFilePath ? null : value;
        }

        // ページマークの並び順
        public PagemarkOrder PagemarkOrder
        {
            get { return _pagemarkOrder; }
            set { SetProperty(ref _pagemarkOrder, value); }
        }
    }
}

