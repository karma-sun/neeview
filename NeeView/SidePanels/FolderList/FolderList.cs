// Copyright (c) 2016-2017 Mitsuhiro Ito (nee)
//
// This software is released under the MIT License.
// http://opensource.org/licenses/mit-license.php

using NeeView.ComponentModel;
using NeeView.Windows.Property;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace NeeView
{
    //
    public class SelectedChangedEventArgs : EventArgs
    {
        public bool IsFocus { get; set; }
    }



    //
    public class FolderList : BindableBase
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
        private SearchEngine _searchEngine;

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

            this.FolderPanel = folderPanel;
            _bookHub = bookHub;

            _bookHub.FolderListSync += (s, e) => SyncWeak(e);
            _bookHub.HistoryChanged += (s, e) => RefleshIcon(e.Key);
            _bookHub.BookmarkChanged += (s, e) => RefleshIcon(e.Key);
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

        #endregion

        #region Properties

        //
        public FolderPanelModel FolderPanel { get; private set; }

        /// <summary>
        /// PanelListItemStyle property.
        /// </summary>
        private PanelListItemStyle _panelListItemStyle;
        public PanelListItemStyle PanelListItemStyle
        {
            get { return _panelListItemStyle; }
            set { if (_panelListItemStyle != value) { _panelListItemStyle = value; RaisePropertyChanged(); } }
        }

        /// <summary>
        /// フォルダーアイコン表示位置
        /// </summary>
        private FolderIconLayout _folderIconLayout = FolderIconLayout.Default;
        public FolderIconLayout FolderIconLayout
        {
            get { return _folderIconLayout; }
            set { if (_folderIconLayout != value) { _folderIconLayout = value; RaisePropertyChanged(); } }
        }

        /// <summary>
        /// IsVisibleHistoryMark property.
        /// </summary>
        private bool _isVisibleHistoryMark = true;
        public bool IsVisibleHistoryMark
        {
            get { return _isVisibleHistoryMark; }
            set { if (_isVisibleHistoryMark != value) { _isVisibleHistoryMark = value; RaisePropertyChanged(); } }
        }

        /// <summary>
        /// IsVisibleBookmarkMark property.
        /// </summary>
        private bool _isVisibleBookmarkMark = true;
        public bool IsVisibleBookmarkMark
        {
            get { return _isVisibleBookmarkMark; }
            set { if (_isVisibleBookmarkMark != value) { _isVisibleBookmarkMark = value; RaisePropertyChanged(); } }
        }

        /// </summary>
        private string _home;
        public string Home
        {
            get { return _home; }
            set { if (_home != value) { _home = value; RaisePropertyChanged(); } }
        }

        /// <summary>
        /// 追加されたファイルを挿入する？
        /// OFFの場合はリスト末尾に追加する
        /// </summary>
        public bool IsInsertItem { get; set; } = true;

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
                }
            }
        }

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
        private string Place => FolderCollection?.Place;

        /// <summary>
        /// フォルダー履歴
        /// </summary>
        public History<string> History { get; private set; } = new History<string>();

        /// <summary>
        /// IsFolderSearchVisible property.
        /// </summary>
        private bool _IsFolderSearchVisible;
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

        /// <summary>
        /// SearchHistory property.
        /// </summary>
        private ObservableCollection<string> _searchHistory = new ObservableCollection<string>();
        public ObservableCollection<string> SearchHistory
        {
            get { return _searchHistory; }
            set { if (_searchHistory != value) { _searchHistory = value; RaisePropertyChanged(); } }
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
            if (folder == null || folder.ParentPath == null) return;
            _lastPlaceDictionary[folder.ParentPath] = folder.Path;
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
        public void ResetPlace(string place)
        {
            SetPlace(place ?? GetFixedHome(), null, FolderSetPlaceOption.IsUpdateHistory);
        }

        /// <summary>
        /// フォルダーリスト更新
        /// </summary>
        /// <param name="place">フォルダーパス</param>
        /// <param name="select">初期選択項目</param>
        public void SetPlace(string place, string select, FolderSetPlaceOption options)
        {
            // 現在フォルダーの情報を記憶
            SavePlace(GetFolderItem(0));

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
            if (CheckFolderListUpdateneNcessary(place, options.HasFlag(FolderSetPlaceOption.ClearSearchKeyword)))
            {
                _isDarty = false;

                // 検索エンジン停止
                _searchEngine?.Dispose();
                _searchEngine = null;
                this.SearchKeyword = null;

                // FolderCollection 更新
                this.FolderCollection = CreateFolderCollection(place, null);
                this.SelectedItem = FixedItem(select);

                RaiseSelectedItemChanged(options.HasFlag(FolderSetPlaceOption.IsFocus));

                // 最終フォルダー更新
                BookHistory.Current.LastFolder = Place;

                // 履歴追加
                if (options.HasFlag(FolderSetPlaceOption.IsUpdateHistory))
                {
                    if (place != this.History.GetCurrent())
                    {
                        this.History.Add(place);
                    }
                }
            }
            else
            {
                // 選択項目のみ変更
                this.SelectedItem = FixedItem(select);
            }

            // 変更通知
            PlaceChanged?.Invoke(this, null);
        }

        /// <summary>
        /// リストの更新必要性チェック
        /// </summary>
        /// <param name="place"></param>
        /// <param name="releaseSearchMode">検索モード解除</param>
        /// <returns></returns>
        private bool CheckFolderListUpdateneNcessary(string place, bool releaseSearchMode)
        {
            return (_isDarty || this.FolderCollection == null || place != this.FolderCollection.Place || (releaseSearchMode && this.FolderCollection.Mode == FolderCollectionMode.Search));
        }


        /// <summary>
        /// FolderCollection 作成
        /// </summary>
        /// <param name="place"></param>
        /// <param name="searchResult"></param>
        /// <returns></returns>
        private FolderCollection CreateFolderCollection(string place, NeeLaboratory.IO.Search.SearchResultWatcher searchResult)
        {
            if (searchResult == null)
            {
                return CreateEntryCollection(place);
            }
            else
            {
                return CreateSearchCollection(place, searchResult);
            }
        }

        /// <summary>
        /// FolderCollection 作成
        /// </summary>
        /// <param name="place"></param>
        /// <returns></returns>
        private FolderCollection CreateEntryCollection(string place)
        {
            FolderCollection collection;

            try
            {
                collection = new FolderCollection(place);
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex.Message);

                // 救済措置。取得に失敗した時はカレントディレクトリに移動
                collection = new FolderCollection(Environment.CurrentDirectory);
            }

            collection.ParameterChanged += (s, e) => App.Current?.Dispatcher.BeginInvoke((Action)(delegate () { Reflesh(true); }));
            collection.Deleting += FolderCollection_Deleting;
            return collection;
        }

        /// <summary>
        /// FolderCollection作成(検索結果)
        /// </summary>
        /// <param name="searchResult"></param>
        /// <returns></returns>
        private FolderCollection CreateSearchCollection(string place, NeeLaboratory.IO.Search.SearchResultWatcher searchResult)
        {
            var collection = new FolderCollection(place, searchResult);

            // TODO: 検索結果に対しては処理はちがうだろ？
            collection.ParameterChanged += (s, e) => App.Current?.Dispatcher.BeginInvoke((Action)(delegate () { Reflesh(true); }));
            collection.Deleting += FolderCollection_Deleting;

            return collection;
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
        public void Reflesh(bool force)
        {
            if (this.FolderCollection == null) return;

            _isDarty = force || this.FolderCollection.IsDarty();

            SetPlace(Place, null, FolderSetPlaceOption.IsUpdateHistory);
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
        /// <param name="e"></param>
        public void SyncWeak(FolderListSyncArguments e)
        {
            if (e != null && e.isKeepPlace)
            {
                if (this.FolderCollection == null || this.FolderCollection.Contains(e.Path)) return;
            }

            var options = FolderSetPlaceOption.IsUpdateHistory;
            SetPlace(System.IO.Path.GetDirectoryName(e.Path), e.Path, options);
        }

        /// <summary>
        /// フォルダーアイコンの表示更新
        /// </summary>
        /// <param name="path">更新するパス。nullならば全て更新</param>
        public void RefleshIcon(string path)
        {
            this.FolderCollection?.RefleshIcon(path);
        }


        // サムネイル要求
        public void RequestThumbnail(int start, int count, int margin, int direction)
        {
            if (this.PanelListItemStyle.HasThumbnail())
            {
                ThumbnailManager.Current.RequestThumbnail(FolderCollection.Items, QueueElementPriority.FolderThumbnail, start, count, margin, direction);
            }
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
            _bookHub.RequestLoad(path, null, option, false);
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

        // 次のフォルダーに移動
        public void NextFolder(BookLoadOption option = BookLoadOption.None)
        {
            if (_bookHub.IsBusy()) return; // 相対移動の場合はキャンセルしない
            var result = MoveFolder(+1, option);
            if (result != true)
            {
                InfoMessage.Current.SetMessage(InfoMessageType.Notify, "次のブックはありません");
            }
        }

        // 前のフォルダーに移動
        public void PrevFolder(BookLoadOption option = BookLoadOption.None)
        {
            if (_bookHub.IsBusy()) return; // 相対移動の場合はキャンセルしない
            var result = MoveFolder(-1, option);
            if (result != true)
            {
                InfoMessage.Current.SetMessage(InfoMessageType.Notify, "前のブックはありません");
            }
        }


        /// <summary>
        /// コマンドの「前のフォルダーに移動」「次のフォルダーへ移動」に対応
        /// </summary>
        public bool MoveFolder(int direction, BookLoadOption options)
        {
            var item = this.GetFolderItem(direction);
            if (item != null)
            {
                SetPlace(Place, item.Path, FolderSetPlaceOption.IsUpdateHistory);
                _bookHub.RequestLoad(item.TargetPath, null, options, false);
                return true;
            }

            return false;
        }

        /// <summary>
        /// 検索ボックスにフォーカス要求
        /// </summary>
        public void RaiseSearchBoxFocus()
        {
            SearchBoxFocus?.Invoke(this, null);
        }

        /// <summary>
        /// 検索キーワードの正規化
        /// </summary>
        /// <returns></returns>
        private string GetFixedSearchKeyword() => _searchKeyword?.Trim();


        /// <summary>
        /// 検索
        /// </summary>
        /// <param name="isForce">強制更新</param>
        /// <returns></returns>
        public async Task UpdateFolderCollectionAsync(bool isForce)
        {
            var keyword = GetFixedSearchKeyword();

            if (string.IsNullOrEmpty(keyword))
            {
                await UpdateEntryFolderCollectionAsync(isForce);
            }
            else
            {
                await UpdateSearchFolderCollectionAsync(keyword, isForce);
            }
        }

        public async Task UpdateEntryFolderCollectionAsync(bool isForce)
        {
            await Task.Yield();

            // 同じリストは作らない
            if (!isForce && this.FolderCollection != null && this.FolderCollection.Place == this.Place && this.FolderCollection.Mode == FolderCollectionMode.Entry) return;

            this.FolderCollection = CreateFolderCollection(this.Place, null);
        }

        /// <summary>
        /// 検索結果リストとしてFolderListを更新
        /// </summary>
        /// <param name="keyword"></param>
        /// <param name="isForce"></param>
        /// <returns></returns>
        public async Task UpdateSearchFolderCollectionAsync(string keyword, bool isForce)
        {
            // 同じリストは作らない
            if (!isForce && this.FolderCollection != null && this.FolderCollection.Mode == FolderCollectionMode.Search && this.FolderCollection.SearchKeyword == keyword) return;

            _searchEngine = _searchEngine ?? new SearchEngine(this.Place);

            var option = new NeeLaboratory.IO.Search.SearchOption() { AllowFolder = true, IsOptionEnabled = true };
            var result = await _searchEngine.SearchAsync(keyword, option);

            this.FolderCollection = CreateFolderCollection(this.Place, result);
        }


        /// <summary>
        /// 検索履歴更新
        /// </summary>
        public void UpdateSearchHistory()
        {
            var keyword = GetFixedSearchKeyword();
            if (string.IsNullOrEmpty(keyword)) return;
            this.SearchHistory.Remove(keyword);
            this.SearchHistory.Insert(0, keyword);
            while (this.SearchHistory.Count > 5) this.SearchHistory.RemoveAt(5);
        }



        #endregion

        #region Commands


        public void SetHome_Executed()
        {
            if (_bookHub == null) return;
            this.Home = Place;
        }

        //
        public void MoveToHome_Executed()
        {
            if (_bookHub == null) return;

            var place = GetFixedHome();
            SetPlace(place, null, FolderSetPlaceOption.IsFocus | FolderSetPlaceOption.IsUpdateHistory | FolderSetPlaceOption.IsTopSelect | FolderSetPlaceOption.ClearSearchKeyword);
        }


        //
        public void MoveTo_Executed(string path)
        {
            this.SetPlace(path, null, FolderSetPlaceOption.IsFocus | FolderSetPlaceOption.IsUpdateHistory);
        }

        //
        public bool MoveToPrevious_CanExecutre()
        {
            return this.History.CanPrevious();
        }

        //
        public void MoveToPrevious_Executed()
        {
            if (!this.History.CanPrevious()) return;

            var place = this.History.GetPrevious();
            SetPlace(place, null, FolderSetPlaceOption.IsFocus);
            this.History.Move(-1);
        }

        //
        public bool MoveToNext_CanExecute()
        {
            return this.History.CanNext();
        }

        //
        public void MoveToNext_Executed()
        {
            if (!this.History.CanNext()) return;

            var place = this.History.GetNext();
            SetPlace(place, null, FolderSetPlaceOption.IsFocus);
            this.History.Move(+1);
        }

        //
        public void MoveToHistory_Executed(KeyValuePair<int, string> item)
        {
            var place = this.History.GetHistory(item.Key);
            SetPlace(place, null, FolderSetPlaceOption.IsFocus);
            this.History.SetCurrent(item.Key + 1);
        }

        //
        public bool MoveToParent_CanExecute()
        {
            return (Place != null);
        }

        //
        public void MoveToParent_Execute()
        {
            if (Place == null) return;
            var parent = System.IO.Path.GetDirectoryName(Place);
            SetPlace(parent, Place, FolderSetPlaceOption.IsFocus | FolderSetPlaceOption.IsUpdateHistory);
        }

        //
        public void Sync_Executed()
        {
            string place = _bookHub?.Book?.Place;

            if (place != null)
            {
                _isDarty = true; // 強制更新
                SetPlace(System.IO.Path.GetDirectoryName(place), place, FolderSetPlaceOption.IsFocus | FolderSetPlaceOption.IsUpdateHistory);

                RaiseSelectedItemChanged(true);
            }
            else if (Place != null)
            {
                _isDarty = true; // 強制更新
                SetPlace(Place, null, FolderSetPlaceOption.IsFocus);

                RaiseSelectedItemChanged(true);
            }
        }

        //
        public void ToggleFolderRecursive_Executed()
        {
            this.FolderCollection.FolderParameter.IsFolderRecursive = !this.FolderCollection.FolderParameter.IsFolderRecursive;
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
            [PropertyMember("フォルダーリスト追加ファイルは挿入", Tips = "フォルダーリストで追加されたファイルを現在のソート順で挿入します。\nFalseのときはリストの終端に追加します。")]
            public bool IsInsertItem { get; set; }

            [DataMember]
            public bool IsFolderSearchBoxVisible { get; set; }

            [OnDeserializing]
            private void Deserializing(StreamingContext c)
            {
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

            // Preference反映
            ///RaisePropertyChanged(nameof(FolderIconLayout));
        }

        #endregion
    }




    /// <summary>
    /// 旧フォルダーリスト設定。
    /// 互換性のために残してあります
    /// </summary>
    [DataContract]
    public class FolderListSetting
    {
        [DataMember]
        public bool IsVisibleHistoryMark { get; set; }

        [DataMember]
        public bool IsVisibleBookmarkMark { get; set; }
    }
}
