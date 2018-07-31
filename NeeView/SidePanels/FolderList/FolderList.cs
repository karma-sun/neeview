using NeeLaboratory.ComponentModel;
using NeeView.Collections.Generic;
using NeeView.Windows.Property;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;

namespace NeeView
{
    //
    public class SelectedChangedEventArgs : EventArgs
    {
        public bool IsFocus { get; set; }
        public bool IsNewFolder { get; set; }
    }

    //
    public class BusyChangedEventArgs : EventArgs
    {
        public bool IsBusy { get; set; }

        public BusyChangedEventArgs(bool isBusy)
        {
            this.IsBusy = isBusy;
        }
    }

    //
    public class FolderItemPosition
    {
        public FolderItemPosition(QueryPath path)
        {
            this.Path = path;
            this.Index = -1;
        }

        public FolderItemPosition(QueryPath path, int index)
        {
            this.Path = path;
            this.Index = index;
        }

        public QueryPath Path { get; set; }
        public QueryPath TargetPath { get; set; }
        public int Index { get; set; }
    }


    //
    public class FolderList : BindableBase, IDisposable
    {
        public static FolderList Current { get; private set; }

        #region Fields

        private BookHub _bookHub;

        /// <summary>
        /// そのフォルダーで最後に選択されていた項目の記憶
        /// </summary>
        private Dictionary<QueryPath, FolderItemPosition> _lastPlaceDictionary = new Dictionary<QueryPath, FolderItemPosition>();

        /// <summary>
        /// 更新フラグ
        /// </summary>
        private bool _isDarty;

        /// <summary>
        /// 検索エンジン
        /// </summary>
        private FolderSearchEngine _searchEngine;

        private CancellationTokenSource _updateFolderCancellationTokenSource;
        private CancellationTokenSource _cruiseFolderCancellationTokenSource;

        #endregion Fields

        #region Constructors

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="bookHub"></param>
        /// <param name="folderPanel"></param>
        public FolderList(BookHub bookHub, FolderPanelModel folderPanel)
        {
            Current = this;

            _searchEngine = new FolderSearchEngine();
            FolderCollectionFactory.Current.SearchEngine = _searchEngine;

            this.FolderPanel = folderPanel;
            _bookHub = bookHub;

            _bookHub.FolderListSync += async (s, e) => await SyncWeak(e);
            _bookHub.HistoryChanged += (s, e) => RefreshIcon(new QueryPath(e.Key));

            _bookHub.BookmarkChanged += (s, e) =>
            {
                switch (e.Action)
                {
                    case EntryCollectionChangedAction.Reset:
                    case EntryCollectionChangedAction.Replace:
                        RefreshIcon(null);
                        break;
                    default:
                        if (e.Item.Value is Bookmark bookmark)
                        {
                            RefreshIcon(new QueryPath(bookmark.Place));
                        }
                        break;
                }
            };

            _bookHub.LoadRequested += (s, e) => CancelMoveCruiseFolder();
        }

        #endregion Constructors

        #region Events

        public event EventHandler PlaceChanged;

        //
        public event EventHandler<SelectedChangedEventArgs> SelectedChanging;
        public event EventHandler<SelectedChangedEventArgs> SelectedChanged;

        // FolderCollection総入れ替え
        public event EventHandler CollectionChanged;

        // 検索ボックスにフォーカスを
        public event EventHandler SearchBoxFocus;

        public event ErrorEventHandler FolderTreeFocus;

        // リスト更新処理中イベント
        public event EventHandler<BusyChangedEventArgs> BusyChanged;

        #endregion

        #region Properties

        //
        public FolderPanelModel FolderPanel { get; private set; }

        //
        private PanelListItemStyle _panelListItemStyle;
        public PanelListItemStyle PanelListItemStyle
        {
            get { return _panelListItemStyle; }
            set { if (_panelListItemStyle != value) { _panelListItemStyle = value; RaisePropertyChanged(); } }
        }

        // サムネイル画像が表示される？？
        public bool IsThumbnailVisibled
        {
            get
            {
                switch (_panelListItemStyle)
                {
                    default:
                        return false;
                    case PanelListItemStyle.Tile:
                        return true;
                    case PanelListItemStyle.Content:
                        return ThumbnailProfile.Current.ThumbnailWidth > 0.0;
                    case PanelListItemStyle.Banner:
                        return ThumbnailProfile.Current.BannerWidth > 0.0;
                }
            }
        }

        /// <summary>
        /// IsVisibleHistoryMark property.
        /// </summary>
        private bool _isVisibleHistoryMark = true;
        [PropertyMember("@ParamFolderListIsVisibleHistoryMark", Tips = "@ParamFolderListIsVisibleHistoryMarkTips")]
        public bool IsVisibleHistoryMark
        {
            get { return _isVisibleHistoryMark; }
            set { if (_isVisibleHistoryMark != value) { _isVisibleHistoryMark = value; RaisePropertyChanged(); } }
        }

        /// <summary>
        /// IsVisibleBookmarkMark property.
        /// </summary>
        private bool _isVisibleBookmarkMark = true;
        [PropertyMember("@ParamFolderListIsVisibleBookmarkMark", Tips = "@ParamFolderListIsVisibleBookmarkMarkTips")]
        public bool IsVisibleBookmarkMark
        {
            get { return _isVisibleBookmarkMark; }
            set { if (_isVisibleBookmarkMark != value) { _isVisibleBookmarkMark = value; RaisePropertyChanged(); } }
        }

