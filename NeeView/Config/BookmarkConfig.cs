using NeeLaboratory.ComponentModel;
using NeeView.Windows.Controls;
using NeeView.Windows.Property;
using System.Text.Json.Serialization;

namespace NeeView
{
    public class BookmarkConfig : FolderListConfig
    {
        private bool _isSaveBookmark = true;
        private bool _isSyncBookshelfEnabled = true;
        private FolderOrder _bookmarkFolderOrder;

        [JsonInclude, JsonPropertyName(nameof(BookmarkFilePath))]
        public string _bookmarkFilePath;


        /// <summary>
        /// 本の読み込みで本棚の更新を要求する
        /// </summary>
        [PropertyMember]
        public bool IsSyncBookshelfEnabled
        {
            get { return _isSyncBookshelfEnabled; }
            set { SetProperty(ref _isSyncBookshelfEnabled, value); }
        }

        // ブックマークの保存
        [PropertyMember]
        public bool IsSaveBookmark
        {
            get { return _isSaveBookmark; }
            set { SetProperty(ref _isSaveBookmark, value); }
        }

        // ブックマークの保存場所
        [JsonIgnore]
        [PropertyPath(FileDialogType = FileDialogType.SaveFile, Filter = "JSON|*.json")]
        public string BookmarkFilePath
        {
            get { return _bookmarkFilePath ?? SaveData.DefaultBookmarkFilePath; }
            set { SetProperty(ref _bookmarkFilePath, string.IsNullOrWhiteSpace(value) || value.Trim() == SaveData.DefaultBookmarkFilePath ? null : value.Trim()); }
        }

        // ブックマークの既定の並び順
        [PropertyMember]
        public FolderOrder BookmarkFolderOrder
        {
            get { return _bookmarkFolderOrder; }
            set { SetProperty(ref _bookmarkFolderOrder, value); }
        }

    }
}

