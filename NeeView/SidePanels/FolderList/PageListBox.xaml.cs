using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
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
        #region Fields

        private PageListViewModel _vm;
        private ListBoxThumbnailLoader _thumbnailLoader;

        #endregion

        #region Constructors

        // static constructor
        static PageListBox()
        {
            RemoveCommand.InputGestures.Add(new KeyGesture(Key.Delete));
        }

        // constructor
        public PageListBox()
        {
            InitializeComponent();
        }

        // constructor
        public PageListBox(PageListViewModel vm) : this()
        {
            InitializeCommand();

            _vm = vm;
            _vm.ViewItemsChanged += ViewModel_ViewItemsChanged;
            this.DataContext = _vm;

            // タッチスクロール操作の終端挙動抑制
            this.ListBox.ManipulationBoundaryFeedback += SidePanel.Current.ScrollViewer_ManipulationBoundaryFeedback;

            _thumbnailLoader = new ListBoxThumbnailLoader(this, QueueElementPriority.PageListThumbnail);
        }

        #endregion

        #region IPageListPanel support

        public ListBox PageCollectionListBox => this.ListBox;

        public bool IsThumbnailVisibled => _vm.Model.IsThumbnailVisibled;

        #endregion

        #region Commands

        // delete command
        public static readonly RoutedCommand RemoveCommand = new RoutedCommand("RemoveCommand", typeof(PageListBox));

        private void InitializeCommand()
        {
            this.ListBox.CommandBindings.Add(new CommandBinding(RemoveCommand, Remove_Exec, Remove_CanExec));
        }

        //
        private void Remove_CanExec(object sender, CanExecuteRoutedEventArgs e)
        {
            var item = (sender as ListBox)?.SelectedItem as Page;
            e.CanExecute = item != null && _vm.CanRemove(item) && FileIOProfile.Current.IsEnabled;
        }

        //
        private async void Remove_Exec(object sender, ExecutedRoutedEventArgs e)
        {
            var item = (sender as ListBox)?.SelectedItem as Page;
            if (item != null)
            {
                await _vm.Remove(item);
            }
        }

        #endregion

        #region Methods

        private void ViewModel_ViewItemsChanged(object sender, ViewItemsChangedEventArgs e)
        {
            UpdateViewItems(e.ViewItems, e.Direction);
        }

        //
        private void UpdateViewItems()
        {
            if (_vm.ViewItems == null) return;

            UpdateViewItems(_vm.ViewItems, 0);
        }

        //
        private void UpdateViewItems(List<Page> items, int direction)
        {
            if (!this.ListBox.IsLoaded) return;
            if (_vm.Model.PageCollection == null) return;
            if (_vm.IsPageCollectionDarty) return;

            if (items.Count == 1)
            {
                this.ListBox.ScrollIntoView(items.First());
            }
            else if (direction < 0)
            {
                this.ListBox.ScrollIntoView(items.First());
            }
            else if (direction > 0)
            {
                this.ListBox.ScrollIntoView(items.Last());
            }
            else
            {
                foreach (var item in items)
                {
                    this.ListBox.ScrollIntoView(item);
                    this.ListBox.UpdateLayout();
                }
            }
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
                _vm.Jump(page);
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
                    _vm.Jump(page);
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
            _vm.IsPageCollectionDarty = false;
            UpdateViewItems();
        }

        #endregion

    }
}
