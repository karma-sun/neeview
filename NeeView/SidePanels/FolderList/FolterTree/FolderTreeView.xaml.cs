using NeeLaboratory.Windows.Input;
using NeeLaboratory.Windows.Media;
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
        public static string DragDropFormat = $"{Config.Current.ProcessId}.TreeViewItem";

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
        }

        public bool IsRenaming { get; private set; }

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
                    var item = this.TreeView.SelectedItem as BookmarkFolderNode;
                    if (item != null)
                    {
                        var newItem = _vm.NewBookmarkFolder(item);
                        if (newItem != null)
                        {
                            ////newItem.IsSelected = true;
                            this.TreeView.UpdateLayout();
                            RenameBookmarkFolder(newItem);
                        }
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
                    var item = this.TreeView.SelectedItem as BookmarkFolderNode;
                    if (item != null)
                    {
                        RenameBookmarkFolder(item);
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
            await BookmarkCollection.Current.RemoveUnlinkedAsync(_removeUnlinkedCommandCancellationTokenSource.Token);
        }


        #endregion


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
                    var newName = BookmarkFolder.GetValidateName(ev.NewValue);
                    if (string.IsNullOrEmpty(newName))
                    {
                        newName = ev.OldValue;
                    }

                    if (ev.OldValue != newName)
                    {
                        var node = item.BookmarkSource;
                        var conflict = node.Parent.Children.FirstOrDefault(e => e != node && e.Value is BookmarkFolder && e.Value.Name == newName);
                        if (conflict != null)
                        {
                            var dialog = new MessageDialog(string.Format(Properties.Resources.DialogMergeFolder, newName), Properties.Resources.DialogMergeFolderTitle);
                            dialog.Commands.Add(UICommands.Yes);
                            dialog.Commands.Add(UICommands.No);
                            var result = dialog.ShowDialog();

                            if (result == UICommands.Yes)
                            {
                                BookmarkCollection.Current.Merge(node, conflict);
                            }
                        }
                        else
                        {
                            var folder = (BookmarkFolder)node.Value;
                            folder.Name = newName;
                            BookmarkCollection.Current.RaiseBookmarkChangedEvent(new BookmarkCollectionChangedEventArgs(EntryCollectionChangedAction.Rename, node.Parent, node) { OldName = ev.OldValue });
                        }
                    }
                };
                rename.Closed += (s, ev) =>
                {
                    this.TreeView.Focus();
                };
                rename.Close += (s, ev) =>
                {
                    IsRenaming = false;
                };

                MainWindow.Current.RenameManager.Open(rename);
                IsRenaming = true;
            }
        }


        public bool FocusSelectedItem()
        {
            if (this.TreeView.SelectedItem == null)
            {
                _vm.SelectRootQuickAccess();
            }

            return this.TreeView.Focus();
        }

        private void ScrollIntoView()
        {
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
           ScrollIntoView();
        }

        private void TreeView_ScrollChanged(object sender, ScrollChangedEventArgs e)
        {
            ((MainWindow)App.Current.MainWindow).RenameManager.Stop();
        }

        private void TreeView_SelectedItemChanged(object sender, RoutedPropertyChangedEventArgs<object> e)
        {
            _vm.Model.SelectedItem = this.TreeView.SelectedItem as FolderTreeNodeBase;

        }

        private void TreeView_IsVisibleChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            _vm.IsVisibleChanged((bool)e.NewValue);
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
                    contextMenu.Items.Add(new MenuItem() { Header = Properties.Resources.FolderTreeMenuAddCurrentQuickAccess, Command = AddQuickAccessCommand });
                    break;

                case QuickAccessNode quickAccess:
                    contextMenu.Items.Add(new MenuItem() { Header = Properties.Resources.FolderTreeMenuRemoveQuickAccess, Command = RemoveCommand });
                    break;

                case RootDirectoryNode rootFolder:
                    contextMenu.Items.Add(new MenuItem() { Header = Properties.Resources.FolderTreeMenuRefreshFolder, Command = RefreshFolderCommand });
                    break;

                case DirectoryNode folder:
                    contextMenu.Items.Add(new MenuItem() { Header = Properties.Resources.FolderTreeMenuExplorer, Command = OpenExplorerCommand });
                    contextMenu.Items.Add(new MenuItem() { Header = Properties.Resources.FolderTreeMenuAddQuickAccess, Command = AddQuickAccessCommand });
                    break;

                case RootBookmarkFolderNode rootBookmarkFolder:
                    contextMenu.Items.Add(new MenuItem() { Header = Properties.Resources.WordNewFolder, Command = NewFolderCommand });
                    contextMenu.Items.Add(new Separator());
                    contextMenu.Items.Add(new MenuItem() { Header = Properties.Resources.BookmarkMenuDeleteInvalid, Command = RemoveUnlinkedCommand });
                    break;

                case BookmarkFolderNode bookmarkFolder:
                    contextMenu.Items.Add(new MenuItem() { Header = Properties.Resources.WordRemove, Command = RemoveCommand });
                    contextMenu.Items.Add(new MenuItem() { Header = Properties.Resources.WordRename, Command = RenameCommand });
                    contextMenu.Items.Add(new Separator());
                    contextMenu.Items.Add(new MenuItem() { Header = Properties.Resources.WordNewFolder, Command = NewFolderCommand });
                    break;

                default:
                    e.Handled = true;
                    break;
            }
        }

