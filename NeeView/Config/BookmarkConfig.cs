using NeeLaboratory.ComponentModel;
using NeeView.Windows.Controls;
using NeeView.Windows.Property;

namespace NeeView
{
    public class BookmarkConfig : FolderListConfig
    {
        private bool _isSaveBookmark = true;
        private string _bookmarkFilePath;

        private bool _isSyncBookshelfEnabled = true;


        /// <summary>
        /// 本の読み込みで本棚の更新を要求する
        /// </summary>
        public bool IsSyncBookshelfEnabled
        {
            get { return _isSyncBookshelfEnabled; }
            set { SetProperty(ref _isSyncBookshelfEnabled, value); }
        }

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

