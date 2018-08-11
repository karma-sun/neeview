using NeeLaboratory.Windows.Input;
using NeeLaboratory.Windows.Media;
using NeeView.Collections;
using NeeView.Collections.Generic;
using NeeView.Windows;
using System;
using System.Collections.Generic;
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
    // HACK: FolderList.Current の除外。MVVMの依存関係がおかしいので。DependencyPropertyで対応可能？

    /// <summary>
    /// FolderListBox.xaml の相互作用ロジック
    /// </summary>
    public partial class FolderListBox : UserControl, IPageListPanel
    {
        #region Fields

        public static string DragDropFormat = $"{Config.Current.ProcessId}.FolderListBox";

        private FolderListBoxViewModel _vm;
        private ListBoxThumbnailLoader _thumbnailLoader;
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
        public FolderListBox(FolderListBoxViewModel vm) : this()
        {
            _vm = vm;
            this.DataContext = vm;

            InitializeCommand();

            // タッチスクロール操作の終端挙動抑制
            this.ListBox.ManipulationBoundaryFeedback += SidePanel.Current.ScrollViewer_ManipulationBoundaryFeedback;

            this.ListBox.AddHandler(ScrollViewer.ScrollChangedEvent, new ScrollChangedEventHandler(ListBox_ScrollChanged));
            _thumbnailLoader = new ListBoxThumbnailLoader(this, QueueElementPriority.FolderThumbnail);

            this.Loaded += FolderListBox_Loaded;
            this.Unloaded += FolderListBox_Unloaded;
        }

        private void FolderListBox_Loaded(object sender, RoutedEventArgs e)
        {
            _vm.Loaded();
            _vm.SelectedChanging += SelectedChanging;
            _vm.SelectedChanged += SelectedChanged;
        }

        private void FolderListBox_Unloaded(object sender, RoutedEventArgs e)
        {
            _vm.Unloaded();
            _vm.SelectedChanging -= SelectedChanging;
            _vm.SelectedChanged -= SelectedChanged;
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
        public bool IsThumbnailVisibled => FolderList.Current.IsThumbnailVisibled;

        public IEnumerable<IHasPage> CollectPageList(IEnumerable<object> objs) => objs.OfType<IHasPage>();

        #endregion

        #region Commands

        public static readonly RoutedCommand LoadWithRecursiveCommand = new RoutedCommand("LoadWithRecursiveCommand", typeof(FolderListBox));
        public static readonly RoutedCommand OpenCommand = new RoutedCommand("OpenCommand", typeof(FolderListBox));
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
            this.ListBox.CommandBindings.Add(new CommandBinding(OpenCommand, Open_Executed));
            this.ListBox.CommandBindings.Add(new CommandBinding(OpenExplorerCommand, OpenExplorer_Executed));
            this.ListBox.CommandBindings.Add(new CommandBinding(CopyCommand, Copy_Executed, Copy_CanExecute));
            this.ListBox.CommandBindings.Add(new CommandBinding(RemoveCommand, Remove_Executed, Remove_CanExecute));
            this.ListBox.CommandBindings.Add(new CommandBinding(RenameCommand, Rename_Executed, Rename_CanExecute));
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
                    : ArchiverManager.Current.GetSupportedType(item.TargetPath.SimplePath).IsRecursiveSupported();
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
            _vm.Model.LoadBook(item, option);
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


        public void Remove_CanExecute(object sender, CanExecuteRoutedEventArgs e)
        {
            var item = (sender as ListBox)?.SelectedItem as FolderItem;
            e.CanExecute = CanRemoveExecute(item);
        }

        private bool CanRemoveExecute(FolderItem item)
        {
            if (item == null || !item.IsEditable)
            {
                return false;
            }
            else if (item.IsFileSystem())
            {
                return FileIOProfile.Current.IsEnabled;
            }
            else if (item.Attributes.HasFlag(FolderItemAttribute.Bookmark | FolderItemAttribute.Directory))
            {
                return true;
            }
            else if (item.Attributes.HasFlag(FolderItemAttribute.Pagemark | FolderItemAttribute.Directory))
            {
                return true;
            }

            return false;
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
            if (item.Attributes.HasFlag(FolderItemAttribute.Pagemark))
            {
                _vm.Model.RemovePagemark(item);
            }
            else if (item.IsFileSystem())
            {
                var removed = await FileIO.Current.RemoveAsync(item.Path.SimplePath, Properties.Resources.DialogFileDeleteBookTitle);
                if (removed)
                {
                    _vm.FolderCollection?.RequestDelete(item.Path);
                }
            }
        }


        public void Rename_CanExecute(object sender, CanExecuteRoutedEventArgs e)
        {
            var item = (sender as ListBox)?.SelectedItem as FolderItem;
            e.CanExecute = CanRenameExecute(item);
        }

        private bool CanRenameExecute(FolderItem item)
        {
            if (item == null || !item.IsEditable)
            {
                return false;
            }
            else if (item.IsFileSystem())
            {
                return FileIOProfile.Current.IsEnabled;
            }
            else if (item.Attributes.HasFlag(FolderItemAttribute.Bookmark | FolderItemAttribute.Directory))
            {
                return true;
            }
            else if (item.Attributes.HasFlag(FolderItemAttribute.Pagemark | FolderItemAttribute.Directory))
            {
                return true;
            }

            return false;
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

            if (CanRenameExecute(item))
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
                        if (item.Source is TreeListNode<IBookmarkEntry> bookmarkNode)
                        {
                            BookmarkCollectionService.Rename(bookmarkNode, ev.NewValue);
                        }
                        else if (item.Source is TreeListNode<IPagemarkEntry> pagemarkNode)
                        {
                            PagemarkCollectionService.Rename(pagemarkNode, ev.NewValue);
                        }
                        else if (ev.OldValue != ev.NewValue)
                        {
                            var newName = item.IsShortcut ? ev.NewValue + ".lnk" : ev.NewValue;
                            //Debug.WriteLine($"{ev.OldValue} => {newName}");
                            var src = item.Path;
                            var dst = await FileIO.Current.RenameAsync(item, newName);
                            if (dst != null)
                            {
                                _vm.FolderCollection?.RequestRename(src, new QueryPath(dst));
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
                        FolderList.Current.IsRenaming = false;
                    };

                    FolderList.Current.IsRenaming = true;
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
                _vm.Model.LoadBook(item);
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
                var path = item.IsFileSystem() ? item.Path.SimplePath : item.TargetPath.SimplePath;
                path = item.Attributes.AnyFlag(FolderItemAttribute.Bookmark | FolderItemAttribute.ArchiveEntry | FolderItemAttribute.Empty) ? ArchiverManager.Current.GetExistPathName(path) : path;
                System.Diagnostics.Process.Start("explorer.exe", "/select,\"" + path + "\"");
            }
        }

        public void Open_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            var item = (sender as ListBox)?.SelectedItem as FolderItem;
            if (item != null)
            {
                _vm.Model.MoveToSafety(item);
            }
        }

        private RelayCommand _NewFolderCommand;
        public RelayCommand NewFolderCommand
        {
            get { return _NewFolderCommand = _NewFolderCommand ?? new RelayCommand(NewFolderCommand_Executed); }
        }

        private void NewFolderCommand_Executed()
        {
            _vm.Model.NewFolder();
        }

        private RelayCommand _AddBookmarkCommand;
        public RelayCommand AddBookmarkCommand
        {
            get { return _AddBookmarkCommand = _AddBookmarkCommand ?? new RelayCommand(AddBookmarkCommand_Executed); }
        }

        private void AddBookmarkCommand_Executed()
        {
            _vm.Model.AddBookmark();
        }



        #endregion

        #region DragDrop

        private void DragStartBehavior_DragBegin(object sender, Windows.DragStartEventArgs e)
        {
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

            if (item.Attributes.HasFlag(FolderItemAttribute.Empty))
            {
                e.Cancel = true;
                return;
            }


            if (item.Attributes.AnyFlag(FolderItemAttribute.Bookmark))
            {
                e.Data.SetData(item.Source);
                e.Data.SetData(item.TargetPath);
                e.AllowedEffects |= DragDropEffects.Move;
                return;
            }

            if (item.Attributes.AnyFlag(FolderItemAttribute.Pagemark))
            {
                if (item.Attributes.AnyFlag(FolderItemAttribute.ReadOnly))
                {
                    e.Data.SetData(item.Source);
                    ////e.Data.SetData(item.TargetPath);
                    return;
                }
                else
                {
                    e.Data.SetData(item.Source);
                    ////e.Data.SetData(item.TargetPath);
                    e.AllowedEffects |= DragDropEffects.Move;
                }
                return;
            }

            if (item.IsFileSystem())
            {
                e.Data.SetFileDropList(new System.Collections.Specialized.StringCollection() { item.TargetPath.SimplePath });
                return;
            }
        }

        private void FolderList_DragEnter(object sender, DragEventArgs e)
        {
            FolderList_DragDrop(sender, e, false);
            DragDropHelper.AutoScroll(sender, e);
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

            var dragData = e.Data.GetData<ListBoxItem>(DragDropFormat);
            if (dragData != null)
            {
                if (listBoxItem == null || listBoxItem == dragData)
                {
                    e.Effects = DragDropEffects.None;
                    e.Handled = true;
                    return;
                }
            }

            // bookmark
            {
                TreeListNode<IBookmarkEntry> bookmarkNode = null;
                if (listBoxItem == null)
                {
                    if (_vm.FolderCollection is BookmarkFolderCollection bookmarkFolderCollection)
                    {
                        bookmarkNode = bookmarkFolderCollection.BookmarkPlace;
                    }
                }
                else
                {
                    if (listBoxItem.Content is FolderItem target && target.Attributes.HasFlag(FolderItemAttribute.Bookmark | FolderItemAttribute.Directory))
                    {
                        bookmarkNode = target.Source as TreeListNode<IBookmarkEntry>;
                    }
                }

                if (bookmarkNode != null)
                {
                    DropToBookmark(sender, e, isDrop, bookmarkNode, e.Data.GetData<TreeListNode<IBookmarkEntry>>());
                    if (e.Handled) return;

                    DropToBookmark(sender, e, isDrop, bookmarkNode, e.Data.GetData<QueryPath>());
                    if (e.Handled) return;

                    DropToBookmark(sender, e, isDrop, bookmarkNode, e.Data.GetFileDrop());
                    if (e.Handled) return;
                }
            }

            // pagemark
            {
                TreeListNode<IPagemarkEntry> pagemarkNode = null;
                if (listBoxItem == null)
                {
                    if (_vm.FolderCollection is PagemarkFolderCollection pagemarkFolderCollection)
                    {
                        pagemarkNode = pagemarkFolderCollection.PagemarkPlace;
                    }
                }
                else
                {
                    if (listBoxItem.Content is FolderItem target && target.Attributes.HasFlag(FolderItemAttribute.Pagemark | FolderItemAttribute.Directory))
                    {
                        pagemarkNode = target.Source as TreeListNode<IPagemarkEntry>;
                    }
                }

                if (pagemarkNode != null)
                {
                    DropToPagemark(sender, e, isDrop, pagemarkNode, e.Data.GetData<TreeListNode<IPagemarkEntry>>());
                    if (e.Handled) return;
                }
            }

            e.Effects = DragDropEffects.None;
            e.Handled = true;
        }

        private void DropToBookmark(object sender, DragEventArgs e, bool isDrop, TreeListNode<IBookmarkEntry> node, TreeListNode<IBookmarkEntry> bookmarkEntry)
        {
            if (bookmarkEntry == null)
            {
                return;
            }

            if (!node.Children.Contains(bookmarkEntry) && !node.ParentContains(bookmarkEntry))
            {
                if (isDrop)
                {
                    _vm.Model.SelectBookmark(node, true);
                    BookmarkCollection.Current.MoveToChild(bookmarkEntry, node);
                }
                e.Effects = DragDropEffects.Move;
                e.Handled = true;
            }
        }

        private void DropToBookmark(object sender, DragEventArgs e, bool isDrop, TreeListNode<IBookmarkEntry> node, QueryPath query)
        {
            if (query == null)
            {
                return;
            }

            if (node.Value is BookmarkFolder && query.Scheme == QueryScheme.File && query.Search == null)
            {
                if (isDrop)
                {
                    var bookmark = BookmarkCollectionService.AddToChild(node, query);
                    _vm.Model.SelectBookmark(bookmark, true);
                }
                e.Effects = DragDropEffects.Copy;
                e.Handled = true;
            }
        }

        private void DropToBookmark(object sender, DragEventArgs e, bool isDrop, TreeListNode<IBookmarkEntry> node, string[] fileNames)
        {
            if (fileNames == null)
            {
                return;
            }
            if ((e.AllowedEffects & DragDropEffects.Copy) != DragDropEffects.Copy)
            {
                return;
            }

            bool isDropped = false;
            foreach (var fileName in fileNames)
            {
                if (ArchiverManager.Current.IsSupported(fileName, true, true) || System.IO.Directory.Exists(fileName))
                {
                    if (isDrop)
                    {
                        var bookmark = BookmarkCollectionService.AddToChild(node, new QueryPath(fileName));
                        _vm.Model.SelectBookmark(bookmark, true);
                    }
                    isDropped = true;
                }
            }
            if (isDropped)
            {
                e.Effects = DragDropEffects.Copy;
                e.Handled = true;
            }
        }


        private void DropToPagemark(object sender, DragEventArgs e, bool isDrop, TreeListNode<IPagemarkEntry> node, TreeListNode<IPagemarkEntry> pagemarkEntry)
        {
            if (pagemarkEntry == null)
            {
                return;
            }

            if (node == PagemarkCollection.Current.Items && pagemarkEntry.Value is Pagemark)
            {
                return;
            }

            if (e.AllowedEffects.HasFlag(DragDropEffects.Move) && !node.Children.Contains(pagemarkEntry) && !node.ParentContains(pagemarkEntry))
            {
                if (isDrop)
                {
                    _vm.Model.SelectPagemark(node, true);
                    PagemarkCollection.Current.MoveToChild(pagemarkEntry, node);
                }
                e.Effects = DragDropEffects.Move;
                e.Handled = true;
            }
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

        #endregion

        #region Methods

        /// <summary>
        /// フォーカス取得
        /// </summary>
        /// <param name="isFocus"></param>
        public void FocusSelectedItem(bool isFocus)
        {
            if (this.ListBox.SelectedIndex < 0) this.ListBox.SelectedIndex = 0;
            if (this.ListBox.SelectedIndex < 0) return;

            // 選択項目が表示されるようにスクロール
            this.ListBox.ScrollIntoView(this.ListBox.SelectedItem);

            if (this.ListBox.IsLoaded && ((isFocus && this.IsFocusEnabled) || _vm.Model.IsFocusAtOnce))
            {
                _vm.Model.IsFocusAtOnce = false;
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

            _thumbnailLoader.Load();

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
            FocusSelectedItem(false);
        }

        private void FolderList_IsVisibleChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            if ((bool)e.NewValue == true)
            {
                FocusSelectedItem(false);
            }
        }

        private void FolderList_ContextMenuOpening(object sender, ContextMenuEventArgs e)
        {
            var items = this.ListBox.ContextMenu.Items;
            items.Clear();

            if (_vm.FolderCollection is BookmarkFolderCollection)
            {
                items.Add(new MenuItem() { Header = Properties.Resources.FolderTreeMenuAddBookmark, Command = AddBookmarkCommand });
                items.Add(new MenuItem() { Header = Properties.Resources.WordNewFolder, Command = NewFolderCommand });
            }
            else if (_vm.FolderCollection is PagemarkFolderCollection)
            {
                items.Add(new MenuItem() { Header = Properties.Resources.WordNewFolder, Command = NewFolderCommand });
            }
        }

        private void FolderList_PreviewKeyDown(object sender, System.Windows.Input.KeyEventArgs e)
        {
            if (e.Key == Key.Home)
            {
                _vm.Model.MoveToHome();
                e.Handled = true;
            }
            else
            if (Keyboard.Modifiers == ModifierKeys.Alt)
            {
                Key key = e.Key == Key.System ? e.SystemKey : e.Key;

                if (key == Key.Up)
                {
                    _vm.Model.MoveToUp();
                    e.Handled = true;
                }
                else if (key == Key.Down)
                {
                    var item = (sender as ListBox)?.SelectedItem as FolderItem;
                    if (item != null)
                    {
                        _vm.Model.MoveToSafety(item);
                        e.Handled = true;
                    }
                }

                else if (key == Key.Left)
                {
                    _vm.Model.MoveToPrevious();
                    e.Handled = true;
                }
                else if (key == Key.Right)
                {
                    _vm.Model.MoveToNext();
                    e.Handled = true;
                }
            }
        }

        private void FolderList_KeyDown(object sender, System.Windows.Input.KeyEventArgs e)
        {
            bool isLRKeyEnabled = SidePanelProfile.Current.IsLeftRightKeyEnabled && FolderList.Current.PanelListItemStyle != PanelListItemStyle.Thumbnail;

            if ((isLRKeyEnabled && e.Key == Key.Left) || e.Key == Key.Back) // ←, Backspace
            {
                _vm.Model.MoveToUp();
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

            var item = (sender as ListBoxItem)?.Content as FolderItem;
            if (item != null && !item.IsEmpty())
            {
                _vm.Model.LoadBook(item);
                e.Handled = true;
            }
        }

        //
        private void FolderListItem_MouseDoubleClick(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            var item = (sender as ListBoxItem)?.Content as FolderItem;
            _vm.Model.MoveToSafety(item);

            e.Handled = true;
        }

        //
        private void FolderListItem_KeyDown(object sender, System.Windows.Input.KeyEventArgs e)
        {
            bool isLRKeyEnabled = SidePanelProfile.Current.IsLeftRightKeyEnabled && FolderList.Current.PanelListItemStyle != PanelListItemStyle.Thumbnail;
            var item = (sender as ListBoxItem)?.Content as FolderItem;

            if (e.Key == Key.Return)
            {
                _vm.Model.LoadBook(item);
                e.Handled = true;
            }
            else if (isLRKeyEnabled && e.Key == Key.Right) // →
            {
                _vm.Model.MoveToSafety(item);
                e.Handled = true;
            }
            else if ((isLRKeyEnabled && e.Key == Key.Left) || e.Key == Key.Back) // ←, Backspace
            {
                if (item != null)
                {
                    _vm.Model.MoveToUp();
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


            if (item.Attributes.HasFlag(FolderItemAttribute.System))
            {
                contextMenu.Items.Add(new MenuItem() { Header = Properties.Resources.BookshelfItemMenuOpen, Command = OpenCommand });
            }
            else if (item.Attributes.HasFlag(FolderItemAttribute.Bookmark))
            {
                if (item.IsDirectory)
                {
                    contextMenu.Items.Add(new MenuItem() { Header = Properties.Resources.BookshelfItemMenuDelete, Command = RemoveCommand });
                    contextMenu.Items.Add(new MenuItem() { Header = Properties.Resources.BookshelfItemMenuRename, Command = RenameCommand });
                }
                else
                {
                    contextMenu.Items.Add(new MenuItem() { Header = Properties.Resources.BookshelfItemMenuDeleteBookmark, Command = RemoveCommand });
                    contextMenu.Items.Add(new Separator());
                    contextMenu.Items.Add(new MenuItem() { Header = Properties.Resources.BookshelfItemMenuExplorer, Command = OpenExplorerCommand });
                    contextMenu.Items.Add(new MenuItem() { Header = Properties.Resources.BookshelfItemMenuCopy, Command = CopyCommand });
                    ////contextMenu.Items.Add(new Separator());
                    ////contextMenu.Items.Add(new MenuItem() { Header = Properties.Resources.BookshelfItemMenuDelete, Command = RemoveCommand });
                    ////contextMenu.Items.Add(new MenuItem() { Header = Properties.Resources.BookshelfItemMenuRename, Command = RenameCommand });
                }
            }
            else if (item.Attributes.HasFlag(FolderItemAttribute.Pagemark))
            {
                contextMenu.Items.Add(new MenuItem() { Header = Properties.Resources.BookshelfItemMenuSubfolder, Command = LoadWithRecursiveCommand, IsChecked = item.IsRecursived });
                contextMenu.Items.Add(new Separator());
                if (item.Source != PagemarkCollection.Current.Items)
                {
                    contextMenu.Items.Add(new MenuItem() { Header = Properties.Resources.BookshelfItemMenuDelete, Command = RemoveCommand });
                    contextMenu.Items.Add(new MenuItem() { Header = Properties.Resources.BookshelfItemMenuRename, Command = RenameCommand });
                }
            }
            else if (item.Attributes.HasFlag(FolderItemAttribute.Empty))
            {
                bool canExplorer = !(_vm.FolderCollection is BookmarkFolderCollection);
                contextMenu.Items.Add(new MenuItem() { Header = Properties.Resources.BookshelfItemMenuExplorer, Command = OpenExplorerCommand, IsEnabled = canExplorer });
                contextMenu.Items.Add(new MenuItem() { Header = Properties.Resources.BookshelfItemMenuCopy, Command = CopyCommand, IsEnabled = false });
            }
            else if (item.IsFileSystem())
            {
                contextMenu.Items.Add(new MenuItem() { Header = Properties.Resources.BookshelfItemMenuSubfolder, Command = LoadWithRecursiveCommand, IsChecked = item.IsRecursived });
                contextMenu.Items.Add(new Separator());
                contextMenu.Items.Add(new MenuItem() { Header = Properties.Resources.BookshelfItemMenuExplorer, Command = OpenExplorerCommand });
                contextMenu.Items.Add(new MenuItem() { Header = Properties.Resources.BookshelfItemMenuCopy, Command = CopyCommand });
                contextMenu.Items.Add(new Separator());
                contextMenu.Items.Add(new MenuItem() { Header = Properties.Resources.BookshelfItemMenuDelete, Command = RemoveCommand });
                contextMenu.Items.Add(new MenuItem() { Header = Properties.Resources.BookshelfItemMenuRename, Command = RenameCommand });
            }
        }

        #endregion
    }
}