#region DragDrop

        private DependencyObject _lastDropTarget;

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
                    e.AllowedEffects = DragDropEffects.Move;
                    break;

                case DirectoryNode direcory:
                    e.AllowedEffects = DragDropEffects.Copy | DragDropEffects.Link;
                    e.Data.SetFileDropList(new System.Collections.Specialized.StringCollection() { direcory.Path });
                    break;

                case RootBookmarkFolderNode RootbookmarkFolder:
                    e.Cancel = true;
                    break;

                case BookmarkFolderNode bookmarkFolder:
                    e.Data.SetData(bookmarkFolder.Source);
                    e.AllowedEffects = DragDropEffects.Move;
                    break;

                default:
                    e.Cancel = true;
                    break;
            }

            _lastDropTarget = null;
        }

        private void TreeView_DragEnter(object sender, DragEventArgs e)
        {
            TreeView_DragDrop(sender, e, false);
        }

        private void TreeView_DragLeave(object sender, DragEventArgs e)
        {
        }

        private void TreeView_PreviewDragOver(object sender, DragEventArgs e)
        {
            TreeView_DragDrop(sender, e, false);
        }

        private void TreeView_Drop(object sender, DragEventArgs e)
        {
            TreeView_DragDrop(sender, e, true);
        }

        private void TreeView_DragDrop(object sender, DragEventArgs e, bool isDrop)
        {
            var element = PointToViewItem(this.TreeView, e.GetPosition(this.TreeView));

            if (element is TreeViewItem viewItem)
            {
                var item = (e.Data.GetData(DragDropFormat) as TreeViewItem)?.DataContext;

                switch (viewItem.DataContext)
                {
                    case QuickAccessNode quickAccessTarget:
                        if (item is QuickAccessNode quicklAccess && quicklAccess != quickAccessTarget)
                        {
                            if (isDrop)
                            {
                                _vm.MoveQuickAccess(quicklAccess, quickAccessTarget);
                            }
                            e.Effects = DragDropEffects.Move;
                            e.Handled = true;
                            return;
                        }
                        break;

                    case BookmarkFolderNode bookmarkFolderTarget:
                        if (item is BookmarkFolderNode bookmarkFolder && bookmarkFolder != bookmarkFolderTarget)
                        {
                            if (!bookmarkFolderTarget.BookmarkSource.ParentContains(bookmarkFolder.BookmarkSource))
                            {
                                if (isDrop)
                                {
                                    BookmarkCollection.Current.MoveToChild(bookmarkFolder.BookmarkSource, bookmarkFolderTarget.BookmarkSource);
                                }
                                e.Effects = DragDropEffects.Move;
                                e.Handled = true;
                                return;
                            }
                        }
                        break;
                }

                // bookmark!
                var bookmarkEntry = (TreeListNode<IBookmarkEntry>)e.Data.GetData(typeof(TreeListNode<IBookmarkEntry>));
                if (bookmarkEntry != null)
                {
                    if (viewItem.DataContext is BookmarkFolderNode bookmarkFolderTarget)
                    {
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
                                return;
                            }
                        }

                        if (bookmarkEntry.Value is Bookmark)
                        {
                            if (isDrop)
                            {
                                BookmarkCollection.Current.MoveToChild(bookmarkEntry, bookmarkFolderTarget.BookmarkSource);
                            }
                            e.Effects = DragDropEffects.Move;
                            e.Handled = true;
                            return;
                        }
                    }
                }
            }

            e.Effects = DragDropEffects.None;
            e.Handled = true;
        }

        private TreeViewItem PointToViewItem(TreeView treeView, Point point)
        {
            var element = VisualTreeHelper.HitTest(treeView, point)?.VisualHit;

            if (!(element is TreeViewItem))
            {
                element = VisualTreeUtility.GetParentElement<TreeViewItem>(element) ?? _lastDropTarget;
            }

            _lastDropTarget = element;

            return _lastDropTarget as TreeViewItem;
        }

#endregion
    }
}
