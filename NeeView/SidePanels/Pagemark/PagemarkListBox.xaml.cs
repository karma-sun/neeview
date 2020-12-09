using NeeView.Windows.Media;
using NeeView.Collections;
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
    public class PagemarkListVertualCollection : VirtualCollection<TreeViewItem, IPagemarkEntry>
    {
        public static PagemarkListVertualCollection Current { get; private set; }

        public PagemarkListVertualCollection(TreeView treeView) : base(treeView)
        {
        }

        public static void SetCurrent(PagemarkListVertualCollection current)
        {
            Current = current;
        }
    }

    /// <summary>
    /// PagemarkListBox.xaml の相互作用ロジック
    /// </summary>
    public partial class PagemarkListBox : UserControl, IDisposable
    {
        public static readonly string DragDropFormat = FormatVersion.CreateFormatName(Environment.ProcessId.ToString(), nameof(PagemarkListBox));


        #region Fields

        private PagemarkListBoxViewModel _vm;
        private PagemarkListVertualCollection _virtualCollection;
        private PageThumbnailJobClient _jobClient;
        private bool _focusRequest;

        #endregion

        #region Constructors

        static PagemarkListBox()
        {
            InitializeCommandStatic();
        }

        public PagemarkListBox()
        {
            InitializeComponent();
        }

        public PagemarkListBox(PagemarkListBoxViewModel vm) : this()
        {
            _vm = vm;
            this.DataContext = _vm;

            InitializeCommand();

            _virtualCollection = new PagemarkListVertualCollection(this.TreeView);
            PagemarkListVertualCollection.SetCurrent(_virtualCollection);

            _jobClient = new PageThumbnailJobClient("PagemarkList", JobCategories.BookThumbnailCategory);
            _virtualCollection.CollectionChanged += VirtualCollection_CollectionChanged;

            // タッチスクロール操作の終端挙動抑制
            this.TreeView.ManipulationBoundaryFeedback += SidePanelFrame.Current.ScrollViewer_ManipulationBoundaryFeedback;

            this.TreeView.AddHandler(ScrollViewer.ScrollChangedEvent, new ScrollChangedEventHandler(TreeView_ScrollChanged));

            this.Loaded += PagemarkListBox_Loaded;
            this.Unloaded += PagemarkListBox_Unloaded;

            _vm.Model.SelectedItemChanged += (s, e) =>
            {
                ScrollIntoView();
            };
        }

        #endregion

        #region Commands

        public static readonly RoutedCommand OpenCommand = new RoutedCommand("OpenCommand", typeof(PagemarkListBox));
        public static readonly RoutedCommand RemoveCommand = new RoutedCommand("RemoveCommand", typeof(PagemarkListBox));
        public static readonly RoutedCommand RenameCommand = new RoutedCommand("RenameCommand", typeof(PagemarkListBox));

        private static void InitializeCommandStatic()
        {
            OpenCommand.InputGestures.Add(new KeyGesture(Key.Enter));
            RemoveCommand.InputGestures.Add(new KeyGesture(Key.Delete));
            RenameCommand.InputGestures.Add(new KeyGesture(Key.F2));
        }

        public void InitializeCommand()
        {
            this.TreeView.CommandBindings.Add(new CommandBinding(OpenCommand, Open_Exec));
            this.TreeView.CommandBindings.Add(new CommandBinding(RemoveCommand, Remove_Exec));
            this.TreeView.CommandBindings.Add(new CommandBinding(RenameCommand, Rename_Executed));
        }


        public void Open_Exec(object sender, ExecutedRoutedEventArgs e)
        {
            var item = (sender as TreeView)?.SelectedItem as TreeListNode<IPagemarkEntry>;
            if (item != null)
            {
                if (item.Value is PagemarkFolder)
                {
                    item.IsExpanded = !item.IsExpanded;
                }
                else
                {
                    _vm.Decide(item, true);
                }
            }
        }

        public void Remove_Exec(object sender, ExecutedRoutedEventArgs e)
        {
            var item = (sender as TreeView)?.SelectedItem as TreeListNode<IPagemarkEntry>;
            if (item != null)
            {
                _vm.Remove(item);
            }
        }

        public void Rename_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            var item = (sender as TreeView)?.SelectedItem as TreeListNode<IPagemarkEntry>;
            Rename(item);
        }

        #endregion

        #region Methods

        private void VirtualCollection_CollectionChanged(object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
        {
            RequestLoadThumbnail();
        }

        private void RequestLoadThumbnail()
        {
            var pages = _virtualCollection.Items
                .Cast<Pagemark>()
                .Select(a => a.GetPage())
                .ToList();

            ////Debug.WriteLine($"Pagemark.Thumbnail: " + string.Join(",", pages.Select(a => a.ToString())));
            _jobClient.Order(pages);
        }

        private void CancelLoadTumbnail()
        {
            _jobClient.CancelOrder();
        }


        private void Rename(TreeListNode<IPagemarkEntry> item)
        {
            var treetView = this.TreeView;
            if (item != null && item.Value is Pagemark pagemark)
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
                            bool isRenamed = _vm.Model.Rename(item, ev.NewValue);
                            ev.Cancel = !isRenamed;
                        }
                    };
                    rename.Closed += (s, ev) =>
                    {
                        this.TreeView.Focus();
                    };
                    rename.Close += (s, ev) =>
                    {
                    };

                    ((MainWindow)Application.Current.MainWindow).RenameManager.Open(rename);
                }
            }
        }

        private void ScrollIntoViewX()
        {
            if (!this.TreeView.IsVisible)
            {
                return;
            }

            var index = _vm.Model.IndexOfSelectedItem();
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

        private void ScrollIntoView(TreeListNode<IPagemarkEntry> entry)
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

        #endregion

        #region Event Methods

        private void PagemarkListBox_Loaded(object sender, RoutedEventArgs e)
        {
            _vm.Loaded();

            Config.Current.Panels.ContentItemProfile.PropertyChanged += PanelListtemProfile_PropertyChanged;
            Config.Current.Panels.BannerItemProfile.PropertyChanged += PanelListtemProfile_PropertyChanged;
            Config.Current.Panels.ThumbnailItemProfile.PropertyChanged += PanelListtemProfile_PropertyChanged;
        }

        private void PagemarkListBox_Unloaded(object sender, RoutedEventArgs e)
        {
            Config.Current.Panels.ContentItemProfile.PropertyChanged -= PanelListtemProfile_PropertyChanged;
            Config.Current.Panels.BannerItemProfile.PropertyChanged -= PanelListtemProfile_PropertyChanged;
            Config.Current.Panels.ThumbnailItemProfile.PropertyChanged -= PanelListtemProfile_PropertyChanged;

            _vm.Unloaded();
        }

        private void PanelListtemProfile_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            this.TreeView.Items?.Refresh();
        }

        // 表示/非表示イベント
        private async void TreeView_IsVisibleChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            if (e.NewValue as bool? == true)
            {
                if (_vm.Model.SelectedItem == null)
                {
                    var item = _vm.Model.PagemarkCollection.Items.FirstOrDefault();
                    if (item != null)
                    {
                        _vm.Model.SelectedItem = item;
                    }
                }

                await Task.Yield();
                ScrollIntoView();
                if (_focusRequest)
                {
                    _focusRequest = false;
                    this.TreeView.Focus();
                }
            }
            else
            {
                CancelLoadTumbnail();
            }

            _virtualCollection.CleanUp();
        }


        private void TreeView_ScrollChanged(object sender, ScrollChangedEventArgs e)
        {
            ((MainWindow)App.Current.MainWindow).RenameManager.Stop();

            PagemarkListVertualCollection.Current.Refresh();

            _virtualCollection.CleanUp();
        }

        private void TreeView_SelectedItemChanged(object sender, RoutedPropertyChangedEventArgs<object> e)
        {
            _vm.Model.SelectedItem = TreeView.SelectedItem as TreeListNode<IPagemarkEntry>;
        }

        private void TreeVew_KeyDown(object sender, KeyEventArgs e)
        {
            bool isLRKeyEnabled = Config.Current.Panels.IsLeftRightKeyEnabled;

            // このパネルで使用するキーのイベントを止める
            if (Keyboard.Modifiers == ModifierKeys.None)
            {
                if (e.Key == Key.Up || e.Key == Key.Down || (isLRKeyEnabled && (e.Key == Key.Left || e.Key == Key.Right)) || e.Key == Key.Return || e.Key == Key.Delete)
                {
                    e.Handled = true;
                }
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

            if (!container.IsSelected)
            {
                return;
            }

            var item = container.DataContext as TreeListNode<IPagemarkEntry>;
            if (item == null)
            {
                e.Handled = true;
                return;
            }

            var contextMenu = container.ContextMenu;
            contextMenu.Items.Clear();

            switch (item.Value)
            {
                case Pagemark pagemark:
                    contextMenu.Items.Add(new MenuItem() { Header = Properties.Resources.PagemarkItemMenuOpen, Command = OpenCommand });
                    contextMenu.Items.Add(new Separator());
                    contextMenu.Items.Add(new MenuItem() { Header = Properties.Resources.PagemarkItemMenuDelete, Command = RemoveCommand });
                    contextMenu.Items.Add(new MenuItem() { Header = Properties.Resources.PagemarkItemMenuRename, Command = RenameCommand });
                    break;

                case PagemarkFolder folder:
                    contextMenu.Items.Add(new MenuItem() { Header = Properties.Resources.PagemarkItemMenuOpen, Command = OpenCommand });
                    contextMenu.Items.Add(new Separator());
                    contextMenu.Items.Add(new MenuItem() { Header = Properties.Resources.PagemarkItemMenuDeleteFolder, Command = RemoveCommand });
                    break;
            }
        }

        private void TreeViewItem_MouseRightButtonDown(object sender, MouseButtonEventArgs e)
        {
            var item = (sender as TreeViewItem)?.DataContext as TreeListNode<IPagemarkEntry>;
            if (item != null)
            {
                _vm.Model.SelectedItem = item;
                e.Handled = true;
            }
        }


        // 履歴項目決定
        private void TreeViewItem_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            var item = (sender as TreeViewItem)?.DataContext as TreeListNode<IPagemarkEntry>;
            if (item != null)
            {
                _vm.Decide(item, true);
            }
        }

        private void TreeViewItem_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            // nop.
        }


        // 履歴項目決定(キー)
        private void TreeViewItem_KeyDown(object sender, KeyEventArgs e)
        {
            var item = (sender as TreeViewItem)?.DataContext as TreeListNode<IPagemarkEntry>;

            if (Keyboard.Modifiers == ModifierKeys.None)
            {
                if (e.Key == Key.Return)
                {
                    if (item.Value is PagemarkFolder)
                    {
                        item.IsExpanded = !item.IsExpanded;
                    }
                    else
                    {
                        _vm.Decide(item, true);
                    }
                    e.Handled = true;
                }
            }
        }

        public void Refresh()
        {
            this.TreeView.Items.Refresh();
        }

        public void FocusAtOnce()
        {
            bool focused = this.TreeView.Focus();
            if (!focused)
            {
                _focusRequest = true;
            }
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
                    _jobClient.Dispose();
                }

                _disposedValue = true;
            }
        }

        public void Dispose()
        {
            Dispose(true);
        }

        #endregion

        #region DragDrop

        private async Task DragStartBehavior_DragBeginAsync(object sender, DragStartEventArgs e, CancellationToken token)
        {
            var data = e.DragItem as TreeViewItem;
            if (data == null)
            {
                e.Cancel = true;
                return;
            }

            var node = data.Header as TreeListNode<IPagemarkEntry>;
            if (node == null)
            {
                e.Cancel = true;
                return;
            }

            var pagemark = node.Value as Pagemark;
            if (pagemark == null)
            {
                e.Cancel = true;
                return;
            }

            var item = pagemark.GetPage();
            if (item == null)
            {
                e.Cancel = true;
                return;
            }

            await item.InitializeEntryAsync(token);

            await Task.Run(() => ClipboardUtility.SetData(e.Data, new List<Page>() { item }, new CopyFileCommandParameter(), token));

            e.AllowedEffects = DragDropEffects.Copy | DragDropEffects.Scroll;
        }

        #endregion


        #region UI Accessor

        public List<TreeListNode<IPagemarkEntry>> GetItems()
        {
            return GetItemsEnumerator().ToList();
        }

        public List<TreeListNode<IPagemarkEntry>> GetExpandedItems()
        {
            return GetExpandedItemsEnumerator().ToList();
        }


        public List<TreeListNode<IPagemarkEntry>> GetSelectedItems()
        {
            return GetExpandedItemsEnumerator().Where(e => e.IsSelected).ToList();
        }

        public void SetSelectedItems(IEnumerable<TreeListNode<IPagemarkEntry>> selectedItems)
        {
            foreach (var item in GetExpandedItemsEnumerator())
            {
                item.IsSelected = false;
            }

            if (selectedItems == null) return;

            foreach (var item in selectedItems.Intersect(GetExpandedItems()))
            {
                item.IsSelected = true;
            }
        }

        private IEnumerable<TreeListNode<IPagemarkEntry>> GetItemsEnumerator()
        {
            var nodes = this.TreeView.Items?.Cast<TreeListNode<IPagemarkEntry>>();

            foreach (var node in nodes)
            {
                yield return node;
                foreach (var child in node)
                {
                    yield return child;
                }
            }
        }

        private IEnumerable<TreeListNode<IPagemarkEntry>> GetExpandedItemsEnumerator()
        {
            var nodes = this.TreeView.Items?.Cast<TreeListNode<IPagemarkEntry>>();

            foreach (var node in nodes)
            {
                yield return node;
                if (node.IsExpanded)
                {
                    foreach (var child in node.GetExpandedCollection())
                    {
                        yield return child;
                    }
                }
            }
        }

        #endregion UI Accessor
    }



    public class DepthToWidthConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is int depth && depth > 0)
            {
                return (depth - 1) * 32;
            }
            else
            {
                return 0;
            }
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }

    public class PagemarkNodeToNote : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is TreeListNode<IPagemarkEntry> node)
            {
                if (node.Value is PagemarkFolder folder)
                {
                    var directory = LoosePath.GetDirectoryName(folder.Path);
                    return SidePanelProfile.Current.GetDecoratePlaceName(directory);
                }
                else
                {
                    return "";
                }
            }

            return DependencyProperty.UnsetValue;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }

}