        private string _home;
        [PropertyPath("@ParamFolderListHome", IsDirectory = true)]
        public string Home
        {
            get { return _home; }
            set { if (_home != value) { _home = value; RaisePropertyChanged(); } }
        }

        /// <summary>
        /// 追加されたファイルを挿入する？
        /// OFFにするとリスト末尾に追加する
        /// </summary>
        [PropertyMember("@ParamFolderListIsInsertItem", Tips = "@ParamFolderListIsInsertItemTips")]
        public bool IsInsertItem { get; set; } = true;


        [PropertyMember("@ParamFolderListIsMultipleRarFilterEnabled", Tips = "@ParamFolderListIsMultipleRarFilterEnabledTips")]
        public bool IsMultipleRarFilterEnabled { get; set; }


        [PropertyMember("@ParamFolderListIsCruise", Tips = "@ParamFolderListIsCruiseTips")]
        public bool IsCruise { get; set; }

        [PropertyMember("@ParamFolderListIsCloseBookWhenMove")]
        public bool IsCloseBookWhenMove { get; set; }

        [PropertyMember("@ParamFolderListIsSyncFolderTree")]
        public bool IsSyncFolderTree { get; set; }

        private string _excludePattern;
        [PropertyMember("@ParamFolderListExcludePattern", Tips = "@ParamFolderListExcludePatternTips")]
        public string ExcludePattern
        {
            get { return _excludePattern; }
            set
            {
                if (_excludePattern != value)
                {
                    _excludePattern = value;

                    try
                    {
                        _excludeRegex = string.IsNullOrWhiteSpace(_excludePattern) ? null : new Regex(_excludePattern, RegexOptions.IgnoreCase | RegexOptions.Compiled);
                    }
                    catch (Exception ex)
                    {
                        Debug.WriteLine($"FolderList exclute: {ex.Message}");
                        _excludePattern = null;
                    }

                    RaisePropertyChanged();
                    RaisePropertyChanged(nameof(ExcludeRegex));
                }
            }
        }

        // 除外パターンの正規表現
        private Regex _excludeRegex;
        public Regex ExcludeRegex => _excludeRegex;

        /// <summary>
        /// フォルダーコレクション
        /// </summary>
        private FolderCollection _folderCollection;
        public FolderCollection FolderCollection
        {
            get { return _folderCollection; }
            set
            {
                if (_folderCollection != value)
                {
                    _folderCollection?.Dispose();
                    _folderCollection = value;
                    CollectionChanged?.Invoke(this, null);
                    RaisePropertyChanged(nameof(FolderOrder));
                    RaisePropertyChanged(nameof(IsFolderSearchCollection));
                    RaisePropertyChanged(nameof(IsFolderSearchEnabled));
                    RaisePropertyChanged(nameof(IsContextMenuEnabled));
                }
            }
        }

        /// <summary>
        /// リストのコンテキストメニュー表示有効？
        /// </summary>
        public bool IsContextMenuEnabled => FolderCollection is BookmarkFolderCollection;

        /// <summary>
        /// 検索リスト？
        /// </summary>
        public bool IsFolderSearchCollection => FolderCollection is FolderSearchCollection;

        /// <summary>
        /// 検索許可？
        /// </summary>
        public bool IsFolderSearchEnabled => Place?.Path != null && !(FolderCollection is FolderArchiveCollection) && !(FolderCollection is BookmarkFolderCollection);

        /// <summary>
        /// SelectedItem property.
        /// </summary>
        private FolderItem _selectedItem;
        public FolderItem SelectedItem
        {
            get { return _selectedItem; }
            set { if (_selectedItem != value) { _selectedItem = value; RaisePropertyChanged(); } }
        }

        /// <summary>
        /// 現在のフォルダー
        /// </summary>
        public QueryPath Place => this.FolderCollection?.Place;

        /// <summary>
        /// フォルダー履歴
        /// </summary>
        public History<QueryPath> History { get; private set; } = new History<QueryPath>();

        /// <summary>
        /// IsFolderSearchVisible property.
        /// </summary>
        public bool IsFolderSearchBoxVisible => true;

        /// <summary>
        /// SearchKeyword property.
        /// </summary>
        private string _searchKeyword;
        public string SearchKeyword
        {
            get { return _searchKeyword; }
            set { if (_searchKeyword != value) { _searchKeyword = value; RaisePropertyChanged(); var task = UpdateFolderCollectionAsync(false); } }
        }

        // 履歴情報のバインド用
        public BookHistoryCollection BookHistory => BookHistoryCollection.Current;


        /// <summary>
        /// フォルダーツリーの表示
        /// </summary>
        private bool _isFolderTreeVisible = true;
        public bool IsFolderTreeVisible
        {
            get { return _isFolderTreeVisible; }
            set { SetProperty(ref _isFolderTreeVisible, value); }
        }



        private FolderTreeLayout _FolderTreeLayout;
        [PropertyMember("@ParamFolderTreeLayout")]
        public FolderTreeLayout FolderTreeLayout
        {
            get { return _FolderTreeLayout; }
            set
            {
                if (SetProperty(ref _FolderTreeLayout, value))
                {
                    RaisePropertyChanged(nameof(FolderTreeDock));
                    RaisePropertyChanged(nameof(FolderTreeAreaWidth));
                    RaisePropertyChanged(nameof(FolderTreeAreaHeight));
                }
            }
        }

        public Dock FolderTreeDock
        {
            get { return FolderTreeLayout == FolderTreeLayout.Left ? Dock.Left : Dock.Top; }
        }

