using Microsoft.Win32;
using NeeLaboratory.ComponentModel;
using NeeLaboratory.Windows.Input;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Input;

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

        /// <summary>
        /// フォーカスをあわせる
        /// </summary>
        Focus = (1 << 0),

        /// <summary>
        /// フォルダー履歴更新
        /// </summary>
        UpdateHistory = (1 << 1),

        /// <summary>
        /// 先頭を選択した態にする
        /// </summary>
        TopSelect = (1 << 3),

        /// <summary>
        /// 検索キーワードをクリア
        /// TODO: 未使用に付き削除
        /// </summary>
        ResetKeyword = (1 << 4),

        /// <summary>
        /// 同じ場所でも作り直す
        /// </summary>
        Refresh = (1 << 5),

        /// <summary>
        /// ブックマークではなく、ファイルシステムの場所を優先
        /// </summary>
        FileSystem = (1 << 6),
    }

    /// <summary>
    /// FolderList : ViewModel
    /// </summary>
    public class FolderListViewModel : BindableBase
    {
        #region Fields

        private FolderListBox _folderListBox;
        private PanelListItemStyleToBooleanConverter _panelListItemStyleToBooleanConverter = new PanelListItemStyleToBooleanConverter();

        #endregion

        #region Constructor

        /// <summary>
        /// Constructor
        /// </summary>
        public FolderListViewModel(FolderList model)
        {
            _model = model;

            _model.History.Changed +=
                (s, e) => UpdateCommandCanExecute();

            ////_model.PropertyChanged +=
            ////    Model_PropertyChanged;

            _model.PlaceChanged +=
                (s, e) => MoveToUp.RaiseCanExecuteChanged();

            _model.CollectionChanged +=
                Model_CollectionChanged;

            _model.FolderListConfig.AddPropertyChanged(nameof(FolderListConfig.PanelListItemStyle), (s, e) =>
            {
                UpdateFolderListBox();
            });

            InitializeMoreMenu();

            UpdateFolderListBox();
        }

        #endregion

        #region Properties

        /// <summary>
        /// ListBoxコントロール。
        /// コレクションやレイアウトの変更の都度再生成する
        /// </summary>
        public FolderListBox FolderListBox
        {
            get { return _folderListBox; }
            private set
            {
                if (SetProperty(ref _folderListBox, value))
                {
                    RaisePropertyChanged(nameof(FolderCollection));
                }
            }
        }

        public FolderCollection FolderCollection => _model.FolderCollection;


        private FolderList _model;
        public FolderList Model
        {
            get { return _model; }
            set { if (_model != value) { _model = value; RaisePropertyChanged(); } }
        }

        /// <summary>
        /// コンボボックス用リスト
        /// </summary>
        private Dictionary<FolderOrder, string> _folderOrderList = AliasNameExtensions.GetAliasNameDictionary<FolderOrder>();
        public Dictionary<FolderOrder, string> FolderOrderList
        {
            get { return _folderOrderList; }
            set { SetProperty(ref _folderOrderList, value); }
        }

        public FolderOrder FolderOrder
        {
            get { return FolderCollection != null ? FolderCollection.FolderParameter.FolderOrder : default; }
            set { if (FolderCollection != null) { FolderCollection.FolderParameter.FolderOrder = value; } }
        }


        /// <summary>
        /// MoreMenu property.
        /// </summary>
        private ContextMenu _MoreMenu;
        public ContextMenu MoreMenu
        {
            get { return _MoreMenu; }
            set { if (_MoreMenu != value) { _MoreMenu = value; RaisePropertyChanged(); } }
        }

        #endregion Properties

        #region Commands

        /// <summary>
        /// コマンド実行可能状態を更新
        /// </summary>
        private void UpdateCommandCanExecute()
        {
            this.MoveToPrevious.RaiseCanExecuteChanged();
            this.MoveToNext.RaiseCanExecuteChanged();
        }

        /// <summary>
        /// 履歴取得
        /// </summary>
        internal List<KeyValuePair<int, QueryPath>> GetHistory(int direction, int size)
        {
            return _model.History.GetHistory(direction, size);
        }

        /// <summary>
        /// SetHome command.
        /// </summary>
        private RelayCommand _SetHome;
        public RelayCommand SetHome
        {
            get { return _SetHome = _SetHome ?? new RelayCommand(_model.SetHome, _model.CanSetHome); }
        }

        /// <summary>
        /// MoveToHome command.
        /// </summary>
        private RelayCommand _MoveToHome;
        public RelayCommand MoveToHome
        {
            get { return _MoveToHome = _MoveToHome ?? new RelayCommand(_model.MoveToHome); }
        }

        /// <summary>
        /// MoveTo command.
        /// </summary>
        private RelayCommand<QueryPath> _MoveTo;
        public RelayCommand<QueryPath> MoveTo
        {
            get { return _MoveTo = _MoveTo ?? new RelayCommand<QueryPath>(_model.MoveTo); }
        }

        /// <summary>
        /// MoveToPrevious command.
        /// </summary>
        private RelayCommand _MoveToPrevious;
        public RelayCommand MoveToPrevious
        {
            get { return _MoveToPrevious = _MoveToPrevious ?? new RelayCommand(_model.MoveToPrevious, _model.CanMoveToPrevious); }
        }

        /// <summary>
        /// MoveToNext command.
        /// </summary>
        private RelayCommand _MoveToNext;
        public RelayCommand MoveToNext
        {
            get { return _MoveToNext = _MoveToNext ?? new RelayCommand(_model.MoveToNext, _model.CanMoveToNext); }
        }

        /// <summary>
        /// MoveToHistory command.
        /// </summary>
        private RelayCommand<KeyValuePair<int, QueryPath>> _MoveToHistory;
        public RelayCommand<KeyValuePair<int, QueryPath>> MoveToHistory
        {
            get { return _MoveToHistory = _MoveToHistory ?? new RelayCommand<KeyValuePair<int, QueryPath>>(_model.MoveToHistory); }
        }

        /// <summary>
        /// MoveToUp command.
        /// </summary>
        private RelayCommand _MoveToUp;
        public RelayCommand MoveToUp
        {
            get { return _MoveToUp = _MoveToUp ?? new RelayCommand(_model.MoveToParent, _model.CanMoveToParent); }
        }

        /// <summary>
        /// Sync command.
        /// 現在開いているフォルダーで更新
        /// </summary>
        private RelayCommand _Sync;
        public RelayCommand Sync
        {
            get { return _Sync = _Sync ?? new RelayCommand(_model.Sync); }
        }

        /// <summary>
        /// ToggleFolderRecursive command.
        /// </summary>
        private RelayCommand _ToggleFolderRecursive;
        public RelayCommand ToggleFolderRecursive
        {
            get { return _ToggleFolderRecursive = _ToggleFolderRecursive ?? new RelayCommand(_model.ToggleFolderRecursive); }
        }


        /// <summary>
        /// Search command.
        /// </summary>
        private RelayCommand _Search;
        public RelayCommand Search
        {
            get { return _Search = _Search ?? new RelayCommand(Search_Executed); }
        }

        //
        private void Search_Executed()
        {
            _model.RequestSearchPlace(true);
        }

        /// <summary>
        /// ClearSearch command.
        /// </summary>
        private RelayCommand _ClearSearch;
        public RelayCommand ClearSearch
        {
            get { return _ClearSearch = _ClearSearch ?? new RelayCommand(ClearSearch_Executed); }
        }

        //
        private void ClearSearch_Executed()
        {
            _model.InputKeyword = "";
            _model.SetSearchKeywordAndSearch("");
        }

        private RelayCommand _AddQuickAccess;
        public RelayCommand AddQuickAccess
        {
            get
            {
                return _AddQuickAccess = _AddQuickAccess ?? new RelayCommand(Execute, CanExecute);

                bool CanExecute()
                {
                    return _model.Place != null;
                }

                void Execute()
                {
                    _model.AddQuickAccess();
                }
            }
        }

        private RelayCommand _ExportPlaylist;
        public RelayCommand ExportPlaylist
        {
            get
            {
                return _ExportPlaylist = _ExportPlaylist ?? new RelayCommand(Execute, CanExecute);

                bool CanExecute()
                {
                    // プレイリスト、もしくはソート可能ならば保存可能とする
                    return _model.FolderCollection is PlaylistFolderCollection || (_model.IsFolderOrderEnabled && !_model.FolderCollection.IsEmpty());
                }

                void Execute()
                {
                    var dialog = new SaveFileDialog();
                    dialog.OverwritePrompt = true;
                    dialog.DefaultExt = PlaylistArchive.Extension;
                    dialog.AddExtension = true;
                    dialog.FileName = "NeeViewPlaylist-" + DateTime.Now.ToString("yyyyMMdd") + PlaylistArchive.Extension;
                    dialog.Filter = "NeeView Playlist|*" + PlaylistArchive.Extension + "|All|*.*";
                    if (dialog.ShowDialog(MainWindow.Current) == true)
                    {
                        try
                        {
                            if (_model.FolderCollection is PlaylistFolderCollection playlistFolderCollection)
                            {
                                System.IO.File.Copy(playlistFolderCollection.Place.SimplePath, dialog.FileName, true);
                            }
                            else
                            {
                                var playlist = new Playlist(_model.FolderCollection.Items.Where(e => e.TargetPath.Scheme == QueryScheme.File).Select(e => e.TargetPath.SimplePath));
                                PlaylistFile.Save(dialog.FileName, playlist, true);
                            }
                        }
                        catch (Exception ex)
                        {
                            new MessageDialog(ex.Message, Properties.Resources.DialogExportPlaylistFailedTitle).ShowDialog();
                        }
                    }
                }
            }
        }




        private RelayCommand<FolderTreeLayout> _SetFolderTreeLayout;
        public RelayCommand<FolderTreeLayout> SetFolderTreeLayout
        {
            get
            {
                return _SetFolderTreeLayout = _SetFolderTreeLayout ?? new RelayCommand<FolderTreeLayout>(Execute);

                void Execute(FolderTreeLayout layout)
                {
                    _model.FolderListConfig.FolderTreeLayout = layout;
                    SidePanel.Current.SetVisibleFolderTree(true, true);
                }
            }
        }

        private RelayCommand _NewFolderCommand;
        public RelayCommand NewFolderCommand
        {
            get { return _NewFolderCommand = _NewFolderCommand ?? new RelayCommand(NewFolderCommand_Executed); }
        }

        private void NewFolderCommand_Executed()
        {
            _model.NewFolder();
        }


        private RelayCommand _AddBookmarkCommand;
        public RelayCommand AddBookmarkCommand
        {
            get { return _AddBookmarkCommand = _AddBookmarkCommand ?? new RelayCommand(AddBookmarkCommand_Executed); }
        }

        private void AddBookmarkCommand_Executed()
        {
            _model.AddBookmark();
        }




        public ICommand ToggleVisiblePageList => RoutedCommandTable.Current.Commands["ToggleVisiblePageList"];

        public ICommand ToggleVisibleFoldersTree => RoutedCommandTable.Current.Commands["ToggleVisibleFoldersTree"];

        #endregion Commands

        #region MoreMenu

        //
        private void InitializeMoreMenu()
        {
            this.MoreMenu = new ContextMenu();
            UpdateMoreMenu();
        }

        public void UpdateMoreMenu()
        {
            var items = this.MoreMenu.Items;

            items.Clear();
            items.Add(CreateListItemStyleMenuItem(Properties.Resources.WordStyleList, PanelListItemStyle.Normal));
            items.Add(CreateListItemStyleMenuItem(Properties.Resources.WordStyleContent, PanelListItemStyle.Content));
            items.Add(CreateListItemStyleMenuItem(Properties.Resources.WordStyleBanner, PanelListItemStyle.Banner));
            items.Add(CreateListItemStyleMenuItem(Properties.Resources.WordStyleThumbnail, PanelListItemStyle.Thumbnail));
            items.Add(new Separator());
            items.Add(CreateCommandMenuItem(Properties.Resources.BookshelfMoreMenuExportPlaylist, ExportPlaylist));
            items.Add(CreateCommandMenuItem(Properties.Resources.BookshelfMoreMenuAddQuickAccess, AddQuickAccess));
            items.Add(CreateCommandMenuItem(Properties.Resources.BookshelfMoreMenuClearHistory, "ClearHistoryInPlace", FolderPanelModel.Current));

            switch (_model.FolderCollection)
            {
                case FolderEntryCollection folderEntryCollection:
                    items.Add(new Separator());
                    items.Add(CreateRecursiveFlagMenuItem(Properties.Resources.BookshelfMoreMenuSubfolder));
                    break;

                case FolderArchiveCollection folderArchiveCollection:
                    break;

                case FolderSearchCollection folderSearchCollection:
                    break;

                case BookmarkFolderCollection bookmarFolderCollection:
                    items.Add(new Separator());
                    items.Add(CreateCommandMenuItem(Properties.Resources.WordNewFolder, NewFolderCommand));
                    items.Add(CreateCommandMenuItem(Properties.Resources.FolderTreeMenuAddBookmark, AddBookmarkCommand));
                    break;
            }
        }

        //
        private MenuItem CreateCheckFlagMenuItem(string header, Binding binding)
        {
            var item = new MenuItem();
            item.Header = header;
            item.IsCheckable = true;
            item.SetBinding(MenuItem.IsCheckedProperty, binding);
            return item;
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
        private MenuItem CreateCommandMenuItem(string header, ICommand command)
        {
            var item = new MenuItem();
            item.Header = header;
            item.Command = command;
            return item;
        }

        //
        private MenuItem CreateCommandMenuItem(string header, string command, FolderPanelModel source)
        {
            var item = new MenuItem();
            item.Header = header;
            item.Command = RoutedCommandTable.Current.Commands[command];
            item.CommandParameter = MenuCommandTag.Tag; // コマンドがメニューからであることをパラメータで伝えてみる
            var binding = CommandTable.Current.GetElement(command).CreateIsCheckedBinding();
            if (binding != null)
            {
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
            var binding = new Binding(nameof(FolderListConfig.PanelListItemStyle))
            {
                Converter = _panelListItemStyleToBooleanConverter,
                ConverterParameter = style,
                Source = _model.FolderListConfig,
            };
            item.SetBinding(MenuItem.IsCheckedProperty, binding);

            return item;
        }


        /// <summary>
        /// SetListItemStyle command.
        /// </summary>
        private RelayCommand<PanelListItemStyle> _SetListItemStyle;
        public RelayCommand<PanelListItemStyle> SetListItemStyle
        {
            get { return _SetListItemStyle = _SetListItemStyle ?? new RelayCommand<PanelListItemStyle>(SetListItemStyle_Executed); }
        }

        //
        private void SetListItemStyle_Executed(PanelListItemStyle style)
        {
            _model.FolderListConfig.PanelListItemStyle = style;
        }

        #endregion MoreMenu

#region Methods

#if false
        private void Model_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            switch (e.PropertyName)
            {
                case nameof(_model.PanelListItemStyle):
                    UpdateFolderListBox();
                    break;
            }
        }
#endif

        private void Model_CollectionChanged(object sender, EventArgs e)
        {
            UpdateFolderListBox();
            UpdateFolderOrerList();

        }

        public void UpdateFolderListBox()
        {
            var vm = new FolderListBoxViewModel(_model, _model.FolderListBoxModel);
            FolderListBox = new FolderListBox(vm);

            SidePanel.Current.RaiseContentChanged();
        }

        public void UpdateFolderOrerList()
        {
            FolderOrderList = FolderCollection.FolderOrderClass.GetFolderOrderMap();
            RaisePropertyChanged(nameof(FolderOrder));
        }

        /// <summary>
        /// リスト項目へのフォーカス許可
        /// </summary>
        /// <param name="isEnabled"></param>
        public void SetListFocusEnabled(bool isEnabled)
        {
            if (_folderListBox != null)
            {
                _folderListBox.IsFocusEnabled = isEnabled;
            }
        }

        /// <summary>
        /// 検索履歴更新
        /// </summary>
        public void UpdateSearchHistory()
        {
            _model.UpdateSearchHistory();
        }

        /// <summary>
        /// 可能な場合のみ、フォルダー移動
        /// </summary>
        /// <param name="folderInfo"></param>
        public void MoveToSafety(FolderItem folderInfo)
        {
            if (folderInfo != null && folderInfo.CanOpenFolder())
            {
                _model.MoveTo(folderInfo.TargetPath);
            }
        }

#endregion Methods
    }
}
