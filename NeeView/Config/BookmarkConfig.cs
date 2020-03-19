using NeeLaboratory.ComponentModel;
using NeeView.Windows.Controls;
using NeeView.Windows.Property;

namespace NeeView
{
    public class BookmarkConfig : BindableBase
    {
        private bool _isSaveBookmark = true;
        private string _bookmarkFilePath;

        // ブックマークの保存
        [PropertyMember("@ParamIsSaveBookmark")]
        public bool IsSaveBookmark
        {
            get { return _isSaveBookmark; }
            set { SetProperty(ref _isSaveBookmark, value); }
        }

        // ブックマークの保存場所
        [PropertyPath("@ParamBookmarkFilePath", FileDialogType = FileDialogType.SaveFile, Filter = "XML|*.xml")]
        public string BookmarkFilePath
        {
            get => _bookmarkFilePath;
            set => _bookmarkFilePath = string.IsNullOrWhiteSpace(value) || value == SaveData.DefaultBookmarkFilePath ? null : value;
        }
    }
}