        /// <summary>
        /// フォルダーツリーエリアの幅
        /// </summary>
        private double _folderTreeAreaWidth = 192.0;
        public double FolderTreeAreaWidth
        {
            get { return _folderTreeAreaWidth; }
            set
            {
                var width = Math.Max(Math.Min(value, _areaWidth - 32.0), 32.0 - 6.0);
                SetProperty(ref _folderTreeAreaWidth, width);
            }
        }

        /// <summary>
        /// フォルダーリストエリアの幅
        /// クイックアクセスエリアの幅計算用
        /// </summary>
        private double _areaWidth = double.PositiveInfinity;
        public double AreaWidth
        {
            get { return _areaWidth; }
            set
            {
                if (SetProperty(ref _areaWidth, value))
                {
                    // 再設定する
                    FolderTreeAreaWidth = _folderTreeAreaWidth;
                }
            }
        }


        /// <summary>
        /// フォルダーツリーエリアの高さ
        /// </summary>
        private double _folderTreeAreaHeight = 72.0;
        public double FolderTreeAreaHeight
        {
            get { return _folderTreeAreaHeight; }
            set
            {
                var height = Math.Max(Math.Min(value, _areaHeight - 32.0), 32.0 - 6.0);
                SetProperty(ref _folderTreeAreaHeight, height);
            }
        }

        /// <summary>
        /// フォルダーリストエリアの高さ
        /// クイックアクセスエリアの高さ計算用
        /// </summary>
        private double _areaHeight = double.PositiveInfinity;
        public double AreaHeight
        {
            get { return _areaHeight; }
            set
            {
                if (SetProperty(ref _areaHeight, value))
                {
                    // 再設定する
                    FolderTreeAreaHeight = _folderTreeAreaHeight;
                }
            }
        }


        /// <summary>
        /// 外部の変化によるフォルダーリストの変更を禁止
        /// </summary>
        private bool _IsLocked;
        public bool IsLocked
        {
            get { return _IsLocked; }
            set { SetProperty(ref _IsLocked, value); }
        }


        public ThumbnailProfile ThumbnailProfile => ThumbnailProfile.Current;


        #endregion

        #region Methods

        /// <summary>
        /// 補正されたHOME取得
        /// </summary>
        /// <returns></returns>
        public QueryPath GetFixedHome()
        {
            var path = new QueryPath(_home);

            switch (path.Scheme)
            {
                case QueryScheme.File:
                    if (Directory.Exists(_home))
                    {
                        return path;
                    }
                    else
                    {
                        return GetDefaultHome();
                    }

                case QueryScheme.Bookmark:
                    if (BookmarkCollection.Current.FindNode(_home)?.Value is BookmarkFolder)
                    {
                        return path;
                    }
                    else
                    {
                        return new QueryPath(QueryScheme.Bookmark, null, null);
                    }

                default:
                    Debug.WriteLine($"Not support yet: {_home}");
                    return GetDefaultHome();
            }
        }

        private QueryPath GetDefaultHome()
        {
            var myPicture = System.Environment.GetFolderPath(System.Environment.SpecialFolder.MyPictures);
            if (Directory.Exists(myPicture))
            {
                return new QueryPath(myPicture);
            }

            // 救済措置
            return new QueryPath(Environment.CurrentDirectory);
        }

        /// <summary>
        /// ふさわしい選択項目インデックスを取得
        /// </summary>
        /// <param name="path">選択したいパス</param>
        /// <returns></returns>
        internal int FixedIndexOfPath(QueryPath path)
        {
            var index = this.FolderCollection.IndexOfPath(path);
            return index < 0 ? 0 : index;
        }

        //
        internal FolderItem FixedItem(FolderItemPosition pos)
        {
            if (pos == null)
            {
                return this.FolderCollection.FirstOrDefault();
            }

            if (pos.Index >= 0)
            {
                var item = this.FolderCollection.Items.ElementAtOrDefault(pos.Index);
                if (item != null && item.Path == pos.Path)
                {
                    return item;
                }
            }

            if (pos.TargetPath != null)
            {
                return this.FolderCollection.Items.FirstOrDefault(e => e.Path == pos.Path && e.TargetPath == pos.TargetPath) ?? this.FolderCollection.FirstOrDefault();
            }
            else
            {
                return this.FolderCollection.Items.FirstOrDefault(e => e.Path == pos.Path) ?? this.FolderCollection.FirstOrDefault();
            }
        }

        /// <summary>
        /// フォルダー状態保存
        /// </summary>
        private void SavePlace(FolderItem folder, int index)
        {
            if (folder == null || folder.Place == null) return;
            _lastPlaceDictionary[folder.Place] = new FolderItemPosition(folder.Path, index);
        }

        /// <summary>
        /// 項目変更前通知
        /// </summary>
        public void RaiseSelectedItemChanging()
        {
            SelectedChanging?.Invoke(this, null);
        }

        /// <summary>
        /// 項目変更後通知
        /// </summary>
        /// <param name="isFocus"></param>
        public void RaiseSelectedItemChanged(bool isFocus = false)
        {
            SelectedChanged?.Invoke(this, new SelectedChangedEventArgs() { IsFocus = isFocus });
        }


