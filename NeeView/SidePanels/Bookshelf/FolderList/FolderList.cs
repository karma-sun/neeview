using NeeLaboratory.ComponentModel;
using NeeView.Collections;
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
    public class BusyChangedEventArgs : EventArgs
    {
        public bool IsBusy { get; set; }

        public BusyChangedEventArgs(bool isBusy)
        {
            this.IsBusy = isBusy;
        }
    }

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


    /// <summary>
    /// FolderList Model
    /// </summary>
    public class FolderList : BindableBase, IDisposable
    {
        public static FolderList Current { get; private set; }

        #region Fields

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
        public FolderList()
        {
            Current = this;

            _folderListBoxModel = new FolderListBoxModel(null);

            _searchEngine = new FolderSearchEngine();
            FolderCollectionFactory.Current.SearchEngine = _searchEngine;


            BookHub.Current.FolderListSync += async (s, e) => await SyncWeak(e);
            BookHub.Current.HistoryChanged += (s, e) => _folderListBoxModel.RefreshIcon(new QueryPath(e.Key));

            BookHub.Current.BookmarkChanged += (s, e) =>
            {
                switch (e.Action)
                {
                    case EntryCollectionChangedAction.Reset:
                    case EntryCollectionChangedAction.Replace:
                        _folderListBoxModel.RefreshIcon(null);
                        break;
                    default:
                        if (e.Item.Value is Bookmark bookmark)
                        {
                            _folderListBoxModel.RefreshIcon(new QueryPath(bookmark.Place));
                        }
                        break;
                }
            };

            BookHub.Current.LoadRequested += (s, e) => CancelMoveCruiseFolder();
        }

        #endregion Constructors

        #region Events

        public event EventHandler PlaceChanged;

        // FolderCollection総入れ替え
        public event EventHandler CollectionChanged;

        // 検索ボックスにフォーカスを
        public event EventHandler SearchBoxFocus;

        public event ErrorEventHandler FolderTreeFocus;

        // リスト更新処理中イベント
        public event EventHandler<BusyChangedEventArgs> BusyChanged;

        #endregion

        #region Properties

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
                    case PanelListItemStyle.Thumbnail:
                        return true;
                    case PanelListItemStyle.Content:
                        return SidePanelProfile.Current.ContentItemImageWidth > 0.0;
                    case PanelListItemStyle.Banner:
                        return SidePanelProfile.Current.BannerItemImageWidth > 0.0;
                }
            }
        }

        /// <summary>
        /// IsVisibleHistoryMark property.
        /// </summary>
        private bool _isVisibleHistoryMark = true;
        [PropertyMember("@ParamBookshelfIsVisibleHistoryMark", Tips = "@ParamBookshelfIsVisibleHistoryMarkTips")]
        public bool IsVisibleHistoryMark
        {
            get { return _isVisibleHistoryMark; }
            set
            {
                if (SetProperty(ref _isVisibleHistoryMark, value))
                {
                    _folderCollection?.RefreshIcon(null);
                }
            }
        }

        /// <summary>
        /// IsVisibleBookmarkMark property.
        /// </summary>
        private bool _isVisibleBookmarkMark = true;
        [PropertyMember("@ParamBookshelfIsVisibleBookmarkMark", Tips = "@ParamBookshelfIsVisibleBookmarkMarkTips")]
        public bool IsVisibleBookmarkMark
        {
            get { return _isVisibleBookmarkMark; }
            set
            {
                if (SetProperty(ref _isVisibleBookmarkMark, value))
                {
                    _folderCollection?.RefreshIcon(null);
                }
            }
        }

        private string _home;
        [PropertyPath("@ParamBookshelfHome", IsDirectory = true)]
        public string Home
        {
            get { return _home; }
            set { if (_home != value) { _home = value; RaisePropertyChanged(); } }
        }

        /// <summary>
        /// 追加されたファイルを挿入する？
        /// OFFにするとリスト末尾に追加する
        /// </summary>
        [PropertyMember("@ParamBookshelfIsInsertItem", Tips = "@ParamBookshelfIsInsertItemTips")]
        public bool IsInsertItem { get; set; } = true;


        [PropertyMember("@ParamBookshelfIsMultipleRarFilterEnabled", Tips = "@ParamBookshelfIsMultipleRarFilterEnabledTips")]
        public bool IsMultipleRarFilterEnabled { get; set; }


        [PropertyMember("@ParamBookshelfIsCruise", Tips = "@ParamBookshelfIsCruiseTips")]
        public bool IsCruise { get; set; }

        [PropertyMember("@ParamBookshelfIsCloseBookWhenMove")]
        public bool IsCloseBookWhenMove { get; set; }

        [PropertyMember("@ParamBookshelfIsSyncFolderTree")]
        public bool IsSyncFolderTree { get; set; }

        private string _excludePattern;
        [PropertyMember("@ParamBookshelfExcludePattern", Tips = "@ParamBookshelfExcludePatternTips")]
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
                    RaisePropertyChanged(nameof(Place));
                    RaisePropertyChanged(nameof(IsPlaceValid));
                    RaisePropertyChanged(nameof(FolderOrder));
                    RaisePropertyChanged(nameof(IsFolderOrderEnabled));
                    RaisePropertyChanged(nameof(IsFolderSearchCollection));
                    RaisePropertyChanged(nameof(IsFolderSearchEnabled));
                }
            }
        }

        private FolderListBoxModel _folderListBoxModel;
        public FolderListBoxModel FolderListBoxModel
        {
            get { return _folderListBoxModel; }
            private set { SetProperty(ref _folderListBoxModel, value); }
        }

        private bool _isRenaming;
        public bool IsRenaming
        {
            get { return _isRenaming; }
            set { SetProperty(ref _isRenaming, value); }
        }

        /// <summary>
        /// 検索リスト？
        /// </summary>
        public bool IsFolderSearchCollection => FolderCollection is FolderSearchCollection;

        /// <summary>
        /// 検索許可？
        /// </summary>
        public bool IsFolderSearchEnabled => Place?.Path != null && !(FolderCollection is FolderArchiveCollection) && !(FolderCollection is BookmarkFolderCollection);

        /// <summary>
        /// 現在のフォルダー
        /// </summary>
        public QueryPath Place => _folderCollection?.Place;

        /// <summary>
        /// 現在のフォルダーが有効？
        /// </summary>
        public bool IsPlaceValid => Place != null;

        /// <summary>
        /// フォルダー履歴
        /// </summary>
        public HistoryCollection<QueryPath> History { get; private set; } = new HistoryCollection<QueryPath>();

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
            set
            {
                if (_searchKeyword != value)
                {
                    _searchKeyword = value;
                    RaisePropertyChanged();
                    RequestSearchPlace(false);
                }
            }
        }

        /// <summary>
        /// フォルダーツリーの表示
        /// </summary>
        private bool _isFolderTreeVisible = true;
        public bool IsFolderTreeVisible
        {
            get { return _isFolderTreeVisible; }
            set { SetProperty(ref _isFolderTreeVisible, value); }
        }



        private FolderTreeLayout _FolderTreeLayout = FolderTreeLayout.Left;
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
            set { SetProperty(ref _IsLocked, value && Place != null); }
        }

        // フォーカス要求
        public bool IsFocusAtOnce { get; set; }

        public PageListPlacementService PageListPlacementService => PageListPlacementService.Current;


        #endregion

        #region Methods

        /// <summary>
        /// フォーカス要求
        /// </summary>
        public void FocusAtOnce()
        {
            IsFocusAtOnce = true;
        }

        /// <summary>
        /// 補正されたHOME取得
        /// </summary>
        /// <returns></returns>
        public QueryPath GetFixedHome()
        {
            var path = new QueryPath(_home);

            switch (path.Scheme)
            {
                case QueryScheme.Root:
                    return path;

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
        /// フォルダー状態保存
        /// </summary>
        private void SavePlace(FolderItem folder, int index)
        {
            if (folder == null || folder.Place == null) return;
            _lastPlaceDictionary[folder.Place] = new FolderItemPosition(folder.Path, index);
        }

        /// <summary>
        /// 検索更新
        /// </summary>
        public void RequestSearchPlace(bool isForce)
        {
            if (Place == null) return;

            var path = Place.ReplaceSearch(GetFixedSearchKeyword());

            var option = isForce ? FolderSetPlaceOption.Refresh : FolderSetPlaceOption.None;
            var task = SetPlaceAsync(path, null, option);
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
            if (_folderListBoxModel?.FolderCollection is BookmarkFolderCollection && !options.HasFlag(FolderSetPlaceOption.FileSystem))
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
            if (_folderListBoxModel != null)
            {
                SavePlace(_folderListBoxModel.SelectedItem, _folderListBoxModel.GetFolderItemIndex(_folderListBoxModel.SelectedItem));
            }

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
                var collection = await CreateFolderCollectionAsync(path, true);
                if (collection != null)
                {
                    this.FolderCollection = collection;
                    this.FolderListBoxModel = new FolderListBoxModel(this.FolderCollection);
                    this.FolderListBoxModel.SetSelectedItem(select, options.HasFlag(FolderSetPlaceOption.Focus));
                    if (options.HasFlag(FolderSetPlaceOption.Focus))
                    {
                        FocusAtOnce();
                    }

                    CollectionChanged?.Invoke(this, null);

                    // 最終フォルダー更新
                    BookHistoryCollection.Current.LastFolder = Place.SimpleQuery;

                    // 履歴追加
                    if (options.HasFlag(FolderSetPlaceOption.UpdateHistory))
                    {
                        var place = Place.ReplaceSearch(null);
                        if (place != this.History.GetCurrent())
                        {
                            this.History.Add(place);
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
                _folderListBoxModel.SetSelectedItem(select, false);
                PlaceChanged?.Invoke(this, null);
            }
        }

        /// <summary>
        /// リストの更新必要性チェック
        /// </summary>
        private bool CheckFolderListUpdateneNcessary(QueryPath path, FolderSetPlaceOption options)
        {
            if (_isDarty || _folderCollection == null || path != _folderCollection.Place || options.HasFlag(FolderSetPlaceOption.Refresh))
            {
                return true;
            }

            return false;
        }

        /// <summary>
        /// フォルダーリスト更新
        /// </summary>
        /// <param name="force">必要が無い場合も更新する</param>
        public async Task RefreshAsync(bool force)
        {
            if (_folderCollection == null) return;

            _isDarty = force || _folderCollection.IsDarty();

            await SetPlaceAsync(Place, null, FolderSetPlaceOption.UpdateHistory);
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

            var collection = _folderCollection;

            if (e != null && e.isKeepPlace)
            {
                // すでにリストに存在している場合は何もしない
                if (collection == null || collection.Contains(path)) return;
            }

            var options = FolderSetPlaceOption.UpdateHistory;

            if (collection != null)
            {
                if (collection.Place.FullPath == parent.FullPath && collection.Contains(path))
                {
                    await SetPlaceAsync(collection.Place, new FolderItemPosition(path), options);
                    return;
                }
            }

            await SetPlaceAsync(parent, new FolderItemPosition(path), options);
        }

        // 現在の場所のフォルダーの並び順
        public FolderOrder FolderOrder
        {
            get { return GetFolderOrder(); }
        }

        public bool IsFolderOrderEnabled
        {
            get { return _folderCollection != null && (_folderCollection is FolderEntryCollection collection && collection.Place.Path != null || _folderCollection is BookmarkFolderCollection); }
        }

        /// <summary>
        /// フォルダーの並びを設定
        /// </summary>
        public void SetFolderOrder(FolderOrder folderOrder)
        {
            _folderListBoxModel.SetFolderOrder(folderOrder);
        }

        /// <summary>
        /// フォルダーの並びを取得
        /// </summary>
        public FolderOrder GetFolderOrder()
        {
            return _folderListBoxModel.GetFolderOrder();
        }

        /// <summary>
        /// フォルダーの並びを順番に切り替える
        /// </summary>
        public void ToggleFolderOrder()
        {
            _folderListBoxModel.ToggleFolderOrder();
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
            var items = _folderCollection?.Items.Select(e => e.TargetPath.SimplePath).Where(e => e != null);
            BookHistoryCollection.Current.Remove(items);

            _folderListBoxModel.RefreshIcon(null);
        }

        #endregion Methods

        #region MoveFolder

        // 次のフォルダーに移動
        public async Task NextFolder(BookLoadOption option = BookLoadOption.None)
        {
            if (BookHub.Current.IsBusy()) return; // 相対移動の場合はキャンセルしない
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
            if (BookHub.Current.IsBusy()) return; // 相対移動の場合はキャンセルしない
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
        private async Task<bool> MoveFolder(int direction, BookLoadOption options)
        {
            var isCruise = IsCruise && !(_folderCollection is FolderSearchCollection);

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
        private async Task<bool> MoveNextFolder(int direction, BookLoadOption options)
        {
            var item = _folderListBoxModel.GetFolderItem(_folderListBoxModel.SelectedItem, direction);
            if (item == null)
            {
                return false;
            }

            int index = _folderListBoxModel.GetFolderItemIndex(item);

            await SetPlaceAsync(_folderCollection.Place, new FolderItemPosition(item.Path, index), FolderSetPlaceOption.UpdateHistory);
            BookHub.Current.RequestLoad(item.TargetPath.SimplePath, null, options | BookLoadOption.IsBook, false);
            return true;
        }


        /// <summary>
        /// 巡回フォルダー移動
        /// </summary>
        private async Task<bool> MoveCruiseFolder(int direction, BookLoadOption options)
        {
            // TODO: NowLoad表示をどうしよう。BookHubに処理を移してそこで行う？

            var item = _folderListBoxModel.SelectedItem;
            if (item == null) return false;

            _cruiseFolderCancellationTokenSource?.Cancel();
            _cruiseFolderCancellationTokenSource = new CancellationTokenSource();

            try
            {
                var node = new FolderNode(_folderCollection, item);
                var cancel = _cruiseFolderCancellationTokenSource.Token;
                var next = (direction < 0) ? await node.CruisePrev(cancel) : await node.CruiseNext(cancel);
                if (next == null) return false;
                if (next.Content == null) return false;

                await SetPlaceAsync(next.Content.Place, new FolderItemPosition(next.Content.Path) { TargetPath = next.Content.TargetPath }, FolderSetPlaceOption.UpdateHistory);
                BookHub.Current.RequestLoad(next.Content.TargetPath.SimplePath, null, options | BookLoadOption.IsBook, false);

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

        #endregion MoveFolder

        #region CreateFolderCollection

        /// <summary>
        /// コレクション作成
        /// </summary>
        public async Task<FolderCollection> CreateFolderCollectionAsync(QueryPath path, bool isForce)
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
                    return collection;
                }
                else
                {
                }
            }
            catch (OperationCanceledException)
            {
                Debug.WriteLine($"UpdateFolderCollectionAsync: Canceled: {path}");
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"UpdateFolderCollectionAsync: {ex.Message}");
            }
            finally
            {
                BusyChanged?.Invoke(this, new BusyChangedEventArgs(false));
            }

            return null;
        }

        /// <summary>
        /// コレクション作成
        /// </summary>
        private async Task<FolderCollection> CreateFolderCollectionAsync(QueryPath path, bool isForce, CancellationToken token)
        {
            var factory = FolderCollectionFactory.Current;

            if (!isForce && _folderCollection.Place.Equals(path))
            {
                return null;
            }

            if (path.Search != null && _folderCollection is FolderSearchCollection && _folderCollection.Place.FullPath == path.FullPath)
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

        #endregion CreateFolderCollection

        #region Commands
        // NOTE: RelayCommandの実体なので、async void が使用されている場合がある。

        public void AddQuickAccess()
        {
            IsFolderTreeVisible = true;
            FolderTreeModel.Current.AddQuickAccess(GetCurentQueryPath());
        }

        public string GetCurentQueryPath()
        {
            return Place?.SimpleQuery;
        }

        public bool CanSetHome()
        {
            return Place != null;
        }

        public void SetHome()
        {
            if (BookHub.Current == null) return;
            if (Place == null) return;
            this.Home = Place.SimplePath;
        }

        public async void MoveToHome()
        {
            if (BookHub.Current == null) return;

            var place = GetFixedHome();
            await SetPlaceAsync(place, null, FolderSetPlaceOption.Focus | FolderSetPlaceOption.UpdateHistory | FolderSetPlaceOption.TopSelect | FolderSetPlaceOption.ResetKeyword);

            CloseBookIfNecessary();
        }

        public async void MoveTo(QueryPath path)
        {
            await this.SetPlaceAsync(path, null, FolderSetPlaceOption.Focus | FolderSetPlaceOption.UpdateHistory);

            CloseBookIfNecessary();
        }

        public bool CanMoveToPrevious()
        {
            return this.History.CanPrevious();
        }

        public async void MoveToPrevious()
        {
            if (!this.History.CanPrevious()) return;

            var place = this.History.GetPrevious();
            await SetPlaceAsync(place, null, FolderSetPlaceOption.Focus);
            this.History.Move(-1);

            CloseBookIfNecessary();
        }

        public bool CanMoveToNext()
        {
            return this.History.CanNext();
        }

        public async void MoveToNext()
        {
            if (!this.History.CanNext()) return;

            var place = this.History.GetNext();
            await SetPlaceAsync(place, null, FolderSetPlaceOption.Focus);
            this.History.Move(+1);

            CloseBookIfNecessary();
        }

        public async void MoveToHistory(KeyValuePair<int, QueryPath> item)
        {
            var place = this.History.GetHistory(item.Key);
            await SetPlaceAsync(place, null, FolderSetPlaceOption.Focus);
            this.History.SetCurrent(item.Key + 1);

            CloseBookIfNecessary();
        }

        public bool CanMoveToParent()
        {
            return _folderCollection?.GetParentQuery() != null;
        }

        public async void MoveToParent()
        {
            var parent = _folderCollection?.GetParentQuery();
            if (parent == null)
            {
                return;
            }

            await SetPlaceAsync(parent, new FolderItemPosition(Place), FolderSetPlaceOption.Focus | FolderSetPlaceOption.UpdateHistory);
            CloseBookIfNecessary();
        }

        public async void Sync()
        {
            var place = BookHub.Current?.Book?.Place;

            if (place != null)
            {
                // TODO: Queryの求め方はこれでいいのか？
                var path = new QueryPath(place);
                var parent = new QueryPath(BookHub.Current?.Book?.Archiver?.Parent?.FullPath ?? LoosePath.GetDirectoryName(place));

                _isDarty = true; // 強制更新
                await SetPlaceAsync(parent, new FolderItemPosition(path), FolderSetPlaceOption.Focus | FolderSetPlaceOption.UpdateHistory | FolderSetPlaceOption.ResetKeyword | FolderSetPlaceOption.FileSystem);

                _folderListBoxModel.RaiseSelectedItemChanged(true);
            }
            else if (Place != null)
            {
                _isDarty = true; // 強制更新
                await SetPlaceAsync(Place, null, FolderSetPlaceOption.Focus | FolderSetPlaceOption.FileSystem);

                _folderListBoxModel.RaiseSelectedItemChanged(true);
            }

            if (IsSyncFolderTree && Place != null)
            {
                FolderTreeModel.Current.SyncDirectory(Place.SimplePath);
            }
        }

        public void ToggleFolderRecursive()
        {
            _folderListBoxModel.ToggleFolderRecursive_Executed();
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
            _folderListBoxModel.NewFolder();
        }

        public void SelectBookmark(TreeListNode<IBookmarkEntry> node, bool isFocus)
        {
            _folderListBoxModel.SelectBookmark(node, isFocus);
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
            return _folderListBoxModel.AddBookmark(path, isFocus);
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

            [DataMember, DefaultValue(FolderTreeLayout.Left)]
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
