namespace NeeView
{
    public class BookmarkPanelConfig : FolderListConfig
    {
        private bool _isSyncBookshelfEnabled = true;


        /// <summary>
        /// 本の読み込みで本棚の更新を要求する
        /// </summary>
        public bool IsSyncBookshelfEnabled
        {
            get { return _isSyncBookshelfEnabled; }
            set { SetProperty(ref _isSyncBookshelfEnabled, value); }
        }
    }

}


