using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using System.Text;
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
    /// HistoryListBox.xaml の相互作用ロジック
    /// </summary>
    public partial class HistoryListBox : UserControl, IPageListPanel, IDisposable
    {
        private HistoryListBoxViewModel _vm;
        private ListBoxThumbnailLoader _thumbnailLoader;
        private PageThumbnailJobClient _jobClient;
        private bool _focusRequest;



        static HistoryListBox()
        {
            InitializeCommandStatic();
        }

        public HistoryListBox()
        {
            InitializeComponent();
        }

        public HistoryListBox(HistoryListBoxViewModel vm) : this()
        {
            _vm = vm;
            this.DataContext = vm;

            InitializeCommand();

            // タッチスクロール操作の終端挙動抑制
            this.ListBox.ManipulationBoundaryFeedback += SidePanelFrame.Current.ScrollViewer_ManipulationBoundaryFeedback;

            this.Loaded += HistoryListBox_Loaded;
            this.Unloaded += HistoryListBox_Unloaded;
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

        #region IPageListBox support

        public ListBox PageCollectionListBox => this.ListBox;

        public bool IsThumbnailVisibled => _vm.IsThumbnailVisibled;

        public IEnumerable<IHasPage> CollectPageList(IEnumerable<object> objs) => objs.OfType<IHasPage>();

        #endregion

        #region Commands

        public static readonly RoutedCommand OpenBookCommand = new RoutedCommand("OpenBookCommand", typeof(HistoryListBox));
        public static readonly RoutedCommand RemoveCommand = new RoutedCommand("RemoveCommand", typeof(HistoryListBox));

        public static void InitializeCommandStatic()
        {
            OpenBookCommand.InputGestures.Add(new KeyGesture(Key.Enter));
            RemoveCommand.InputGestures.Add(new KeyGesture(Key.Delete));
        }

        public void InitializeCommand()
        {
            this.ListBox.CommandBindings.Add(new CommandBinding(OpenBookCommand, OpenBook_Exec));
            this.ListBox.CommandBindings.Add(new CommandBinding(RemoveCommand, Remove_Exec));
        }

        public void OpenBook_Exec(object sender, ExecutedRoutedEventArgs e)
        {
            var item = this.ListBox.SelectedItem as BookHistory;
            if (item == null) return;

            _vm.Load(item?.Path);
            e.Handled = true;
        }

        public void Remove_Exec(object sender, ExecutedRoutedEventArgs e)
        {
            var items = this.ListBox.SelectedItems?.Cast<BookHistory>().ToList();
            if (items == null || !items.Any()) return;

            _vm.Remove(items);
            FocusSelectedItem(true);
        }

        #endregion


        private void HistoryListBox_Loaded(object sender, RoutedEventArgs e)
        {
            _jobClient = new PageThumbnailJobClient("HistoryList", JobCategories.BookThumbnailCategory);
            _thumbnailLoader = new ListBoxThumbnailLoader(this, _jobClient);
            _thumbnailLoader.Load();

            _vm.SelectedItemChanged += ViewModel_SelectedItemChanged;

            Config.Current.Panels.ContentItemProfile.PropertyChanged += PanelListtemProfile_PropertyChanged;
            Config.Current.Panels.BannerItemProfile.PropertyChanged += PanelListtemProfile_PropertyChanged;
            Config.Current.Panels.ThumbnailItemProfile.PropertyChanged += PanelListtemProfile_PropertyChanged;
        }


        private void HistoryListBox_Unloaded(object sender, RoutedEventArgs e)
        {
            _vm.SelectedItemChanged -= ViewModel_SelectedItemChanged;

            Config.Current.Panels.ContentItemProfile.PropertyChanged -= PanelListtemProfile_PropertyChanged;
            Config.Current.Panels.BannerItemProfile.PropertyChanged -= PanelListtemProfile_PropertyChanged;
            Config.Current.Panels.ThumbnailItemProfile.PropertyChanged -= PanelListtemProfile_PropertyChanged;

            _jobClient?.Dispose();
        }


        private void ViewModel_SelectedItemChanged(object sender, EventArgs e)
        {
            this.ListBox.SetAnchorItem(null);

            if (this.ListBox.IsFocused)
            {
                FocusSelectedItem(true);
            }

            this.ListBox.ScrollIntoView(this.ListBox.SelectedItem);
        }

        private void PanelListtemProfile_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            this.ListBox.Items?.Refresh();
        }

        // フォーカス
        public bool FocusSelectedItem(bool focus)
        {
            if (this.ListBox.SelectedIndex < 0) return false;

            this.ListBox.ScrollIntoView(this.ListBox.SelectedItem);

            if (focus)
            {
                ListBoxItem lbi = (ListBoxItem)(this.ListBox.ItemContainerGenerator.ContainerFromIndex(this.ListBox.SelectedIndex));
                return lbi?.Focus() ?? false;
            }
            else
            {
                return false;
            }
        }

        public void Refresh()
        {
            this.ListBox.Items.Refresh();
        }

        public void FocusAtOnce()
        {
            var focused = FocusSelectedItem(true);
            if (!focused)
            {
                _focusRequest = true;
            }
        }


        // 履歴項目決定
        private void HistoryListItem_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (Keyboard.Modifiers != ModifierKeys.None) return;

            var item = ((sender as ListBoxItem)?.Content as BookHistory);
            if (!Config.Current.Panels.OpenWithDoubleClick)
            {
                _vm.Load(item?.Path);
            }
        }

        private void HistoryListItem_MouseDoubleClick(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            var item = ((sender as ListBoxItem)?.Content as BookHistory);
            if (Config.Current.Panels.OpenWithDoubleClick)
            {
                _vm.Load(item?.Path);
            }
        }



        // 履歴項目決定(キー)
        private void HistoryListItem_KeyDown(object sender, KeyEventArgs e)
        {
            var item = ((sender as ListBoxItem)?.Content as BookHistory);

            if (Keyboard.Modifiers == ModifierKeys.None)
            {
                if (e.Key == Key.Return)
                {
                    _vm.Load(item?.Path);
                    e.Handled = true;
                }
            }
        }

        // リストのキ入力
        private void HistoryListBox_KeyDown(object sender, KeyEventArgs e)
        {
            // このパネルで使用するキーのイベントを止める
            if (Keyboard.Modifiers == ModifierKeys.None)
            {
                if (e.Key == Key.Up || e.Key == Key.Down || (_vm.IsLRKeyEnabled() && (e.Key == Key.Left || e.Key == Key.Right)) || e.Key == Key.Return || e.Key == Key.Delete)
                {
                    e.Handled = true;
                }
            }
        }

        // 表示/非表示イベント
        private async void HistoryListBox_IsVisibleChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            _vm.Visibility = (bool)e.NewValue ? Visibility.Visible : Visibility.Hidden;

            if (_vm.Visibility == Visibility.Visible)
            {
                _vm.UpdateItems();
                this.ListBox.UpdateLayout();

                await Task.Yield();

                if (this.ListBox.SelectedIndex < 0) this.ListBox.SelectedIndex = 0;
                FocusSelectedItem(_focusRequest);
                _focusRequest = false;
            }
        }

        private void HistoryListBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
        }

        // リスト全体が変化したときにサムネイルを更新する
        private void HistoryListBox_TargetUpdated(object sender, DataTransferEventArgs e)
        {
            AppDispatcher.BeginInvoke(() => _thumbnailLoader?.Load());
        }

        #region UI Accessor

        public List<BookHistory> GetItems()
        {
            _vm.UpdateItems();
            return this.ListBox.Items?.Cast<BookHistory>().ToList();
        }

        public List<BookHistory> GetSelectedItems()
        {
            return this.ListBox.SelectedItems.Cast<BookHistory>().ToList();
        }

        public void SetSelectedItems(IEnumerable<BookHistory> selectedItems)
        {
            this.ListBox.SetSelectedItems(selectedItems?.Intersect(GetItems()).ToList());
        }

        #endregion UI Accessor
    }

    public class ArchiveEntryToDecoratePlaceNameConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is ArchiveEntry entry)
            {
                var directory = entry.RootArchiver?.SystemPath ?? LoosePath.GetDirectoryName(entry.SystemPath);
                return SidePanelProfile.GetDecoratePlaceName(directory);
            }

            return null;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
