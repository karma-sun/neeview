using NeeView.Collections.Generic;
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
    /// BookmarkListBox.xaml の相互作用ロジック
    /// </summary>
    public partial class BookmarkListBox : UserControl, IPageListPanel
    {
        #region Fields

        public static string DragDropFormat = $"{Config.Current.ProcessId}.BookmarkItem";

        private BookmarkListBoxViewModel _vm;
        private ListBoxThumbnailLoader _thumbnailLoader;
        private bool _storeFocus;

        #endregion

        #region Constructors

        public BookmarkListBox()
        {
            InitializeComponent();
        }

        public BookmarkListBox(BookmarkListBoxViewModel vm) : this()
        {
            _vm = vm;
            this.DataContext = vm;

            InitialieCommand();

            // タッチスクロール操作の終端挙動抑制
            this.ListBox.ManipulationBoundaryFeedback += SidePanel.Current.ScrollViewer_ManipulationBoundaryFeedback;

            _thumbnailLoader = new ListBoxThumbnailLoader(this, QueueElementPriority.BookmarkThumbnail);

            this.Loaded += BookmarkListBox_Loaded;
            this.Unloaded += BookmarkListBox_Unloaded;
        }



        #endregion

        #region IPageListPanel Supprt

        public ListBox PageCollectionListBox => this.ListBox;

        public bool IsThumbnailVisibled => _vm.Model.IsThumbnailVisibled;

        public IEnumerable<IHasPage> CollectPageList(IEnumerable<object> objs) => objs.OfType<TreeListNode<IBookmarkEntry>>().Select(e => e.Value);


        #endregion

        #region Commands

        public static readonly RoutedCommand RemoveCommand = new RoutedCommand("RemoveCommand", typeof(BookmarkListBox));

        private void InitialieCommand()
        {
            RemoveCommand.InputGestures.Add(new KeyGesture(Key.Delete));
            this.ListBox.CommandBindings.Add(new CommandBinding(RemoveCommand, Remove_Exec));
        }

        public void Remove_Exec(object sender, ExecutedRoutedEventArgs e)
        {
            var item = (sender as ListBox)?.SelectedItem as TreeListNode<IBookmarkEntry>;
            if (item != null)
            {
                _vm.Remove(item);
            }
        }

        #endregion

        #region Methods

        private void BookmarkListBox_Loaded(object sender, RoutedEventArgs e)
        {
            _vm.Changing += List_Changing;
            _vm.Changed += List_Changed;
            _vm.Loaded();
        }

        private void BookmarkListBox_Unloaded(object sender, RoutedEventArgs e)
        {
            _vm.Changing -= List_Changing;
            _vm.Changed -= List_Changed;
            _vm.Unloaded();
        }

        private void List_Changing(object sender, EventArgs e)
        {
            StoreFocus();
        }

        private void List_Changed(object sender, EventArgs e)
        {
            Dispatcher.BeginInvoke((Action)(() => RestoreFocus()));
        }

        public void StoreFocus()
        {
            var index = this.ListBox.SelectedIndex;

            ListBoxItem lbi = index >= 0 ? (ListBoxItem)(this.ListBox.ItemContainerGenerator.ContainerFromIndex(index)) : null;
            _storeFocus = lbi != null ? lbi.IsFocused : false;
        }

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

        public void FocusSelectedItem()
        {
            if (this.ListBox.SelectedIndex < 0) return;

            this.ListBox.ScrollIntoView(this.ListBox.SelectedItem);

            ListBoxItem lbi = (ListBoxItem)(this.ListBox.ItemContainerGenerator.ContainerFromIndex(this.ListBox.SelectedIndex));
            lbi?.Focus();
        }

        #endregion

        #region Event Methods

        // 同期
        private void UpdateButton_Click(object sender, RoutedEventArgs e)
        {
            // nop.                
        }

        // 履歴項目決定
        private void BookmarkListItem_MouseSingleClick(object sender, MouseButtonEventArgs e)
        {
            var item = (sender as ListBoxItem)?.Content as TreeListNode<IBookmarkEntry>;
            if (item != null)
            {
                _vm.Decide(item);
                e.Handled = true;
            }
        }

        // 履歴項目決定(キー)
        private void BookmarkListItem_KeyDown(object sender, KeyEventArgs e)
        {
            var historyItem = (sender as ListBoxItem)?.Content as TreeListNode<IBookmarkEntry>;
            {
                if (e.Key == Key.Return)
                {
                    _vm.Decide(historyItem);
                    e.Handled = true;
                }
            }
        }

        // リストのキ入力
        private void BookmarkList_KeyDown(object sender, KeyEventArgs e)
        {
            bool isLRKeyEnabled = SidePanelProfile.Current.IsLeftRightKeyEnabled;

            // このパネルで使用するキーのイベントを止める
            if (e.Key == Key.Up || e.Key == Key.Down || (isLRKeyEnabled && (e.Key == Key.Left || e.Key == Key.Right)) || e.Key == Key.Return || e.Key == Key.Delete)
            {
                e.Handled = true;
            }
        }

        private void BookmarkListBox_PreviewDragOver(object sender, DragEventArgs e)
        {
            ListBoxDragSortExtension.PreviewDragOver(sender, e, DragDropFormat);
        }

        private void BookmarkListBox_Drop(object sender, DragEventArgs e)
        {
            var list = (sender as ListBox).Tag as ObservableCollection<TreeListNode<IBookmarkEntry>>;
            if (list != null)
            {
                ////ListBoxDragSortExtension.Drop<TreeListNode<IBookmarkEntry>>(sender, e, DragDropFormat, list);

                var dropInfo = ListBoxDragSortExtension.GetDropInfo(sender, e, DragDropFormat, list);
                _vm.Move(dropInfo);

                e.Handled = true;
            }
        }


        // 表示/非表示イベント
        private async void BookmarkListBox_IsVisibleChanged(object sender, DependencyPropertyChangedEventArgs e)
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

        #endregion

    }
}
