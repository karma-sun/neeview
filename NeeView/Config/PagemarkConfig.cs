using NeeLaboratory.ComponentModel;
using NeeView.Windows.Controls;
using NeeView.Windows.Property;

namespace NeeView
{
    public class PagemarkConfig : BindableBase
    {
        private bool _isSavePagemark = true;
        private string _pagemarkFilePath;

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
    }
}

