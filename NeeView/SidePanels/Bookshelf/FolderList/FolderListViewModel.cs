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
    /// FolderList : ViewModel
    /// </summary>
    public class FolderListViewModel : BindableBase
    {
        private PanelListItemStyleToBooleanConverter _panelListItemStyleToBooleanConverter = new PanelListItemStyleToBooleanConverter();
        private BookshelfFolderList _model;
        private Dictionary<FolderOrder, string> _folderOrderList = AliasNameExtensions.GetAliasNameDictionary<FolderOrder>();


        public FolderListViewModel(BookshelfFolderList model)
        {
            _model = model;

            _model.History.Changed +=
                (s, e) => UpdateCommandCanExecute();

            _model.PlaceChanged +=
                (s, e) => MoveToUp.RaiseCanExecuteChanged();

            _model.CollectionChanged +=
                Model_CollectionChanged;

            InitializeMoreMenu();
        }



        public FolderCollection FolderCollection => _model.FolderCollection;


        public BookshelfFolderList Model
        {
            get { return _model; }
            set { if (_model != value) { _model = value; RaisePropertyChanged(); } }
        }

        /// <summary>
        /// コンボボックス用リスト
        /// </summary>
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


        #region Commands

        private RelayCommand _setHome;
        private RelayCommand _moveToHome;
        private RelayCommand<QueryPath> _moveTo;
        private RelayCommand _moveToPrevious;
        private RelayCommand _moveToNext;
        private RelayCommand<KeyValuePair<int, QueryPath>> _moveToHistory;
        private RelayCommand _moveToUp;
        private RelayCommand _sync;
        private RelayCommand _toggleFolderRecursive;
        private RelayCommand _search;
        private RelayCommand _clearSearch;
        private RelayCommand _addQuickAccess;
        private RelayCommand _exportPlaylist;
        private RelayCommand<FolderTreeLayout> _setFolderTreeLayout;
        private RelayCommand _newFolderCommand;
        private RelayCommand _addBookmarkCommand;
        private RelayCommand<PanelListItemStyle> _setListItemStyle;

        public ICommand ToggleVisibleFoldersTree => RoutedCommandTable.Current.Commands["ToggleVisibleFoldersTree"];


        public RelayCommand SetHome
        {
            get { return _setHome = _setHome ?? new RelayCommand(_model.SetHome, _model.CanSetHome); }
        }

        public RelayCommand MoveToHome
        {
            get { return _moveToHome = _moveToHome ?? new RelayCommand(_model.MoveToHome); }
        }

        public RelayCommand<QueryPath> MoveTo
        {
            get { return _moveTo = _moveTo ?? new RelayCommand<QueryPath>(_model.MoveTo); }
        }

        public RelayCommand MoveToPrevious
        {
            get { return _moveToPrevious = _moveToPrevious ?? new RelayCommand(_model.MoveToPrevious, _model.CanMoveToPrevious); }
        }

        public RelayCommand MoveToNext
        {
            get { return _moveToNext = _moveToNext ?? new RelayCommand(_model.MoveToNext, _model.CanMoveToNext); }
        }

        public RelayCommand<KeyValuePair<int, QueryPath>> MoveToHistory
        {
            get { return _moveToHistory = _moveToHistory ?? new RelayCommand<KeyValuePair<int, QueryPath>>(_model.MoveToHistory); }
        }

        public RelayCommand MoveToUp
        {
            get { return _moveToUp = _moveToUp ?? new RelayCommand(_model.MoveToParent, _model.CanMoveToParent); }
        }

        public RelayCommand Sync
        {
            get { return _sync = _sync ?? new RelayCommand(_model.Sync); }
        }

        public RelayCommand ToggleFolderRecursive
        {
            get { return _toggleFolderRecursive = _toggleFolderRecursive ?? new RelayCommand(_model.ToggleFolderRecursive); }
        }

        public RelayCommand Search
        {
            get
            {
                return _search = _search ?? new RelayCommand(Execute);

                void Execute()
                {
                    _model.RequestSearchPlace(true);
                }
            }
        }

        public RelayCommand ClearSearch
        {
            get
            {
                return _clearSearch = _clearSearch ?? new RelayCommand(Execute);

                void Execute()
                {
                    _model.InputKeyword = "";
                    _model.SetSearchKeywordAndSearch("");
                }
            }
        }

        public RelayCommand AddQuickAccess
        {
            get
            {
                return _addQuickAccess = _addQuickAccess ?? new RelayCommand(Execute, CanExecute);

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

        public RelayCommand ExportPlaylist
        {
            get
            {
                return _exportPlaylist = _exportPlaylist ?? new RelayCommand(Execute, CanExecute);

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

        public RelayCommand<FolderTreeLayout> SetFolderTreeLayout
        {
            get
            {
                return _setFolderTreeLayout = _setFolderTreeLayout ?? new RelayCommand<FolderTreeLayout>(Execute);

                void Execute(FolderTreeLayout layout)
                {
                    _model.FolderListConfig.FolderTreeLayout = layout;
                    SidePanelFrame.Current.SetVisibleFolderTree(true, true);
                }
            }
        }

        public RelayCommand NewFolderCommand
        {
            get
            {
                return _newFolderCommand = _newFolderCommand ?? new RelayCommand(Execute);

                void Execute()
                {
                    _model.NewFolder();
                }
            }
        }

        public RelayCommand AddBookmarkCommand
        {
            get
            {
                return _addBookmarkCommand = _addBookmarkCommand ?? new RelayCommand(Execute);

                void Execute()
                {
                    _model.AddBookmark();
                }

            }
        }

        public RelayCommand<PanelListItemStyle> SetListItemStyle
        {
            get
            {
                return _setListItemStyle = _setListItemStyle ?? new RelayCommand<PanelListItemStyle>(Execute);

                void Execute(PanelListItemStyle style)
                {
                    _model.FolderListConfig.PanelListItemStyle = style;
                }
            }
        }


        /// <summary>
        /// コマンド実行可能状態を更新
        /// </summary>
        private void UpdateCommandCanExecute()
        {
            this.MoveToPrevious.RaiseCanExecuteChanged();
            this.MoveToNext.RaiseCanExecuteChanged();
        }

        #endregion Commands

        #region MoreMenu

        private ContextMenu _moreMenu;


        public ContextMenu MoreMenu
        {
            get { return _moreMenu; }
            set { if (_moreMenu != value) { _moreMenu = value; RaisePropertyChanged(); } }
        }


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
            items.Add(CreateCommandMenuItem(Properties.Resources.BookshelfMoreMenuClearHistory, "ClearHistoryInPlace"));

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

            if (_model.IsFolderSearchEnabled)
            {
                var subItem = new MenuItem() { Header = Properties.Resources.BookshelfMoreMenuSearchOptions };
                subItem.Items.Add(CreateCheckFlagMenuItem(Properties.Resources.BookshelfMoreMenuSearchIncremental, new Binding(nameof(BookshelfConfig.IsIncrementalSearchEnabled)) { Source = Config.Current.Bookshelf }));
                subItem.Items.Add(CreateCheckFlagMenuItem(Properties.Resources.BookshelfMoreMenuSearchIncludeSubdirectories, new Binding(nameof(BookshelfConfig.IsSearchIncludeSubdirectories)) { Source = Config.Current.Bookshelf }));
                items.Add(new Separator());
                items.Add(subItem);
            }
        }

        private MenuItem CreateCheckFlagMenuItem(string header, Binding binding)
        {
            var item = new MenuItem();
            item.Header = header;
            item.IsCheckable = true;
            item.SetBinding(MenuItem.IsCheckedProperty, binding);
            return item;
        }

        private MenuItem CreateRecursiveFlagMenuItem(string header)
        {
            var item = new MenuItem();
            item.Header = header;
            item.Command = ToggleFolderRecursive;
            item.SetBinding(MenuItem.IsCheckedProperty, new Binding("FolderCollection.FolderParameter.IsFolderRecursive"));
            return item;
        }

        private MenuItem CreateCommandMenuItem(string header, ICommand command)
        {
            var item = new MenuItem();
            item.Header = header;
            item.Command = command;
            return item;
        }

        private MenuItem CreateCommandMenuItem(string header, string command)
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

        #endregion MoreMenu


        /// <summary>
        /// Model CollectionChanged event
        /// </summary>
        private void Model_CollectionChanged(object sender, EventArgs e)
        {
            UpdateFolderOrerList();
            RaisePropertyChanged(nameof(FolderCollection));
        }

        /// <summary>
        /// 履歴取得
        /// </summary>
        internal List<KeyValuePair<int, QueryPath>> GetHistory(int direction, int size)
        {
            return _model.History.GetHistory(direction, size);
        }

        /// <summary>
        /// 並び順リスト更新
        /// </summary>
        public void UpdateFolderOrerList()
        {
            FolderOrderList = FolderCollection.FolderOrderClass.GetFolderOrderMap();
            RaisePropertyChanged(nameof(FolderOrder));
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

    }
}
