using NeeLaboratory.Windows.Input;
using NeeLaboratory.Windows.Media;
using NeeView.Collections;
using NeeView.Collections.Generic;
using NeeView.Windows;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace NeeView
{
    /// <summary>
    /// FolderTreeView.xaml の相互作用ロジック
    /// </summary>
    public partial class FolderTreeView : UserControl
    {
        public static string DragDropFormat = $"{Environment.ProcessId}.TreeViewItem";

        private CancellationTokenSource _removeUnlinkedCommandCancellationTokenSource;
        private FolderTreeViewModel _vm;


        public FolderTreeView()
        {
            InitializeComponent();

            _vm = new FolderTreeViewModel();
            _vm.SelectedItemChanged += ViewModel_SelectedItemChanged;

            // タッチスクロール操作の終端挙動抑制
            this.TreeView.ManipulationBoundaryFeedback += SidePanel.Current.ScrollViewer_ManipulationBoundaryFeedback;

            this.TreeView.AddHandler(ScrollViewer.ScrollChangedEvent, new ScrollChangedEventHandler(TreeView_ScrollChanged));

            this.Root.DataContext = _vm;

            this.Loaded += FolderTreeView_Loaded;
            this.Unloaded += FolderTreeView_Unloaded;
        }

        #region Dependency Properties

        public FolderTreeModel Model
        {
            get { return (FolderTreeModel)GetValue(ModelProperty); }
            set { SetValue(ModelProperty, value); }
        }

        public static readonly DependencyProperty ModelProperty =
            DependencyProperty.Register("Model", typeof(FolderTreeModel), typeof(FolderTreeView), new PropertyMetadata(null, ModelPropertyChanged));

        private static void ModelPropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is FolderTreeView control)
            {
                control.UpdateModel();
            }
        }

        #endregion

        #region Properties

        #endregion Properties

        #region Commands

        private RelayCommand _addQuickAccessCommand;
        public RelayCommand AddQuickAccessCommand
        {
            get
            {
                return _addQuickAccessCommand = _addQuickAccessCommand ?? new RelayCommand(Execute);

                void Execute()
                {
                    var item = this.TreeView.SelectedItem;
                    if (item != null)
                    {
                        _vm.AddQuickAccess(item);
                    }
                }
            }
        }

        private RelayCommand _removeCommand;
        public RelayCommand RemoveCommand
        {
            get
            {
                return _removeCommand = _removeCommand ?? new RelayCommand(Execute);

                void Execute()
                {
                    switch (this.TreeView.SelectedItem)
                    {
                        case QuickAccessNode quickAccess:
                            _vm.RemoveQuickAccess(quickAccess);
                            break;

                        case RootBookmarkFolderNode rootBookmarkFolder:
                            break;

                        case BookmarkFolderNode bookmarkFolder:
                            _vm.RemoveBookmarkFolder(bookmarkFolder);
                            break;
                    }
                }
            }
        }

        private RelayCommand _RefreshFolderCommand;
        public RelayCommand RefreshFolderCommand
        {
            get
            {
                return _RefreshFolderCommand = _RefreshFolderCommand ?? new RelayCommand(Execute);

                void Execute()
                {
                    _vm.RefreshFolder();
                }
            }
        }

        private RelayCommand _OpenExplorerCommand;
        public RelayCommand OpenExplorerCommand
        {
            get
            {
                return _OpenExplorerCommand = _OpenExplorerCommand ?? new RelayCommand(Execute);

                void Execute()
                {
                    var item = this.TreeView.SelectedItem as DirectoryNode;
                    if (item != null)
                    {
                        System.Diagnostics.Process.Start("explorer.exe", item.Path);
                    }
                }
            }
        }


        private RelayCommand _NewFolderCommand;
        public RelayCommand NewFolderCommand
        {
            get
            {
                return _NewFolderCommand = _NewFolderCommand ?? new RelayCommand(Execute);

                void Execute()
                {
                    switch (this.TreeView.SelectedItem)
                    {
                        case BookmarkFolderNode bookmarkFolderNode:
                            {
                                var newItem = _vm.NewBookmarkFolder(bookmarkFolderNode);
                                if (newItem != null)
                                {
                                    this.TreeView.UpdateLayout();
                                    RenameBookmarkFolder(newItem);
                                }
                            }
                            break;
                    }
                }
            }
        }


        private RelayCommand _RenameCommand;
        public RelayCommand RenameCommand
        {
            get
            {
                return _RenameCommand = _RenameCommand ?? new RelayCommand(Execute);

                void Execute()
                {
                    switch (this.TreeView.SelectedItem)
                    {
                        case BookmarkFolderNode bookmarkFolderNode:
                            RenameBookmarkFolder(bookmarkFolderNode);
                            break;
                    }
                }
            }
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
            if (this.TreeView.SelectedItem is RootBookmarkFolderNode)
            {
                await BookmarkCollection.Current.RemoveUnlinkedAsync(_removeUnlinkedCommandCancellationTokenSource.Token);
            }
        }

        private RelayCommand _addBookmarkCommand;
        public RelayCommand AddBookmarkCommand
        {
            get
            {
                return _addBookmarkCommand = _addBookmarkCommand ?? new RelayCommand(Execute);

                void Execute()
                {
                    var item = this.TreeView.SelectedItem as BookmarkFolderNode;
                    if (item != null)
                    {
                        _vm.AddBookmarkTo(item);
                    }
                }
            }
        }

        #endregion


        private void UpdateModel()
        {
            _vm.Model = Model;
            this.TreeView.ItemsSource = Model?.Root.Children;
        }

        private void FolderTreeView_Loaded(object sender, RoutedEventArgs e)
        {
            FocusSelectedItem();
        }

        private void FolderTreeView_Unloaded(object sender, RoutedEventArgs e)
        {
        }

        private void RenameBookmarkFolder(BookmarkFolderNode item)
        {
            if (item is RootBookmarkFolderNode)
            {
                return;
            }

            var treetView = this.TreeView;
            var treeViewItem = VisualTreeUtility.FindContainer<TreeViewItem>(treetView, item);
            var textBlock = VisualTreeUtility.FindVisualChild<TextBlock>(treeViewItem, "FileNameTextBlock");

            if (textBlock != null)
            {
                var rename = new RenameControl() { Target = textBlock };
                rename.Closing += (s, ev) =>
                {
                    BookmarkCollectionService.Rename(item.BookmarkSource, ev.NewValue);
                };
                rename.Closed += (s, ev) =>
                {
                    this.TreeView.Focus();
                };
                rename.Close += (s, ev) =>
                {
                };

                MainWindow.Current.RenameManager.Open(rename);
            }
        }

        public void FocusSelectedItem()
        {
            if (!_vm.IsValid) return;

            if (this.TreeView.SelectedItem == null)
            {
                _vm.SelectRootQuickAccess();
            }

            if (_vm.Model.IsFocusAtOnce)
            {
                _vm.Model.IsFocusAtOnce = false;
                ScrollIntoView(true);
            }
        }

        private void ScrollIntoView(bool isFocus)
        {
            if (!_vm.IsValid) return;

            if (!this.TreeView.IsVisible)
            {
                return;
            }

            var selectedItem = _vm.Model.SelectedItem;
            if (selectedItem == null)
            {
                return;
            }

            ////Debug.WriteLine("ScrollIntoView:");

            this.TreeView.UpdateLayout();

            ItemsControl container = this.TreeView;
            var lastContainer = container;
            foreach (var node in selectedItem.Hierarchy.Skip(1))
            {
                if (node.Parent == null)
                {
                    break;
                }

                var index = node.Parent.Children.IndexOf(node);
                if (index < 0)
                {
                    break;
                }

                container = ScrollIntoView(container, index);
                if (container == null)
                {
                    break;
                }

                container.UpdateLayout();
                lastContainer = container;
            }

            if (isFocus)
            {
                bool isFocused = lastContainer.Focus();
                ////Debug.WriteLine($"FolderTree.Focused: {isFocused}");
            }

            _vm.Model.SelectedItem = selectedItem;

            ////Debug.WriteLine("ScrollIntoView: done.");
        }

        // from https://docs.microsoft.com/ja-jp/dotnet/framework/wpf/controls/how-to-find-a-treeviewitem-in-a-treeview
        // HACK: BindingErrorが出る
        private TreeViewItem ScrollIntoView(ItemsControl container, int index)
        {
            // Expand the current container
            if (container is TreeViewItem && !((TreeViewItem)container).IsExpanded)
            {
                container.SetValue(TreeViewItem.IsExpandedProperty, true);
                container.UpdateLayout();
            }

            // Try to generate the ItemsPresenter and the ItemsPanel.
            // by calling ApplyTemplate.  Note that in the 
            // virtualizing case even if the item is marked 
            // expanded we still need to do this step in order to 
            // regenerate the visuals because they may have been virtualized away.
            container.ApplyTemplate();
            ItemsPresenter itemsPresenter = (ItemsPresenter)container.Template.FindName("ItemsHost", container);
            if (itemsPresenter != null)
            {
                itemsPresenter.ApplyTemplate();
            }
            else
            {
                // The Tree template has not named the ItemsPresenter, 
                // so walk the descendents and find the child.
                itemsPresenter = VisualTreeUtility.FindVisualChild<ItemsPresenter>(container);
                if (itemsPresenter == null)
                {
                    container.UpdateLayout();
                    itemsPresenter = VisualTreeUtility.FindVisualChild<ItemsPresenter>(container);
                }
            }

            Panel itemsHostPanel = (Panel)VisualTreeHelper.GetChild(itemsPresenter, 0);

            // Ensure that the generator for this panel has been created.
            UIElementCollection children = itemsHostPanel.Children;

            if (itemsHostPanel is CustomVirtualizingStackPanel virtualizingPanel)
            {
                virtualizingPanel.BringIntoView(index);
                var subContainer = (TreeViewItem)container.ItemContainerGenerator.ContainerFromIndex(index);
                return subContainer;
            }
            else
            {
                var subContainer = (TreeViewItem)container.ItemContainerGenerator.ContainerFromIndex(index);
                // Bring the item into view to maintain the 
                // same behavior as with a virtualizing panel.
                subContainer?.BringIntoView();
                return subContainer;
            }
        }


        private void ViewModel_SelectedItemChanged(object sender, EventArgs e)
        {
            ScrollIntoView(false);
        }

        private void TreeView_ScrollChanged(object sender, ScrollChangedEventArgs e)
        {
            ((MainWindow)App.Current.MainWindow).RenameManager.Stop();
        }

        private void TreeView_SelectedItemChanged(object sender, RoutedPropertyChangedEventArgs<object> e)
        {
            if (!_vm.IsValid) return;

            _vm.Model.SelectedItem = this.TreeView.SelectedItem as FolderTreeNodeBase;
        }

        private async void TreeView_IsVisibleChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            if (!_vm.IsValid) return;

            var isVisible = (bool)e.NewValue;
            _vm.IsVisibleChanged(isVisible);
            if (isVisible)
            {
                await Task.Yield();
                FocusSelectedItem();
            }
        }

        private void TreeViewItem_Selected(object sender, RoutedEventArgs e)
        {
#if false
            // scroll into view
            if (sender is TreeViewItem item)
            {
                item.BringIntoView();
                e.Handled = true;
            }
#endif
        }

        private void TreeViewItem_MouseRightButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (sender is TreeViewItem item)
            {
                item.IsSelected = true;
                e.Handled = true;
            }
        }


        private void TreeViewItem_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            if (!_vm.IsValid) return;

            if (sender is TreeViewItem viewItem)
            {
                if (viewItem.IsSelected)
                {
                    _vm.Decide(viewItem.DataContext);
                }
                e.Handled = true;
            }
        }

        private void TreeViewItem_KeyDown(object sender, KeyEventArgs e)
        {
            if (!_vm.IsValid) return;

            if (!(sender is TreeViewItem viewItem))
            {
                return;
            }

            if (e.Key == Key.Return)
            {
                if (viewItem.IsSelected)
                {
                    _vm.Decide(viewItem.DataContext);
                }
                e.Handled = true;
            }
            else if (e.Key == Key.Delete)
            {
                if (viewItem.IsSelected)
                {
                    RemoveCommand.Execute(viewItem.DataContext);
                }
                e.Handled = true;
            }
            else if (e.Key == Key.F2)
            {
                if (viewItem.IsSelected)
                {
                    RenameCommand.Execute(viewItem.DataContext);
                }
                e.Handled = true;
            }
        }

        private void TreeViewItem_ContextMenuOpening(object sender, ContextMenuEventArgs e)
        {
            if (!(sender is TreeViewItem viewItem))
            {
                return;
            }

            if (!viewItem.IsSelected)
            {
                return;
            }

            var contextMenu = viewItem.ContextMenu;
            contextMenu.Items.Clear();

            switch (viewItem.DataContext)
            {
                case RootQuickAccessNode rootQuickAccess:
                    contextMenu.Items.Add(CreateMenuItem(Properties.Resources.FolderTreeMenuAddCurrentQuickAccess, AddQuickAccessCommand));
                    break;

                case QuickAccessNode quickAccess:
                    contextMenu.Items.Add(CreateMenuItem(Properties.Resources.FolderTreeMenuRemoveQuickAccess, RemoveCommand));
                    break;

                case RootDirectoryNode rootFolder:
                    contextMenu.Items.Add(CreateMenuItem(Properties.Resources.FolderTreeMenuRefreshFolder, RefreshFolderCommand));
                    break;

                case DirectoryNode folder:
                    contextMenu.Items.Add(CreateMenuItem(Properties.Resources.FolderTreeMenuExplorer, OpenExplorerCommand));
                    contextMenu.Items.Add(CreateMenuItem(Properties.Resources.FolderTreeMenuAddQuickAccess, AddQuickAccessCommand));
                    break;

                case RootBookmarkFolderNode rootBookmarkFolder:
                    contextMenu.Items.Add(CreateMenuItem(Properties.Resources.FolderTreeMenuDeleteInvalidBookmark, RemoveUnlinkedCommand));
                    contextMenu.Items.Add(new Separator());
                    contextMenu.Items.Add(CreateMenuItem(Properties.Resources.WordNewFolder, NewFolderCommand));
                    contextMenu.Items.Add(CreateMenuItem(Properties.Resources.FolderTreeMenuAddBookmark, AddBookmarkCommand));
                    break;

                case BookmarkFolderNode bookmarkFolder:
                    contextMenu.Items.Add(CreateMenuItem(Properties.Resources.WordRemove, RemoveCommand));
                    contextMenu.Items.Add(CreateMenuItem(Properties.Resources.WordRename, RenameCommand));
                    contextMenu.Items.Add(new Separator());
                    contextMenu.Items.Add(CreateMenuItem(Properties.Resources.WordNewFolder, NewFolderCommand));
                    contextMenu.Items.Add(CreateMenuItem(Properties.Resources.FolderTreeMenuAddBookmark, AddBookmarkCommand));
                    break;

                default:
                    e.Handled = true;
                    break;
            }
        }

        //
        private MenuItem CreateMenuItem(string header, ICommand command)
        {
            var item = new MenuItem();
            item.Header = header;
            item.Command = command;
            return item;
        }

        //
        private MenuItem CreateMenuItem(string header, string command, object source)
        {
            var item = new MenuItem();
            item.Header = header;
            item.Command = RoutedCommandTable.Current.Commands[command];
            item.CommandParameter = MenuCommandTag.Tag; // コマンドがメニューからであることをパラメータで伝えてみる
            var binding = CommandTable.Current.GetElement(command).CreateIsCheckedBinding();
            if (binding != null)
            {
                binding.Source = source;
                item.SetBinding(MenuItem.IsCheckedProperty, binding);
            }

            return item;
        }

        #region DragDrop

        private void DragStartBehavior_DragBegin(object sender, DragStartEventArgs e)
        {
            var data = e.Data.GetData(DragDropFormat) as TreeViewItem;
            if (data == null)
            {
                return;
            }

            switch (data.DataContext)
            {
                case QuickAccessNode quickAccess:
                    e.AllowedEffects = DragDropEffects.Copy | DragDropEffects.Move;
                    ////e.Data.SetData(quickAccess);
                    break;

                case DirectoryNode direcory:
                    e.AllowedEffects = DragDropEffects.Copy;
                    e.Data.SetFileDropList(new System.Collections.Specialized.StringCollection() { direcory.Path });
                    break;

                //case RootBookmarkFolderNode rootBookmarkFolder:
                //    break;

                case BookmarkFolderNode bookmarkFolder:
                    e.Data.SetData(bookmarkFolder.Source);
                    e.AllowedEffects = DragDropEffects.Copy | DragDropEffects.Move;
                    break;

                default:
                    e.Cancel = true;
                    break;
            }
        }

        private void TreeView_DragEnter(object sender, DragEventArgs e)
        {
            TreeView_DragDrop(sender, e, false);
            DragDropHelper.AutoScroll(sender, e);
        }

        private void TreeView_DragLeave(object sender, DragEventArgs e)
        {
        }

        private void TreeView_PreviewDragOver(object sender, DragEventArgs e)
        {
            TreeView_DragDrop(sender, e, false);
            DragDropHelper.AutoScroll(sender, e);
        }

        private void TreeView_Drop(object sender, DragEventArgs e)
        {
            TreeView_DragDrop(sender, e, true);
        }

        private void TreeView_DragDrop(object sender, DragEventArgs e, bool isDrop)
        {
            if (!_vm.IsValid) return;

            var treeViewItem = PointToViewItem(this.TreeView, e.GetPosition(this.TreeView));
            if (treeViewItem != null)
            {
                var dragData = e.Data.GetData<TreeViewItem>(DragDropFormat);
                if (dragData == treeViewItem)
                {
                    e.Effects = DragDropEffects.None;
                    e.Handled = true;
                    return;
                }

                switch (treeViewItem.DataContext)
                {
                    case RootQuickAccessNode rootQuickAccessNode:
                        {
                            DropToQuickAccess(sender, e, isDrop, null, e.Data.GetData<TreeListNode<IBookmarkEntry>>());
                            if (e.Handled) return;

                            DropToQuickAccess(sender, e, isDrop, null, e.Data.GetData<QueryPath>());
                            if (e.Handled) return;

                            DropToQuickAccess(sender, e, isDrop, null, e.Data.GetFileDrop());
                            if (e.Handled) return;
                        }
                        break;

                    case QuickAccessNode quickAccessTarget:
                        {
                            DropToQuickAccess(sender, e, isDrop, quickAccessTarget, dragData?.DataContext as QuickAccessNode);
                            if (e.Handled) return;

                            DropToQuickAccess(sender, e, isDrop, quickAccessTarget, e.Data.GetData<TreeListNode<IBookmarkEntry>>());
                            if (e.Handled) return;

                            DropToQuickAccess(sender, e, isDrop, quickAccessTarget, e.Data.GetData<QueryPath>());
                            if (e.Handled) return;

                            DropToQuickAccess(sender, e, isDrop, quickAccessTarget, e.Data.GetFileDrop());
                            if (e.Handled) return;
                        }
                        break;

                    case BookmarkFolderNode bookmarkFolderTarget:
                        {
                            DropToBookmark(sender, e, isDrop, bookmarkFolderTarget, dragData?.DataContext as BookmarkFolderNode);
                            if (e.Handled) return;

                            DropToBookmark(sender, e, isDrop, bookmarkFolderTarget, e.Data.GetData<TreeListNode<IBookmarkEntry>>());
                            if (e.Handled) return;

                            DropToBookmark(sender, e, isDrop, bookmarkFolderTarget, e.Data.GetData<QueryPath>());
                            if (e.Handled) return;

                            DropToBookmark(sender, e, isDrop, bookmarkFolderTarget, e.Data.GetFileDrop());
                            if (e.Handled) return;
                        }
                        break;
                }
            }

            e.Effects = DragDropEffects.None;
            e.Handled = true;
        }

        private void DropToQuickAccess(object sender, DragEventArgs e, bool isDrop, QuickAccessNode quickAccessTarget, QuickAccessNode quickAccess)
        {
            if (quickAccess == null)
            {
                return;
            }

            if (quickAccess != quickAccessTarget)
            {
                if (isDrop)
                {
                    _vm.MoveQuickAccess(quickAccess, quickAccessTarget);
                }
                e.Effects = DragDropEffects.Move;
                e.Handled = true;
            }
        }

        private void DropToQuickAccess(object sender, DragEventArgs e, bool isDrop, QuickAccessNode quickAccessTarget, TreeListNode<IBookmarkEntry> bookmarkEntry)
        {
            if (bookmarkEntry == null)
            {
                return;
            }

            if (bookmarkEntry.Value is BookmarkFolder bookmarkFolder)
            {
                if (isDrop)
                {
                    var query = bookmarkEntry.CreateQuery(QueryScheme.Bookmark);
                    _vm.Model.InsertQuickAccess(quickAccessTarget, query.SimplePath);
                }
                e.Effects = DragDropEffects.Copy;
                e.Handled = true;
            }
        }

        private void DropToQuickAccess(object sender, DragEventArgs e, bool isDrop, QuickAccessNode quickAccessTarget, QueryPath query)
        {
            if (query == null)
            {
                return;
            }

            if ((query.Scheme == QueryScheme.File && Directory.Exists(query.SimplePath))
                || (query.Scheme == QueryScheme.Bookmark && BookmarkCollection.Current.FindNode(query)?.Value is BookmarkFolder))
            {
                if (isDrop)
                {
                    _vm.Model.InsertQuickAccess(quickAccessTarget, query.SimpleQuery);
                }
                e.Effects = DragDropEffects.Copy;
                e.Handled = true;
            }
        }

        private void DropToQuickAccess(object sender, DragEventArgs e, bool isDrop, QuickAccessNode quickAccessTarget, string[] fileNames)
        {
            if (fileNames == null)
            {
                return;
            }
            if ((e.AllowedEffects & DragDropEffects.Copy) != DragDropEffects.Copy)
            {
                return;
            }

            bool isDropped = false;
            foreach (var fileName in fileNames)
            {
                if (System.IO.Directory.Exists(fileName))
                {
                    if (isDrop)
                    {
                        _vm.Model.InsertQuickAccess(quickAccessTarget, fileName);
                    }
                    isDropped = true;
                }
            }
            if (isDropped)
            {
                e.Effects = DragDropEffects.Copy;
                e.Handled = true;
            }
        }


        public void DropToBookmark(object sender, DragEventArgs e, bool isDrop, BookmarkFolderNode bookmarkFolderTarget, BookmarkFolderNode bookmarkFolder)
        {
            if (bookmarkFolder == null)
            {
                return;
            }

            if (!bookmarkFolderTarget.BookmarkSource.ParentContains(bookmarkFolder.BookmarkSource))
            {
                if (isDrop)
                {
                    BookmarkCollection.Current.MoveToChild(bookmarkFolder.BookmarkSource, bookmarkFolderTarget.BookmarkSource);
                }
                e.Effects = DragDropEffects.Move;
                e.Handled = true;
            }
        }

        public void DropToBookmark(object sender, DragEventArgs e, bool isDrop, BookmarkFolderNode bookmarkFolderTarget, TreeListNode<IBookmarkEntry> bookmarkEntry)
        {
            if (bookmarkEntry == null)
            {
                return;
            }

            if (bookmarkEntry.Value is BookmarkFolder)
            {
                if (bookmarkFolderTarget.Source != bookmarkEntry && !bookmarkFolderTarget.BookmarkSource.ParentContains(bookmarkEntry))
                {
                    if (isDrop)
                    {
                        BookmarkCollection.Current.MoveToChild(bookmarkEntry, bookmarkFolderTarget.BookmarkSource);
                    }
                    e.Effects = DragDropEffects.Move;
                    e.Handled = true;
                }
            }
            else if (bookmarkEntry.Value is Bookmark)
            {
                if (isDrop)
                {
                    BookmarkCollection.Current.MoveToChild(bookmarkEntry, bookmarkFolderTarget.BookmarkSource);
                }
                e.Effects = DragDropEffects.Move;
                e.Handled = true;
            }
        }

        public void DropToBookmark(object sender, DragEventArgs e, bool isDrop, BookmarkFolderNode bookmarkFolderTarget, QueryPath query)
        {
            if (query == null)
            {
                return;
            }

            if (query.Search == null && (query.Scheme == QueryScheme.File || query.IsRoot(QueryScheme.Pagemark)))
            {
                if (isDrop)
                {
                    BookmarkCollectionService.AddToChild(bookmarkFolderTarget.BookmarkSource, query);
                }
                e.Effects = DragDropEffects.Copy;
                e.Handled = true;
            }
        }

        public void DropToBookmark(object sender, DragEventArgs e, bool isDrop, BookmarkFolderNode bookmarkFolderTarget, string[] fileNames)
        {
            if (fileNames == null)
            {
                return;
            }
            if ((e.AllowedEffects & DragDropEffects.Copy) != DragDropEffects.Copy)
            {
                return;
            }


            bool isDropped = false;
            foreach (var fileName in fileNames)
            {
                if (ArchiverManager.Current.IsSupported(fileName, true, true) || System.IO.Directory.Exists(fileName))
                {
                    if (isDrop)
                    {
                        BookmarkCollectionService.AddToChild(bookmarkFolderTarget.BookmarkSource, new QueryPath(fileName));
                    }
                    isDropped = true;
                }
            }
            if (isDropped)
            {
                e.Effects = DragDropEffects.Copy;
                e.Handled = true;
            }
        }

        private TreeViewItem PointToViewItem(TreeView treeView, Point point)
        {
            var element = VisualTreeUtility.HitTest<TreeViewItem>(treeView, point);

            // NOTE: リストアイテム間に隙間がある場合があるので、Y座標をずらして再検証する
            if (element == null)
            {
                element = VisualTreeUtility.HitTest<TreeViewItem>(treeView, new Point(point.X, point.Y + 1));
            }

            return element;
        }

        #endregion
    }

    public static class IDataObjectExtensions
    {
        public static T GetData<T>(this IDataObject data)
            where T : class
        {
            return data.GetData(typeof(T)) as T;
        }

        public static T GetData<T>(this IDataObject data, string format)
            where T : class
        {
            return data.GetData(format) as T;
        }

        public static string[] GetFileDrop(this IDataObject data)
        {
            return (string[])data.GetData(DataFormats.FileDrop, false);
        }
    }
}
