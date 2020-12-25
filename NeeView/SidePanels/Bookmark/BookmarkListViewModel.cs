using NeeLaboratory.ComponentModel;
using NeeLaboratory.Windows.Input;
using NeeView.Windows;
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
using System.Threading;
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

        private PanelListItemStyleToBooleanConverter _panelListItemStyleToBooleanConverter = new PanelListItemStyleToBooleanConverter();
        private CancellationTokenSource _removeUnlinkedCommandCancellationTokenSource;
        private DpiScaleProvider _dpiProvider = new DpiScaleProvider();

        #endregion

        #region Constructor

        public BookmarkListViewModel(FolderList model)
        {
            _model = model;

            _model.PlaceChanged +=
                (s, e) => MoveToUp.RaiseCanExecuteChanged();

            _model.CollectionChanged +=
                (s, e) => RaisePropertyChanged(nameof(FolderCollection));

            _dpiProvider.DpiChanged +=
                (s, e) => RaisePropertyChanged(nameof(DpiScale));

            InitializeMoreMenu();
        }

        #endregion

        #region Properties

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

        public DpiScale DpiScale => _dpiProvider.DpiScale;

        #endregion Properties

        #region Commands

        /// <summary>
        /// MoveTo command.
        /// </summary>
        private RelayCommand<QueryPath> _MoveTo;
        public RelayCommand<QueryPath> MoveTo
        {
            get { return _MoveTo = _MoveTo ?? new RelayCommand<QueryPath>(_model.MoveTo); }
        }

        /// <summary>
        /// MoveToUp command.
        /// </summary>
        private RelayCommand _MoveToUp;
        public RelayCommand MoveToUp
        {
            get { return _MoveToUp = _MoveToUp ?? new RelayCommand(_model.MoveToParent, _model.CanMoveToParent); }
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
                    SidePanelFrame.Current.SetVisibleBookmarkFolderTree(true, true);
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


        private RelayCommand _removeUnlinkedCommand;
        public RelayCommand RemoveUnlinkedCommand
        {
            get { return _removeUnlinkedCommand = _removeUnlinkedCommand ?? new RelayCommand(RemoveUnlinkedCommand_Executed); }
        }

        private async void RemoveUnlinkedCommand_Executed()
        {
            // 直前の命令はキャンセル
            _removeUnlinkedCommandCancellationTokenSource?.Cancel();
            _removeUnlinkedCommandCancellationTokenSource = new CancellationTokenSource();
            await BookmarkCollection.Current.RemoveUnlinkedAsync(_removeUnlinkedCommandCancellationTokenSource.Token);
        }

        private RelayCommand _ToggleVisibleFoldersTree;
        public RelayCommand ToggleVisibleFoldersTree
        {
            get { return _ToggleVisibleFoldersTree = _ToggleVisibleFoldersTree ?? new RelayCommand(ToggleVisibleFoldersTree_Executed); }
        }

        private void ToggleVisibleFoldersTree_Executed()
        {
            _model.FolderListConfig.IsFolderTreeVisible = !_model.FolderListConfig.IsFolderTreeVisible;
        }

        #endregion Commands

        #region MoreMenu

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
            items.Add(CreateCommandMenuItem(Properties.Resources.FolderTreeMenuDeleteInvalidBookmark, RemoveUnlinkedCommand));
            items.Add(new Separator());
            items.Add(CreateCommandMenuItem(Properties.Resources.WordNewFolder, NewFolderCommand));
            items.Add(CreateCommandMenuItem(Properties.Resources.FolderTreeMenuAddBookmark, AddBookmarkCommand));
            items.Add(new Separator());
            items.Add(CreateCheckFlagMenuItem(Properties.Resources.BookmarkList_MoreMenu_SyncBookshelf, new Binding(nameof(BookmarkConfig.IsSyncBookshelfEnabled)) { Source = Config.Current.Bookmark }));
        }

        private MenuItem CreateCheckFlagMenuItem(string header, Binding binding)
        {
            var item = new MenuItem();
            item.Header = header;
            item.IsCheckable = true;
            item.SetBinding(MenuItem.IsCheckedProperty, binding);
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

        /// <summary>
        /// SetListItemStyle command.
        /// </summary>
        private RelayCommand<PanelListItemStyle> _SetListItemStyle;
        public RelayCommand<PanelListItemStyle> SetListItemStyle
        {
            get { return _SetListItemStyle = _SetListItemStyle ?? new RelayCommand<PanelListItemStyle>(SetListItemStyle_Executed); }
        }

        private void SetListItemStyle_Executed(PanelListItemStyle style)
        {
            _model.FolderListConfig.PanelListItemStyle = style;
        }

        #endregion MoreMenu

        #region Methods

        public void SetDpiScale(DpiScale dpiScale)
        {
            _dpiProvider.SetDipScale(dpiScale);
        }

        #endregion Methods
    }
}
