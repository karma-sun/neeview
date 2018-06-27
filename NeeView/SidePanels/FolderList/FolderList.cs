using NeeLaboratory.ComponentModel;
using NeeView.Windows.Property;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;

namespace NeeView
{
    //
    public class SelectedChangedEventArgs : EventArgs
    {
        public bool IsFocus { get; set; }
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
    public class FolderList : BindableBase, IDisposable
    {
        public static FolderList Current { get; private set; }

        #region Fields

        private BookHub _bookHub;

        /// <summary>
        /// そのフォルダーで最後に選択されていた項目の記憶
        /// </summary>
        private Dictionary<string, string> _lastPlaceDictionary = new Dictionary<string, string>();

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

        #endregion

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
            _bookHub.HistoryChanged += (s, e) => RefleshIcon(e.Key);

            _bookHub.BookmarkChanged += (s, e) =>
            {
                switch (e.Action)
                {
                    case NotifyCollectionChangedAction.Reset:
                    case NotifyCollectionChangedAction.Replace:
                        RefleshIcon(null);
                        break;
                    default:
                        if (e.Item.Value is Bookmark bookmark)
                        {
                            RefleshIcon(bookmark.Place);
                        }
                        break;
                }
            };

            _bookHub.LoadRequested += (s, e) => CancelMoveCruiseFolder();
        }

        #endregion

        #region Events

        public event EventHandler PlaceChanged;

        //
        public event EventHandler SelectedChanging;
        public event EventHandler<SelectedChangedEventArgs> SelectedChanged;

        // FolderCollection総入れ替え
        public event EventHandler CollectionChanged;

        // 検索ボックスにフォーカスを
        public event EventHandler SearchBoxFocus;

        public event ErrorEventHandler QuickAccessFocus;

