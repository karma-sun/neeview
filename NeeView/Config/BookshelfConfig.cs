using NeeView.Windows.Controls;
using NeeView.Windows.Property;
using System.Windows;

namespace NeeView
{
    public class BookshelfConfig : FolderListConfig
    {
        private string _home;
        private bool _isVisibleHistoryMark = true;
        private bool _isVisibleBookmarkMark = true;
        private bool _isPlacedInBookshelf = true;
        private string _excludePattern;
        private bool _isPageListVisible;
        private bool _isSyncFolderTree;
        private bool _isCloseBookWhenMove;
        private bool _isOpenNextBookWhenRemove = true;
        private bool _isInsertItem = true;
        private bool _isMultipleRarFilterEnabled;
        private bool _isCruise;
        private bool _isIncrementalSearchEnabled = true;
        private bool _isSearchIncludeSubdirectories = true;


        /// <summary>
        /// ホームのパス
        /// </summary>
        [PropertyPath("@ParamBookshelfHome", FileDialogType = FileDialogType.Directory)]
        public string Home
        {
            get { return _home; }
            set { if (_home != value) { _home = value; RaisePropertyChanged(); } }
        }

        /// <summary>
        /// 項目に履歴記号を表示する
        /// </summary>
        [PropertyMember("@ParamBookshelfIsVisibleHistoryMark", Tips = "@ParamBookshelfIsVisibleHistoryMarkTips")]
        public bool IsHistoryMark
        {
            get { return _isVisibleHistoryMark; }
            set { SetProperty(ref _isVisibleHistoryMark, value); }
        }

        /// <summary>
        /// 項目にブックマーク記号を表示する
        /// </summary>
        [PropertyMember("@ParamBookshelfIsVisibleBookmarkMark", Tips = "@ParamBookshelfIsVisibleBookmarkMarkTips")]
        public bool IsBookmarkMark
        {
            get { return _isVisibleBookmarkMark; }
            set { SetProperty(ref _isVisibleBookmarkMark, value); }
        }

        /// <summary>
        /// フォルダーツリーと連動する
        /// </summary>
        [PropertyMember("@ParamBookshelfIsSyncFolderTree")]
        public bool IsSyncFolderTree
        {
            get { return _isSyncFolderTree; }
            set { SetProperty(ref _isSyncFolderTree, value); }
        }

        /// <summary>
        /// ページリストをドッキング
        /// </summary>
        [PropertyMember("@ParamPageListPlacementInBookshelf", Tips = "@ParamPageListPlacementInBookshelfTips")]
        public bool IsPageListDocked
        {
            get { return _isPlacedInBookshelf; }
            set { SetProperty(ref _isPlacedInBookshelf, value); }
        }

        public bool IsPageListVisible
        {
            get { return _isPageListVisible; }
            set { SetProperty(ref _isPageListVisible, value); }
        }

        /// <summary>
        /// 項目移動したら閲覧中のブックを閉じる
        /// </summary>
        [PropertyMember("@ParamBookshelfIsCloseBookWhenMove")]
        public bool IsCloseBookWhenMove
        {
            get { return _isCloseBookWhenMove; }
            set { SetProperty(ref _isCloseBookWhenMove, value); }
        }

        /// <summary>
        /// 閲覧中のブックを削除したら項目移動
        /// </summary>
        [PropertyMember("@ParamBookshelfIsOpenNextBookWhenRemove")]
        public bool IsOpenNextBookWhenRemove
        {
            get { return _isOpenNextBookWhenRemove; }
            set { SetProperty(ref _isOpenNextBookWhenRemove, value); }
        }

        /// <summary>
        /// 追加されたファイルを挿入する？
        /// OFFにするとリスト末尾に追加する
        /// </summary>
        [PropertyMember("@ParamBookshelfIsInsertItem", Tips = "@ParamBookshelfIsInsertItemTips")]
        public bool IsInsertItem
        {
            get { return _isInsertItem; }
            set { SetProperty(ref _isInsertItem, value); }
        }

        /// <summary>
        /// 分割RARファイルの場合、先頭のファイルのみを表示
        /// </summary>
        [PropertyMember("@ParamBookshelfIsMultipleRarFilterEnabled", Tips = "@ParamBookshelfIsMultipleRarFilterEnabledTips")]
        public bool IsMultipleRarFilterEnabled
        {
            get { return _isMultipleRarFilterEnabled; }
            set { SetProperty(ref _isMultipleRarFilterEnabled, value); }
        }

        /// <summary>
        /// サブフォルダーを含めた巡回移動
        /// </summary>
        [PropertyMember("@ParamBookshelfIsCruise", Tips = "@ParamBookshelfIsCruiseTips")]
        public bool IsCruise
        {
            get { return _isCruise; }
            set { SetProperty(ref _isCruise, value); }
        }

        /// <summary>
        /// 項目除外パターン
        /// </summary>
        [PropertyMember("@ParamBookshelfExcludePattern", Tips = "@ParamBookshelfExcludePatternTips")]
        public string ExcludePattern
        {
            get { return _excludePattern; }
            set { SetProperty(ref _excludePattern, value); }
        }

        /// <summary>
        /// インクリメンタルサーチ有効
        /// </summary>
        public bool IsIncrementalSearchEnabled
        {
            get { return _isIncrementalSearchEnabled; }
            set { SetProperty(ref _isIncrementalSearchEnabled, value); }
        }

        /// <summary>
        /// サブフォルダーを含めた検索を行う
        /// </summary>
        public bool IsSearchIncludeSubdirectories
        {
            get { return _isSearchIncludeSubdirectories; }
            set { SetProperty(ref _isSearchIncludeSubdirectories, value); }
        }

        #region 非公開パラメーター

        [PropertyMapIgnore]
        public GridLength GridLength0 { get; set; } = new GridLength(1, GridUnitType.Star);

        [PropertyMapIgnore]
        public GridLength GridLength2 { get; set; } = new GridLength(1, GridUnitType.Star);

        #endregion

    }

}


