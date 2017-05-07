// Copyright (c) 2016 Mitsuhiro Ito (nee)
//
// This software is released under the MIT License.
// http://opensource.org/licenses/mit-license.php

using NeeView.Windows.Input;
using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;

namespace NeeView
{
    /// <summary>
    /// Messanger: MoveFolderメッセージのパラメータ
    /// </summary>
    public class MoveFolderParams
    {
        public int Distance { get; set; }
        public BookLoadOption BookLoadOption { get; set; }
    }

    /// <summary>
    /// Messenger: FolderOrderメッセージのパラメータ
    /// </summary>
    public class FolderOrderParams
    {
        public FolderOrder FolderOrder { get; set; }
    }

    [Flags]
    public enum FolderSetPlaceOption
    {
        None,
        IsFocus = (1 << 0),
        IsUpdateHistory = (1 << 1),
        IsTopSelect = (1 << 3),
    }

    //
    public class FolderListChangedEventArgs : EventArgs
    {
        public bool IsFocused { get; set; }

        public FolderListChangedEventArgs(bool isFocused)
        {
            this.IsFocused = isFocused;
        }
    }

    /// <summary>
    /// FolderListControl ViewModel
    /// </summary>
    public class FolderListViewModel : INotifyPropertyChanged
    {
        #region NotifyPropertyChanged
        public event System.ComponentModel.PropertyChangedEventHandler PropertyChanged;

        protected void RaisePropertyChanged([System.Runtime.CompilerServices.CallerMemberName] string name = "")
        {
            PropertyChanged?.Invoke(this, new System.ComponentModel.PropertyChangedEventArgs(name));
        }
        #endregion

        /// <summary>
        /// コンボボックス用リスト
        /// </summary>
        public Dictionary<FolderOrder, string> FolderOrderList => FolderOrderExtension.FolderOrderList;

        /// <summary>
        /// BookHub property.
        /// </summary>
        private BookHub _bookHub;
        public BookHub BookHub
        {
            get { return _bookHub; }
            set
            {
                _bookHub = value;
                _bookHub.FolderListSync += (s, e) => SyncWeak(e);
                _bookHub.HistoryChanged += (s, e) => RefleshIcon(e.Key);
                _bookHub.BookmarkChanged += (s, e) => RefleshIcon(e.Key);
                RaisePropertyChanged();
            }
        }


        #region MoreMenu

        /// <summary>
        /// MoreMenu property.
        /// </summary>
        public ContextMenu MoreMenu
        {
            get { return _MoreMenu; }
            set { if (_MoreMenu != value) { _MoreMenu = value; RaisePropertyChanged(); } }
        }

        //
        private ContextMenu _MoreMenu;


        //
        private void InitializeMoreMenu(FolderPanelModel source)
        {
            var menu = new ContextMenu();
            menu.Items.Add(CreateCommandMenuItem("ページリスト", CommandType.ToggleVisiblePageList, source));
            menu.Items.Add(new Separator());
            menu.Items.Add(CreateListItemStyleMenuItem("一覧表示", PanelListItemStyle.Normal));
            menu.Items.Add(CreateListItemStyleMenuItem("コンテンツ表示", PanelListItemStyle.Content));
            menu.Items.Add(CreateListItemStyleMenuItem("バナー表示", PanelListItemStyle.Banner));
            menu.Items.Add(new Separator());
            menu.Items.Add(CreateRecursiveFlagMenuItem("この場所ではサブフォルダーを読み込む"));

            this.MoreMenu = menu;
        }

        //
        private MenuItem CreateRecursiveFlagMenuItem(string header)
        {
            var item = new MenuItem();
            item.Header = header;
            item.Command = ToggleFolderRecursive;
            item.SetBinding(MenuItem.IsCheckedProperty, new Binding("FolderCollection.FolderParameter.IsFolderRecursive"));
            return item;
        }

