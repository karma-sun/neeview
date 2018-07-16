using NeeLaboratory.Windows.Media;
using NeeView.Collections.Generic;
using NeeView.Windows;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace NeeView
{
    public class BookmarkListVertualCollection : VirtualCollection<TreeViewItem, IBookmarkEntry>
    {
        public static BookmarkListVertualCollection Current { get; private set; }

        public BookmarkListVertualCollection(TreeView treeView) : base(treeView)
        {
        }

        public static void SetCurrent(BookmarkListVertualCollection current)
        {
            Current = current;
        }
    }


    /// <summary>
    /// BookmarkListBox.xaml の相互作用ロジック
    /// </summary>
    public partial class BookmarkListBox : UserControl, IDisposable
    {
        #region Fields

        public static string DragDropFormat = $"{Config.Current.ProcessId}.BookmarkItem";

        private BookmarkListBoxViewModel _vm;
        private BookmarkListVertualCollection _virtualCollection;

        #endregion Fields

        #region Constructors

        static BookmarkListBox()
        {
            InitializeCommandStatic();
        }

        public BookmarkListBox()
        {
            InitializeComponent();
        }

        public BookmarkListBox(BookmarkListBoxViewModel vm) : this()
        {
            _vm = vm;
            this.DataContext = vm;

            InitialieCommand();

            _virtualCollection = new BookmarkListVertualCollection(this.TreeView);
            BookmarkListVertualCollection.SetCurrent(_virtualCollection);

            // タッチスクロール操作の終端挙動抑制
            this.TreeView.ManipulationBoundaryFeedback += SidePanel.Current.ScrollViewer_ManipulationBoundaryFeedback;

            this.TreeView.AddHandler(ScrollViewer.ScrollChangedEvent, new ScrollChangedEventHandler(TreeView_ScrollChanged));

            this.Loaded += BookmarkListBox_Loaded;
            this.Unloaded += BookmarkListBox_Unloaded;

            _vm.Model.SelectedItemChanged += (s, e) =>
            {
                ScrollIntoView();
            };
        }

        #endregion Constructors

        #region Properties

        public bool IsRenaming { get; private set; }

        #endregion

        #region Commands

        private static void InitializeCommandStatic()
        {
            RemoveCommand.InputGestures.Add(new KeyGesture(Key.Delete));
            RenameCommand.InputGestures.Add(new KeyGesture(Key.F2));
        }

        private void InitialieCommand()
        {
            this.TreeView.CommandBindings.Add(new CommandBinding(RemoveCommand, Remove_Exec));
            this.TreeView.CommandBindings.Add(new CommandBinding(RenameCommand, Rename_Executed));
        }

        public static readonly RoutedCommand RemoveCommand = new RoutedCommand("RemoveCommand", typeof(BookmarkListBox));

        public void Remove_Exec(object sender, ExecutedRoutedEventArgs e)
        {
            var item = (sender as TreeView)?.SelectedItem as TreeListNode<IBookmarkEntry>;
            if (item != null)
            {
                _vm.Remove(item);
            }
        }

        public static readonly RoutedCommand RenameCommand = new RoutedCommand("RenameCommand", typeof(BookmarkListBox));

        public void Rename_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            var item = (sender as TreeView)?.SelectedItem as TreeListNode<IBookmarkEntry>;
            Rename(item);
        }

        #endregion Commands

        #region Methods

        private void BookmarkCollection_Changed(object sender, CollectionChangeEventArgs e)
        {
            // if new folder, enter rename mode.
            if (e.Action == CollectionChangeAction.Add)
            {
                if (e.Element is TreeListNode<IBookmarkEntry> node)
                {
                    if (node.Value is BookmarkFolder)
                    {
                        Dispatcher.BeginInvoke((Action)(() =>
                        {
                            var scrollViewer = VisualTreeUtility.GetChildElement<ScrollViewer>(this.TreeView);
                            if (scrollViewer != null)
                            {
                                scrollViewer.ScrollToVerticalOffset(0);
                            }

                            this.TreeView.UpdateLayout();
                            Rename(node);
                        }));
                    }
                }
            }
        }

        private void Rename(TreeListNode<IBookmarkEntry> item)
        {
            var treetView = this.TreeView;

            if (item != null && item.Value is BookmarkFolder folder)
            {
                var listViewItem = VisualTreeUtility.FindContainer<TreeViewItem>(treetView, item);
                var textBlock = VisualTreeUtility.FindVisualChild<TextBlock>(listViewItem, "FileNameTextBlock");

                if (textBlock != null)
                {
                    var rename = new RenameControl() { Target = textBlock };
                    rename.Closing += (s, ev) =>
                    {
                        if (ev.OldValue != ev.NewValue)
                        {
                            folder.Name = ev.NewValue;
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

                    ((MainWindow)Application.Current.MainWindow).RenameManager.Open(rename);
                    IsRenaming = true;
                }
            }
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

            ItemsControl container = this.TreeView;
            foreach (var parent in selectedItem.Hierarchy.Skip(1))
            {
                ScrollIntoView(parent);
                parent.IsExpanded = true;
                this.TreeView.UpdateLayout();
            }
        }

        private void ScrollIntoView(TreeListNode<IBookmarkEntry> entry)
        {
            if (!this.TreeView.IsVisible)
            {
                return;
            }

            var index = _vm.Model.IndexOfExpanded(entry);
            if (index < 0)
            {
                return;
            }

            var item = VisualTreeUtility.FindVisualChild<TreeViewItem>(this.TreeView);
            var header = VisualTreeUtility.FindVisualChild<Border>(item, "Bd");

            if (header != null)
            {
                var unitHeight = header.ActualHeight;
                var scrollVerticalOffset = unitHeight * index;

                var scrollViewer = VisualTreeUtility.GetChildElement<ScrollViewer>(this.TreeView);
                if (scrollViewer != null)
                {
                    if (scrollVerticalOffset - scrollViewer.ActualHeight + unitHeight > scrollViewer.VerticalOffset)
                    {
                        scrollViewer.ScrollToVerticalOffset(scrollVerticalOffset - scrollViewer.ActualHeight + unitHeight);
                    }
                    else if (scrollVerticalOffset < scrollViewer.VerticalOffset)
                    {
                        scrollViewer.ScrollToVerticalOffset(scrollVerticalOffset);
                    }
                }
            }
        }

        #endregion Methods

        #region DragDrop

        private TreeViewItem _dropTarget;

        private void DragStartBehavior_DragBegin(object sender, MouseEventArgs e)
        {
            _dropTarget = null;
        }

        private void TreeView_DragEnter(object sender, DragEventArgs e)
        {
            var element = e.OriginalSource as DependencyObject;
            var item = VisualTreeUtility.GetParentElement<TreeViewItem>(element);
            if (item != null)
            {
                if (_dropTarget != item)
                {
                    var node = item.DataContext as TreeListNode<IBookmarkEntry>;
                }
                _dropTarget = item;
            }
        }

        private void TreeView_PreviewDragOver(object sender, DragEventArgs e)
        {
            e.Effects = _dropTarget != null ? DragDropEffects.Move : DragDropEffects.None;
        }


        private void TreeView_Drop(object sender, DragEventArgs e)
        {
            if (!(_dropTarget?.DataContext is TreeListNode<IBookmarkEntry> dropTarget))
            {
                return;
            }

            var dropTargetVisual = VisualTreeUtility.FindVisualChild<Border>(_dropTarget, "Bd");

            var dropInfo = new DropInfo<TreeListNode<IBookmarkEntry>>(e, DragDropFormat, dropTarget, dropTargetVisual);
            if (!dropInfo.IsValid())
            {
                return;
            }

            _vm.Move(dropInfo);
            e.Handled = true;
        }

        #endregion DragDrop

        #region Event Methods

        private void BookmarkListBox_Loaded(object sender, RoutedEventArgs e)
        {
            _vm.Changed += BookmarkCollection_Changed;
            _vm.Loaded();
        }

        private void BookmarkListBox_Unloaded(object sender, RoutedEventArgs e)
        {
            _vm.Changed -= BookmarkCollection_Changed;
            _vm.Unloaded();
        }

        // 表示/非表示イベント
        private async void TreeView_IsVisibleChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            if (e.NewValue as bool? == true)
            {
                if (_vm.Model.SelectedItem == null)
                {
                    var item = _vm.Model.BookmarkCollection.Items.FirstOrDefault();
                    if (item != null)
                    {
                        _vm.Model.SelectedItem = item;
                    }
                }

                await Task.Yield();
                ScrollIntoView();
                this.TreeView.Focus();
            }
        }

        private void TreeView_ScrollChanged(object sender, ScrollChangedEventArgs e)
        {
            ((MainWindow)App.Current.MainWindow).RenameManager.Stop();

            BookmarkListVertualCollection.Current.Refresh();
        }

        private void TreeView_SelectedItemChanged(object sender, RoutedPropertyChangedEventArgs<object> e)
        {
            _vm.Model.SelectedItem = TreeView.SelectedItem as TreeListNode<IBookmarkEntry>;
        }

        private void TreeVew_KeyDown(object sender, KeyEventArgs e)
        {
            bool isLRKeyEnabled = SidePanelProfile.Current.IsLeftRightKeyEnabled;

            // このパネルで使用するキーのイベントを止める
            if (e.Key == Key.Up || e.Key == Key.Down || (isLRKeyEnabled && (e.Key == Key.Left || e.Key == Key.Right)) || e.Key == Key.Return || e.Key == Key.Delete)
            {
                e.Handled = true;
            }
        }

        private void TreeViewItem_Selected(object sender, RoutedEventArgs e)
        {
            // nop.
        }

        private void TreeViewItem_ContextMenuOpening(object sender, ContextMenuEventArgs e)
        {
            var container = sender as TreeViewItem;
            if (container == null)
            {
                return;
            }

            var item = container.DataContext as TreeListNode<IBookmarkEntry>;
            if (item == null)
            {
                e.Handled = true;
                return;
            }

            var contextMenu = container.ContextMenu;
            contextMenu.Items.Clear();

            switch (item.Value)
            {
                case Bookmark bookmark:
                    contextMenu.Items.Add(new MenuItem() { Header = Properties.Resources.BookmarkItemMenuDelete, Command = RemoveCommand });
                    break;

                case BookmarkFolder folder:
                    contextMenu.Items.Add(new MenuItem() { Header = Properties.Resources.BookmarkItemMenuDeleteFolder, Command = RemoveCommand });
                    contextMenu.Items.Add(new MenuItem() { Header = Properties.Resources.BookmarkItemMenuRename, Command = RenameCommand });
                    break;
            }
        }

        private void TreeViewItem_MouseRightButtonDown(object sender, MouseButtonEventArgs e)
        {
            var item = (sender as TreeViewItem)?.DataContext as TreeListNode<IBookmarkEntry>;
            if (item != null)
            {
                _vm.Model.SelectedItem = item;
                e.Handled = true;
            }
        }

        // 履歴項目決定
        private void TreeViewItem_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            var item = (sender as TreeViewItem)?.DataContext as TreeListNode<IBookmarkEntry>;
            if (item != null)
            {
                _vm.Decide(item);
                e.Handled = true;
            }
        }

        // 履歴項目決定(キー)
        private void TreeViewItem_KeyDown(object sender, KeyEventArgs e)
        {
            var item = (sender as TreeViewItem)?.DataContext as TreeListNode<IBookmarkEntry>;
            {
                if (e.Key == Key.Return)
                {
                    _vm.Decide(item);
                    e.Handled = true;
                }
            }
        }

        #endregion Event Methods

        #region IDisposable Support
        private bool _disposedValue = false;

        protected virtual void Dispose(bool disposing)
        {
            if (!_disposedValue)
            {
                if (disposing)
                {
                    _virtualCollection.Dispose();
                }

                _disposedValue = true;
            }
        }

        public void Dispose()
        {
            Dispose(true);
        }

        #endregion
    }
}