        /// <summary>
        /// リスト更新処理中イベント
        /// </summary>
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
                    case PanelListItemStyle.Content:
                        return ThumbnailProfile.Current.ThumbnailWidth > 0.0;
                    case PanelListItemStyle.Banner:
                        return ThumbnailProfile.Current.BannerWidth > 0.0;
                }
            }
        }

        /// <summary>
        /// フォルダーアイコン表示位置
        /// </summary>
        private FolderIconLayout _folderIconLayout = FolderIconLayout.Default;
        [PropertyMember("@ParamFolderListFolderIconLayout")]
        public FolderIconLayout FolderIconLayout
        {
            get { return _folderIconLayout; }
            set { if (_folderIconLayout != value) { _folderIconLayout = value; RaisePropertyChanged(); } }
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
        public bool IsFolderSearchEnabled => Place != null && !(FolderCollection is FolderArchiveCollection);

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
        public string Place => this.FolderCollection?.Place;

        /// <summary>
        /// フォルダー履歴
        /// </summary>
        public History<string> History { get; private set; } = new History<string>();

        /// <summary>
        /// IsFolderSearchVisible property.
        /// </summary>
        private bool _IsFolderSearchVisible = true;
        public bool IsFolderSearchBoxVisible
        {
            get { return _IsFolderSearchVisible; }
            set { if (_IsFolderSearchVisible != value) { _IsFolderSearchVisible = value; RaisePropertyChanged(); if (!_IsFolderSearchVisible) this.SearchKeyword = ""; } }
        }

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
        /// クイックアクセスエリアの表示
        /// </summary>
        private bool _isQuickAccessVisible = false;
        public bool IsQuickAccessVisible
        {
            get { return _isQuickAccessVisible; }
            set { SetProperty(ref _isQuickAccessVisible, value); }
        }

        /// <summary>
        /// クイックアクセスエリアの高さ
        /// </summary>
        private double _quickAccessAreaHeight = 32.0 - 6.0;
        public double QuickAccessAreaHeight
        {
            get { return _quickAccessAreaHeight; }
            set
            {
                var height = Math.Max(Math.Min(value, _areaHeight - 32.0), 32.0 - 6.0);
                SetProperty(ref _quickAccessAreaHeight, height);
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
                    QuickAccessAreaHeight = _quickAccessAreaHeight;
                }
            }
        }




        #endregion

        #region Methods

        /// <summary>
        /// 補正されたHOME取得
        /// </summary>
        /// <returns></returns>
        public string GetFixedHome()
        {
            if (Directory.Exists(_home)) return _home;

            var myPicture = System.Environment.GetFolderPath(System.Environment.SpecialFolder.MyPictures);
            if (Directory.Exists(myPicture)) return myPicture;

            // 救済措置。
            return Environment.CurrentDirectory;
        }

        /// <summary>
        /// ふさわしい選択項目インデックスを取得
        /// </summary>
        /// <param name="path">選択したいパス</param>
        /// <returns></returns>
        internal int FixedIndexOfPath(string path)
        {
            var index = this.FolderCollection.IndexOfPath(path);
            return index < 0 ? 0 : index;
        }

        //
        internal FolderItem FixedItem(string path)
        {
            return this.FolderCollection.FirstOrDefault(path) ?? this.FolderCollection.FirstOrDefault();
        }

        /// <summary>
        /// フォルダー状態保存
        /// </summary>
        /// <param name="folder"></param>
        private void SavePlace(FolderItem folder)
        {
            if (folder == null || folder.Place == null) return;
            _lastPlaceDictionary[folder.Place] = folder.Path;
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
        /// <param name="place"></param>
        public void ResetPlace(string queryPath)
        {
            var task = SetPlaceAsync(queryPath ?? GetFixedHome(), null, FolderSetPlaceOption.IsUpdateHistory);
        }

        /// <summary>
        /// フォルダーリスト更新
        /// </summary>
        /// <param name="place">フォルダーパス</param>
        /// <param name="select">初期選択項目</param>
        public async Task SetPlaceAsync(string queryPath, string select, FolderSetPlaceOption options)
        {
            // 現在フォルダーの情報を記憶
            SavePlace(GetFolderItem(0));

            var query = new QueryPath(queryPath);
            var place = query.Path;
            var keyword = query.Search ?? "";

            // 初期項目
            if (select == null && place != null)
            {
                _lastPlaceDictionary.TryGetValue(place, out select);
            }

            if (options.HasFlag(FolderSetPlaceOption.IsTopSelect))
            {
                select = null;
            }

            // 更新が必要であれば、新しいFolderListBoxを作成する
            if (CheckFolderListUpdateneNcessary(place, keyword, options))
            {
                _isDarty = false;

                // 検索キーワードクリア
                if (this.FolderCollection == null || place != Place || options.HasFlag(FolderSetPlaceOption.ResetKeyword))
                {
                    _searchKeyword = keyword;
                    RaisePropertyChanged(nameof(SearchKeyword));
                    if (keyword != "")
                    {
                        UpdateSearchHistory();
                    }
                }

                // FolderCollection 更新
                var isSuccess = await UpdateFolderCollectionAsync(place, true);
                if (isSuccess)
                {
                    this.SelectedItem = FixedItem(select);

                    RaiseSelectedItemChanged(options.HasFlag(FolderSetPlaceOption.IsFocus));

                    // 最終フォルダー更新
                    BookHistoryCollection.Current.LastFolder = Place;

                    // 履歴追加
                    if (options.HasFlag(FolderSetPlaceOption.IsUpdateHistory))
                    {
                        if (Place != this.History.GetCurrent())
                        {
                            this.History.Add(Place);
                        }
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
        private bool CheckFolderListUpdateneNcessary(string place, string keyword, FolderSetPlaceOption options)
        {
            if (_isDarty || this.FolderCollection == null || place != this.FolderCollection.Place)
            {
                return true;
            }

            if (options.HasFlag(FolderSetPlaceOption.ResetKeyword) && keyword != _searchKeyword)
            {
                return true;
            }

            return false;
        }

        /// <summary>
        /// フォルダーリスト項目変更前処理
        /// 項目が削除される前に有効な選択項目に変更する
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void FolderCollection_Deleting(object sender, System.IO.FileSystemEventArgs e)
        {
            if (e.ChangeType != System.IO.WatcherChangeTypes.Deleted) return;

            var item = this.FolderCollection.FirstOrDefault(e.FullPath);
            if (item != this.SelectedItem) return;

            RaiseSelectedItemChanging();
            this.SelectedItem = GetNeighbor(item);
            RaiseSelectedItemChanged();
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



        /// <summary>
        /// フォルダーリスト更新
        /// </summary>
        /// <param name="force">必要が無い場合も更新する</param>
        public async Task RefleshAsync(bool force)
        {
            if (this.FolderCollection == null) return;

            _isDarty = force || this.FolderCollection.IsDarty();

            await SetPlaceAsync(Place, null, FolderSetPlaceOption.IsUpdateHistory);
        }



        /// <summary>
        /// 選択項目を基準とした項目取得
        /// </summary>
        /// <param name="offset">選択項目から前後した項目を指定</param>
        /// <returns></returns>
        internal FolderItem GetFolderItem(int offset)
        {
            if (this.FolderCollection?.Items == null) return null;

            int index = this.FolderCollection.Items.IndexOf(this.SelectedItem);
            if (index < 0) return null;

            int next = (this.FolderCollection.FolderParameter.FolderOrder == FolderOrder.Random)
                ? (index + this.FolderCollection.Items.Count + offset) % this.FolderCollection.Items.Count
                : index + offset;

            if (next < 0 || next >= this.FolderCollection.Items.Count) return null;

            return this.FolderCollection[next];
        }


        /// <summary>
        /// 現在開いているフォルダーで更新(弱)
        /// e.isKeepPlaceが有効の場合、フォルダーは移動せず現在選択項目のみの移動を試みる
        /// </summary>
        public async Task SyncWeak(FolderListSyncEventArgs e)
        {
            if (e != null && e.isKeepPlace)
            {
                if (this.FolderCollection == null || this.FolderCollection.Contains(e.Path)) return;
            }

            var options = FolderSetPlaceOption.IsUpdateHistory;
            await SetPlaceAsync(e.Parent, e.Path, options);
        }

        /// <summary>
        /// フォルダーアイコンの表示更新
        /// </summary>
        /// <param name="path">更新するパス。nullならば全て更新</param>
        public void RefleshIcon(string path)
        {
            this.FolderCollection?.RefleshIcon(path);
        }

        // ブックの読み込み
        public void LoadBook(string path)
        {
            BookLoadOption option = BookLoadOption.SkipSamePlace | (this.FolderCollection.FolderParameter.IsFolderRecursive ? BookLoadOption.DefaultRecursive : BookLoadOption.None);
            LoadBook(path, option);
        }

        // ブックの読み込み
        public void LoadBook(string path, BookLoadOption option)
        {
            _bookHub.RequestLoad(path, null, option | BookLoadOption.IsBook, false);
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
        /// クイックアクセスにフォーカス要求
        /// </summary>
        public void RaiseQuickAccessFocus()
        {
            QuickAccessFocus?.Invoke(this, null);
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
            BookHistoryCollection.Current.RemovePlace(this.Place);
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
            var item = this.GetFolderItem(direction);
            if (item == null) return false;

            await SetPlaceAsync(this.FolderCollection.Place, item.Path, FolderSetPlaceOption.IsUpdateHistory);
            _bookHub.RequestLoad(item.TargetPath, null, options | BookLoadOption.IsBook, false);
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
            var cancel = _cruiseFolderCancellationTokenSource.Token;

            try
            {
                var node = new FolderNode(this.FolderCollection, item);
                var next = (direction < 0) ? await node.CruisePrev(cancel) : await node.CruiseNext(cancel);
                if (next == null) return false;

                await SetPlaceAsync(next.Content.Place, next.Content.Path, FolderSetPlaceOption.IsUpdateHistory);
                _bookHub.RequestLoad(next.Content.TargetPath, null, options | BookLoadOption.IsBook, false);
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
            return await UpdateFolderCollectionAsync(Place, isForce);
        }

        /// <summary>
        /// コレクション更新
        /// </summary>
        public async Task<bool> UpdateFolderCollectionAsync(string place, bool isForce)
        {
            try
            {
                BusyChanged?.Invoke(this, new BusyChangedEventArgs(true));

                _updateFolderCancellationTokenSource?.Cancel();
                _updateFolderCancellationTokenSource = new CancellationTokenSource();

                var collection = await CreateFolderCollectionAsync(place, isForce, _updateFolderCancellationTokenSource.Token);

                if (collection != null && !_updateFolderCancellationTokenSource.Token.IsCancellationRequested)
                {
                    collection.ParameterChanged += async (s, e) => await RefleshAsync(true);
                    collection.Deleting += FolderCollection_Deleting;
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
                Debug.WriteLine($"UpdateFolderCollectionAsync: Canceled: {place}");
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
        private async Task<FolderCollection> CreateFolderCollectionAsync(string place, bool isForce, CancellationToken token)
        {
            var factory = FolderCollectionFactory.Current;

            var keyword = GetFixedSearchKeyword();
            if (!string.IsNullOrEmpty(keyword))
            {
                if (!isForce && FolderCollection is FolderSearchCollection && FolderCollection.IsSame(place, keyword)) return null;
                factory.SearchEngine.CancelSearch();
                return await factory.CreateSearchFolderCollectionAsync(place, keyword, true, token);
            }
            else
            {
                if (!isForce && !(FolderCollection is FolderSearchCollection) && FolderCollection.IsSame(place, null)) return null;
                factory.SearchEngine.Reset();
                return await factory.CreateFolderCollectionAsync(place, true, token);
            }
        }

        #endregion

        #region Commands


        public void AddQuickAccess()
        {
            if (Place.StartsWith(Temporary.TempDirectory))
            {
                ToastService.Current.Show(new Toast(Properties.Resources.DialogQuickAccessTempError));
                return;
            }

            IsQuickAccessVisible = true;

            var item = new QuickAccess(new QueryPath(Place, GetFixedSearchKeyword()).FullPath);
            QuickAccessCollection.Current.Add(item);
        }

        //
        public void SetHome_Executed()
        {
            if (_bookHub == null) return;
            this.Home = Place;
        }

        //
        public async void MoveToHome_Executed()
        {
            if (_bookHub == null) return;

            var place = GetFixedHome();
            await SetPlaceAsync(place, null, FolderSetPlaceOption.IsFocus | FolderSetPlaceOption.IsUpdateHistory | FolderSetPlaceOption.IsTopSelect | FolderSetPlaceOption.ResetKeyword);

            CloseBookIfNecessary();
        }


        //
        public async void MoveTo_Executed(string path)
        {
            await this.SetPlaceAsync(path, null, FolderSetPlaceOption.IsFocus | FolderSetPlaceOption.IsUpdateHistory);

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
            await SetPlaceAsync(place, null, FolderSetPlaceOption.IsFocus);
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
            await SetPlaceAsync(place, null, FolderSetPlaceOption.IsFocus);
            this.History.Move(+1);

            CloseBookIfNecessary();
        }

        //
        public async void MoveToHistory_Executed(KeyValuePair<int, string> item)
        {
            var place = this.History.GetHistory(item.Key);
            await SetPlaceAsync(place, null, FolderSetPlaceOption.IsFocus);
            this.History.SetCurrent(item.Key + 1);

            CloseBookIfNecessary();
        }

        //
        public bool MoveToParent_CanExecute()
        {
            return (Place != null);
        }

        //
        public async void MoveToParent_Execute()
        {
            if (Place == null) return;

            var parent = this.FolderCollection?.GetParentPlace();
            await SetPlaceAsync(parent, Place, FolderSetPlaceOption.IsFocus | FolderSetPlaceOption.IsUpdateHistory);

            CloseBookIfNecessary();
        }


        //
        public async void Sync_Executed()
        {
            string place = _bookHub?.Book?.Place;

            if (place != null)
            {
                var parent = _bookHub?.Book?.Archiver?.Parent?.FullPath ?? LoosePath.GetDirectoryName(place);

                _isDarty = true; // 強制更新
                await SetPlaceAsync(parent, place, FolderSetPlaceOption.IsFocus | FolderSetPlaceOption.IsUpdateHistory | FolderSetPlaceOption.ResetKeyword);

                RaiseSelectedItemChanged(true);
            }
            else if (Place != null)
            {
                _isDarty = true; // 強制更新
                await SetPlaceAsync(Place, null, FolderSetPlaceOption.IsFocus);

                RaiseSelectedItemChanged(true);
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
            public FolderIconLayout FolderIconLayout { get; set; }

            [DataMember]
            public bool IsVisibleHistoryMark { get; set; }

            [DataMember]
            public bool IsVisibleBookmarkMark { get; set; }

            [DataMember]
            public string Home { get; set; }

            [DataMember, DefaultValue(true)]
            public bool IsInsertItem { get; set; }

            [DataMember, DefaultValue(true)]
            public bool IsFolderSearchBoxVisible { get; set; }

            [DataMember]
            public bool IsMultipleRarFilterEnabled { get; set; }

            [DataMember]
            public string ExcludePattern { get; set; }

            [DataMember]
            public bool IsCruise { get; set; }

            [DataMember]
            public bool IsCloseBookWhenMove { get; set; }

            [DataMember]
            public double QuickAccessAreaHeight { get; set; }

            [DataMember, DefaultValue(false)]
            public bool IsQuickAccessVisible { get; set; }

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
            memento.FolderIconLayout = this.FolderIconLayout;
            memento.IsVisibleHistoryMark = this.IsVisibleHistoryMark;
            memento.IsVisibleBookmarkMark = this.IsVisibleBookmarkMark;
            memento.Home = this.Home;
            memento.IsInsertItem = this.IsInsertItem;
            memento.IsFolderSearchBoxVisible = this.IsFolderSearchBoxVisible;
            memento.IsMultipleRarFilterEnabled = this.IsMultipleRarFilterEnabled;
            memento.ExcludePattern = this.ExcludePattern;
            memento.IsCruise = this.IsCruise;
            memento.IsCloseBookWhenMove = this.IsCloseBookWhenMove;
            memento.QuickAccessAreaHeight = this.QuickAccessAreaHeight;
            memento.IsQuickAccessVisible = this.IsQuickAccessVisible;

            return memento;
        }

        //
        public void Restore(Memento memento)
        {
            if (memento == null) return;

            this.PanelListItemStyle = memento.PanelListItemStyle;
            this.FolderIconLayout = memento.FolderIconLayout;
            this.IsVisibleHistoryMark = memento.IsVisibleHistoryMark;
            this.IsVisibleBookmarkMark = memento.IsVisibleBookmarkMark;
            this.Home = memento.Home;
            this.IsInsertItem = memento.IsInsertItem;
            this.IsFolderSearchBoxVisible = memento.IsFolderSearchBoxVisible;
            this.IsMultipleRarFilterEnabled = memento.IsMultipleRarFilterEnabled;
            this.ExcludePattern = memento.ExcludePattern;
            this.IsCruise = memento.IsCruise;
            this.IsCloseBookWhenMove = memento.IsCloseBookWhenMove;
            this.QuickAccessAreaHeight = memento.QuickAccessAreaHeight;
            this.IsQuickAccessVisible = memento.IsQuickAccessVisible;
        }

        #endregion
    }

}