        /// <summary>
        /// 場所の初期化。
        /// nullを指定した場合、HOMEフォルダに移動。
        /// </summary>
        public void ResetPlace(string queryPath)
        {
            if (IsLocked)
            {
                return;
            }

            var path = queryPath != null ? new QueryPath(queryPath) : GetFixedHome();

            var task = SetPlaceAsync(path, null, FolderSetPlaceOption.UpdateHistory);
        }

        /// <summary>
        /// フォルダーリスト更新要求
        /// </summary>
        public void RequestPlace(QueryPath path, FolderItemPosition select, FolderSetPlaceOption options)
        {
            if (IsLocked)
            {
                return;
            }

            var task = SetPlaceAsync(path, select, options);
        }

        /// <summary>
        /// フォルダーリスト更新
        /// </summary>
        /// <param name="place">フォルダーパス</param>
        /// <param name="select">初期選択項目</param>
        public async Task SetPlaceAsync(QueryPath path, FolderItemPosition select, FolderSetPlaceOption options)
        {
            if (path == null)
            {
                return;
            }

            // 可能であればファイルパスをブックマークフォルダーパスに変換
            if (FolderCollection is BookmarkFolderCollection && !options.HasFlag(FolderSetPlaceOption.FileSystem))
            {
                if (path.Scheme == QueryScheme.File && select != null && select.Path.Scheme == QueryScheme.File)
                {
                    var node = BookmarkCollection.Current.FindNode(select.Path);
                    if (node != null && node.Parent != null)
                    {
                        path = node.Parent.CreateQuery(QueryScheme.Bookmark);
                        select = new FolderItemPosition(node.CreateQuery(QueryScheme.Bookmark)) { TargetPath = select.Path };
                    }
                }
            }

            // 現在フォルダーの情報を記憶
            SavePlace(this.SelectedItem, GetFolderItemIndex(this.SelectedItem));

            // 初期項目
            if (select == null)
            {
                _lastPlaceDictionary.TryGetValue(path, out select);
            }

            if (options.HasFlag(FolderSetPlaceOption.TopSelect))
            {
                select = null;
            }


            // 更新が必要であれば、新しいFolderListBoxを作成する
            if (CheckFolderListUpdateneNcessary(path, options))
            {
                _isDarty = false;

                // FolderCollection 更新
                var isSuccess = await UpdateFolderCollectionAsync(path, true);
                if (isSuccess)
                {
                    this.SelectedItem = FixedItem(select);

                    RaiseSelectedItemChanged(options.HasFlag(FolderSetPlaceOption.Focus));

                    // 最終フォルダー更新
                    BookHistoryCollection.Current.LastFolder = Place.FullQuery;

                    // 履歴追加
                    if (options.HasFlag(FolderSetPlaceOption.UpdateHistory))
                    {
                        if (Place != this.History.GetCurrent())
                        {
                            this.History.Add(Place);
                        }
                    }

                    // 検索キーワード更新
                    if (Place.Search != GetFixedSearchKeyword())
                    {
                        UpdateSearchHistory();
                        _searchKeyword = Place.Search;
                        RaisePropertyChanged(nameof(SearchKeyword));
                    }

                    PlaceChanged?.Invoke(this, null);
                }
            }
            else
            {
                // 選択項目のみ変更
                this.SelectedItem = FixedItem(select);
                PlaceChanged?.Invoke(this, null);
            }
        }

        /// <summary>
        /// リストの更新必要性チェック
        /// </summary>
        private bool CheckFolderListUpdateneNcessary(QueryPath path, FolderSetPlaceOption options)
        {
            if (_isDarty || this.FolderCollection == null || path != this.FolderCollection.Place || options.HasFlag(FolderSetPlaceOption.Refresh))
            {
                return true;
            }

            return false;
        }

        // となりを取得
        public FolderItem GetNeighbor(FolderItem item)
        {
            var items = this.FolderCollection?.Items;
            if (items == null || items.Count <= 0) return null;

            int index = items.IndexOf(item);
            if (index < 0) return items[0];

            if (index + 1 < items.Count)
            {
                return items[index + 1];
            }
            else if (index > 0)
            {
                return items[index - 1];
            }
            else
            {
                return item;
            }
        }

        private void FolderCollection_CollectionChanging(object sender, FolderCollectionChangedEventArgs e)
        {
            if (e.Action == CollectionChangeAction.Remove)
            {
                SelectedChanging?.Invoke(this, new SelectedChangedEventArgs());
                if (SelectedItem == e.Item)
                {
                    SelectedItem = GetNeighbor(SelectedItem);
                }
            }
        }

        private void FolderCollection_CollectionChanged(object sender, FolderCollectionChangedEventArgs e)
        {
            if (e.Action == CollectionChangeAction.Remove)
            {
                if (SelectedItem == null)
                {
                    SelectedItem = FolderCollection.Items?.FirstOrDefault();
                }
                SelectedChanged?.Invoke(this, new SelectedChangedEventArgs());
            }
        }


        /// <summary>
        /// フォルダーリスト更新
        /// </summary>
        /// <param name="force">必要が無い場合も更新する</param>
        public async Task RefreshAsync(bool force)
        {
            if (this.FolderCollection == null) return;

            _isDarty = force || this.FolderCollection.IsDarty();

            await SetPlaceAsync(Place, null, FolderSetPlaceOption.UpdateHistory);
        }

