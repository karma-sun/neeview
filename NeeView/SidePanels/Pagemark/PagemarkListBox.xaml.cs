using NeeView.Windows;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
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
    /// PagemarkListBox.xaml の相互作用ロジック
    /// </summary>
    public partial class PagemarkListBox : UserControl, IPageListPanel
    {
        #region Fields

        public static string DragDropFormat = $"{Config.Current.ProcessId}.PagemarkItem";

        private PagemarkListViewModel _vm;
        private ListBoxThumbnailLoader _thumbnailLoader;
        private bool _storeFocus;

        #endregion

        #region Constructors

        public PagemarkListBox()
        {
            InitializeComponent();
        }

        public PagemarkListBox(PagemarkListViewModel vm) : this()
        {
            _vm = vm;
            this.DataContext = _vm;

            InitializeCommand();

            // タッチスクロール操作の終端挙動抑制
            this.ListBox.ManipulationBoundaryFeedback += SidePanel.Current.ScrollViewer_ManipulationBoundaryFeedback;

            _thumbnailLoader = new ListBoxThumbnailLoader(this, QueueElementPriority.PagemarkThumbnail);
        }

        #endregion

        #region IPageListPanel Support

        public ListBox PageCollectionListBox => this.ListBox;

        public bool IsThumbnailVisibled => _vm.Model.IsThumbnailVisibled;

        public IEnumerable<IHasPage> CollectPageList(IEnumerable<object> objs) => objs.OfType<IHasPage>();
        
        #endregion

        #region Commands

        public static readonly RoutedCommand RemoveCommand = new RoutedCommand("RemoveCommand", typeof(PagemarkListBox));

        public void InitializeCommand()
        {
            RemoveCommand.InputGestures.Add(new KeyGesture(Key.Delete));
            this.ListBox.CommandBindings.Add(new CommandBinding(RemoveCommand, Remove_Exec));
        }

        public void Remove_Exec(object sender, ExecutedRoutedEventArgs e)
        {
            var item = (sender as ListBox)?.SelectedItem as Pagemark;
            if (item != null)
            {
                _vm.Remove(item);
            }
        }

        #endregion

        #region Methods

        //
        public void StoreFocus()
        {
            var index = this.ListBox.SelectedIndex;

            ListBoxItem lbi = index >= 0 ? (ListBoxItem)(this.ListBox.ItemContainerGenerator.ContainerFromIndex(index)) : null;
            _storeFocus = lbi != null ? lbi.IsFocused : false;
        }

        //
        public void RestoreFocus()
        {
            if (_storeFocus)
            {
                this.ListBox.ScrollIntoView(this.ListBox.SelectedItem);

                var index = this.ListBox.SelectedIndex;
                var lbi = index >= 0 ? (ListBoxItem)(this.ListBox.ItemContainerGenerator.ContainerFromIndex(index)) : null;
                lbi?.Focus();
            }
        }

        #endregion

        #region Event Methods

        // 同期
        private void UpdateButton_Click(object sender, RoutedEventArgs e)
        {
            // nop.                
        }

        // 履歴項目決定
        private void PagemarkListItem_MouseSingleClick(object sender, MouseButtonEventArgs e)
        {
            var historyItem = (sender as ListBoxItem)?.Content as Pagemark;
            if (historyItem != null)
            {
                _vm.Load(historyItem);
                e.Handled = true;
            }
        }

        // 履歴項目決定(キー)
        private void PagemarkListItem_KeyDown(object sender, KeyEventArgs e)
        {
            var historyItem = (sender as ListBoxItem)?.Content as Pagemark;
            {
                if (e.Key == Key.Return)
                {
                    _vm.Load(historyItem);
                    e.Handled = true;
                }
            }
        }

        // リストのキ入力
        private void PagemarkList_KeyDown(object sender, KeyEventArgs e)
        {
            bool isLRKeyEnabled = SidePanelProfile.Current.IsLeftRightKeyEnabled;

            // このパネルで使用するキーのイベントを止める
            if (e.Key == Key.Up || e.Key == Key.Down || (isLRKeyEnabled && (e.Key == Key.Left || e.Key == Key.Right)) || e.Key == Key.Return || e.Key == Key.Delete)
            {
                e.Handled = true;
            }
        }

        private void PagemarkListBox_PreviewDragOver(object sender, DragEventArgs e)
        {
            ListBoxDragSortExtension.PreviewDragOver(sender, e, DragDropFormat);
        }

        private void PagemarkListBox_Drop(object sender, DragEventArgs e)
        {
            var list = (sender as ListBox).Tag as ObservableCollection<Pagemark>;
            if (list != null)
            {
                ListBoxDragSortExtension.Drop<Pagemark>(sender, e, DragDropFormat, list);
                e.Handled = true;
            }
        }


        // 表示/非表示イベント
        private async void PagemarkListBox_IsVisibleChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            if (e.NewValue as bool? == true)
            {
                if (this.ListBox.SelectedIndex < 0)
                {
                    this.ListBox.SelectedIndex = 0;
                }

                await Task.Yield();
                FocusSelectedItem();
            }
        }

        //
        public void FocusSelectedItem()
        {
            if (this.ListBox.SelectedIndex < 0) return;

            this.ListBox.ScrollIntoView(this.ListBox.SelectedItem);

            ListBoxItem lbi = (ListBoxItem)(this.ListBox.ItemContainerGenerator.ContainerFromIndex(this.ListBox.SelectedIndex));
            lbi?.Focus();
        }

        #endregion

    }
}
