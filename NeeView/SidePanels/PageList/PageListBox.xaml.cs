using NeeView.Collections.Generic;
using System;
using System.Collections.Generic;
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
    /// <summary>
    /// PageListBox.xaml の相互作用ロジック
    /// </summary>
    public partial class PageListBox : UserControl, IPageListPanel, IDisposable
    {
        public static string DragDropFormat = FormatVersion.CreateFormatName(Environment.ProcessId.ToString(), nameof(PageListBox));

        private PageListBoxViewModel _vm;
        private ListBoxThumbnailLoader _thumbnailLoader;
        private PageThumbnailJobClient _jobClient;

        static PageListBox()
        {
            InitializeCommandStatic();
        }

        public PageListBox()
        {
            InitializeComponent();
        }

        public PageListBox(PageListBoxViewModel vm) : this()
        {
            InitializeCommand();

            _vm = vm;
            _vm.CollectionChanged += ViewModel_CollectionChanged;

            this.DataContext = _vm;

            // タッチスクロール操作の終端挙動抑制
            this.ListBox.ManipulationBoundaryFeedback += SidePanel.Current.ScrollViewer_ManipulationBoundaryFeedback;

            this.Loaded += PageListBox_Loaded;
            this.Unloaded += PageListBox_Unloaded;
        }

        private void ViewModel_CollectionChanged(object sender, EventArgs e)
        {
            _thumbnailLoader?.Load();

            if (this.ListBox.IsFocused)
            {
                FocusSelectedItem(true);
            }
        }

        #region IDisposable Support
        private bool _disposedValue = false;

        protected virtual void Dispose(bool disposing)
        {
            if (!_disposedValue)
            {
                if (disposing)
                {
                    if (_jobClient != null)
                    {
                        _jobClient.Dispose();
                    }
                }
                _disposedValue = true;
            }
        }

        public void Dispose()
        {
            Dispose(true);
        }
        #endregion


        #region IPageListPanel support

        public ListBox PageCollectionListBox => this.ListBox;

        public bool IsThumbnailVisibled => PageList.Current.IsThumbnailVisibled;

        public IEnumerable<IHasPage> CollectPageList(IEnumerable<object> objs) => objs.OfType<IHasPage>();

        #endregion


        #region Commands

        public static readonly RoutedCommand OpenCommand = new RoutedCommand("OpenCommand", typeof(PageListBox));
        public static readonly RoutedCommand OpenBookCommand = new RoutedCommand("OpenBookCommand", typeof(PageListBox));
        public static readonly RoutedCommand CopyCommand = new RoutedCommand("CopyCommand", typeof(PageListBox));
        public static readonly RoutedCommand RemoveCommand = new RoutedCommand("RemoveCommand", typeof(PageListBox));

        private static void InitializeCommandStatic()
        {
            OpenCommand.InputGestures.Add(new KeyGesture(Key.Return));
            OpenBookCommand.InputGestures.Add(new KeyGesture(Key.Down, ModifierKeys.Alt));
            CopyCommand.InputGestures.Add(new KeyGesture(Key.C, ModifierKeys.Control));
            RemoveCommand.InputGestures.Add(new KeyGesture(Key.Delete));
        }

        private void InitializeCommand()
        {
            this.ListBox.CommandBindings.Add(new CommandBinding(OpenCommand, Open_Exec, Open_CanExec));
            this.ListBox.CommandBindings.Add(new CommandBinding(OpenBookCommand, OpenBook_Exec, OpenBook_CanExec));
            this.ListBox.CommandBindings.Add(new CommandBinding(CopyCommand, Copy_Exec, Copy_CanExec));
            this.ListBox.CommandBindings.Add(new CommandBinding(RemoveCommand, Remove_Exec, Remove_CanExec));
        }


        private void Open_CanExec(object sender, CanExecuteRoutedEventArgs e)
        {
            var page = (sender as ListBox)?.SelectedItem as Page;
            e.CanExecute = page != null;
        }

        private void Open_Exec(object sender, ExecutedRoutedEventArgs e)
        {
            var page = (sender as ListBox)?.SelectedItem as Page;
            if (page == null) return;

            _vm.Model.Jump(page);
        }

        private void OpenBook_CanExec(object sender, CanExecuteRoutedEventArgs e)
        {
            var page = (sender as ListBox)?.SelectedItem as Page;
            e.CanExecute = page != null && page.PageType == PageType.Folder;
        }

        private void OpenBook_Exec(object sender, ExecutedRoutedEventArgs e)
        {
            var page = (sender as ListBox)?.SelectedItem as Page;
            if (page == null) return;

            if (page.PageType == PageType.Folder)
            {
                BookHub.Current.RequestLoad(page.Entry.SystemPath, null, BookLoadOption.IsBook | BookLoadOption.SkipSamePlace, true);
            }
        }


        private void Copy_CanExec(object sender, CanExecuteRoutedEventArgs e)
        {
            var listBox = (ListBox)sender;
            if (listBox.SelectedItems == null)
            {
                e.CanExecute = false;
            }
            else
            {
                e.CanExecute = listBox.SelectedItems.Count > 0;
            }
        }

        private void Copy_Exec(object sender, ExecutedRoutedEventArgs e)
        {
            var listBox = (ListBox)sender;
            if (listBox.SelectedItems != null && listBox.SelectedItems.Count > 0)
            {
                try
                {
                    App.Current.MainWindow.Cursor = Cursors.Wait;
                    _vm.Model.Copy(listBox.SelectedItems.Cast<Page>().ToList());
                }
                finally
                {
                    App.Current.MainWindow.Cursor = null;
                }
            }
        }


        private void Remove_CanExec(object sender, CanExecuteRoutedEventArgs e)
        {
            var listBox = (ListBox)sender;
            if (listBox.SelectedItems == null)
            {
                e.CanExecute = false;
            }
            else
            {
                e.CanExecute = listBox.SelectedItems.Cast<Page>().All(x => _vm.Model.CanRemove(x));
            }
        }

        private async void Remove_Exec(object sender, ExecutedRoutedEventArgs e)
        {
            var listBox = (ListBox)sender;
            if (listBox.SelectedItems != null && listBox.SelectedItems.Count > 0)
            {
                await _vm.Model.RemoveAsync(listBox.SelectedItems.Cast<Page>().ToList());
            }
        }

        #endregion


        private void PageListBox_Loaded(object sender, RoutedEventArgs e)
        {
            _vm.Loaded();
            _vm.ViewItemsChanged += ViewModel_ViewItemsChanged;

            _jobClient = new PageThumbnailJobClient("PageList", JobCategories.PageThumbnailCategory);
            _thumbnailLoader = new ListBoxThumbnailLoader(this, _jobClient);
            _thumbnailLoader.Load();

            Config.Current.Panels.ContentItemProfile.PropertyChanged += PanelListtemProfile_PropertyChanged;
            Config.Current.Panels.BannerItemProfile.PropertyChanged += PanelListtemProfile_PropertyChanged;
            Config.Current.Panels.ThumbnailItemProfile.PropertyChanged += PanelListtemProfile_PropertyChanged;

            FocusSelectedItem(false);
        }

        private void PageListBox_Unloaded(object sender, RoutedEventArgs e)
        {
            _vm.Unloaded();
            _vm.ViewItemsChanged -= ViewModel_ViewItemsChanged;

            Config.Current.Panels.ContentItemProfile.PropertyChanged -= PanelListtemProfile_PropertyChanged;
            Config.Current.Panels.BannerItemProfile.PropertyChanged -= PanelListtemProfile_PropertyChanged;
            Config.Current.Panels.ThumbnailItemProfile.PropertyChanged -= PanelListtemProfile_PropertyChanged;

            _jobClient?.Dispose();
        }

        private void PanelListtemProfile_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            this.ListBox.Items?.Refresh();
        }

        private void ViewModel_ViewItemsChanged(object sender, ViewItemsChangedEventArgs e)
        {
            UpdateViewItems(e.ViewItems, e.Direction);
        }

        private void UpdateViewItems()
        {
            if (_vm.Model.ViewItems == null) return;

            UpdateViewItems(_vm.Model.ViewItems, 0);
        }

        private void UpdateViewItems(List<Page> items, int direction)
        {
            if (!this.ListBox.IsLoaded) return;
            if (_vm.Model.PageCollection == null) return;
            if (!this.IsVisible) return;

            if (items.Count == 1)
            {
                ScrollIntoView(items.First());
            }
            else if (direction < 0)
            {
                ScrollIntoView(items.First());
            }
            else if (direction > 0)
            {
                ScrollIntoView(items.Last());
            }
            else
            {
                foreach (var item in items)
                {
                    ScrollIntoView(item);
                    this.ListBox.UpdateLayout();
                }
            }
        }

        private void ScrollIntoView(object item)
        {
            ////Debug.WriteLine($"PL:ScrollIntoView: {item}");
            this.ListBox.ScrollIntoView(item);
        }

        public void FocusSelectedItem(bool isForce)
        {
            if (this.ListBox.SelectedIndex < 0) return;

            UpdateViewItems();

            if (isForce || _vm.FocusAtOnce)
            {
                _vm.FocusAtOnce = false;
                ListBoxItem lbi = (ListBoxItem)(this.ListBox.ItemContainerGenerator.ContainerFromIndex(this.ListBox.SelectedIndex));
                lbi?.Focus();
            }
        }

        // 選択項目変更
        private void PageList_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            _vm.Model.SelectedItems = this.ListBox.SelectedItems.Cast<Page>().ToList();
        }

        // リストのキ入力
        private void PageList_KeyDown(object sender, KeyEventArgs e)
        {
            var page = this.ListBox.SelectedItem as Page;

            if (Keyboard.Modifiers == ModifierKeys.Alt)
            {
                Key key = e.Key == Key.System ? e.SystemKey : e.Key;

                if (key == Key.Up)
                {
                    // 現在ブックの上の階層に移動
                    BookHub.Current.RequestLoadParent();
                    e.Handled = true;
                }
                else if (key == Key.Down)
                {
                    // 選択ブックに移動
                    if (page != null && page.PageType == PageType.Folder)
                    {
                        BookHub.Current.RequestLoad(page.Entry.SystemPath, null, BookLoadOption.IsBook | BookLoadOption.SkipSamePlace, true);
                    }
                    e.Handled = true;
                }
                else if (key == Key.Left)
                {
                    // 直前のページに移動
                    PageHistory.Current.MoveToPrevious();
                    e.Handled = true;
                }
                else if (key == Key.Right)
                {
                    // 直後のページに移動
                    PageHistory.Current.MoveToNext();
                    e.Handled = true;
                }
            }
            else if (Keyboard.Modifiers == ModifierKeys.None)
            {
                if (e.Key == Key.Return)
                {
                    // 項目決定
                    _vm.Model.Jump(page);
                    e.Handled = true;
                }
                if (e.Key == Key.Back)
                {
                    // 直前のページに移動
                    PageHistory.Current.MoveToPrevious();
                    e.Handled = true;
                }
            }

            var isLRKeyEnabled = Config.Current.Panels.IsLeftRightKeyEnabled;

            // このパネルで使用するキーのイベントを止める
            if (Keyboard.Modifiers == ModifierKeys.None)
            {
                if (e.Key == Key.Up || e.Key == Key.Down || (isLRKeyEnabled && (e.Key == Key.Left || e.Key == Key.Right)) || e.Key == Key.Return || e.Key == Key.Delete)
                {
                    e.Handled = true;
                }
            }
        }

        private async void PaegList_IsVisibleChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            if ((bool)e.NewValue)
            {
                await Task.Yield();
                FocusSelectedItem(false);
            }
        }

        private void PageList_TargetUpdated(object sender, DataTransferEventArgs e)
        {
            UpdateViewItems();
        }


        // 項目クリック
        private void PageListItem_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            var page = (sender as ListBoxItem)?.Content as Page;
            if (page != null)
            {
                _vm.Model.Jump(page);
            }
        }

        // 項目ダブルクリック
        private void PageListItem_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            var page = (sender as ListBoxItem)?.Content as Page;
            if (page != null && page.PageType == PageType.Folder)
            {
                BookHub.Current.RequestLoad(page.Entry.SystemPath, null, BookLoadOption.IsBook | BookLoadOption.SkipSamePlace, true);
                e.Handled = true;
            }
        }


        private void PageListItem_ContextMenuOpening(object sender, ContextMenuEventArgs e)
        {
            var container = sender as ListBoxItem;
            if (container == null)
            {
                return;
            }

            var item = container.Content as Page;
            if (item == null)
            {
                return;
            }

            var contextMenu = container.ContextMenu;
            if (contextMenu == null)
            {
                return;
            }

            contextMenu.Items.Clear();

            if (item.PageType == PageType.Folder)
            {
                contextMenu.Items.Add(new MenuItem() { Header = Properties.Resources.PagelistItemMenuOpenBook, Command = OpenBookCommand });
                contextMenu.Items.Add(new Separator());
            }

            contextMenu.Items.Add(new MenuItem() { Header = Properties.Resources.PagelistItemMenuOpen, Command = OpenCommand });
            contextMenu.Items.Add(new Separator());
            contextMenu.Items.Add(new MenuItem() { Header = Properties.Resources.PageListItemMenuCopy, Command = CopyCommand });
            contextMenu.Items.Add(new MenuItem() { Header = Properties.Resources.PageListItemMenuDelete, Command = RemoveCommand });
        }


        #region DragDrop

        private async Task DragStartBehavior_DragBeginAsync(object sender, Windows.DragStartEventArgs e, CancellationToken token)
        {
            var pages = this.ListBox.SelectedItems.Cast<Page>().ToList();
            if (!pages.Any())
            {
                e.Cancel = true;
                return;
            }

            await Task.Run(() => ClipboardUtility.SetData(e.Data, pages, new CopyFileCommandParameter() { MultiPagePolicy = MultiPagePolicy.All }, token));

            e.AllowedEffects = DragDropEffects.Copy | DragDropEffects.Scroll;
        }

        #endregion

    }


    /// <summary>
    /// Page,PageNameFormat から表示ページ名を取得
    /// </summary>
    public class PageNameFormatConverter : IMultiValueConverter
    {
        public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
        {
            if (values[0] is Page page && values[1] is PageNameFormat format)
            {
                switch (format)
                {
                    default:
                    case PageNameFormat.Raw:
                        return page.EntryFullName;
                    case PageNameFormat.Smart:
                        return page.GetSmartFullName();
                    case PageNameFormat.NameOnly:
                        return page.EntryLastName;
                }
            }
            return null;
        }

        public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }

    public class PageToNoteConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is Page page)
            {
                if (page.LastWriteTime == default && page.Length == 0) return null;

                var timeString = $"{page.LastWriteTime:yyyy/MM/dd HH:mm:ss}";
                var sizeString = FileSizeToStringConverter.ByteToDispString(page.Length);
                return timeString + (string.IsNullOrEmpty(sizeString) ? "" : "   " + sizeString);
            }

            return value;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }

    /// <summary>
    /// ArchivePageなら表示
    /// </summary>
    public class ArchviePageToVisibilityConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return (value is Page page && page.PageType == PageType.Folder) ? Visibility.Visible : Visibility.Collapsed;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