        /// <summary>
        /// 選択項目を基準とした項目取得
        /// </summary>
        /// <param name="offset">選択項目から前後した項目を指定</param>
        /// <returns></returns>
        internal FolderItem GetFolderItem(FolderItem item, int offset)
        {
            if (this.FolderCollection?.Items == null) return null;

            int index = this.FolderCollection.Items.IndexOf(item);
            if (index < 0) return null;

            int next = (this.FolderCollection.FolderParameter.FolderOrder == FolderOrder.Random)
                ? (index + this.FolderCollection.Items.Count + offset) % this.FolderCollection.Items.Count
                : index + offset;

            if (next < 0 || next >= this.FolderCollection.Items.Count) return null;

            return this.FolderCollection[next];
        }

        internal int GetFolderItemIndex(FolderItem item)
        {
            if (this.FolderCollection?.Items == null) return -1;

            return this.FolderCollection.Items.IndexOf(item);
        }


        /// <summary>
        /// 現在開いているフォルダーで更新(弱)
        /// e.isKeepPlaceが有効の場合、すでにリストに存在している場合は何もしない
        /// </summary>
        public async Task SyncWeak(FolderListSyncEventArgs e)
        {
            if (IsLocked)
            {
                return;
            }

            // TODO: 
            var parent = new QueryPath(e.Parent);
            var path = new QueryPath(e.Path);

            if (e != null && e.isKeepPlace)
            {
                // すでにリストに存在している場合は何もしない
                if (this.FolderCollection == null || this.FolderCollection.Contains(path)) return;
            }

            var options = FolderSetPlaceOption.UpdateHistory;

            if (this.FolderCollection != null)
            {
                if (this.FolderCollection.Place.FullPath == parent.FullPath && this.FolderCollection.Contains(path))
                {
                    await SetPlaceAsync(this.FolderCollection.Place, new FolderItemPosition(path), options);
                    return;
                }
            }

            await SetPlaceAsync(parent, new FolderItemPosition(path), options);
        }

        /// <summary>
        /// フォルダーアイコンの表示更新
        /// </summary>
        /// <param name="path">更新するパス。nullならば全て更新</param>
        public void RefreshIcon(QueryPath path)
        {
            this.FolderCollection?.RefreshIcon(path);
        }

        // ブックの読み込み
        public void LoadBook(FolderItem item)
        {
            if (item == null) return;

            BookLoadOption option = BookLoadOption.SkipSamePlace | (this.FolderCollection.FolderParameter.IsFolderRecursive ? BookLoadOption.DefaultRecursive : BookLoadOption.None);
            LoadBook(item, option);
        }

        // ブックの読み込み
        public void LoadBook(FolderItem item, BookLoadOption option)
        {
            if (item.Attributes.HasFlag(FolderItemAttribute.System))
            {
                return;
            }

            var query = item.TargetPath;
            if (query.Path == null)
            {
                return;
            }

            _bookHub.RequestLoad(query.SimplePath, null, option | BookLoadOption.IsBook, false);
        }

        // 現在の場所のフォルダーの並び順
        public FolderOrder FolderOrder
        {
            get { return GetFolderOrder(); }
        }

        /// <summary>
        /// フォルダーの並びを設定
        /// </summary>
        public void SetFolderOrder(FolderOrder folderOrder)
        {
            if (FolderCollection == null) return;
            this.FolderCollection.FolderParameter.FolderOrder = folderOrder;
            RaisePropertyChanged(nameof(FolderOrder));
        }

        /// <summary>
        /// フォルダーの並びを取得
        /// </summary>
        public FolderOrder GetFolderOrder()
        {
            if (this.FolderCollection == null) return default(FolderOrder);
            return this.FolderCollection.FolderParameter.FolderOrder;
        }

        /// <summary>
        /// フォルダーの並びを順番に切り替える
        /// </summary>
        public void ToggleFolderOrder()
        {
            if (this.FolderCollection == null) return;
            this.FolderCollection.FolderParameter.FolderOrder.GetToggle();
            RaisePropertyChanged(nameof(FolderOrder));
        }

        /// <summary>
        /// 検索ボックスにフォーカス要求
        /// </summary>
        public void RaiseSearchBoxFocus()
        {
            SearchBoxFocus?.Invoke(this, null);
        }

        /// <summary>
        /// フォルダーツリーにフォーカス要求
        /// </summary>
        public void RaiseFolderTreeFocus()
        {
            FolderTreeFocus?.Invoke(this, null);
        }

        /// <summary>
        /// 検索キーワードの正規化
        /// </summary>
        private string GetFixedSearchKeyword()
        {
            return _searchKeyword?.Trim();
        }

        /// <summary>
        /// 検索履歴更新
        /// </summary>
        public void UpdateSearchHistory()
        {
            var keyword = GetFixedSearchKeyword();
            BookHistoryCollection.Current.AddSearchHistory(keyword);
        }

        /// <summary>
        /// 現在の場所の履歴を削除
        /// </summary>
        public void ClearHistory()
        {
            var items = FolderCollection?.Items.Select(e => e.TargetPath.SimplePath).Where(e => e != null);
            BookHistoryCollection.Current.Remove(items);

            RefreshIcon(null);
        }

        #endregion

        #region MoveFolder

        // 次のフォルダーに移動
        public async Task NextFolder(BookLoadOption option = BookLoadOption.None)
        {
            if (_bookHub.IsBusy()) return; // 相対移動の場合はキャンセルしない
            var result = await MoveFolder(+1, option);
            if (result != true)
            {
                SoundPlayerService.Current.PlaySeCannotMove();
                InfoMessage.Current.SetMessage(InfoMessageType.Notify, Properties.Resources.NotifyBookNextFailed);
            }
        }

