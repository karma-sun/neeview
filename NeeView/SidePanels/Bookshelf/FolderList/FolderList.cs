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

        public QueryPath Path { get; private set; }
        ////public QueryPath TargetPath { get; set; }
        public int Index { get; private set; }
    }


    /// <summary>
    /// FolderList Model
    /// </summary>
    public abstract class FolderList : BindableBase, IDisposable
    {
        #region Fields

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

        #endregion Fields

        #region Constructors

        protected FolderList(bool isSyncBookHub, bool isOverlayEnabled, FolderListConfig folderListConfig)
        {
            _folderListConfig = folderListConfig;
            _folderListBoxModel = new FolderListBoxModel(null);

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
                BookHub.Current.HistoryChanged += (s, e) => _folderListBoxModel.RefreshIcon(new QueryPath(e.Key));
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

        public FolderListConfig FolderListConfig => _folderListConfig;

        public FolderCollectionFactory FolderCollectionFactory { get; }

#if false
        private PanelListItemStyle _panelListItemStyle;
        public PanelListItemStyle PanelListItemStyle
        {
            get { return _panelListItemStyle; }
            set { if (_panelListItemStyle != value) { _panelListItemStyle = value; RaisePropertyChanged(); } }
        }
#endif

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
                        return SidePanelProfile.Current.ContentItemImageWidth > 0.0;
                    case PanelListItemStyle.Banner:
                        return SidePanelProfile.Current.BannerItemImageWidth > 0.0;
                }
            }
        }

