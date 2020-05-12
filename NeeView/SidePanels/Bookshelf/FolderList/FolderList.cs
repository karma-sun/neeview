using NeeLaboratory.ComponentModel;
using NeeLaboratory.IO.Search;
using NeeView.Collections;
using NeeView.Collections.Generic;
using NeeView.Windows.Controls;
using NeeView.Windows.Data;
using NeeView.Windows.Property;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
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
    /// <summary>
    /// FolderList Model
    /// </summary>
    public abstract class FolderList : BindableBase, IDisposable
    {
        private const int _historyCapacity = 100;

        private static SearchKeyAnalyzer _searchKeyAnalyzer = new SearchKeyAnalyzer();

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

        /// <summary>
        /// 検索キーワード
        /// </summary>
        private DelayValue<string> _searchKeyword;

        private CancellationTokenSource _updateFolderCancellationTokenSource;
        private CancellationTokenSource _cruiseFolderCancellationTokenSource;

        private FolderListConfig _folderListConfig;

        private volatile bool _isCollectionCreating;
        private List<Action> _collectionCreatedCallback = new List<Action>();

        private int _busyCount;

        private FolderCollection _folderCollection;
        private FolderItem _selectedItem;

        private string _inputKeyword;
        private string _searchKeywordErrorMessage;

        private object _lock = new object();

        private double _areaWidth = double.PositiveInfinity;
        private double _areaHeight = double.PositiveInfinity;
        private bool _IsLocked;



        protected FolderList(bool isSyncBookHub, bool isOverlayEnabled, FolderListConfig folderListConfig)
        {
            _folderListConfig = folderListConfig;

            _searchEngine = new FolderSearchEngine();
            FolderCollectionFactory = new FolderCollectionFactory(_searchEngine, isOverlayEnabled);

            _searchKeyword = new DelayValue<string>();
            _searchKeyword.ValueChanged += (s, e) =>
            {
                if (IsIncrementalSearchEnabled())
                {
                    RequestSearchPlace(false);
                }
                else
                {
                    UpdateSearchKeywordErrorMessage();
                }
            };

            if (isSyncBookHub)
            {
                BookHub.Current.FolderListSync += async (s, e) => await SyncWeak(e);
                BookHub.Current.HistoryChanged += (s, e) => RefreshIcon(new QueryPath(e.Key));
                BookHub.Current.LoadRequested += (s, e) => CancelMoveCruiseFolder();
            }

            if (isOverlayEnabled)
            {
                BookHub.Current.BookmarkChanged += (s, e) =>
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
                                RefreshIcon(new QueryPath(bookmark.Path));
                            }
                            break;
                    }
                };
            }

            // ブックマーク監視
            BookmarkCollection.Current.BookmarkChanged += BookmarkCollection_BookmarkChanged;

            _folderListConfig.AddPropertyChanged(nameof(FolderListConfig.FolderTreeLayout), (s, e) =>
            {
                RaisePropertyChanged(nameof(FolderTreeDock));
                RaisePropertyChanged(nameof(FolderTreeAreaWidth));
                RaisePropertyChanged(nameof(FolderTreeAreaHeight));
            });

            _folderListConfig.AddPropertyChanged(nameof(FolderListConfig.PanelListItemStyle), (s, e) =>
            {
                RaisePropertyChanged(nameof(PanelListItemStyle));
            });

            _folderListConfig.AddPropertyChanged(nameof(FolderListConfig.IsFolderTreeVisible), (s, e) =>
            {
                RaisePropertyChanged(nameof(IsFolderTreeVisible));
            });

            _folderListConfig.AddPropertyChanged(nameof(FolderListConfig.FolderTreeLayout), (s, e) =>
            {
                RaisePropertyChanged(nameof(FolderTreeLayout));
            });
        }



        public event EventHandler PlaceChanged;

        // FolderCollection総入れ替え
        public event EventHandler CollectionChanged;

        // 検索ボックスにフォーカスを
        public event EventHandler SearchBoxFocus;

        public event ErrorEventHandler FolderTreeFocus;

        // リスト更新処理中イベント
        public event EventHandler<FolderListBusyChangedEventArgs> BusyChanged;

        public event EventHandler<FolderListSelectedChangedEventArgs> SelectedChanging;
        public event EventHandler<FolderListSelectedChangedEventArgs> SelectedChanged;



        public FolderItem SelectedItem
        {
            get { return _selectedItem; }
            set { SetProperty(ref _selectedItem, value); }
        }

        /// <summary>
        /// リスト自体のコンテキストメニュー表示が有効？
        /// </summary>
        public bool IsContextMenuEnabled => FolderCollection is BookmarkFolderCollection;

        /// <summary>
        /// フォーカス要求
        /// </summary>
        public bool IsFocusAtOnce { get; set; }

        /// <summary>
        /// 本を読み込むときに本棚の更新を要求する
        /// </summary>
        public virtual bool IsSyncBookshelfEnabled
        {
            get { return false; }
            set { }
        }

        public FolderListConfig FolderListConfig => _folderListConfig;

        public FolderCollectionFactory FolderCollectionFactory { get; }

        public PanelListItemStyle PanelListItemStyle
        {
            get { return _folderListConfig.PanelListItemStyle; }
            set { _folderListConfig.PanelListItemStyle = value; }
        }

        // サムネイル画像が表示される？？
        public bool IsThumbnailVisibled
        {
            get
            {
                switch (_folderListConfig.PanelListItemStyle)
                {
                    default:
                        return false;
                    case PanelListItemStyle.Thumbnail:
                        return true;
                    case PanelListItemStyle.Content:
                        return Config.Current.Panels.ContentItemProfile.ImageWidth > 0.0;
                    case PanelListItemStyle.Banner:
                        return Config.Current.Panels.BannerItemProfile.ImageWidth > 0.0;
                }
            }
        }

        /// <summary>
        /// フォルダーコレクション
        /// </summary>
        public FolderCollection FolderCollection
        {
            get { return _folderCollection; }
            private set
            {
                if (_folderCollection != value)
                {
                    _folderCollection?.Dispose();
                    _folderCollection = value;
                }
            }
        }

        /// <summary>
        /// 検索リスト？
        /// </summary>
        public bool IsFolderSearchCollection => FolderCollection is FolderSearchCollection;

        /// <summary>
        /// 検索許可？
        /// </summary>
        public bool IsFolderSearchEnabled => Place?.Path != null && FolderCollection.IsSearchEnabled;

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
        public HistoryLimitedCollection<QueryPath> History { get; private set; } = new HistoryLimitedCollection<QueryPath>(_historyCapacity);

        /// <summary>
        /// 検索BOXの表示
        /// </summary>
        public bool IsFolderSearchBoxVisible => true;

        /// <summary>
        /// 入力キーワード
        /// </summary>
        public string InputKeyword
        {
            get { return _inputKeyword; }
            set
            {
                if (SetProperty(ref _inputKeyword, value))
                {
                    SetSearchKeywordDelay(_inputKeyword);
                }
            }
        }

        /// <summary>
        /// 検索キーワードエラーメッセージ
        /// </summary>
        public string SearchKeywordErrorMessage
        {
            get { return _searchKeywordErrorMessage; }
            set { SetProperty(ref _searchKeywordErrorMessage, value); }
        }

        public bool IsFolderTreeVisible
        {
            get { return _folderListConfig.IsFolderTreeVisible; }
            set { _folderListConfig.IsFolderTreeVisible = value; }
        }

        public FolderTreeLayout FolderTreeLayout
        {
            get => FolderListConfig.FolderTreeLayout;
            set => FolderListConfig.FolderTreeLayout = value;
        }

        public Dock FolderTreeDock
        {
            get { return _folderListConfig.FolderTreeLayout == FolderTreeLayout.Left ? Dock.Left : Dock.Top; }
        }

        /// <summary>
        /// フォルダーツリーエリアの幅
        /// </summary>
        public double FolderTreeAreaWidth
        {
            get { return _folderListConfig.FolderTreeAreaWidth; }
            set
            {
                var width = Math.Max(Math.Min(value, _areaWidth - 32.0), 32.0 - 6.0);
                if (_folderListConfig.FolderTreeAreaWidth != width)
                {
                    _folderListConfig.FolderTreeAreaWidth = width;
                    RaisePropertyChanged();
                }
            }
        }

        /// <summary>
        /// フォルダーリストエリアの幅
        /// クイックアクセスエリアの幅計算用
        /// </summary>
        public double AreaWidth
        {
            get { return _areaWidth; }
            set
            {
                if (SetProperty(ref _areaWidth, value))
                {
                    // 再設定する
                    FolderTreeAreaWidth = _folderListConfig.FolderTreeAreaWidth;
                }
            }
        }

        /// <summary>
        /// フォルダーツリーエリアの高さ
        /// </summary>
        public double FolderTreeAreaHeight
        {
            get { return _folderListConfig.FolderTreeAreaHeight; }
            set
            {
                var height = Math.Max(Math.Min(value, _areaHeight - 32.0), 32.0 - 6.0);
                if (_folderListConfig.FolderTreeAreaHeight != height)
                {
                    _folderListConfig.FolderTreeAreaHeight = height;
                    RaisePropertyChanged();
                }
            }
        }

        /// <summary>
        /// フォルダーリストエリアの高さ
        /// クイックアクセスエリアの高さ計算用
        /// </summary>
        public double AreaHeight
        {
            get { return _areaHeight; }
            set
            {
                if (SetProperty(ref _areaHeight, value))
                {
                    // 再設定する
                    FolderTreeAreaHeight = _folderListConfig.FolderTreeAreaHeight;
                }
            }
        }

        /// <summary>
        /// 外部の変化によるフォルダーリストの変更を禁止
        /// </summary>
        public bool IsLocked
        {
            get { return _IsLocked; }
            set { SetProperty(ref _IsLocked, value && Place != null); }
        }

        public PageListPlacementService PageListPlacementService => PageListPlacementService.Current;



        protected virtual bool IsIncrementalSearchEnabled() => false;

        protected virtual bool IsSearchIncludeSubdirectories() => false;


        // 検索キーワード即時反映
        public void SetSearchKeyword(string keyword)
        {
            _searchKeyword.SetValue(keyword, 0, DelayValueOverwriteOption.Force);
        }

        // 検索キーワード遅延反映
        public void SetSearchKeywordDelay(string keyword)
        {
            _searchKeyword.SetValue(keyword, 500);
        }

        public void SetSearchKeywordAndSearch(string keyword)
        {
            SetSearchKeyword(keyword);

            // 検索を重複させないための処置
            if (!IsIncrementalSearchEnabled())
            {
                RequestSearchPlace(false);
            }

            UpdateSearchHistory();
        }

        private void RaiseCollectionChanged()
        {
            CollectionChanged?.Invoke(this, null);
            RaisePropertyChanged(nameof(FolderCollection));
            RaisePropertyChanged(nameof(Place));
            RaisePropertyChanged(nameof(IsPlaceValid));
            RaisePropertyChanged(nameof(FolderOrder));
            RaisePropertyChanged(nameof(IsFolderOrderEnabled));
            RaisePropertyChanged(nameof(IsFolderSearchCollection));
            RaisePropertyChanged(nameof(IsFolderSearchEnabled));
        }

        public virtual void IsVisibleChanged(bool isVisible)
        {
        }

        /// <summary>
        /// フォーカス要求
        /// </summary>
        public void FocusAtOnce()
        {
            this.IsFocusAtOnce = true;
        }

        /// <summary>
        /// HOME取得
        /// </summary>
        public abstract QueryPath GetFixedHome();


        /// <summary>
        /// フォルダー状態保存
        /// </summary>
        private void SavePlace(QueryPath place, FolderItem folder, int index)
        {
            if (folder == null || place == null) return;
            Debug.Assert(folder.Place == place);

            _lastPlaceDictionary[place] = new FolderItemPosition(folder.TargetPath, index);
        }


        /// <summary>
        /// 検索キーワードのフォーマットチェック
        /// </summary>
        private void UpdateSearchKeywordErrorMessage()
        {
            var keyword = GetFixedSearchKeyword();

            try
            {
                _searchKeyAnalyzer.Analyze(keyword);
                SearchKeywordErrorMessage = null;
            }
            catch (SearchKeywordOptionException ex)
            {
                SearchKeywordErrorMessage = string.Format(Properties.Resources.NotifySearchKeywordOptionError, ex.Option);
            }
            catch (SearchKeywordDateTimeException)
            {
                SearchKeywordErrorMessage = Properties.Resources.NotifySearchKeywordDateTimeError;
            }
            catch (SearchKeywordRegularExpressionException ex)
            {
                SearchKeywordErrorMessage = ex.InnerException.Message;
            }
            catch (Exception ex)
            {
                SearchKeywordErrorMessage = ex.Message;
            }
        }

        /// <summary>
        /// 検索更新
        /// </summary>
        public void RequestSearchPlace(bool isForce)
        {
            UpdateSearchKeywordErrorMessage();

            if (Place == null)
            {
                SearchKeywordErrorMessage = null;
                return;
            }

            if (!IsFolderSearchEnabled)
            {
                SearchKeywordErrorMessage = null;
                return;
            }

            if (SearchKeywordErrorMessage != null)
            {
                return;
            }

            // 検索パス作成
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


        public void SetDarty()
        {
            _isDarty = true;
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

            path = path.ToEntityPath();

            // 現在フォルダーの情報を記憶
            SavePlace(Place, SelectedItem, GetFolderItemIndex(SelectedItem));

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
                try
                {
                    OnCollectionCreating();
                    _isDarty = false;

                    // FolderCollection 更新
                    var collection = await CreateFolderCollectionAsync(path, true);
                    if (collection != null)
                    {
                        this.FolderCollection = collection;
                        this.FolderCollection.CollectionChanging += FolderCollection_CollectionChanging;
                        this.FolderCollection.CollectionChanged += FolderCollection_CollectionChanged;
                        RaiseCollectionChanged();

                        this.SetSelectedItem(select, options.HasFlag(FolderSetPlaceOption.Focus));

                        // 最終フォルダー更新
                        Config.Current.StartUp.LastFolderPath = Place.SimpleQuery;

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
                            // 入力文字のみ更新
                            _inputKeyword = Place.Search;
                            RaisePropertyChanged(nameof(InputKeyword));
                        }

                        PlaceChanged?.Invoke(this, null);
                    }
                }
                finally
                {
                    OnCollectionCreated();
                }
            }
            else
            {
                // 選択項目のみ変更
                SetSelectedItem(select, false);
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
        public async Task RefreshAsync(bool force, bool resetSearchEngine)
        {
            if (_folderCollection == null) return;

            _isDarty = force || _folderCollection.IsDarty();

            if (resetSearchEngine)
            {
                FolderCollectionFactory.SearchEngine.Reset();
            }

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
            get
            {
                if (_folderCollection is null)
                {
                    return false;
                }
                if (_folderCollection is FolderEntryCollection collection)
                {
                    return collection.Place.Path != null;
                }
                else
                {
                    return _folderCollection.FolderOrderClass != FolderOrderClass.None;
                }
            }
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
            return _searchKeyword.Value?.Trim();
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

            RefreshIcon(null);
        }


        #region MoveFolder

        // 次のフォルダーに移動
        public async Task NextFolder(BookLoadOption option = BookLoadOption.None)
        {
            if (BookHub.Current.IsBusy) return; // 相対移動の場合はキャンセルしない
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
            if (BookHub.Current.IsBusy) return; // 相対移動の場合はキャンセルしない
            var result = await MoveFolder(-1, option);
            if (result != true)
            {
                SoundPlayerService.Current.PlaySeCannotMove();
                InfoMessage.Current.SetMessage(InfoMessageType.Notify, Properties.Resources.NotifyBookPrevFailed);
            }
        }

        // ランダムなフォルダーに移動
        public async Task RandomFolder(BookLoadOption option = BookLoadOption.None)
        {
            if (BookHub.Current.IsBusy) return;
            await MoveRandomFolder(option);
        }

        // 巡回移動できる？
        protected virtual bool IsCruise()
        {
            return false;
        }

        /// <summary>
        /// コマンドの「前のフォルダーに移動」「次のフォルダーへ移動」に対応
        /// </summary>
        private async Task<bool> MoveFolder(int direction, BookLoadOption options)
        {
            var isCruise = IsCruise() && !(_folderCollection is FolderSearchCollection);

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
            var item = GetFolderItem(SelectedItem, direction);
            if (item == null)
            {
                return false;
            }

            int index = GetFolderItemIndex(item);

            await SetPlaceAsync(_folderCollection.Place, new FolderItemPosition(item.TargetPath, index), FolderSetPlaceOption.UpdateHistory);
            RequestLoad(item, null, options, false);

            return true;
        }

        /// <summary>
        /// ランダムフォルダー移動
        /// </summary>
        private async Task<bool> MoveRandomFolder(BookLoadOption options)
        {
            var currentBookAddress = BookOperation.Current.Book?.Address;

            var items = _folderCollection.Where(e => !e.IsEmpty() && e.EntityPath.Scheme == QueryScheme.File && e.EntityPath.SimplePath != currentBookAddress);
            if (!items.Any())
            {
                return false;
            }

            var item = items.ElementAt(new Random().Next(items.Count()));
            if (item == null)
            {
                return false;
            }

            int index = GetFolderItemIndex(item);

            await SetPlaceAsync(_folderCollection.Place, new FolderItemPosition(item.TargetPath, index), FolderSetPlaceOption.UpdateHistory);
            RequestLoad(item, null, options, false);

            return true;
        }

        /// <summary>
        /// 巡回フォルダー移動
        /// </summary>
        private async Task<bool> MoveCruiseFolder(int direction, BookLoadOption options)
        {
            // TODO: NowLoad表示をどうしよう。BookHubに処理を移してそこで行う？

            var item = SelectedItem;
            if (item == null) return false;

            _cruiseFolderCancellationTokenSource?.Cancel();
            _cruiseFolderCancellationTokenSource?.Dispose();
            _cruiseFolderCancellationTokenSource = new CancellationTokenSource();
            var token = _cruiseFolderCancellationTokenSource.Token;

            try
            {
                var node = new FolderNode(_folderCollection, item);
                var next = (direction < 0) ? await node.CruisePrev(token) : await node.CruiseNext(token);
                if (next == null) return false;
                if (next.Content == null) return false;

                await SetPlaceAsync(new QueryPath(next.Place), new FolderItemPosition(next.Content.TargetPath), FolderSetPlaceOption.UpdateHistory);
                RequestLoad(next.Content, null, options, false);
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

        private void RequestLoad(FolderItem item, string start, BookLoadOption option, bool isRefreshFolderList)
        {
            var additionalOption = BookLoadOption.IsBook | (item.CanRemove() ? BookLoadOption.None : BookLoadOption.Undeliteable);
            BookHub.Current.RequestLoad(item.TargetPath.SimplePath, start, option | additionalOption, isRefreshFolderList);
        }

        #endregion MoveFolder

        #region CreateFolderCollection

        public void IncrementBusy()
        {
            var count = Interlocked.Increment(ref _busyCount);
            if (count == 1)
            {
                BusyChanged?.Invoke(this, new FolderListBusyChangedEventArgs(true));
            }
        }

        public void DecrementBusy()
        {
            var count = Interlocked.Decrement(ref _busyCount);
            if (count == 0)
            {
                BusyChanged?.Invoke(this, new FolderListBusyChangedEventArgs(false));
            }
        }

        /// <summary>
        /// コレクション作成
        /// </summary>
        public async Task<FolderCollection> CreateFolderCollectionAsync(QueryPath path, bool isForce)
        {
            try
            {
                IncrementBusy();

                _updateFolderCancellationTokenSource?.Cancel();
                _updateFolderCancellationTokenSource?.Dispose();
                _updateFolderCancellationTokenSource = new CancellationTokenSource();
                var token = _updateFolderCancellationTokenSource.Token;

                var collection = await CreateFolderCollectionAsync(path, isForce, token);
                if (collection != null && !token.IsCancellationRequested)
                {
                    collection.ParameterChanged += async (s, e) => await RefreshAsync(true, false);
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
                DecrementBusy();
            }

            return null;
        }

        /// <summary>
        /// コレクション作成
        /// </summary>
        private async Task<FolderCollection> CreateFolderCollectionAsync(QueryPath path, bool isForce, CancellationToken token)
        {
            if (!isForce && _folderCollection.Place.Equals(path))
            {
                return null;
            }

            // サブディレクトリー検索フラグを確定
            _searchEngine.IncludeSubdirectories = IsSearchIncludeSubdirectories();

            if (path.Search != null && _folderCollection is FolderSearchCollection && _folderCollection.Place.FullPath == path.FullPath)
            {
                ////Debug.WriteLine($"SearchEngine: Cancel");
                FolderCollectionFactory.SearchEngine.CancelSearch();
            }
            else
            {
                ////Debug.WriteLine($"SearchEngine: Reset");
                FolderCollectionFactory.SearchEngine.Reset();
            }

            return await FolderCollectionFactory.CreateFolderCollectionAsync(path, true, token);
        }

        #endregion CreateFolderCollection

        #region Commands
        // NOTE: RelayCommandの実体なので、async void が使用されている場合がある。

        public void AddQuickAccess()
        {
            _folderListConfig.IsFolderTreeVisible = true;
            BookshelfFolderTreeModel.Current.AddQuickAccess(GetCurentQueryPath());
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
            Config.Current.Bookshelf.Home = Place.SimplePath;
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

        public virtual bool CanMoveToPrevious()
        {
            return this.History.CanPrevious();
        }

        public async void MoveToPrevious()
        {
            if (!CanMoveToPrevious()) return;

            var place = this.History.GetPrevious();
            await SetPlaceAsync(place, null, FolderSetPlaceOption.Focus);
            this.History.Move(-1);

            CloseBookIfNecessary();
        }

        public virtual bool CanMoveToNext()
        {
            return this.History.CanNext();
        }

        public async void MoveToNext()
        {
            if (!CanMoveToNext()) return;

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

        public virtual bool CanMoveToParent()
        {
            var parentQuery = _folderCollection?.GetParentQuery();
            if (parentQuery == null) return false;
            return true;
        }

        public async void MoveToParent()
        {
            if (!CanMoveToParent()) return;

            var parent = _folderCollection?.GetParentQuery();
            if (parent == null)
            {
                return;
            }

            await SetPlaceAsync(parent, new FolderItemPosition(Place), FolderSetPlaceOption.Focus | FolderSetPlaceOption.UpdateHistory);
            CloseBookIfNecessary();
        }

        public abstract void Sync();


        public void ToggleFolderRecursive()
        {
            ToggleFolderRecursive_Executed();
        }

        protected virtual void CloseBookIfNecessary()
        {
        }

        #region FolderCollection生成衝突の回避用

        private void AddCollectionCreatedCallback(Action callback)
        {
            lock (_lock)
            {
                _collectionCreatedCallback.Add(callback);
            }
        }

        private void OnCollectionCreating()
        {
            lock (_lock)
            {
                _isCollectionCreating = true;
            }
        }

        private void OnCollectionCreated()
        {
            List<Action> collections;

            lock (_lock)
            {
                _isCollectionCreating = false;
                if (_collectionCreatedCallback.Count == 0) return;
                collections = _collectionCreatedCallback;
                _collectionCreatedCallback = new List<Action>();
            }

            foreach (var callback in collections)
            {
                callback.Invoke();
            }
        }

        #endregion

        /// <summary>
        /// ブックマークの変更監視
        /// </summary>
        private void BookmarkCollection_BookmarkChanged(object sender, BookmarkCollectionChangedEventArgs e)
        {
            if (_isCollectionCreating)
            {
                AddCollectionCreatedCallback(() => BookmarkCollection_BookmarkChanged(sender, e));
                return;
            }

            if (!(FolderCollection is BookmarkFolderCollection folderCollection))
            {
                return;
            }

            switch (e.Action)
            {
                case EntryCollectionChangedAction.Remove:
                    if (!BookmarkCollection.Current.Contains(folderCollection.BookmarkPlace))
                    {
                        RefreshBookmarkFolder();
                    }
                    break;

                case EntryCollectionChangedAction.Rename:
                    if (!BookmarkCollection.Current.Contains(folderCollection.BookmarkPlace))
                    {
                        RefreshBookmarkFolder();
                    }
                    else
                    {
                        var query = folderCollection.BookmarkPlace.CreateQuery();
                        if (!folderCollection.Place.Equals(query))
                        {
                            RequestPlace(query, null, FolderSetPlaceOption.UpdateHistory | FolderSetPlaceOption.ResetKeyword | FolderSetPlaceOption.Refresh);
                        }
                    }
                    break;

                case EntryCollectionChangedAction.Replace:
                case EntryCollectionChangedAction.Reset:
                    RefreshBookmarkFolder();
                    break;
            }
        }

        /// <summary>
        /// ブックマークフォルダーを同じパスで作り直す。存在しなければルートで作る。
        /// </summary>
        private void RefreshBookmarkFolder()
        {
            if (!(FolderCollection is BookmarkFolderCollection))
            {
                return;
            }

            ////Debug.WriteLine($"{this}: Refresh BookmarkFolder");
            var query = FolderCollection.Place;
            var node = BookmarkCollection.Current.FindNode(query);
            if (node == null || !(node.Value is BookmarkFolder))
            {
                query = new QueryPath(QueryScheme.Bookmark, null, null);
            }

            RequestPlace(query, null, FolderSetPlaceOption.UpdateHistory | FolderSetPlaceOption.ResetKeyword | FolderSetPlaceOption.Refresh);
        }

        #endregion Commands

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
                    if (_folderCollection != null)
                    {
                        _folderCollection.Dispose();
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


        public void SetSelectedItem(FolderItemPosition select, bool isFocus)
        {
            RaiseSelectedItemChanging();
            this.SelectedItem = FixedItem(select);
            RaiseSelectedItemChanged(isFocus);
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

        /// <summary>
        /// 選択項目の復元
        /// </summary>
        internal FolderItem FixedItem(FolderItemPosition pos)
        {
            if (pos == null)
            {
                return this.FolderCollection.FirstOrDefault();
            }

            if (pos.Index >= 0)
            {
                var item = this.FolderCollection.Items.ElementAtOrDefault(pos.Index);
                if (item != null && item.TargetPath == pos.Path)
                {
                    return item;
                }
            }

            // アーカイブ内のパスの場合、有効な項目になるまで場所を遡る
            var path = pos.Path;
            do
            {
                var select = this.FolderCollection.Items.FirstOrDefault(e => e.TargetPath == path);
                if (select != null)
                {
                    return select;
                }
                path = path.GetParent();
            }
            while (path != null && path.FullPath.Length > this.FolderCollection.Place.FullPath.Length);
            return this.FolderCollection.FirstOrDefault();
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
            SelectedChanged?.Invoke(this, new FolderListSelectedChangedEventArgs() { IsFocus = isFocus });
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
                SelectedChanging?.Invoke(this, new FolderListSelectedChangedEventArgs());
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
                SelectedChanged?.Invoke(this, new FolderListSelectedChangedEventArgs());
            }
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

            // ブックマークフォルダーは本として開けないようにする
            if (item.Attributes.HasFlag(FolderItemAttribute.Directory | FolderItemAttribute.Bookmark))
            {
                return;
            }

            var query = item.TargetPath;
            if (query.Scheme != QueryScheme.Pagemark && query.Path == null)
            {
                return;
            }

            var additionalOption = BookLoadOption.IsBook | (item.CanRemove() ? BookLoadOption.None : BookLoadOption.Undeliteable);
            BookHub.Current.RequestLoad(query.SimplePath, null, option | additionalOption, IsSyncBookshelfEnabled);
        }

        /// <summary>
        /// フォルダーの並びを設定
        /// </summary>
        public void SetFolderOrder(FolderOrder folderOrder)
        {
            if (FolderCollection == null) return;
            if (!FolderCollection.FolderOrderClass.GetFolderOrderMap().ContainsKey(folderOrder)) return;

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
            SetFolderOrder(GetNextFolderOrder());
            RaisePropertyChanged(nameof(FolderOrder));
        }

        public FolderOrder GetNextFolderOrder()
        {
            if (this.FolderCollection == null) return default;

            var orders = FolderCollection.FolderOrderClass.GetFolderOrderMap().Keys;
            var now = this.FolderCollection.FolderParameter.FolderOrder;
            var index = orders.IndexOf(now);
            return orders.ElementAt((index + 1) % orders.Count);
        }

        public void ToggleFolderRecursive_Executed()
        {
            this.FolderCollection.FolderParameter.IsFolderRecursive = !this.FolderCollection.FolderParameter.IsFolderRecursive;
        }

        public void NewFolder()
        {
            if (FolderCollection is BookmarkFolderCollection)
            {
                NewBookmarkFolder();
            }
        }

        public void NewBookmarkFolder()
        {
            if (FolderCollection is BookmarkFolderCollection bookmarkFolderCollection)
            {
                var node = BookmarkCollection.Current.AddNewFolder(bookmarkFolderCollection.BookmarkPlace);

                var item = bookmarkFolderCollection.FirstOrDefault(e => e.Attributes.HasFlag(FolderItemAttribute.Directory) && e.Name == node.Value.Name);

                if (item != null)
                {
                    SelectedItem = item;
                    SelectedChanged?.Invoke(this, new FolderListSelectedChangedEventArgs() { IsFocus = true, IsNewFolder = true });
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
                SelectedChanged?.Invoke(this, new FolderListSelectedChangedEventArgs() { IsFocus = isFocus });
            }
        }

        public bool AddBookmark()
        {
            var address = BookHub.Current.Book?.Address;
            if (address == null)
            {
                return false;
            }

            return AddBookmark(new QueryPath(address), true);
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
                    SelectedChanged?.Invoke(this, new FolderListSelectedChangedEventArgs() { IsFocus = isFocus });
                }
            }

            return true;
        }

        public bool RemoveBookmark(IEnumerable<FolderItem> items)
        {
            var nodes = items.Select(e => e.Source as TreeListNode<IBookmarkEntry>).Where(e => e != null).Reverse().ToList();
            if (!nodes.Any())
            {
                return false;
            }

            var mementos = new List<TreeListNodeMemento<IBookmarkEntry>>();
            int count = 0;

            foreach (var node in nodes)
            {
                var memento = new TreeListNodeMemento<IBookmarkEntry>(node);

                bool isRemoved = BookmarkCollection.Current.Remove(node);
                if (isRemoved)
                {
                    mementos.Add(memento);

                    if (node.Value is BookmarkFolder)
                    {
                        count += node.Count(e => e.Value is Bookmark);
                    }
                    else
                    {
                        count++;
                    }
                }
            }

            if (count >= 2)
            {
                var toast = new Toast(string.Format(Properties.Resources.DialogPagemarkFolderDelete, count), null, ToastIcon.Information, Properties.Resources.WordRestore,
                    () => { foreach (var memento in mementos) BookmarkCollection.Current.Restore(memento); });
                ToastService.Current.Show("BookmarkList", toast);
            }

            return (count > 0);
        }


        public FolderItem FindFolderItem(string address)
        {
            var path = new QueryPath(address);
            var select = this.FolderCollection.Items.FirstOrDefault(e => e.TargetPath == path);

            return select;
        }

        public async Task RemoveAsync(FolderItem item)
        {
            await RemoveAsync(new FolderItem[] { item });
        }

        public async Task RemoveAsync(IEnumerable<FolderItem> items)
        {
            if (items == null) return;

            var bookmarks = items.Where(e => e.Attributes.HasFlag(FolderItemAttribute.Bookmark)).ToList();
            var files = items.Where(e => e.IsFileSystem()).ToList();

            if (bookmarks.Any())
            {
                RemoveBookmark(bookmarks);
            }
            else if (files.Any())
            {
                await RemoveFilesAsync(files);
            }
        }

        private async Task RemoveFilesAsync(IEnumerable<FolderItem> items)
        {
            if (!items.Any()) return;

            FolderItem next = null;
            FolderItem currentBook = items.FirstOrDefault(e => e.TargetPath.SimplePath == BookHub.Current.Address);

            if (Config.Current.Bookshelf.IsOpenNextBookWhenRemove && currentBook != null)
            {
                var index = GetFolderItemIndex(currentBook);
                if (index >= 0)
                {
                    next = FolderCollection
                        .Skip(index)
                        .Concat(FolderCollection.Take(index).Reverse())
                        .Where(e => !items.Contains(e))
                        .FirstOrDefault();
                }
            }

            var removed = await FileIO.Current.RemoveFileAsync(items.Select(e => e.TargetPath.SimplePath).ToList(), Properties.Resources.DialogFileDeleteBookTitle);
            if (removed)
            {
                var removes = items.Where(e => !FileIO.Current.Exists(e.TargetPath.SimplePath)).ToList();
                foreach (var item in removes)
                {
                    FolderCollection?.RequestDelete(item.TargetPath);
                }

                if (next != null && !FolderCollection.IsEmpty())
                {
                    SelectedItem = next;
                    LoadBook(SelectedItem);
                }
            }
        }



        #region Memento

        [DataContract]
        public class Memento : IMemento
        {
            [DataMember]
            public PanelListItemStyle PanelListItemStyle { get; set; }

            [DataMember, DefaultValue(FolderTreeLayout.Left)]
            public FolderTreeLayout FolderTreeLayout { get; set; }

            [DataMember, DefaultValue(72.0)]
            public double FolderTreeAreaHeight { get; set; }

            [DataMember, DefaultValue(128.0)]
            public double FolderTreeAreaWidth { get; set; }

            [DataMember, DefaultValue(false)]
            public bool IsFolderTreeVisible { get; set; }

            [DataMember]
            public bool IsSyncFolderTree { get; set; }

            [OnDeserializing]
            private void OnDeserializing(StreamingContext c)
            {
                this.InitializePropertyDefaultValues();
            }

            public void RestoreConfig(FolderListConfig config)
            {
                config.PanelListItemStyle = PanelListItemStyle;
                config.FolderTreeLayout = FolderTreeLayout;
                config.FolderTreeAreaHeight = FolderTreeAreaHeight;
                config.FolderTreeAreaWidth = FolderTreeAreaWidth;
                config.IsFolderTreeVisible = IsFolderTreeVisible;
            }
        }

        public Memento CreateMemento()
        {
            var memento = new Memento();
            memento.PanelListItemStyle = _folderListConfig.PanelListItemStyle;
            memento.FolderTreeLayout = _folderListConfig.FolderTreeLayout;
            memento.FolderTreeAreaHeight = _folderListConfig.FolderTreeAreaHeight;
            memento.FolderTreeAreaWidth = _folderListConfig.FolderTreeAreaWidth;
            memento.IsFolderTreeVisible = _folderListConfig.IsFolderTreeVisible;
            memento.IsSyncFolderTree = Config.Current.Bookshelf.IsSyncFolderTree;

            return memento;
        }

        #endregion
    }

}