        // 前のフォルダーに移動
        public async Task PrevFolder(BookLoadOption option = BookLoadOption.None)
        {
            if (_bookHub.IsBusy()) return; // 相対移動の場合はキャンセルしない
            var result = await MoveFolder(-1, option);
            if (result != true)
            {
                SoundPlayerService.Current.PlaySeCannotMove();
                InfoMessage.Current.SetMessage(InfoMessageType.Notify, Properties.Resources.NotifyBookPrevFailed);
            }
        }


        /// <summary>
        /// コマンドの「前のフォルダーに移動」「次のフォルダーへ移動」に対応
        /// </summary>
        public async Task<bool> MoveFolder(int direction, BookLoadOption options)
        {
            var isCruise = IsCruise && !(this.FolderCollection is FolderSearchCollection);

            if (isCruise)
            {
                return await MoveCruiseFolder(direction, options);
            }
            else
            {
                return await MoveNextFolder(direction, options);
            }
        }

        /// <summary>
        /// 通常フォルダー移動
        /// </summary>
        public async Task<bool> MoveNextFolder(int direction, BookLoadOption options)
        {
            var item = GetFolderItem(this.SelectedItem, direction);
            if (item == null)
            {
                return false;
            }

            int index = GetFolderItemIndex(item);

            await SetPlaceAsync(this.FolderCollection.Place, new FolderItemPosition(item.Path, index), FolderSetPlaceOption.UpdateHistory);
            _bookHub.RequestLoad(item.TargetPath.SimplePath, null, options | BookLoadOption.IsBook, false);
            return true;
        }


        /// <summary>
        /// 巡回フォルダー移動
        /// </summary>
        public async Task<bool> MoveCruiseFolder(int direction, BookLoadOption options)
        {
            // TODO: NowLoad表示をどうしよう。BookHubに処理を移してそこで行う？

            var item = this.SelectedItem;
            if (item == null) return false;

            _cruiseFolderCancellationTokenSource?.Cancel();
            _cruiseFolderCancellationTokenSource = new CancellationTokenSource();

            try
            {
                var node = new FolderNode(this.FolderCollection, item);
                var cancel = _cruiseFolderCancellationTokenSource.Token;
                var next = (direction < 0) ? await node.CruisePrev(cancel) : await node.CruiseNext(cancel);
                if (next == null) return false;
                if (next.Content == null) return false;

                await SetPlaceAsync(next.Content.Place, new FolderItemPosition(next.Content.Path) { TargetPath = next.Content.TargetPath }, FolderSetPlaceOption.UpdateHistory);
                _bookHub.RequestLoad(next.Content.TargetPath.SimplePath, null, options | BookLoadOption.IsBook, false);

                return true;
            }
            catch (OperationCanceledException)
            {
                return false;
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Cruise Exception: {ex.Message}");
                return false;
            }
        }

        /// <summary>
        /// 巡回フォルダー移動キャンセル
        /// </summary>
        public void CancelMoveCruiseFolder()
        {
            _cruiseFolderCancellationTokenSource?.Cancel();
        }

        #endregion

        #region UpdateFolderCollection

        /// <summary>
        /// コレクション更新
        /// </summary>
        public async Task<bool> UpdateFolderCollectionAsync(bool isForce)
        {
            var path = Place.ReplaceSearch(GetFixedSearchKeyword());

            return await UpdateFolderCollectionAsync(path, isForce);
        }

        /// <summary>
        /// コレクション更新
        /// </summary>
        public async Task<bool> UpdateFolderCollectionAsync(QueryPath path, bool isForce)
        {
            try
            {
                BusyChanged?.Invoke(this, new BusyChangedEventArgs(true));

                _updateFolderCancellationTokenSource?.Cancel();
                _updateFolderCancellationTokenSource = new CancellationTokenSource();

                var collection = await CreateFolderCollectionAsync(path, isForce, _updateFolderCancellationTokenSource.Token);

                if (collection != null && !_updateFolderCancellationTokenSource.Token.IsCancellationRequested)
                {
                    collection.ParameterChanged += async (s, e) => await RefreshAsync(true);
                    collection.CollectionChanging += FolderCollection_CollectionChanging;
                    collection.CollectionChanged += FolderCollection_CollectionChanged;
                    this.FolderCollection = collection;
                    return true;
                }
                else
                {
                    return false;
                }
            }
            catch (OperationCanceledException)
            {
                Debug.WriteLine($"UpdateFolderCollectionAsync: Canceled: {path}");
                return false;
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"UpdateFolderCollectionAsync: {ex.Message}");
                return false;
            }
            finally
            {
                BusyChanged?.Invoke(this, new BusyChangedEventArgs(false));
            }
        }

        /// <summary>
        /// コレクション作成
        /// </summary>
        private async Task<FolderCollection> CreateFolderCollectionAsync(QueryPath path, bool isForce, CancellationToken token)
        {
            var factory = FolderCollectionFactory.Current;

            if (!isForce && FolderCollection.Place.Equals(path))
            {
                return null;
            }

            if (path.Search != null && FolderCollection is FolderSearchCollection && FolderCollection.Place.FullPath == path.FullPath)
            {
                ////Debug.WriteLine($"SearchEngine: Cancel");
                factory.SearchEngine.CancelSearch();
            }
            else
            {
                ////Debug.WriteLine($"SearchEngine: Reset");
                factory.SearchEngine.Reset();
            }

            return await factory.CreateFolderCollectionAsync(path, true, token);
        }

        #endregion

        #region Commands