#if false
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
        [PropertyPath("@ParamBookshelfHome", FileDialogType = FileDialogType.Directory)]
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

        [PropertyMember("@ParamBookshelfIsOpenNextBookWhenRemove")]
        public bool IsOpenNextBookWhenRemove { get; set; } = true;

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
#endif

        /// <summary>
        /// フォルダーコレクション
        /// </summary>
        private FolderCollection _folderCollection;
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


        private FolderListBoxModel _folderListBoxModel;
        public FolderListBoxModel FolderListBoxModel
        {
            get { return _folderListBoxModel; }
            private set { SetProperty(ref _folderListBoxModel, value); }
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
        /// 検索BOXの表示
        /// </summary>
        public bool IsFolderSearchBoxVisible => true;

        /// <summary>
        /// 入力キーワード
        /// </summary>
        private string _inputKeyword;
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

#if false
        /// <summary>
        /// インクリメンタルサーチ有効
        /// </summary>
        public bool IsIncrementalSearchEnabled { get; set; } = true;

        /// <summary>
        /// サブフォルダーを含めた検索を行う
        /// </summary>
        public bool IsSearchIncludeSubdirectories
        {
            get { return _searchEngine.IncludeSubdirectories; }
            set
            {
                if (_searchEngine.IncludeSubdirectories != value)
                {
                    _searchEngine.IncludeSubdirectories = value;
                    RequestSearchPlace(true);
                }
            }
        }
#endif

        protected virtual bool IsIncrementalSearchEnabled() => false;
        protected virtual bool IsSearchIncludeSubdirectories() => false;


        /// <summary>
        /// 検索キーワードエラーメッセージ
        /// </summary>
        private string _searchKeywordErrorMessage;
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
#if false
        /// <summary>
        /// フォルダーツリーの表示
        /// </summary>
        private bool _isFolderTreeVisible = false;
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
#endif

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
        private double _areaWidth = double.PositiveInfinity;
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
        private double _areaHeight = double.PositiveInfinity;
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
        private bool _IsLocked;
        public bool IsLocked
        {
            get { return _IsLocked; }
            set { SetProperty(ref _IsLocked, value && Place != null); }
        }

#if false
        /// <summary>
        /// 本の読み込みで本棚の更新を要求する
        /// </summary>
        private bool _isSyncBookshelfEnabled;
        public bool IsSyncBookshelfEnabled
        {
            get { return _isSyncBookshelfEnabled; }
            set
            {
                if (SetProperty(ref _isSyncBookshelfEnabled, value) && FolderListBoxModel != null)
                {
                    FolderListBoxModel.IsSyncBookshelfEnabled = _isSyncBookshelfEnabled;
                }
            }
        }
#endif

        protected virtual bool IsSyncBookshelfEnabled() => false;

        public PageListPlacementService PageListPlacementService => PageListPlacementService.Current;

        #endregion

        #region Methods

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
            this.FolderListBoxModel.IsFocusAtOnce = true;
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
            if (_folderListBoxModel != null)
            {
                SavePlace(Place, _folderListBoxModel.SelectedItem, _folderListBoxModel.GetFolderItemIndex(_folderListBoxModel.SelectedItem));
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
                    // NOTE: Focus要求フラグのみ引き継ぐ
                    bool isFocusAtOnce = this.FolderListBoxModel != null && this.FolderListBoxModel.IsFocusAtOnce;

                    this.FolderCollection = collection;
                    this.FolderListBoxModel = new FolderListBoxModel(this.FolderCollection);
                    this.FolderListBoxModel.IsSyncBookshelfEnabled = IsSyncBookshelfEnabled();
                    this.FolderListBoxModel.SetSelectedItem(select, options.HasFlag(FolderSetPlaceOption.Focus));
                    this.FolderListBoxModel.IsFocusAtOnce = isFocusAtOnce;
                    if (options.HasFlag(FolderSetPlaceOption.Focus))
                    {
                        FocusAtOnce();
                    }

                    RaiseCollectionChanged();

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
        /// フォルダーの次の並び順を取得
        /// </summary>
        public FolderOrder GetNextFolderOrder()
        {
            return _folderListBoxModel.GetNextFolderOrder();
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

            _folderListBoxModel.RefreshIcon(null);
        }

        #endregion Methods

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
            var item = _folderListBoxModel.GetFolderItem(_folderListBoxModel.SelectedItem, direction);
            if (item == null)
            {
                return false;
            }

            int index = _folderListBoxModel.GetFolderItemIndex(item);

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

            var item = _folderListBoxModel.SelectedItem;
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

        /// <summary>
        /// コレクション作成
        /// </summary>
        public async Task<FolderCollection> CreateFolderCollectionAsync(QueryPath path, bool isForce)
        {
            try
            {
                BusyChanged?.Invoke(this, new BusyChangedEventArgs(true));

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
                BusyChanged?.Invoke(this, new BusyChangedEventArgs(false));
            }

            return null;
        }

        /// <summary>
        /// コレクション作成
        /// </summary>
        private async Task<FolderCollection> CreateFolderCollectionAsync(QueryPath path, bool isForce, CancellationToken token)
        {
            ////var factory = FolderCollectionFactory.Current;

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
            Config.Current.Layout.Bookshelf.Home = Place.SimplePath;
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
            _folderListBoxModel.ToggleFolderRecursive_Executed();
        }

        protected virtual void CloseBookIfNecessary()
        {
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
            var place = BookHub.Current.Book?.Address;
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


        /// <summary>
        /// ブックマークの変更監視
        /// </summary>
        private void BookmarkCollection_BookmarkChanged(object sender, BookmarkCollectionChangedEventArgs e)
        {
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
            memento.IsSyncFolderTree = Config.Current.Layout.Bookshelf.IsSyncFolderTree;

            return memento;
        }

        [Obsolete]
        public void Restore(Memento memento)
        {
            if (memento == null) return;

            ////this.PanelListItemStyle = memento.PanelListItemStyle;
            ////this.FolderTreeLayout = memento.FolderTreeLayout;
            ////this.FolderTreeAreaHeight = memento.FolderTreeAreaHeight;
            ////this.FolderTreeAreaWidth = memento.FolderTreeAreaWidth;
            ////this.IsFolderTreeVisible = memento.IsFolderTreeVisible;
            ////this.IsSyncFolderTree = memento.IsSyncFolderTree;
        }

        #endregion
    }

}