        //
        private MenuItem CreateCommandMenuItem(string header, CommandType command, FolderPanelModel source)
        {
            var item = new MenuItem();
            item.Header = header;
            item.Command = ModelContext.BookCommands[command];
            item.CommandParameter = MenuCommandTag.Tag; // コマンドがメニューからであることをパラメータで伝えてみる
            if (ModelContext.CommandTable[command].CreateIsCheckedBinding != null)
            {
                var binding = ModelContext.CommandTable[command].CreateIsCheckedBinding();
                binding.Source = source;
                item.SetBinding(MenuItem.IsCheckedProperty, binding);
            }

            return item;
        }

        //
        private MenuItem CreateListItemStyleMenuItem(string header, PanelListItemStyle style)
        {
            var item = new MenuItem();
            item.Header = header;
            item.Command = SetListItemStyle;
            item.CommandParameter = style;
            var binding = new Binding(nameof(_model.PanelListItemStyle))
            {
                Converter = _PanelListItemStyleToBooleanConverter,
                ConverterParameter = style,
                Source = _model,
            };
            item.SetBinding(MenuItem.IsCheckedProperty, binding);

            return item;
        }


        private PanelListItemStyleToBooleanConverter _PanelListItemStyleToBooleanConverter = new PanelListItemStyleToBooleanConverter();


        /// <summary>
        /// SetListItemStyle command.
        /// </summary>
        public RelayCommand<PanelListItemStyle> SetListItemStyle
        {
            get { return _SetListItemStyle = _SetListItemStyle ?? new RelayCommand<PanelListItemStyle>(SetListItemStyle_Executed); }
        }

        //
        private RelayCommand<PanelListItemStyle> _SetListItemStyle;

        //
        private void SetListItemStyle_Executed(PanelListItemStyle style)
        {
            _model.PanelListItemStyle = style;
        }

        #endregion


        /// <summary>
        /// 現在のフォルダー
        /// </summary>
        private string _place => FolderCollection?.Place;


        /// <summary>
        /// そのフォルダーで最後に選択されていた項目の記憶
        /// </summary>
        private Dictionary<string, string> _lastPlaceDictionary = new Dictionary<string, string>();

        /// <summary>
        /// フォルダー履歴
        /// </summary>
        private History<string> _history = new History<string>();

        /// <summary>
        /// 更新フラグ
        /// </summary>
        private bool _isDarty;

        /// <summary>
        /// Model property.
        /// </summary>
        public FolderList Model
        {
            get { return _model; }
            set { if (_model != value) { _model = value; RaisePropertyChanged(); } }
        }

        //
        private FolderList _model;


        /// <summary>
        /// Constructor
        /// </summary>
        public FolderListViewModel(FolderList model)
        {
            _model = model;

            this.BookHub = _model.BookHub;

            _history.Changed += (s, e) => UpdateCommandCanExecute();

            // regist messenger reciever
            Messenger.AddReciever("SetFolderOrder", CallSetFolderOrder);
            Messenger.AddReciever("GetFolderOrder", CallGetFolderOrder);
            Messenger.AddReciever("ToggleFolderOrder", CallToggleFolderOrder);
            Messenger.AddReciever("MoveFolder", CallMoveFolder);

            _model.PropertyChanged += Model_PropertyChanged;

            _model.PlaceChanged += Model_PlaceChanged;

            InitializeMoreMenu(_model.FolderPanel);
        }

        //
        private void Model_PlaceChanged(object sender, FolderPlaceChangedEventArgs e)
        {
            var oprions = (e.IsFocus ? FolderSetPlaceOption.IsFocus : FolderSetPlaceOption.None) | FolderSetPlaceOption.IsUpdateHistory;
            SetPlace(e.Place, e.Select, oprions);
        }

        //
        private void Model_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            switch (e.PropertyName)
            {
                case nameof(_model.IsVisibleHistoryMark):
                    FolderItem.IsVisibleHistoryMark = _model.IsVisibleHistoryMark;
                    break;
                case nameof(_model.IsVisibleBookmarkMark):
                    FolderItem.IsVisibleBookmarkMark = _model.IsVisibleBookmarkMark;
                    break;
            }
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