        public void AddQuickAccess()
        {
            IsFolderTreeVisible = true;
            FolderTreeModel.Current.AddQuickAccess(GetCurentQueryPath());
        }

        public string GetCurentQueryPath()
        {
            return Place.SimpleQuery;
        }

        //
        public void SetHome_Executed()
        {
            if (_bookHub == null) return;
            this.Home = Place.SimplePath;
        }

        //
        public async void MoveToHome_Executed()
        {
            if (_bookHub == null) return;

            var place = GetFixedHome();
            await SetPlaceAsync(place, null, FolderSetPlaceOption.Focus | FolderSetPlaceOption.UpdateHistory | FolderSetPlaceOption.TopSelect | FolderSetPlaceOption.ResetKeyword);

            CloseBookIfNecessary();
        }


        //
        public async void MoveTo_Executed(QueryPath path)
        {
            await this.SetPlaceAsync(path, null, FolderSetPlaceOption.Focus | FolderSetPlaceOption.UpdateHistory);

            CloseBookIfNecessary();
        }

        //
        public bool MoveToPrevious_CanExecutre()
        {
            return this.History.CanPrevious();
        }

        //
        public async void MoveToPrevious_Executed()
        {
            if (!this.History.CanPrevious()) return;

            var place = this.History.GetPrevious();
            await SetPlaceAsync(place, null, FolderSetPlaceOption.Focus);
            this.History.Move(-1);

            CloseBookIfNecessary();
        }

        //
        public bool MoveToNext_CanExecute()
        {
            return this.History.CanNext();
        }

        //
        public async void MoveToNext_Executed()
        {
            if (!this.History.CanNext()) return;

            var place = this.History.GetNext();
            await SetPlaceAsync(place, null, FolderSetPlaceOption.Focus);
            this.History.Move(+1);

            CloseBookIfNecessary();
        }

        //
        public async void MoveToHistory_Executed(KeyValuePair<int, QueryPath> item)
        {
            var place = this.History.GetHistory(item.Key);
            await SetPlaceAsync(place, null, FolderSetPlaceOption.Focus);
            this.History.SetCurrent(item.Key + 1);

            CloseBookIfNecessary();
        }

        //
        public bool MoveToParent_CanExecute()
        {
            return this.FolderCollection?.GetParentQuery() != null;
        }

        //
        public async void MoveToParent_Execute()
        {
            var parent = this.FolderCollection?.GetParentQuery();
            if (parent == null)
            {
                return;
            }

            await SetPlaceAsync(parent, new FolderItemPosition(Place), FolderSetPlaceOption.Focus | FolderSetPlaceOption.UpdateHistory);
            CloseBookIfNecessary();
        }


        //
        public async void Sync_Executed()
        {
            var place = _bookHub?.Book?.Place;

            if (place != null)
            {
                // TODO: Queryの求め方はこれでいいのか？
                var path = new QueryPath(place);
                var parent = new QueryPath(_bookHub?.Book?.Archiver?.Parent?.FullPath ?? LoosePath.GetDirectoryName(place));

                _isDarty = true; // 強制更新
                await SetPlaceAsync(parent, new FolderItemPosition(path), FolderSetPlaceOption.Focus | FolderSetPlaceOption.UpdateHistory | FolderSetPlaceOption.ResetKeyword | FolderSetPlaceOption.FileSystem);

                RaiseSelectedItemChanged(true);
            }
            else if (Place != null)
            {
                _isDarty = true; // 強制更新
                await SetPlaceAsync(Place, null, FolderSetPlaceOption.Focus | FolderSetPlaceOption.FileSystem);

                RaiseSelectedItemChanged(true);
            }

            if (IsSyncFolderTree)
            {
                FolderTreeModel.Current.SyncDirectory(Place.SimplePath);
            }
        }

        //
        public void ToggleFolderRecursive_Executed()
        {
            this.FolderCollection.FolderParameter.IsFolderRecursive = !this.FolderCollection.FolderParameter.IsFolderRecursive;
        }


        private void CloseBookIfNecessary()
        {
            if (IsCloseBookWhenMove)
            {
                BookHub.Current.RequestUnload(true);
            }
        }

        public void NewFolder()
        {
            if (FolderCollection is BookmarkFolderCollection bookmarkFolderCollection)
            {
                var node = BookmarkCollection.Current.AddNewFolder(bookmarkFolderCollection.BookmarkPlace);

                var item = bookmarkFolderCollection.FirstOrDefault(e => e.Attributes.HasFlag(FolderItemAttribute.Directory) && e.Name == node.Value.Name);

                if (item != null)
                {
                    SelectedItem = item;
                    SelectedChanged?.Invoke(this, new SelectedChangedEventArgs() { IsFocus = true, IsNewFolder = true });
                }
            }
        }

        public void SelectBookmark(TreeListNode<IBookmarkEntry> node, bool isFocus)
        {
            if (!(FolderCollection is BookmarkFolderCollection bookmarkFolderCollection))
            {
                return;
            }

            var item = bookmarkFolderCollection.FirstOrDefault(e => node == (e.Source as TreeListNode<IBookmarkEntry>));
            if (item != null)
            {
                SelectedItem = item;
                SelectedChanged?.Invoke(this, new SelectedChangedEventArgs() { IsFocus = isFocus });
            }
        }

        public bool AddBookmark()
        {
            var place = BookHub.Current.Book?.Place;
            if (place == null)
            {
                return false;
            }

            return AddBookmark(new QueryPath(place), true);
        }

