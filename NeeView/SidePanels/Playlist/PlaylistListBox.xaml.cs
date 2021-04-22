using NeeView.Windows;
using NeeView.Windows.Media;
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
    /// PlaylistListBox.xaml の相互作用ロジック
    /// </summary>
    public partial class PlaylistListBox : UserControl, IPageListPanel, IDisposable
    {
        private PlaylistListBoxViewModel _vm;
        private ListBoxThumbnailLoader _thumbnailLoader;
        private PageThumbnailJobClient _jobClient;
        private bool _focusRequest;

        static PlaylistListBox()
        {
            InitializeCommandStatic();
        }

        public PlaylistListBox()
        {
            InitializeComponent();
            InitializeCommand();
        }

        public PlaylistListBox(PlaylistListBoxViewModel vm) : this()
        {
            _vm = vm;
            this.DataContext = vm;

            // タッチスクロール操作の終端挙動抑制
            this.ListBox.ManipulationBoundaryFeedback += SidePanelFrame.Current.ScrollViewer_ManipulationBoundaryFeedback;

            this.Loaded += PlaylistListBox_Loaded;
            this.Unloaded += PlaylistListBox_Unloaded;
        }



        #region Commands

        public readonly static RoutedCommand RenameCommand = new RoutedCommand(nameof(RenameCommand), typeof(PlaylistListBox));
        public readonly static RoutedCommand RemoveCommand = new RoutedCommand(nameof(RemoveCommand), typeof(PlaylistListBox));

        private static void InitializeCommandStatic()
        {
            RenameCommand.InputGestures.Add(new KeyGesture(Key.F2));
            RemoveCommand.InputGestures.Add(new KeyGesture(Key.Delete));
        }

        private void InitializeCommand()
        {
            this.ListBox.CommandBindings.Add(new CommandBinding(RenameCommand, RenameCommand_Execute, RenameCommand_CanExecute));
            this.ListBox.CommandBindings.Add(new CommandBinding(RemoveCommand, RemoveCommand_Execute, RemoveCommand_CanExecute));
        }

        private void RenameCommand_CanExecute(object sender, CanExecuteRoutedEventArgs e)
        {
            e.CanExecute = true;
        }

        private void RenameCommand_Execute(object sender, ExecutedRoutedEventArgs e)
        {
            var item = this.ListBox.SelectedItem as PlaylistListBoxItem;
            if (item is null) return;
            Rename(item);
        }

        private void RemoveCommand_CanExecute(object sender, CanExecuteRoutedEventArgs e)
        {
            e.CanExecute = true;
        }

        private void RemoveCommand_Execute(object sender, ExecutedRoutedEventArgs e)
        {
            var items = this.ListBox.SelectedItems.Cast<PlaylistListBoxItem>().ToList();
            _vm.Remove(items);
            FocusSelectedItem(true);
        }


        private void Rename(PlaylistListBoxItem item)
        {
            var listBox = this.ListBox;
            if (item != null)
            {
                var listViewItem = VisualTreeUtility.FindContainer<ListBoxItem>(listBox, item);
                var textBlock = VisualTreeUtility.FindVisualChild<TextBlock>(listViewItem, "FileNameTextBlock");

                if (textBlock != null)
                {
                    var rename = new RenameControl() { Target = textBlock };
                    rename.Closing += (s, ev) =>
                    {
                        if (ev.OldValue != ev.NewValue)
                        {
                            bool isRenamed = _vm.Rename(item, ev.NewValue);
                            ev.Cancel = !isRenamed;
                        }
                    };
                    rename.Closed += (s, ev) =>
                    {
                        listViewItem.Focus();
                        if (ev.MoveRename != 0)
                        {
                            RenameNext(ev.MoveRename);
                        }
                    };
                    rename.Close += (s, ev) =>
                    {
                    };

                    MainWindow.Current.RenameManager.Open(rename);
                }
            }
        }

        private void RenameNext(int delta)
        {
            if (this.ListBox.SelectedIndex < 0) return;

            // 選択項目を1つ移動
            this.ListBox.SelectedIndex = (this.ListBox.SelectedIndex + this.ListBox.Items.Count + delta) % this.ListBox.Items.Count;
            this.ListBox.UpdateLayout();

            // リネーム発動
            RenameCommand_Execute(this.ListBox, null);
        }

        #endregion Commands

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

        #endregion IPageListBox support

        #region DragDrop

        private async Task DragStartBehavior_DragBeginAsync(object sender, DragStartEventArgs e, CancellationToken token)
        {
            var items = this.ListBox.SelectedItems
                .Cast<PlaylistListBoxItem>()
                .ToList();

            if (!items.Any())
            {
                e.Cancel = true;
                return;
            }

            var collection = new PlaylistListBoxItemCollection(items);
            e.Data.SetData(collection);
            e.AllowedEffects |= DragDropEffects.Move;

            e.Data.SetData(items.Select(x => new QueryPath(x.Path)).ToQueryPathCollection());

            await Task.CompletedTask;
        }

        private void FolderList_PreviewDragEnter(object sender, DragEventArgs e)
        {
            FolderList_PreviewDragOver(sender, e);
        }

        private void FolderList_PreviewDragOver(object sender, DragEventArgs e)
        {
            FolderList_DragDrop(sender, e, false);
            DragDropHelper.AutoScroll(sender, e);
        }

        private void FolderList_Drop(object sender, DragEventArgs e)
        {
            FolderList_DragDrop(sender, e, true);
        }

        private void FolderList_DragDrop(object sender, DragEventArgs e, bool isDrop)
        {
            var listBoxItem = PointToViewItem(this.ListBox, e.GetPosition(this.ListBox));

            var targetItem = listBoxItem?.Content as PlaylistListBoxItem;

            DropToPlaylist(sender, e, isDrop, targetItem, e.Data.GetData<PlaylistListBoxItemCollection>());
            if (e.Handled) return;

            DropToPlaylist(sender, e, isDrop, targetItem, e.Data.GetData<QueryPathCollection>());
            if (e.Handled) return;

            DropToPlaylist(sender, e, isDrop, targetItem, e.Data.GetFileDrop());
            if (e.Handled) return;
        }

        private void DropToPlaylist(object sender, DragEventArgs e, bool isDrop, PlaylistListBoxItem targetItem, IEnumerable<PlaylistListBoxItem> dropItems)
        {
            if (dropItems == null || !dropItems.Any())
            {
                return;
            }

            e.Effects = dropItems.All(x => x != targetItem) ? DragDropEffects.Move : DragDropEffects.None;
            e.Handled = true;

            if (isDrop && e.Effects == DragDropEffects.Move)
            {
                _vm.Move(dropItems, targetItem);
            }
        }

        private void DropToPlaylist(object sender, DragEventArgs e, bool isDrop, PlaylistListBoxItem node, IEnumerable<QueryPath> queries)
        {
            if (queries == null || !queries.Any())
            {
                return;
            }

            var paths = queries.Where(x => x.Scheme == QueryScheme.File).Select(x => x.SimplePath);
            if (!paths.Any())
            {
                return;
            }

            if (isDrop)
            {
                _vm.Insert(paths, node);
            }

            e.Effects = DragDropEffects.Copy;
            e.Handled = true;
        }

        private void DropToPlaylist(object sender, DragEventArgs e, bool isDrop, PlaylistListBoxItem node, IEnumerable<string> fileNames)
        {
            if (fileNames == null)
            {
                return;
            }

            if ((e.AllowedEffects & DragDropEffects.Copy) != DragDropEffects.Copy)
            {
                return;
            }

            if (isDrop)
            {
                _vm.Insert(fileNames, node);
            }

            e.Effects = DragDropEffects.Copy;
            e.Handled = true;
        }

        private ListBoxItem PointToViewItem(ListBox listBox, Point point)
        {
            var element = VisualTreeUtility.HitTest<ListBoxItem>(listBox, point);

            // NOTE: リストアイテム間に隙間がある場合があるので、Y座標をずらして再検証する
            if (element == null)
            {
                element = VisualTreeUtility.HitTest<ListBoxItem>(listBox, new Point(point.X, point.Y + 1));
            }

            return element;
        }

        #endregion DragDrop


        private void PlaylistListBox_Loaded(object sender, RoutedEventArgs e)
        {
            _jobClient = new PageThumbnailJobClient("Playlist", JobCategories.BookThumbnailCategory);
            _thumbnailLoader = new ListBoxThumbnailLoader(this, _jobClient);
            _thumbnailLoader.Load();

            _vm.SelectedItemChanged += ViewModel_SelectedItemChanged;

            Config.Current.Panels.ContentItemProfile.PropertyChanged += PanelListItemProfile_PropertyChanged;
            Config.Current.Panels.BannerItemProfile.PropertyChanged += PanelListItemProfile_PropertyChanged;
            Config.Current.Panels.ThumbnailItemProfile.PropertyChanged += PanelListItemProfile_PropertyChanged;
        }


        private void PlaylistListBox_Unloaded(object sender, RoutedEventArgs e)
        {
            _vm.SelectedItemChanged -= ViewModel_SelectedItemChanged;

            Config.Current.Panels.ContentItemProfile.PropertyChanged -= PanelListItemProfile_PropertyChanged;
            Config.Current.Panels.BannerItemProfile.PropertyChanged -= PanelListItemProfile_PropertyChanged;
            Config.Current.Panels.ThumbnailItemProfile.PropertyChanged -= PanelListItemProfile_PropertyChanged;

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

        private void PanelListItemProfile_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            this.ListBox.Items?.Refresh();
        }

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


        private void PlaylistListItem_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (Keyboard.Modifiers != ModifierKeys.None) return;

            var item = ((sender as ListBoxItem)?.Content as PlaylistListBoxItem);
            if (!Config.Current.Panels.OpenWithDoubleClick)
            {
                _vm.Open(item);
            }
        }

        private void PlaylistListItem_MouseDoubleClick(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            var item = ((sender as ListBoxItem)?.Content as PlaylistListBoxItem);
            if (Config.Current.Panels.OpenWithDoubleClick)
            {
                _vm.Open(item);
            }
        }



        // 履歴項目決定(キー)
        private void PlaylistListItem_KeyDown(object sender, KeyEventArgs e)
        {
            var item = ((sender as ListBoxItem)?.Content as PlaylistListBoxItem);

            if (Keyboard.Modifiers == ModifierKeys.None)
            {
                if (e.Key == Key.Return)
                {
                    _vm.Open(item);
                    e.Handled = true;
                }
            }
        }

        // リストのキ入力
        private void PlaylistListBox_KeyDown(object sender, KeyEventArgs e)
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
        private async void PlaylistListBox_IsVisibleChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            _vm.Visibility = (bool)e.NewValue ? Visibility.Visible : Visibility.Hidden;

            if (_vm.Visibility == Visibility.Visible)
            {
                ////_vm.UpdateItems();
                this.ListBox.UpdateLayout();

                await Task.Yield();

                if (this.ListBox.SelectedIndex < 0) this.ListBox.SelectedIndex = 0;
                FocusSelectedItem(_focusRequest);
                _focusRequest = false;
            }
        }

        private void PlaylistListBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
        }

        // リスト全体が変化したときにサムネイルを更新する
        private void PlaylistListBox_TargetUpdated(object sender, DataTransferEventArgs e)
        {
            AppDispatcher.BeginInvoke(() => _thumbnailLoader?.Load());
        }

        #region UI Accessor
        // TODO:

        public List<PlaylistItem> GetItems()
        {
            ////_vm.UpdateItems();
            return this.ListBox.Items?.Cast<PlaylistItem>().ToList();
        }

        public List<PlaylistItem> GetSelectedItems()
        {
            return this.ListBox.SelectedItems.Cast<PlaylistItem>().ToList();
        }

        public void SetSelectedItems(IEnumerable<PlaylistItem> selectedItems)
        {
            this.ListBox.SetSelectedItems(selectedItems?.Intersect(GetItems()).ToList());
        }

        #endregion UI Accessor
    }
}