        //
        public event EventHandler<FolderListChangedEventArgs> FolderListChanged;

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

            // 更新が必要であれば、新しいFolderListViewを作成する
            if (CheckFolderListUpdateneNcessary(place))
            {
                _isDarty = false;

                // FolderCollection 更新
                var collection = CreateFolderCollection(place);
                collection.ParameterChanged += (s, e) => App.Current?.Dispatcher.BeginInvoke((Action)(delegate () { Reflesh(true, false); }));
                collection.Deleting += FolderCollection_Deleting;
                this.FolderCollection = collection;
                this.SelectedIndex = FixedIndexOfPath(select);

                FolderListChanged?.Invoke(this, new FolderListChangedEventArgs(options.HasFlag(FolderSetPlaceOption.IsFocus)));

                // 最終フォルダー更新
                ModelContext.BookHistory.LastFolder = _place;

                // 履歴追加
                if (options.HasFlag(FolderSetPlaceOption.IsUpdateHistory))
                {
                    if (place != _history.GetCurrent())
                    {
                        _history.Add(place);
                    }
                }
            }
            else
            {
                // 選択項目のみ変更
                this.SelectedIndex = FixedIndexOfPath(select);
            }

            // コマンド有効状態更新
            MoveToUp.RaiseCanExecuteChanged();
        }

        /// <summary>
        /// リストの更新必要性チェック
        /// </summary>
        /// <param name="place"></param>
        /// <returns></returns>
        private bool CheckFolderListUpdateneNcessary(string place)
        {
            return (_isDarty || this.FolderCollection == null || place != this.FolderCollection.Place);
        }