        public bool AddBookmark(QueryPath path, bool isFocus)
        {
            if (!(FolderCollection is BookmarkFolderCollection bookmarkFolderCollection))
            {
                return false;
            }

            var node = BookmarkCollectionService.AddToChild(bookmarkFolderCollection.BookmarkPlace, path);
            if (node != null)
            {
                var item = bookmarkFolderCollection.FirstOrDefault(e => node == (e.Source as TreeListNode<IBookmarkEntry>));
                if (item != null)
                {
                    SelectedItem = item;
                    SelectedChanged?.Invoke(this, new SelectedChangedEventArgs() { IsFocus = isFocus });
                }
            }

            return true;
        }

        public bool RemoveBookmark(FolderItem item)
        {
            var node = item.Source as TreeListNode<IBookmarkEntry>;
            if (node == null)
            {
                return false;
            }

            var memento = new TreeListNodeMemento<IBookmarkEntry>(node);

            bool isRemoved = BookmarkCollection.Current.Remove(node);
            if (isRemoved)
            {
                if (node.Value is BookmarkFolder)
                {
                    var count = node.Count(e => e.Value is Bookmark);
                    if (count > 0)
                    {
                        var toast = new Toast(string.Format(Properties.Resources.DialogPagemarkFolderDelete, count), Properties.Resources.WordRestore, () => BookmarkCollection.Current.Restore(memento));
                        ToastService.Current.Show("BookmarkList", toast);
                    }
                }
            }

            return isRemoved;
        }

        #endregion

        #region IDisposable Support
        private bool _disposedValue = false;

        protected virtual void Dispose(bool disposing)
        {
            if (!_disposedValue)
            {
                if (disposing)
                {
                    if (_searchEngine != null)
                    {
                        _searchEngine.Dispose();
                    }
                }

                _disposedValue = true;
            }
        }

        public void Dispose()
        {
            Dispose(true);
        }
        #endregion

        #region Memento

        [DataContract]
        public class Memento
        {
            [DataMember]
            public PanelListItemStyle PanelListItemStyle { get; set; }

            [DataMember]
            public bool IsVisibleHistoryMark { get; set; }

            [DataMember]
            public bool IsVisibleBookmarkMark { get; set; }

            [DataMember]
            public string Home { get; set; }

            [DataMember, DefaultValue(true)]
            public bool IsInsertItem { get; set; }

            [DataMember]
            public bool IsMultipleRarFilterEnabled { get; set; }

            [DataMember]
            public string ExcludePattern { get; set; }

            [DataMember]
            public bool IsCruise { get; set; }

            [DataMember]
            public bool IsCloseBookWhenMove { get; set; }

            [DataMember]
            public FolderTreeLayout FolderTreeLayout { get; set; }

            [DataMember, DefaultValue(72.0)]
            public double FolderTreeAreaHeight { get; set; }

            [DataMember, DefaultValue(192.0)]
            public double FolderTreeAreaWidth { get; set; }

            [DataMember, DefaultValue(true)]
            public bool IsFolderTreeVisible { get; set; }

            [DataMember]
            public bool IsSyncFolderTree { get; set; }

            [OnDeserializing]
            private void Deserializing(StreamingContext c)
            {
                this.InitializePropertyDefaultValues();
            }
        }

        //
        public Memento CreateMemento()
        {
            var memento = new Memento();
            memento.PanelListItemStyle = this.PanelListItemStyle;
            memento.IsVisibleHistoryMark = this.IsVisibleHistoryMark;
            memento.IsVisibleBookmarkMark = this.IsVisibleBookmarkMark;
            memento.Home = this.Home;
            memento.IsInsertItem = this.IsInsertItem;
            memento.IsMultipleRarFilterEnabled = this.IsMultipleRarFilterEnabled;
            memento.ExcludePattern = this.ExcludePattern;
            memento.IsCruise = this.IsCruise;
            memento.IsCloseBookWhenMove = this.IsCloseBookWhenMove;
            memento.FolderTreeLayout = this.FolderTreeLayout;
            memento.FolderTreeAreaHeight = this.FolderTreeAreaHeight;
            memento.FolderTreeAreaWidth = this.FolderTreeAreaWidth;
            memento.IsFolderTreeVisible = this.IsFolderTreeVisible;
            memento.IsSyncFolderTree = this.IsSyncFolderTree;

            return memento;
        }

        //
        public void Restore(Memento memento)
        {
            if (memento == null) return;

            this.PanelListItemStyle = memento.PanelListItemStyle;
            this.IsVisibleHistoryMark = memento.IsVisibleHistoryMark;
            this.IsVisibleBookmarkMark = memento.IsVisibleBookmarkMark;
            this.Home = memento.Home;
            this.IsInsertItem = memento.IsInsertItem;
            this.IsMultipleRarFilterEnabled = memento.IsMultipleRarFilterEnabled;
            this.ExcludePattern = memento.ExcludePattern;
            this.IsCruise = memento.IsCruise;
            this.IsCloseBookWhenMove = memento.IsCloseBookWhenMove;
            this.FolderTreeLayout = memento.FolderTreeLayout;
            this.FolderTreeAreaHeight = memento.FolderTreeAreaHeight;
            this.FolderTreeAreaWidth = memento.FolderTreeAreaWidth;
            this.IsFolderTreeVisible = memento.IsFolderTreeVisible;
            this.IsSyncFolderTree = memento.IsSyncFolderTree;
        }

        #endregion
    }

}
