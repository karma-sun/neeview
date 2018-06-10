using NeeLaboratory.Windows.Media;
using NeeView.Collections.Generic;
using NeeView.Windows;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
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

            // タッチスクロール操作の終端挙動抑制
            this.ListBox.ManipulationBoundaryFeedback += SidePanel.Current.ScrollViewer_ManipulationBoundaryFeedback;

            _thumbnailLoader = new ListBoxThumbnailLoader(this, QueueElementPriority.BookmarkThumbnail);

            this.Loaded += BookmarkListBox_Loaded;
            this.Unloaded += BookmarkListBox_Unloaded;
        }



        #endregion

        public bool IsRenaming { get; private set; }

        #region IPageListPanel Supprt

        public ListBox PageCollectionListBox => this.ListBox;

        public bool IsThumbnailVisibled => _vm.Model.IsThumbnailVisibled;

        public IEnumerable<IHasPage> CollectPageList(IEnumerable<object> objs) => objs.OfType<TreeListNode<IBookmarkEntry>>().Select(e => e.Value);


        #endregion

        #region Commands

        private static void InitializeCommandStatic()
        {
            RemoveCommand.InputGestures.Add(new KeyGesture(Key.Delete));
            RenameCommand.InputGestures.Add(new KeyGesture(Key.F2));
        }

        private void InitialieCommand()
        {
            this.ListBox.CommandBindings.Add(new CommandBinding(RemoveCommand, Remove_Exec));
            this.ListBox.CommandBindings.Add(new CommandBinding(RenameCommand, Rename_Executed));
        }


        public static readonly RoutedCommand RemoveCommand = new RoutedCommand("RemoveCommand", typeof(BookmarkListBox));

        public void Remove_Exec(object sender, ExecutedRoutedEventArgs e)
        {
            var item = (sender as ListBox)?.SelectedItem as TreeListNode<IBookmarkEntry>;
            if (item != null)
            {
                _vm.Remove(item);
            }
        }


        public static readonly RoutedCommand RenameCommand = new RoutedCommand("RenameCommand", typeof(BookmarkListBox));

        public void Rename_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            var listView = sender as ListBox;

            var item = (sender as ListBox)?.SelectedItem as TreeListNode<IBookmarkEntry>;
            if (item != null && item.Value is BookmarkFolder folder)
            {
                var listViewItem = VisualTreeUtility.GetListBoxItemFromItem(listView, item);
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
                        listViewItem.Focus();
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
                FocusSelectedItem();
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


        //
        private void BookmarkListItem_ContextMenuOpening(object sender, ContextMenuEventArgs e)
        {
            var container = sender as ListBoxItem;
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


        // 履歴項目決定
        private void BookmarkListItem_MouseSingleClick(object sender, MouseButtonEventArgs e)
        {
            var item = (sender as ListBoxItem)?.Content as TreeListNode<IBookmarkEntry>;
            if (item != null)
            {
                FocusSelectedItem();
                _vm.Decide(item);
                e.Handled = true;
            }
        }

        // 履歴項目決定(キー)
        private void BookmarkListItem_KeyDown(object sender, KeyEventArgs e)
        {
            var item = (sender as ListBoxItem)?.Content as TreeListNode<IBookmarkEntry>;
            {
                if (e.Key == Key.Return)
                {
                    FocusSelectedItem();
                    _vm.Decide(item);
                    e.Handled = true;
                }
                else if (item.Value is BookmarkFolder)
                {
                    if (e.Key == Key.Left)
                    {
                        _vm.Expand(item, false);
                        e.Handled = true;
                    }
                    else if (e.Key == Key.Right)
                    {
                        _vm.Expand(item, true);
                        e.Handled = true;
                    }
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

        private void BookmarkListBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (this.ListBox.SelectedIndex < 0) return;
            this.ListBox.ScrollIntoView(this.ListBox.SelectedItem);
        }

        #endregion
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
}
