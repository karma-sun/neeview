using NeeView.Collections.Generic;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Globalization;
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
            this.ListBox.ManipulationBoundaryFeedback += SidePanelFrame.Current.ScrollViewer_ManipulationBoundaryFeedback;

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

        public static readonly RoutedCommand OpenCommand = new RoutedCommand(nameof(OpenCommand), typeof(PageListBox));
        public static readonly RoutedCommand OpenBookCommand = new RoutedCommand(nameof(OpenBookCommand), typeof(PageListBox));
        public static readonly RoutedCommand OpenExplorerCommand = new RoutedCommand(nameof(OpenExplorerCommand), typeof(PageListBox));
        public static readonly RoutedCommand OpenExternalAppCommand = new RoutedCommand(nameof(OpenExternalAppCommand), typeof(PageListBox));
        public static readonly RoutedCommand CopyCommand = new RoutedCommand(nameof(CopyCommand), typeof(PageListBox));
        public static readonly RoutedCommand CopyToFolderCommand = new RoutedCommand(nameof(CopyToFolderCommand), typeof(PageListBox));
        public static readonly RoutedCommand MoveToFolderCommand = new RoutedCommand(nameof(MoveToFolderCommand), typeof(PageListBox));
        public static readonly RoutedCommand RemoveCommand = new RoutedCommand(nameof(RemoveCommand), typeof(PageListBox));
        public static readonly RoutedCommand OpenDestinationFolderCommand = new RoutedCommand(nameof(OpenDestinationFolderCommand), typeof(PageListBox));
        public static readonly RoutedCommand OpenExternalAppDialogCommand = new RoutedCommand(nameof(OpenExternalAppDialogCommand), typeof(PageListBox));
        public static readonly RoutedCommand PagemarkCommand = new RoutedCommand(nameof(PagemarkCommand), typeof(PageListBox));

        private PageCommandResource _commandResource = new PageCommandResource();

        private static void InitializeCommandStatic()
        {
            OpenCommand.InputGestures.Add(new KeyGesture(Key.Return));
            OpenBookCommand.InputGestures.Add(new KeyGesture(Key.Down, ModifierKeys.Alt));
            CopyCommand.InputGestures.Add(new KeyGesture(Key.C, ModifierKeys.Control));
            RemoveCommand.InputGestures.Add(new KeyGesture(Key.Delete));
            PagemarkCommand.InputGestures.Add(new KeyGesture(Key.M, ModifierKeys.Control));
        }

        private void InitializeCommand()
        {
            this.ListBox.CommandBindings.Add(_commandResource.CreateCommandBinding(OpenCommand));
            this.ListBox.CommandBindings.Add(_commandResource.CreateCommandBinding(OpenBookCommand));
            this.ListBox.CommandBindings.Add(_commandResource.CreateCommandBinding(OpenExplorerCommand));
            this.ListBox.CommandBindings.Add(_commandResource.CreateCommandBinding(OpenExternalAppCommand));
            this.ListBox.CommandBindings.Add(_commandResource.CreateCommandBinding(CopyCommand));
            this.ListBox.CommandBindings.Add(_commandResource.CreateCommandBinding(CopyToFolderCommand));
            this.ListBox.CommandBindings.Add(_commandResource.CreateCommandBinding(MoveToFolderCommand));
            this.ListBox.CommandBindings.Add(_commandResource.CreateCommandBinding(RemoveCommand));
            this.ListBox.CommandBindings.Add(_commandResource.CreateCommandBinding(OpenDestinationFolderCommand));
            this.ListBox.CommandBindings.Add(_commandResource.CreateCommandBinding(OpenExternalAppDialogCommand));
            this.ListBox.CommandBindings.Add(_commandResource.CreateCommandBinding(PagemarkCommand));
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
                    BookHub.Current.RequestLoadParent(this);
                    e.Handled = true;
                }
                else if (key == Key.Down)
                {
                    // 選択ブックに移動
                    if (page != null && page.PageType == PageType.Folder)
                    {
                        BookHub.Current.RequestLoad(this, page.Entry.SystemPath, null, BookLoadOption.IsBook | BookLoadOption.SkipSamePlace, true);
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
            }

            // このパネルで使用するキーのイベントを止める
            if (Keyboard.Modifiers == ModifierKeys.None)
            {
                if (e.Key == Key.Up || e.Key == Key.Down || (_vm.IsLRKeyEnabled() && (e.Key == Key.Left || e.Key == Key.Right)) || e.Key == Key.Return || e.Key == Key.Delete)
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
            if (Keyboard.Modifiers != ModifierKeys.None) return;

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
                BookHub.Current.RequestLoad(this, page.Entry.SystemPath, null, BookLoadOption.IsBook | BookLoadOption.SkipSamePlace, true);
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
                contextMenu.Items.Add(new MenuItem() { Header = Properties.Resources.PageListItem_Menu_OpenBook, Command = OpenBookCommand });
                contextMenu.Items.Add(new Separator());
            }

            var listBox = this.ListBox;
            contextMenu.Items.Add(new MenuItem() { Header = Properties.Resources.PageListItem_Menu_Open, Command = OpenCommand });
            contextMenu.Items.Add(new Separator());
            contextMenu.Items.Add(new MenuItem() { Header = Properties.Resources.PageListItem_Menu_Pagemark, Command = PagemarkCommand, IsChecked = _commandResource.Pagemark_IsChecked(listBox) });
            contextMenu.Items.Add(new Separator());
            contextMenu.Items.Add(new MenuItem() { Header = Properties.Resources.PageListItem_Menu_Explorer, Command = OpenExplorerCommand });
            contextMenu.Items.Add(ExternalAppCollectionUtility.CreateExternalAppItem(_commandResource.OpenExternalApp_CanExecute(listBox), OpenExternalAppCommand, OpenExternalAppDialogCommand));
            contextMenu.Items.Add(new MenuItem() { Header = Properties.Resources.PageListItem_Menu_Copy, Command = CopyCommand });
            contextMenu.Items.Add(DestinationFolderCollectionUtility.CreateDestinationFolderItem(Properties.Resources.PageListItem_Menu_CopyToFolder, _commandResource.CopyToFolder_CanExecute(listBox), CopyToFolderCommand, OpenDestinationFolderCommand));
            contextMenu.Items.Add(DestinationFolderCollectionUtility.CreateDestinationFolderItem(Properties.Resources.PageListItem_Menu_MoveToFolder, _commandResource.MoveToFolder_CanExecute(listBox), MoveToFolderCommand, OpenDestinationFolderCommand));
            contextMenu.Items.Add(new Separator());
            contextMenu.Items.Add(new MenuItem() { Header = Properties.Resources.PageListItem_Menu_Delete, Command = RemoveCommand });
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

            var isSuccess = await Task.Run(() =>
            {
                try
                {
                    return ClipboardUtility.SetData(e.Data, pages, new CopyFileCommandParameter() { MultiPagePolicy = MultiPagePolicy.All }, token);
                }
                catch(OperationCanceledException)
                {
                    return false;
                }
            });

            if (!isSuccess)
            {
                e.Cancel = true;
                return;
            }

            // 全てのファイルがファイルシステムであった場合のみ
            if (pages.All(p => p.Entry.IsFileSystem))
            {
                // 右クリックドラッグでファイル移動を許可
                if (Config.Current.System.IsFileWriteAccessEnabled && e.MouseEventArgs.RightButton == MouseButtonState.Pressed)
                {
                    e.AllowedEffects |= DragDropEffects.Move;
                }

                // TODO: ドラッグ終了時にファイル移動の整合性を取る必要がある。
                // しっかり実装するならページのファイルシステムの監視が必要になる。ファイルの追加削除が自動的にページに反映するように。

                // ひとまずドラッグ完了後のページ削除を限定的に行う。
                e.DragEndAction = () => BookOperation.Current.ValidateRemoveFile(pages);
            }
        }

        #endregion

        #region UI Accessor
        public List<Page> GetItems()
        {
            return this.ListBox.Items?.Cast<Page>().ToList();
        }

        public List<Page> GetSelectedItems()
        {
            return this.ListBox.SelectedItems.Cast<Page>().ToList();
        }

        public void SetSelectedItems(IEnumerable<Page> selectedItems)
        {
            this.ListBox.SetSelectedItems(selectedItems?.Intersect(GetItems()).ToList());
        }

        #endregion UI Accessor
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
