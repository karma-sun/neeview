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
    /// BookmarkList : ViewModel
    /// </summary>
    public class BookmarkListViewModel : BindableBase
    {
        #region Fields

        //
        private FolderListBox _folderListBox;

        //
        private PanelListItemStyleToBooleanConverter _panelListItemStyleToBooleanConverter = new PanelListItemStyleToBooleanConverter();

        #endregion

        #region Constructor

        /// <summary>
        /// Constructor
        /// </summary>
        public BookmarkListViewModel(FolderList model)
        {
            _model = model;

#if false
            _model.History.Changed +=
                (s, e) => UpdateCommandCanExecute();
#endif

            _model.PropertyChanged +=
                Model_PropertyChanged;

            _model.PlaceChanged +=
                (s, e) => MoveToUp.RaiseCanExecuteChanged();

            _model.CollectionChanged +=
                Model_CollectionChanged;

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
        public Dictionary<FolderOrder, string> FolderOrderList => AliasNameExtensions.GetAliasNameDictionary<FolderOrder>();

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

#if false
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
#endif

        /// <summary>
        /// MoveTo command.
        /// </summary>
        private RelayCommand<QueryPath> _MoveTo;
        public RelayCommand<QueryPath> MoveTo
        {
            get { return _MoveTo = _MoveTo ?? new RelayCommand<QueryPath>(_model.MoveTo); }
        }

#if false
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
#endif

        /// <summary>
        /// MoveToUp command.
        /// </summary>
        private RelayCommand _MoveToUp;
        public RelayCommand MoveToUp
        {
            get { return _MoveToUp = _MoveToUp ?? new RelayCommand(_model.MoveToParent, _model.CanMoveToParent); }
        }

#if false
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
            _model.SearchKeyword = "";
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
#endif

        private RelayCommand<FolderTreeLayout> _SetFolderTreeLayout;
        public RelayCommand<FolderTreeLayout> SetFolderTreeLayout
        {
            get
            {
                return _SetFolderTreeLayout = _SetFolderTreeLayout ?? new RelayCommand<FolderTreeLayout>(Execute);

                void Execute(FolderTreeLayout layout)
                {
                    _model.FolderTreeLayout = layout;
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



#if false
        public ICommand ToggleVisiblePageList => RoutedCommandTable.Current.Commands[CommandType.ToggleVisiblePageList];
        public ICommand ToggleVisibleFoldersTree => RoutedCommandTable.Current.Commands[CommandType.ToggleVisibleFoldersTree];
#endif

        private RelayCommand _ToggleVisibleFoldersTree;
        public RelayCommand ToggleVisibleFoldersTree
        {
            get { return _ToggleVisibleFoldersTree = _ToggleVisibleFoldersTree ?? new RelayCommand(ToggleVisibleFoldersTree_Executed); }
        }

        private void ToggleVisibleFoldersTree_Executed()
        {
            _model.IsFolderTreeVisible = !_model.IsFolderTreeVisible;
        }


        #endregion Commands

        #region Methods

        //
        private void Model_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            switch (e.PropertyName)
            {
                case nameof(_model.PanelListItemStyle):
                    UpdateFolderListBox();
                    break;
            }
        }

        //
        private void Model_CollectionChanged(object sender, EventArgs e)
        {
            UpdateFolderListBox();
        }

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
#if false
            items.Add(new Separator());
            items.Add(CreateCommandMenuItem(Properties.Resources.BookshelfMoreMenuAddQuickAccess, AddQuickAccess));
            items.Add(CreateCommandMenuItem(Properties.Resources.BookshelfMoreMenuClearHistory, CommandType.ClearHistoryInPlace, FolderPanelModel.Current));

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
#endif
            items.Add(new Separator());
            items.Add(CreateCommandMenuItem(Properties.Resources.WordNewFolder, NewFolderCommand));
            items.Add(CreateCommandMenuItem(Properties.Resources.FolderTreeMenuAddBookmark, AddBookmarkCommand));
        }

#if false
        //
        private MenuItem CreateRecursiveFlagMenuItem(string header)
        {
            var item = new MenuItem();
            item.Header = header;
            item.Command = ToggleFolderRecursive;
            item.SetBinding(MenuItem.IsCheckedProperty, new Binding("FolderCollection.FolderParameter.IsFolderRecursive"));
            return item;
        }
#endif

        //
        private MenuItem CreateCommandMenuItem(string header, ICommand command)
        {
            var item = new MenuItem();
            item.Header = header;
            item.Command = command;
            return item;
        }

        //
        private MenuItem CreateCommandMenuItem(string header, CommandType command, FolderPanelModel source)
        {
            var item = new MenuItem();
            item.Header = header;
            item.Command = RoutedCommandTable.Current.Commands[command];
            item.CommandParameter = MenuCommandTag.Tag; // コマンドがメニューからであることをパラメータで伝えてみる
            if (CommandTable.Current[command].CreateIsCheckedBinding != null)
            {
                var binding = CommandTable.Current[command].CreateIsCheckedBinding();
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
                Converter = _panelListItemStyleToBooleanConverter,
                ConverterParameter = style,
                Source = _model,
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
            _model.PanelListItemStyle = style;
        }

        #endregion MoreMenu


        //
        public void UpdateFolderListBox()
        {
            var vm = new FolderListBoxViewModel(_model, _model.FolderListBoxModel);
            FolderListBox = new FolderListBox(vm);

            SidePanel.Current.RaiseContentChanged();
        }


#if false
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
#endif

        #endregion Methods
    }
}
