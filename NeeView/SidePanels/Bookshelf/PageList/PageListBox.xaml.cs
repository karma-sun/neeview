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
    /// PageListBox.xaml の相互作用ロジック
    /// </summary>
    public partial class PageListBox : UserControl, IPageListPanel
    {
        private PageListBoxViewModel _vm;
        private ListBoxThumbnailLoader _thumbnailLoader;


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
            this.DataContext = _vm;

            // タッチスクロール操作の終端挙動抑制
            this.ListBox.ManipulationBoundaryFeedback += SidePanel.Current.ScrollViewer_ManipulationBoundaryFeedback;

            _thumbnailLoader = new ListBoxThumbnailLoader(this, QueueElementPriority.PageListThumbnail);

            this.Loaded += PageListBox_Loaded;
            this.Unloaded += PageListBox_Unloaded;
        }


        #region IPageListPanel support

        public ListBox PageCollectionListBox => this.ListBox;

        public bool IsThumbnailVisibled => PageList.Current.IsThumbnailVisibled;

        public IEnumerable<IHasPage> CollectPageList(IEnumerable<object> objs) => objs.OfType<IHasPage>();
        
        #endregion


        #region Commands

        // remove command
        public static readonly RoutedCommand RemoveCommand = new RoutedCommand("RemoveCommand", typeof(PageListBox));

        private static void InitializeCommandStatic()
        {
            RemoveCommand.InputGestures.Add(new KeyGesture(Key.Delete));
        }

        private void InitializeCommand()
        {
            this.ListBox.CommandBindings.Add(new CommandBinding(RemoveCommand, Remove_Exec, Remove_CanExec));
        }

        private void Remove_CanExec(object sender, CanExecuteRoutedEventArgs e)
        {
            var item = (sender as ListBox)?.SelectedItem as Page;
            e.CanExecute = item != null && _vm.Model.CanRemove(item) && FileIOProfile.Current.IsEnabled;
        }

        private async void Remove_Exec(object sender, ExecutedRoutedEventArgs e)
        {
            var item = (sender as ListBox)?.SelectedItem as Page;
            if (item != null)
            {
                await _vm.Model.RemoveAsync(item);
            }
        }

        #endregion


        private void PageListBox_Loaded(object sender, RoutedEventArgs e)
        {
            _vm.Loaded();
            _vm.ViewItemsChanged += ViewModel_ViewItemsChanged;

            FocusSelectedItem();
        }

        private void PageListBox_Unloaded(object sender, RoutedEventArgs e)
        {
            _vm.Unloaded();
            _vm.ViewItemsChanged -= ViewModel_ViewItemsChanged;
        }


        private void ViewModel_ViewItemsChanged(object sender, ViewItemsChangedEventArgs e)
        {
            UpdateViewItems(e.ViewItems, e.Direction);
        }

        //
        private void UpdateViewItems()
        {
            if (_vm.Model.ViewItems == null) return;

            UpdateViewItems(_vm.Model.ViewItems, 0);
        }

        //
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

        //
        public void FocusSelectedItem()
        {
            if (this.ListBox.SelectedIndex < 0) return;

            UpdateViewItems();

            if (_vm.Model.FocusAtOnce)
            {
                _vm.Model.FocusAtOnce = false;
                ListBoxItem lbi = (ListBoxItem)(this.ListBox.ItemContainerGenerator.ContainerFromIndex(this.ListBox.SelectedIndex));
                lbi?.Focus();
            }
        }

        // フォルダーリスト 選択項目変更
        private void PageList_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
        }


        // 履歴項目決定
        private void PageListItem_MouseSingleClick(object sender, MouseButtonEventArgs e)
        {
            var page = (sender as ListBoxItem)?.Content as Page;
            if (page != null)
            {
                _vm.Model.Jump(page);
                e.Handled = true;
            }
        }

        // 履歴項目決定(キー)
        private void PageListItem_KeyDown(object sender, KeyEventArgs e)
        {
            var page = (sender as ListBoxItem)?.Content as Page;
            {
                if (e.Key == Key.Return)
                {
                    _vm.Model.Jump(page);
                    e.Handled = true;
                }
            }
        }

        // リストのキ入力
        private void PageList_KeyDown(object sender, KeyEventArgs e)
        {
            bool isLRKeyEnabled = SidePanelProfile.Current.IsLeftRightKeyEnabled;

            // このパネルで使用するキーのイベントを止める
            if (e.Key == Key.Up || e.Key == Key.Down || (isLRKeyEnabled && (e.Key == Key.Left || e.Key == Key.Right)) || e.Key == Key.Return || e.Key == Key.Delete)
            {
                e.Handled = true;
            }
        }

        private async void PaegList_IsVisibleChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            if ((bool)e.NewValue)
            {
                await Task.Yield();
                FocusSelectedItem();
            }
        }

        private void PageList_TargetUpdated(object sender, DataTransferEventArgs e)
        {
            UpdateViewItems();
        }
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
                        return page.FullPath;
                    case PageNameFormat.Smart:
                        return page.SmartFullPath;
                    case PageNameFormat.NameOnly:
                        return page.LastName;
                }
            }
            return null;
        }

        public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }

}
