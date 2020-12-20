using NeeView.Windows.Controls;
using NeeView.Windows.Property;
using System;
using System.Text.Json.Serialization;
using System.Windows;

namespace NeeView
{
    public class BookshelfConfig : FolderListConfig
    {
        private string _home;
        private bool _IsVisibleItemsCount;
        private bool _isVisibleHistoryMark = true;
        private bool _isVisibleBookmarkMark = true;
        private string _excludePattern;
        private bool _isSyncFolderTree;
        private bool _isCloseBookWhenMove;
        private bool _isOpenNextBookWhenRemove = true;
        private bool _isInsertItem = true;
        private bool _isMultipleRarFilterEnabled;
        private bool _isCruise;
        private bool _isIncrementalSearchEnabled = true;
        private bool _isSearchIncludeSubdirectories = true;
        private FolderOrder _defaultFolderOrder;
        private FolderOrder _playlistFolderOrder;
        private bool _isOrderWithoutFileType;


        /// <summary>
        /// ホームのパス
        /// </summary>
        [PropertyPath(FileDialogType = FileDialogType.Directory)]
        public string Home
        {
            get { return _home; }
            set { if (_home != value) { _home = value; RaisePropertyChanged(); } }
        }

        /// <summary>
        /// コレクションアイテム数の表示
        /// </summary>
        [PropertyMember]
        public bool IsVisibleItemsCount
        {
            get { return _IsVisibleItemsCount; }
            set { SetProperty(ref _IsVisibleItemsCount, value); }
        }

        /// <summary>
        /// 項目に履歴記号を表示する
        /// </summary>
        [PropertyMember]
        public bool IsVisibleHistoryMark
        {
            get { return _isVisibleHistoryMark; }
            set { SetProperty(ref _isVisibleHistoryMark, value); }
        }

        /// <summary>
        /// 項目にブックマーク記号を表示する
        /// </summary>
        [PropertyMember]
        public bool IsVisibleBookmarkMark
        {
            get { return _isVisibleBookmarkMark; }
            set { SetProperty(ref _isVisibleBookmarkMark, value); }
        }

        /// <summary>
        /// フォルダーツリーと連動する
        /// </summary>
        [PropertyMember]
        public bool IsSyncFolderTree
        {
            get { return _isSyncFolderTree; }
            set { SetProperty(ref _isSyncFolderTree, value); }
        }

        /// <summary>
        /// 項目移動したら閲覧中のブックを閉じる
        /// </summary>
        [PropertyMember]
        public bool IsCloseBookWhenMove
        {
            get { return _isCloseBookWhenMove; }
            set { SetProperty(ref _isCloseBookWhenMove, value); }
        }

        /// <summary>
        /// 閲覧中のブックを削除したら項目移動
        /// </summary>
        [PropertyMember]
        public bool IsOpenNextBookWhenRemove
        {
            get { return _isOpenNextBookWhenRemove; }
            set { SetProperty(ref _isOpenNextBookWhenRemove, value); }
        }

        /// <summary>
        /// 追加されたファイルを挿入する？
        /// OFFにするとリスト末尾に追加する
        /// </summary>
        [PropertyMember]
        public bool IsInsertItem
        {
            get { return _isInsertItem; }
            set { SetProperty(ref _isInsertItem, value); }
        }

        /// <summary>
        /// 分割RARファイルの場合、先頭のファイルのみを表示
        /// </summary>
        [PropertyMember]
        public bool IsMultipleRarFilterEnabled
        {
            get { return _isMultipleRarFilterEnabled; }
            set { SetProperty(ref _isMultipleRarFilterEnabled, value); }
        }

        /// <summary>
        /// サブフォルダーを含めた巡回移動
        /// </summary>
        [PropertyMember]
        public bool IsCruise
        {
            get { return _isCruise; }
            set { SetProperty(ref _isCruise, value); }
        }

        /// <summary>
        /// 項目除外パターン
        /// </summary>
        [PropertyMember]
        public string ExcludePattern
        {
            get { return _excludePattern; }
            set { SetProperty(ref _excludePattern, value); }
        }

        /// <summary>
        /// インクリメンタルサーチ有効
        /// </summary>
        [PropertyMember]
        public bool IsIncrementalSearchEnabled
        {
            get { return _isIncrementalSearchEnabled; }
            set { SetProperty(ref _isIncrementalSearchEnabled, value); }
        }

        /// <summary>
        /// サブフォルダーを含めた検索を行う
        /// </summary>
        [PropertyMember]
        public bool IsSearchIncludeSubdirectories
        {
            get { return _isSearchIncludeSubdirectories; }
            set { SetProperty(ref _isSearchIncludeSubdirectories, value); }
        }

        /// <summary>
        /// 既定の並び順
        /// </summary>
        [PropertyMember]
        public FolderOrder DefaultFolderOrder
        {
            get { return _defaultFolderOrder; }
            set { SetProperty(ref _defaultFolderOrder, value); }
        }

        /// <summary>
        /// プレイリストの既定の並び順
        /// </summary>
        [PropertyMember]
        public FolderOrder PlaylistFolderOrder
        {
            get { return _playlistFolderOrder; }
            set { SetProperty(ref _playlistFolderOrder, value); }
        }

        /// <summary>
        /// ファイルタイプを考慮しない並び替え
        /// </summary>
        [PropertyMember]
        public bool IsOrderWithoutFileType
        {
            get { return _isOrderWithoutFileType; }
            set { SetProperty(ref _isOrderWithoutFileType, value); }
        }



        #region 非公開パラメーター


#if false
        /// <summary>
        /// ページリストをドッキング
        /// </summary>
        [Obsolete] // ver.38
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingDefault)]
        public bool IsPageListDocked
        {
            get { return _isPlacedInBookshelf; }
            set { SetProperty(ref _isPlacedInBookshelf, value); }
        }

        [Obsolete] // ver.38
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingDefault)]
        public bool IsPageListVisible
        {
            get { return _isPageListVisible; }
            set { SetProperty(ref _isPageListVisible, value); }
        }

        [Obsolete] // ver.38
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingDefault)]
        public GridLength GridLength0 { get; set; } = new GridLength(1, GridUnitType.Star);

        [Obsolete] // ver.38
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingDefault)]
        public GridLength GridLength2 { get; set; } = new GridLength(1, GridUnitType.Star);
#endif

        #endregion

    }

}


