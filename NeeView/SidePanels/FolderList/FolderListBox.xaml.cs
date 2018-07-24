using NeeLaboratory.Windows.Media;
using NeeView.Collections.Generic;
using System;
using System.Collections.Generic;
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
    /// FolderListBox.xaml の相互作用ロジック
    /// </summary>
    public partial class FolderListBox : UserControl, IPageListPanel
    {
        #region Fields

        public static string DragDropFormat = $"{Config.Current.ProcessId}.FolderListBox";

        private FolderListViewModel _vm;
        private ListBoxThumbnailLoader _thumbnailLoader;
        private bool _lastFocusRequest;
        private bool _storeFocus;
        private bool _isClickEnabled;

        #endregion

        #region Constructors

        // static construcotr
        static FolderListBox()
        {
            InitialieCommandStatic();
        }

        //
        public FolderListBox()
        {
            InitializeComponent();
        }

        //
        public FolderListBox(FolderListViewModel vm) : this()
        {
            _vm = vm;
            this.DataContext = vm;

            InitializeCommand();

            // タッチスクロール操作の終端挙動抑制
            this.ListBox.ManipulationBoundaryFeedback += SidePanel.Current.ScrollViewer_ManipulationBoundaryFeedback;

            this.ListBox.AddHandler(ScrollViewer.ScrollChangedEvent, new ScrollChangedEventHandler(ListBox_ScrollChanged));
            _thumbnailLoader = new ListBoxThumbnailLoader(this, QueueElementPriority.FolderThumbnail);
        }

        #endregion

        #region Properties

        // フォーカス可能フラグ
        public bool IsFocusEnabled { get; set; } = true;

        #endregion

        #region IPanelListBox Support

        //
        public ListBox PageCollectionListBox => this.ListBox;

        // サムネイルが表示されている？
        public bool IsThumbnailVisibled => _vm.Model.IsThumbnailVisibled;

        public IEnumerable<IHasPage> CollectPageList(IEnumerable<object> objs) => objs.OfType<IHasPage>();

        #endregion

        #region Commands

        public static readonly RoutedCommand LoadWithRecursiveCommand = new RoutedCommand("LoadWithRecursiveCommand", typeof(FolderListBox));
        public static readonly RoutedCommand OpenExplorerCommand = new RoutedCommand("OpenExplorerCommand", typeof(FolderListBox));
        public static readonly RoutedCommand CopyCommand = new RoutedCommand("CopyCommand", typeof(FolderListBox));
        public static readonly RoutedCommand RemoveCommand = new RoutedCommand("RemoveCommand", typeof(FolderListBox));
        public static readonly RoutedCommand RenameCommand = new RoutedCommand("RenameCommand", typeof(FolderListBox));

        private static void InitialieCommandStatic()
        {
            CopyCommand.InputGestures.Add(new KeyGesture(Key.C, ModifierKeys.Control));
            RemoveCommand.InputGestures.Add(new KeyGesture(Key.Delete));
            RenameCommand.InputGestures.Add(new KeyGesture(Key.F2));
        }

        private void InitializeCommand()
        {
            this.ListBox.CommandBindings.Add(new CommandBinding(LoadWithRecursiveCommand, LoadWithRecursive_Executed, LoadWithRecursive_CanExecute));
            this.ListBox.CommandBindings.Add(new CommandBinding(OpenExplorerCommand, OpenExplorer_Executed));
            this.ListBox.CommandBindings.Add(new CommandBinding(CopyCommand, Copy_Executed, Copy_CanExecute));
            this.ListBox.CommandBindings.Add(new CommandBinding(RemoveCommand, Remove_Executed, FileCommand_CanExecute));
            this.ListBox.CommandBindings.Add(new CommandBinding(RenameCommand, Rename_Executed, FileCommand_CanExecute));
        }

        /// <summary>
        /// サブフォルダーを読み込む？
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void LoadWithRecursive_CanExecute(object sender, CanExecuteRoutedEventArgs e)
        {
            var item = (sender as ListBox)?.SelectedItem as FolderItem;

            e.CanExecute = item == null || item.Attributes.AnyFlag(FolderItemAttribute.Drive | FolderItemAttribute.Empty)
                ? false
                : BookHub.Current.IsArchiveRecursive
                    ? item.Attributes.HasFlag(FolderItemAttribute.Directory)
                    : ArchiverManager.Current.GetSupportedType(item.TargetPath).IsRecursiveSupported();
        }


        /// <summary>
        /// サブフォルダーを読み込む
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void LoadWithRecursive_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            var item = (sender as ListBox)?.SelectedItem as FolderItem;

            // サブフォルダー読み込み状態を反転する
            var option = item.IsRecursived ? BookLoadOption.NotRecursive : BookLoadOption.Recursive;
            _vm.Model.LoadBook(item.TargetPath, option);
        }

        /// <summary>
        /// ファイル系コマンド実行可能判定
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void FileCommand_CanExecute(object sender, CanExecuteRoutedEventArgs e)
        {
            var item = (sender as ListBox)?.SelectedItem as FolderItem;
            e.CanExecute = (item != null && item.IsEditable && FileIOProfile.Current.IsEnabled);
        }

        /// <summary>
        /// コピーコマンド実行可能判定
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Copy_CanExecute(object sender, CanExecuteRoutedEventArgs e)
        {
            var item = (sender as ListBox)?.SelectedItem as FolderItem;
            e.CanExecute = (item != null && item.IsEditable);
        }

        /// <summary>
        /// コピーコマンド実行
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        public void Copy_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            var item = (sender as ListBox)?.SelectedItem as FolderItem;
            if (item != null)
            {
                FileIO.Current.CopyToClipboard(item);
            }
        }

        /// <summary>
        /// 削除コマンド実行
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        public async void Remove_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            var item = (sender as ListBox)?.SelectedItem as FolderItem;
            if (item == null)
            {
                return;
            }

            if (item.Attributes.HasFlag(FolderItemAttribute.Bookmark))
            {
                _vm.Model.RemoveBookmark(item);
            }
            else if (item.IsFileSystem())
            {
                var removed = await FileIO.Current.RemoveAsync(item.Path, Properties.Resources.DialogFileDeleteBookTitle);
                if (removed)
                {
                    _vm.FolderCollection?.RequestDelete(item.Path);
                }
            }
        }

        /// <summary>
        /// 名前変更コマンド実行
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        public void Rename_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            var listView = sender as ListBox;

            var item = (sender as ListBox)?.SelectedItem as FolderItem;
            if (item == null) return;

            if (item.IsFileSystem() || item.Attributes.HasFlag(FolderItemAttribute.Bookmark | FolderItemAttribute.Directory))
            {
                listView.UpdateLayout();
                var listViewItem = VisualTreeUtility.GetListBoxItemFromItem(listView, item);
                var textBlock = VisualTreeUtility.FindVisualChild<TextBlock>(listViewItem, "FileNameTextBlock");

                // 
                if (textBlock != null)
                {
                    var rename = new RenameControl();
                    rename.Target = textBlock;
                    rename.IsFileName = !item.IsDirectory;
                    rename.Closing += async (s, ev) =>
                    {
                        if (item.Source is TreeListNode<IBookmarkEntry> node)
                        {
                            BookmarkCollectionHelper.Rename(node, ev.NewValue);
                        }
                        else if (ev.OldValue != ev.NewValue)
                        {
                            var newName = item.IsShortcut ? ev.NewValue + ".lnk" : ev.NewValue;
                            //Debug.WriteLine($"{ev.OldValue} => {newName}");
                            var src = item.Path;
                            var dst = await FileIO.Current.RenameAsync(item, newName);
                            if (dst != null)
                            {
                                _vm.FolderCollection?.RequestRename(src, dst);
                            }
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
                        _vm.IsRenaming = false;
                    };

                    _vm.IsRenaming = true;
                    ((MainWindow)Application.Current.MainWindow).RenameManager.Open(rename);
                }
            }
        }


        /// <summary>
        /// 項目を移動して名前変更処理を続行する
        /// </summary>
        /// <param name="delta"></param>
        private void RenameNext(int delta)
        {
            if (this.ListBox.SelectedIndex < 0) return;

            // 選択項目を1つ移動
            this.ListBox.SelectedIndex = (this.ListBox.SelectedIndex + this.ListBox.Items.Count + delta) % this.ListBox.Items.Count;
            this.ListBox.UpdateLayout();

            // ブック切り替え
            var item = this.ListBox.SelectedItem as FolderItem;
            if (item != null)
            {
                _vm.Model.LoadBook(item.TargetPath);
            }

            // リネーム発動
            Rename_Executed(this.ListBox, null);
        }

        public void Rename()
        {
            Rename_Executed(this.ListBox, null);
        }

        /// <summary>
        /// エクスプローラーで開くコマンド実行
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        public void OpenExplorer_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            var item = (sender as ListBox)?.SelectedItem as FolderItem;
            if (item != null)
            {
                var path = item.IsFileSystem() ? item.Path : item.TargetPath;
                path = item.Attributes.AnyFlag(FolderItemAttribute.Bookmark | FolderItemAttribute.ArchiveEntry | FolderItemAttribute.Empty) ? ArchiverManager.Current.GetExistPathName(path) : path;
                System.Diagnostics.Process.Start("explorer.exe", "/select,\"" + path + "\"");
            }
        }

        #endregion

        #region DragDrop

        private DependencyObject _lastDropTarget;

        private void DragStartBehavior_DragBegin(object sender, Windows.DragStartEventArgs e)
        {
            _lastDropTarget = null;

            var data = e.Data.GetData(DragDropFormat) as ListBoxItem;
            if (data == null)
            {
                return;
            }

            var item = data.Content as FolderItem;
            if (item == null)
            {
                return;
            }

            if (item.Attributes.AnyFlag(FolderItemAttribute.Bookmark))
            {
                ////e.Data.SetFileDropList(new System.Collections.Specialized.StringCollection() { item.TargetPath });
                e.Data.SetData(item.Source);
            }
        }

        private void FolderList_DragEnter(object sender, DragEventArgs e)
        {
            FolderList_DragDrop(sender, e, false);
        }

        private void FolderList_PreviewDragOver(object sender, DragEventArgs e)
        {
            FolderList_DragDrop(sender, e, false);
        }

        private void FolderList_Drop(object sender, DragEventArgs e)
        {
            FolderList_DragDrop(sender, e, true);
        }

        private void FolderList_DragDrop(object sender, DragEventArgs e, bool isDrop)
        {
            var container = e.Data.GetData(DragDropFormat) as ListBoxItem;
            var item = container?.Content as FolderItem;

            var targetContainer = PointToViewItem(this.ListBox, e.GetPosition(this.ListBox));
            var target = targetContainer?.Content as FolderItem;

            if (target != null)
            {
                var bookmarkEntry = (TreeListNode<IBookmarkEntry>)e.Data.GetData(typeof(TreeListNode<IBookmarkEntry>));
                if (bookmarkEntry != null)
                {
                    if (target.Attributes.HasFlag(FolderItemAttribute.Bookmark | FolderItemAttribute.Directory) && target.Source != bookmarkEntry)
                    {
                        if (target.Source is TreeListNode<IBookmarkEntry> node && !node.ParentContains(bookmarkEntry))
                        {
                            if (isDrop)
                            {
                                BookmarkCollection.Current.MoveToChild(bookmarkEntry, target.Source as TreeListNode<IBookmarkEntry>);
                            }
                            e.Effects = DragDropEffects.Move;
                            e.Handled = true;
                            return;
                        }
                    }
                }
            }

            e.Effects = DragDropEffects.None;
            e.Handled = true;
        }

        private ListBoxItem PointToViewItem(ListBox coltrol, Point point)
        {
            var element = VisualTreeHelper.HitTest(coltrol, point)?.VisualHit;

            if (!(element is ListBoxItem))
            {
                element = VisualTreeUtility.GetParentElement<ListBoxItem>(element) ?? _lastDropTarget;
            }

            _lastDropTarget = element;

            return _lastDropTarget as ListBoxItem;
        }

        #endregion

        #region Methods

        /// <summary>
        /// フォーカス取得
        /// </summary>
        /// <param name="isFocus"></param>
        public void FocusSelectedItem(bool isFocus)
        {
            _lastFocusRequest = isFocus;

            if (this.ListBox.SelectedIndex < 0) this.ListBox.SelectedIndex = 0;
            if (this.ListBox.SelectedIndex < 0) return;

            // 選択項目が表示されるようにスクロール
            this.ListBox.ScrollIntoView(this.ListBox.SelectedItem);

            if (isFocus && this.IsFocusEnabled)
            {
                ListBoxItem lbi = (ListBoxItem)(this.ListBox.ItemContainerGenerator.ContainerFromIndex(this.ListBox.SelectedIndex));
                lbi?.Focus();
            }
        }

        //
        public void SelectedChanging(object sender, SelectedChangedEventArgs e)
        {
            StoreFocus();
        }

        //
        public void SelectedChanged(object sender, SelectedChangedEventArgs e)
        {
            if (e.IsFocus)
            {
                FocusSelectedItem(true);
            }
            else
            {
                RestoreFocus();
            }

            if (e.IsNewFolder)
            {
                Rename();
            }
        }

        /// <summary>
        /// 選択項目フォーカス状態を取得
        /// リスト項目変更前処理。
        /// </summary>
        public void StoreFocus()
        {
            var index = this.ListBox.SelectedIndex;

            ListBoxItem lbi = index >= 0 ? (ListBoxItem)(this.ListBox.ItemContainerGenerator.ContainerFromIndex(index)) : null;
            _storeFocus = lbi != null ? lbi.IsFocused : false;
        }

        /// <summary>
        /// 選択項目フォーカス反映
        /// リスト変更後処理。
        /// </summary>
        /// <param name="isFocused"></param>
        public void RestoreFocus()
        {
            if (_storeFocus)
            {
                this.ListBox.ScrollIntoView(this.ListBox.SelectedItem);

                var index = this.ListBox.SelectedIndex;
                var lbi = index >= 0 ? (ListBoxItem)(this.ListBox.ItemContainerGenerator.ContainerFromIndex(index)) : null;
                var isSuccess = lbi?.Focus();
            }

            _thumbnailLoader.Load();
        }

        /// <summary>
        /// スクロール変更イベント処理
        /// </summary>
        private void ListBox_ScrollChanged(object sender, ScrollChangedEventArgs e)
        {
            // リネームキャンセル
            ((MainWindow)App.Current.MainWindow).RenameManager.Stop();
        }

        //
        private void FolderList_Loaded(object sender, RoutedEventArgs e)
        {
            FocusSelectedItem(_lastFocusRequest);
        }

        private void FolderList_PreviewKeyDown(object sender, System.Windows.Input.KeyEventArgs e)
        {
            if (e.Key == Key.Home)
            {
                _vm.MoveToHome.Execute(null);
                e.Handled = true;
            }
            else if (Keyboard.Modifiers == ModifierKeys.Alt)
            {
                Key key = e.Key == Key.System ? e.SystemKey : e.Key;

                if (key == Key.Up)
                {
                    _vm.MoveToUp.Execute(null);
                    e.Handled = true;
                }
                else if (key == Key.Left)
                {
                    _vm.MoveToPrevious.Execute(null);
                    e.Handled = true;
                }
                else if (key == Key.Right)
                {
                    _vm.MoveToNext.Execute(null);
                    e.Handled = true;
                }
            }
        }

        private void FolderList_KeyDown(object sender, System.Windows.Input.KeyEventArgs e)
        {
            bool isLRKeyEnabled = SidePanelProfile.Current.IsLeftRightKeyEnabled;

            if ((isLRKeyEnabled && e.Key == Key.Left) || e.Key == Key.Back) // Backspace
            {
                _vm.MoveToUp.Execute(null);
                e.Handled = true;
            }
        }

        private void FolderList_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            var listBox = sender as ListBox;
            if (listBox != null && listBox.IsLoaded)
            {
                // 選択項目が表示されるようにスクロール
                listBox.ScrollIntoView(listBox.SelectedItem);
            }
        }

        //
        private void FolderListItem_MouseLeftButtonDown(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            // 切り替え直後のクリックを無効にするためのフラグ
            _isClickEnabled = true;
        }

        //
        private void FolderListItem_MouseLeftButtonUp(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            if (!_isClickEnabled)
            {
                return;
            }

            var folderInfo = (sender as ListBoxItem)?.Content as FolderItem;
            if (folderInfo != null && !folderInfo.IsEmpty)
            {
                _vm.Model.LoadBook(folderInfo.TargetPath);
                e.Handled = true;
            }
        }

        //
        private void FolderListItem_MouseDoubleClick(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            var folderInfo = (sender as ListBoxItem)?.Content as FolderItem;
            _vm.MoveToSafety(folderInfo);

            e.Handled = true;
        }

        //
        private void FolderListItem_KeyDown(object sender, System.Windows.Input.KeyEventArgs e)
        {
            bool isLRKeyEnabled = SidePanelProfile.Current.IsLeftRightKeyEnabled;
            var folderInfo = (sender as ListBoxItem)?.Content as FolderItem;

            if (e.Key == Key.Return)
            {
                _vm.Model.LoadBook(folderInfo.TargetPath);
                e.Handled = true;
            }
            else if (isLRKeyEnabled && e.Key == Key.Right) // →
            {
                _vm.MoveToSafety(folderInfo);
                e.Handled = true;
            }
            else if ((isLRKeyEnabled && e.Key == Key.Left) || e.Key == Key.Back) // ← Backspace
            {
                if (folderInfo != null)
                {
                    _vm.MoveToUp.Execute(null);
                }
                e.Handled = true;
            }
        }


        //
        private void FolderListItem_MouseDown(object sender, MouseButtonEventArgs e)
        {
            var folderInfo = (sender as ListBoxItem)?.Content as FolderItem;
            if (folderInfo == null) return;

            // 一時的にドラッグ禁止
            ////_vm.Drag_MouseDown(sender, e, folderInfo);
        }

        //
        private void FolderListItem_MouseUp(object sender, MouseButtonEventArgs e)
        {
            // 一時的にドラッグ禁止
            ////_vm.Drag_MouseUp(sender, e);
        }

        //
        private void FolderListItem_MouseMove(object sender, MouseEventArgs e)
        {
            // 一時的にドラッグ禁止
            ////_vm.Drag_MouseMove(sender, e);
        }


        /// <summary>
        /// コンテキストメニュー開始前イベント
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void FolderListItem_ContextMenuOpening(object sender, ContextMenuEventArgs e)
        {
            var container = sender as ListBoxItem;
            if (container == null)
            {
                return;
            }

            var item = container.Content as FolderItem;
            if (item == null)
            {
                return;
            }

            // サブフォルダー読み込みの状態を更新
            var isDefaultRecursive = _vm.FolderCollection != null ? _vm.FolderCollection.FolderParameter.IsFolderRecursive : false;
            item.UpdateIsRecursived(isDefaultRecursive);

            // コンテキストメニュー生成

            var contextMenu = container.ContextMenu;
            if (contextMenu == null)
            {
                return;
            }

            contextMenu.Items.Clear();

            if (item.Attributes.HasFlag(FolderItemAttribute.Bookmark))
            {
                if (item.IsDirectory)
                {
                    contextMenu.Items.Add(new MenuItem() { Header = Properties.Resources.FolderListItemMenuDelete, Command = RemoveCommand });
                    contextMenu.Items.Add(new MenuItem() { Header = Properties.Resources.FolderListItemMenuRename, Command = RenameCommand });
                }
                else
                {
                    contextMenu.Items.Add(new MenuItem() { Header = Properties.Resources.FolderListItemMenuDelete, Command = RemoveCommand });
                    contextMenu.Items.Add(new Separator());
                    contextMenu.Items.Add(new MenuItem() { Header = Properties.Resources.FolderListItemMenuExplorer, Command = OpenExplorerCommand });
                    contextMenu.Items.Add(new MenuItem() { Header = Properties.Resources.FolderListItemMenuCopy, Command = CopyCommand });
                    ////contextMenu.Items.Add(new Separator());
                    ////contextMenu.Items.Add(new MenuItem() { Header = Properties.Resources.FolderListItemMenuDelete, Command = RemoveCommand });
                    ////contextMenu.Items.Add(new MenuItem() { Header = Properties.Resources.FolderListItemMenuRename, Command = RenameCommand });
                }
            }
            else
            {
                contextMenu.Items.Add(new MenuItem() { Header = Properties.Resources.FolderListItemMenuSubfolder, Command = LoadWithRecursiveCommand, IsChecked = item.IsRecursived });
                contextMenu.Items.Add(new Separator());
                contextMenu.Items.Add(new MenuItem() { Header = Properties.Resources.FolderListItemMenuExplorer, Command = OpenExplorerCommand });
                contextMenu.Items.Add(new MenuItem() { Header = Properties.Resources.FolderListItemMenuCopy, Command = CopyCommand });
                contextMenu.Items.Add(new Separator());
                contextMenu.Items.Add(new MenuItem() { Header = Properties.Resources.FolderListItemMenuDelete, Command = RemoveCommand });
                contextMenu.Items.Add(new MenuItem() { Header = Properties.Resources.FolderListItemMenuRename, Command = RenameCommand });
            }
        }

        #endregion

    }
}