        /// <summary>
        /// FolderCollection 作成
        /// </summary>
        /// <param name="place"></param>
        /// <returns></returns>
        private FolderCollection CreateFolderCollection(string place)
        {
            try
            {
                var collection = new FolderCollection(place);
                collection.Initialize();
                return collection;
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex.Message);

                // 救済措置。取得に失敗した時はカレントディレクトリに移動
                var collection = new FolderCollection(Environment.CurrentDirectory);
                collection.Initialize();
                return collection;
            }
        }


        //
        public void Decided(string path)
        {
            BookLoadOption option = BookLoadOption.SkipSamePlace | (this.FolderCollection.FolderParameter.IsFolderRecursive ? BookLoadOption.DefaultRecursive : BookLoadOption.None);
            Decided(path, option);
        }

        //
        public void Decided(string path, BookLoadOption option)
        {
            this.BookHub.RequestLoad(path, null, option, false);
        }

        //
        public void Moved(string path)
        {
            this.SetPlace(path, null, FolderSetPlaceOption.IsFocus | FolderSetPlaceOption.IsUpdateHistory);
        }

        // TODO: コマンドはコマンドとして実行させるべきでは？
        public void MovedParent()
        {
            MoveToParent_Execute();
        }

        //
        public void MovedHome()
        {
            MoveToHome.Execute(null);
        }

        //
        public void MovedPrevious()
        {
            MoveToPrevious.Execute(null);
        }

        //
        public void MovedNext()
        {
            MoveToNext.Execute(null);
        }

        //
        public event EventHandler SelectedItemChanged;

        /// <summary>
        /// 選択項目にフォーカス取得
        /// </summary>
        /// <param name="isFocus"></param>
        public void FocusSelectedItem(bool isFocus)
        {
            SelectedItemChanged?.Invoke(this, null);
        }

        /// <summary>
        /// コマンド実行可能状態を更新
        /// </summary>
        private void UpdateCommandCanExecute()
        {
            this.MoveToPrevious.RaiseCanExecuteChanged();
            this.MoveToNext.RaiseCanExecuteChanged();
        }


        /// <summary>
        /// SetHome command.
        /// </summary>
        private RelayCommand _SetHome;
        public RelayCommand SetHome
        {
            get { return _SetHome = _SetHome ?? new RelayCommand(SetHome_Executed); }
        }

        private void SetHome_Executed()
        {
            if (_bookHub == null) return;
            _bookHub.Home = _place;
        }


        /// <summary>
        /// MoveToHome command.
        /// </summary>
        private RelayCommand _MoveToHome;
        public RelayCommand MoveToHome
        {
            get { return _MoveToHome = _MoveToHome ?? new RelayCommand(MoveToHome_Executed); }
        }

        private void MoveToHome_Executed()
        {
            if (_bookHub == null) return;

            var place = _bookHub.GetFixedHome();
            SetPlace(place, null, FolderSetPlaceOption.IsFocus | FolderSetPlaceOption.IsUpdateHistory | FolderSetPlaceOption.IsTopSelect);
        }


        /// <summary>
        /// MoveToPrevious command.
        /// </summary>
        private RelayCommand _MoveToPrevious;
        public RelayCommand MoveToPrevious
        {
            get { return _MoveToPrevious = _MoveToPrevious ?? new RelayCommand(MoveToPrevious_Executed, MoveToPrevious_CanExecutre); }
        }

        private bool MoveToPrevious_CanExecutre()
        {
            return _history.CanPrevious();
        }

        private void MoveToPrevious_Executed()
        {
            if (!_history.CanPrevious()) return;

            var place = _history.GetPrevious();
            SetPlace(place, null, FolderSetPlaceOption.IsFocus);
            _history.Move(-1);
        }


        /// <summary>
        /// MoveToNext command.
        /// </summary>
        private RelayCommand _MoveToNext;
        public RelayCommand MoveToNext
        {
            get { return _MoveToNext = _MoveToNext ?? new RelayCommand(MoveToNext_Executed, MoveToNext_CanExecute); }
        }

        private bool MoveToNext_CanExecute()
        {
            return _history.CanNext();
        }

        private void MoveToNext_Executed()
        {
            if (!_history.CanNext()) return;

            var place = _history.GetNext();
            SetPlace(place, null, FolderSetPlaceOption.IsFocus);
            _history.Move(+1);
        }


        /// <summary>
        /// 履歴取得
        /// </summary>
        /// <param name="direction"></param>
        /// <param name="size"></param>
        /// <returns></returns>
        internal List<KeyValuePair<int, string>> GetHistory(int direction, int size)
        {
            return _history.GetHistory(direction, size);
        }

        /// <summary>
        /// MoveToHistory command.
        /// </summary>
        private RelayCommand<KeyValuePair<int, string>> _MoveToHistory;
        public RelayCommand<KeyValuePair<int, string>> MoveToHistory
        {
            get { return _MoveToHistory = _MoveToHistory ?? new RelayCommand<KeyValuePair<int, string>>(MoveToHistory_Executed); }
        }

        private void MoveToHistory_Executed(KeyValuePair<int, string> item)
        {
            var place = _history.GetHistory(item.Key);
            SetPlace(place, null, FolderSetPlaceOption.IsFocus);
            _history.SetCurrent(item.Key + 1);
        }


        /// <summary>
        /// MoveToUp command.
        /// </summary>
        private RelayCommand _MoveToUp;
        public RelayCommand MoveToUp
        {
            get { return _MoveToUp = _MoveToUp ?? new RelayCommand(MoveToParent_Execute, MoveToParent_CanExecute); }
        }

        private bool MoveToParent_CanExecute()
        {
            return (_place != null);
        }

        private void MoveToParent_Execute()
        {
            if (_place == null) return;
            var parent = System.IO.Path.GetDirectoryName(_place);
            SetPlace(parent, _place, FolderSetPlaceOption.IsFocus | FolderSetPlaceOption.IsUpdateHistory);
        }


        /// <summary>
        /// Sync command.
        /// 現在開いているフォルダーで更新
        /// </summary>
        private RelayCommand _Sync;
        public RelayCommand Sync
        {
            get { return _Sync = _Sync ?? new RelayCommand(Sync_Executed); }
        }

        private void Sync_Executed()
        {
            string place = BookHub?.CurrentBook?.Place;

            if (place != null)
            {
                _isDarty = true; // 強制更新
                SetPlace(System.IO.Path.GetDirectoryName(place), place, FolderSetPlaceOption.IsFocus | FolderSetPlaceOption.IsUpdateHistory);

                FocusSelectedItem(true);
            }
            else if (_place != null)
            {
                _isDarty = true; // 強制更新
                SetPlace(_place, null, FolderSetPlaceOption.IsFocus);

                FocusSelectedItem(true);
            }
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
                ////if (this.FolderListViewModel == null || !this.FolderListViewModel.Contains(e.Path)) return;
                if (this.FolderCollection == null || this.FolderCollection.Contains(e.Path)) return;
            }

            var options = (e.IsFocus ? FolderSetPlaceOption.IsFocus : FolderSetPlaceOption.None) | FolderSetPlaceOption.IsUpdateHistory;
            SetPlace(System.IO.Path.GetDirectoryName(e.Path), e.Path, options);
        }


        /// <summary>
        /// ToggleFolderRecursive command.
        /// </summary>
        public RelayCommand ToggleFolderRecursive
        {
            get { return _ToggleFolderRecursive = _ToggleFolderRecursive ?? new RelayCommand(ToggleFolderRecursive_Executed); }
        }

        //
        private RelayCommand _ToggleFolderRecursive;

        //
        private void ToggleFolderRecursive_Executed()
        {
            this.FolderCollection.FolderParameter.IsFolderRecursive = !this.FolderCollection.FolderParameter.IsFolderRecursive;
        }





        /// <summary>
        /// フォルダーリスト更新
        /// </summary>
        /// <param name="force">必要が無い場合も更新する</param>
        /// <param name="isFocus">フォーカスを取得する</param>
        public void Reflesh(bool force, bool isFocus)
        {
            if (this.FolderCollection == null) return;

            _isDarty = force || this.FolderCollection.IsDarty();

            var options = (isFocus ? FolderSetPlaceOption.IsFocus : FolderSetPlaceOption.None) | FolderSetPlaceOption.IsUpdateHistory;
            SetPlace(_place, null, options);
        }

        /// <summary>
        /// フォルダーアイコンの表示更新
        /// </summary>
        /// <param name="path">更新するパス。nullならば全て更新</param>
        public void RefleshIcon(string path)
        {
            this.FolderCollection?.RefleshIcon(path);
        }


        /// <summary>
        /// Messenger reciever: フォルダーの並びを設定
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void CallSetFolderOrder(object sender, MessageEventArgs e)
        {
            if (FolderCollection == null) return;
            var param = (FolderOrderParams)e.Parameter;
            ////this.FolderListViewModel?.SetFolderOrder(param.FolderOrder);
            this.FolderCollection.FolderParameter.FolderOrder = param.FolderOrder;
        }

        /// <summary>
        /// Messenger reciever: フォルダーの並びを取得
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void CallGetFolderOrder(object sender, MessageEventArgs e)
        {
            if (this.FolderCollection == null) return;

            var param = (FolderOrderParams)e.Parameter;
            ////param.FolderOrder = this.FolderListViewModel.GetFolderOrder();
            param.FolderOrder = this.FolderCollection.FolderParameter.FolderOrder;
        }

        /// <summary>
        /// Messenger reciever: フォルダーの並びを順番に切り替える
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void CallToggleFolderOrder(object sender, MessageEventArgs e)
        {
            ////this.FolderListViewModel?.ToggleFolderOrder();
            if (this.FolderCollection?.Items == null) return;
            this.FolderCollection.FolderParameter.FolderOrder = this.FolderCollection.FolderParameter.FolderOrder.GetToggle();
        }


        /// <summary>
        /// Messenger reciever: フォルダー前後移動要求
        /// コマンドの「前のフォルダーに移動」「次のフォルダーへ移動」に対応
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void CallMoveFolder(object sender, MessageEventArgs e)
        {
            var param = (MoveFolderParams)e.Parameter;

            ////var item = this.FolderListViewModel?.GetFolderItem(param.Distance);
            var item = this.GetFolderItem(param.Distance);
            if (item != null)
            {
                SetPlace(_place, item.Path, FolderSetPlaceOption.IsUpdateHistory);
                BookHub.RequestLoad(item.TargetPath, null, param.BookLoadOption, false);
                e.Result = true;
            }
        }

        //-------------------------------------

        /// <summary>
        /// フォルダーコレクション
        /// </summary>
        public FolderCollection FolderCollection
        {
            get { return _folderCollection; }
            set
            {
                if (_folderCollection != value)
                {
                    _folderCollection?.Dispose();
                    _folderCollection = value;
                    RaisePropertyChanged();
                    ////RaisePropertyChanged(nameof(Place));
                }
            }
        }

        //
        private FolderCollection _folderCollection;


        /// <summary>
        /// IsRenaming property.
        /// </summary>
        private bool _isRenaming;
        public bool IsRenaming
        {
            get { return _isRenaming; }
            set { if (_isRenaming != value) { _isRenaming = value; RaisePropertyChanged(); } }
        }

        /// <summary>
        /// SelectIndex property.
        /// </summary>
        private int _selectedIndex;
        public int SelectedIndex
        {
            get { return _selectedIndex; }
            set
            {
                _selectedIndex = NVUtility.Clamp(value, 0, this.FolderCollection.Items.Count - 1);
                RaisePropertyChanged();
            }
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


        /// <summary>
        /// 選択項目を基準とした項目取得
        /// </summary>
        /// <param name="offset">選択項目から前後した項目を指定</param>
        /// <returns></returns>
        internal FolderItem GetFolderItem(int offset)
        {
            if (this.FolderCollection?.Items == null) return null;

            int index = this.SelectedIndex;
            if (index < 0) return null;

            int next = (this.FolderCollection.FolderParameter.FolderOrder == FolderOrder.Random)
                ? (index + this.FolderCollection.Items.Count + offset) % this.FolderCollection.Items.Count
                : index + offset;

            if (next < 0 || next >= this.FolderCollection.Items.Count) return null;

            return this.FolderCollection[next];
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

            var index = this.FolderCollection.IndexOfPath(e.FullPath);
            if (SelectedIndex != index) return;

            if (SelectedIndex < this.FolderCollection.Items.Count - 1)
            {
                SelectedIndex++;
            }
            else if (SelectedIndex > 0)
            {
                SelectedIndex--;
            }
        }

        /// <summary>
        /// クリップボードにコピー
        /// </summary>
        /// <param name="info"></param>
        public void Copy(FolderItem info)
        {
            if (info.IsEmpty) return;

            var files = new List<string>();
            files.Add(info.Path);
            var data = new DataObject();
            data.SetData(DataFormats.FileDrop, files.ToArray());
            data.SetData(DataFormats.UnicodeText, string.Join("\r\n", files));
            Clipboard.SetDataObject(data);
        }


        /// <summary>
        /// ファイルを削除
        /// </summary>
        /// <param name="info"></param>
        public async Task RemoveAsync(FolderItem info)
        {
            if (info.IsEmpty) return;

            if (Preference.Current.file_remove_confirm)
            {
                bool isDirectory = System.IO.Directory.Exists(info.Path);
                string itemType = isDirectory ? "フォルダー" : "ファイル";

                var dockPanel = new DockPanel();

                var message = new TextBlock();
                message.Text = $"この{itemType}をごみ箱に移動しますか？";
                message.Margin = new Thickness(0, 0, 0, 10);
                DockPanel.SetDock(message, Dock.Top);
                dockPanel.Children.Add(message);

                var thumbnail = new Image();
                thumbnail.SnapsToDevicePixels = true;
                thumbnail.Source = info.Icon;
                thumbnail.Width = 32;
                thumbnail.Height = 32;
                thumbnail.Margin = new Thickness(0, 0, 4, 0);
                dockPanel.Children.Add(thumbnail);

                var textblock = new TextBlock();
                textblock.Text = info.Path;
                textblock.VerticalAlignment = VerticalAlignment.Bottom;
                textblock.Margin = new Thickness(0, 0, 0, 2);
                dockPanel.Children.Add(textblock);

                //
                var dialog = new MessageDialog(dockPanel, $"{itemType}を削除します");
                dialog.Commands.Add(UICommands.Remove);
                dialog.Commands.Add(UICommands.Cancel);
                var answer = dialog.ShowDialog();

                if (answer != UICommands.Remove) return;
            }

            await this.BookHub.RemoveFileAsync(info.Path);
        }


        /// <summary>
        /// ファイル名前変更
        /// </summary>
        /// <param name="file"></param>
        /// <param name="newName"></param>
        /// <returns></returns>
        public async Task<bool> RenameAsync(FolderItem file, string newName)
        {
            string src = file.Path;
            string dst = System.IO.Path.Combine(System.IO.Path.GetDirectoryName(src), newName);

            if (src == dst) return true;

            //ファイル名に使用できない文字
            char[] invalidChars = System.IO.Path.GetInvalidFileNameChars();
            int invalidCharsIndex = newName.IndexOfAny(invalidChars);
            if (invalidCharsIndex >= 0)
            {
                //
                var dialog = new MessageDialog($"ファイル名に使用できない文字が含まれています。( {newName[invalidCharsIndex]} )", "名前を変更できません");
                dialog.ShowDialog();

                return false;
            }

            // 拡張子変更確認
            if (!file.IsDirectory)
            {
                var srcExt = System.IO.Path.GetExtension(src);
                var dstExt = System.IO.Path.GetExtension(dst);
                if (string.Compare(srcExt, dstExt, true) != 0)
                {
                    var dialog = new MessageDialog($"拡張子を変更すると、使えなくなる可能性があります。\nよろしいですか？", "拡張子を変更します");
                    dialog.Commands.Add(UICommands.Yes);
                    dialog.Commands.Add(UICommands.No);
                    var answer = dialog.ShowDialog();
                    if (answer != UICommands.Yes)
                    {
                        return false;
                    }
                }
            }

            // 大文字小文字の変換は正常
            if (string.Compare(src, dst, true) == 0)
            {
                // nop.
            }

            // 重複ファイル名回避
            else if (System.IO.File.Exists(dst) || System.IO.Directory.Exists(dst))
            {
                string dstBase = dst;
                string dir = System.IO.Path.GetDirectoryName(dst);
                string name = System.IO.Path.GetFileNameWithoutExtension(dst);
                string ext = System.IO.Path.GetExtension(dst);
                int count = 1;

                do
                {
                    dst = $"{dir}\\{name} ({++count}){ext}";
                }
                while (System.IO.File.Exists(dst) || System.IO.Directory.Exists(dst));

                // 確認
                var dialog = new MessageDialog($"{System.IO.Path.GetFileName(dstBase)} は既に存在しています。\n{System.IO.Path.GetFileName(dst)} に名前を変更しますか？", "同じ名前のファイルが存在しています");
                dialog.Commands.Add(new UICommand("名前を変える"));
                dialog.Commands.Add(UICommands.Cancel);
                var answer = dialog.ShowDialog();
                if (answer != dialog.Commands[0])
                {
                    return false;
                }
            }

            // 名前変更実行
            var result = await this.BookHub.RenameFileAsync(src, dst);
            return result;
        }


        // サムネイル要求
        public void RequestThumbnail(int start, int count, int margin, int direction)
        {
            if (_model.PanelListItemStyle.HasThumbnail())
            {
                ThumbnailManager.Current.RequestThumbnail(FolderCollection.Items, QueueElementPriority.FolderThumbnail, start, count, margin, direction);
            }
        }
    }

}
