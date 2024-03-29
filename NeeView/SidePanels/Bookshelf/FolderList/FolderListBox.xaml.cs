﻿using NeeLaboratory.Windows.Input;
using NeeView.Windows.Media;
using NeeView.Collections.Generic;
using NeeView.Windows;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media.Animation;

namespace NeeView
{
    /// <summary>
    /// FolderListBox.xaml の相互作用ロジック
    /// </summary>
    public partial class FolderListBox : UserControl, IPageListPanel, IDisposable
    {
        private FolderListBoxViewModel _vm;
        private ListBoxThumbnailLoader _thumbnailLoader;
        private PageThumbnailJobClient _jobClient;


        static FolderListBox()
        {
            InitialieCommandStatic();
        }

        public FolderListBox()
        {
            InitializeComponent();
        }

        public FolderListBox(FolderListBoxViewModel vm) : this()
        {
            _vm = vm;
            this.DataContext = vm;

            InitializeCommand();

            // タッチスクロール操作の終端挙動抑制
            this.ListBox.ManipulationBoundaryFeedback += SidePanelFrame.Current.ScrollViewer_ManipulationBoundaryFeedback;

            this.ListBox.AddHandler(ScrollViewer.ScrollChangedEvent, new ScrollChangedEventHandler(ListBox_ScrollChanged));


            this.Loaded += FolderListBox_Loaded;
            this.Unloaded += FolderListBox_Unloaded;

            if (_vm.FolderCollection is BookmarkFolderCollection)
            {
                var menu = new ContextMenu();
                menu.Items.Add(new MenuItem() { Header = Properties.Resources.FolderTree_Menu_AddBookmark, Command = AddBookmarkCommand });
                menu.Items.Add(new MenuItem() { Header = Properties.Resources.Word_NewFolder, Command = NewFolderCommand });
                this.ListBox.ContextMenu = menu;
            }
        }


        // フォーカス可能フラグ
        public bool IsFocusEnabled { get; set; } = true;


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

        #region IPanelListBox Support

        //
        public ListBox PageCollectionListBox => this.ListBox;

        // サムネイルが表示されている？
        public bool IsThumbnailVisibled => _vm.IsThumbnailVisibled;

        public IEnumerable<IHasPage> CollectPageList(IEnumerable<object> objs) => objs.OfType<IHasPage>();

        #endregion

        #region Commands

        public static readonly RoutedCommand LoadWithRecursiveCommand = new RoutedCommand("LoadWithRecursiveCommand", typeof(FolderListBox));
        public static readonly RoutedCommand OpenCommand = new RoutedCommand("OpenCommand", typeof(FolderListBox));
        public static readonly RoutedCommand OpenBookCommand = new RoutedCommand("OpenBookCommand", typeof(FolderListBox));
        public static readonly RoutedCommand OpenExplorerCommand = new RoutedCommand("OpenExplorerCommand", typeof(FolderListBox));
        public static readonly RoutedCommand OpenExternalAppCommand = new RoutedCommand("OpenExternalAppCommand", typeof(FolderListBox));
        public static readonly RoutedCommand CopyCommand = new RoutedCommand("CopyCommand", typeof(FolderListBox));
        public static readonly RoutedCommand CopyToFolderCommand = new RoutedCommand("CopyToFolderCommand", typeof(FolderListBox));
        public static readonly RoutedCommand MoveToFolderCommand = new RoutedCommand("MoveToFolderCommand", typeof(FolderListBox));
        public static readonly RoutedCommand RemoveCommand = new RoutedCommand("RemoveCommand", typeof(FolderListBox));
        public static readonly RoutedCommand RenameCommand = new RoutedCommand("RenameCommand", typeof(FolderListBox));
        public static readonly RoutedCommand RemoveHistoryCommand = new RoutedCommand("RemoveHistoryCommand", typeof(FolderListBox));
        public static readonly RoutedCommand ToggleBookmarkCommand = new RoutedCommand("ToggleBookmarkCommand", typeof(FolderListBox));
        public static readonly RoutedCommand OpenDestinationFolderCommand = new RoutedCommand("OpenDestinationFolderCommand", typeof(FolderListBox));
        public static readonly RoutedCommand OpenExternalAppDialogCommand = new RoutedCommand("OpenExternalAppDialogCommand", typeof(FolderListBox));
        public static readonly RoutedCommand OpenInPlaylistCommand = new RoutedCommand("OpenInPlaylistCommand", typeof(FolderListBox));

        private static void InitialieCommandStatic()
        {
            OpenCommand.InputGestures.Add(new KeyGesture(Key.Down, ModifierKeys.Alt));
            OpenBookCommand.InputGestures.Add(new KeyGesture(Key.Enter));
            CopyCommand.InputGestures.Add(new KeyGesture(Key.C, ModifierKeys.Control));
            RemoveCommand.InputGestures.Add(new KeyGesture(Key.Delete));
            RenameCommand.InputGestures.Add(new KeyGesture(Key.F2));
            ToggleBookmarkCommand.InputGestures.Add(new KeyGesture(Key.D, ModifierKeys.Control));
        }

        private void InitializeCommand()
        {
            this.ListBox.CommandBindings.Add(new CommandBinding(LoadWithRecursiveCommand, LoadWithRecursive_Executed, LoadWithRecursive_CanExecute));
            this.ListBox.CommandBindings.Add(new CommandBinding(OpenCommand, Open_Executed));
            this.ListBox.CommandBindings.Add(new CommandBinding(OpenBookCommand, OpenBook_Executed));
            this.ListBox.CommandBindings.Add(new CommandBinding(OpenExplorerCommand, OpenExplorer_Executed, OpenExplorer_CanExecute));
            this.ListBox.CommandBindings.Add(new CommandBinding(OpenExternalAppCommand, OpenExternalApp_Executed, OpenExternalApp_CanExecute));
            this.ListBox.CommandBindings.Add(new CommandBinding(CopyCommand, Copy_Executed, Copy_CanExecute));
            this.ListBox.CommandBindings.Add(new CommandBinding(CopyToFolderCommand, CopyToFolder_Execute, CopyToFolder_CanExecute));
            this.ListBox.CommandBindings.Add(new CommandBinding(MoveToFolderCommand, MoveToFolder_Execute, MoveToFolder_CanExecute));
            this.ListBox.CommandBindings.Add(new CommandBinding(RemoveCommand, Remove_Executed, Remove_CanExecute));
            this.ListBox.CommandBindings.Add(new CommandBinding(RenameCommand, Rename_Executed, Rename_CanExecute));
            this.ListBox.CommandBindings.Add(new CommandBinding(RemoveHistoryCommand, RemoveHistory_Executed, RemoveHistory_CanExecute));
            this.ListBox.CommandBindings.Add(new CommandBinding(ToggleBookmarkCommand, ToggleBookmark_Executed, ToggleBookmark_CanExecute));
            this.ListBox.CommandBindings.Add(new CommandBinding(OpenDestinationFolderCommand, OpenDestinationFolderDialog_Execute));
            this.ListBox.CommandBindings.Add(new CommandBinding(OpenExternalAppDialogCommand, OpenExternalAppDialog_Execute));
            this.ListBox.CommandBindings.Add(new CommandBinding(OpenInPlaylistCommand, OpenInPlaylistCommand_Execute));
        }

        /// <summary>
        /// ブックマーク登録/解除可能？
        /// </summary>
        private void ToggleBookmark_CanExecute(object sender, CanExecuteRoutedEventArgs e)
        {
            var item = (sender as ListBox)?.SelectedItem as FolderItem;
            e.CanExecute = item != null && item.IsFileSystem() && !item.EntityPath.SimplePath.StartsWith(Temporary.Current.TempDirectory);
        }

        /// <summary>
        /// ブックマーク登録/解除
        /// </summary>
        private void ToggleBookmark_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            var item = (sender as ListBox)?.SelectedItem as FolderItem;
            if (item != null)
            {
                if (BookmarkCollection.Current.Contains(item.EntityPath.SimplePath))
                {
                    BookmarkCollectionService.Remove(item.EntityPath);
                }
                else
                {
                    BookmarkCollectionService.Add(item.EntityPath);
                }
            }
        }

        /// <summary>
        /// 履歴から削除できる？
        /// </summary>
        private void RemoveHistory_CanExecute(object sender, CanExecuteRoutedEventArgs e)
        {
            var item = (sender as ListBox)?.SelectedItem as FolderItem;
            e.CanExecute = item != null && BookHistoryCollection.Current.Contains(item.TargetPath.SimplePath);
        }

        /// <summary>
        /// 履歴から削除
        /// </summary>
        private void RemoveHistory_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            var item = (sender as ListBox)?.SelectedItem as FolderItem;
            if (item != null)
            {
                BookHistoryCollection.Current.Remove(item.TargetPath.SimplePath);
            }

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
                : Config.Current.System.ArchiveRecursiveMode == ArchiveEntryCollectionMode.IncludeSubArchives
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
            e.CanExecute = (item != null && item.IsEditable && Config.Current.System.IsFileWriteAccessEnabled);
        }

        /// <summary>
        /// コピーコマンド実行可能判定
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Copy_CanExecute(object sender, CanExecuteRoutedEventArgs e)
        {
            var items = this.ListBox.SelectedItems.Cast<FolderItem>();
            e.CanExecute = items != null && items.All(x => x.IsEditable);
        }

        /// <summary>
        /// コピーコマンド実行
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        public void Copy_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            var items = this.ListBox.SelectedItems.Cast<FolderItem>();
            if (items != null && items.Any())
            {
                CopyToClipboard(items);
            }
        }

        /// <summary>
        /// クリップボードにコピー
        /// </summary>
        private void CopyToClipboard(IEnumerable<FolderItem> infos)
        {
            var collection = new System.Collections.Specialized.StringCollection();
            foreach (var item in infos.Where(e => !e.IsEmpty()).Select(e => e.EntityPath.SimplePath).Where(e => new QueryPath(e).Scheme == QueryScheme.File))
            {
                collection.Add(item);
            }

            if (collection.Count == 0)
            {
                return;
            }

            var data = new DataObject();
            data.SetFileDropList(collection);
            data.SetData(DataFormats.UnicodeText, string.Join(System.Environment.NewLine, collection.Cast<string>()));
            Clipboard.SetDataObject(data);
        }

        /// <summary>
        /// フォルダーにコピーコマンド用
        /// </summary>
        private void CopyToFolder_CanExecute(object sender, CanExecuteRoutedEventArgs e)
        {
            e.CanExecute = CopyToFolder_CanExecute();
        }

        private bool CopyToFolder_CanExecute()
        {
            var items = this.ListBox.SelectedItems.Cast<FolderItem>();
            return items != null && items.All(x => x.IsEditable);
        }

        public void CopyToFolder_Execute(object sender, ExecutedRoutedEventArgs e)
        {
            var folder = e.Parameter as DestinationFolder;
            if (folder == null) return;

            try
            {
                if (!Directory.Exists(folder.Path))
                {
                    throw new DirectoryNotFoundException();
                }

                var items = this.ListBox.SelectedItems.Cast<FolderItem>();
                if (items != null && items.Any())
                {
                    ////Debug.WriteLine($"CopyToFolder: to {folder.Path}");
                    FileIO.CopyToFolder(items.Select(x => x.TargetPath.SimplePath), folder.Path);
                }
            }
            catch (OperationCanceledException)
            {
            }
            catch (Exception ex)
            {
                ToastService.Current.Show(new Toast(ex.Message, Properties.Resources.Bookshelf_CopyToFolderFailed, ToastIcon.Error));
            }
        }

        /// <summary>
        /// フォルダーに移動コマンド用
        /// </summary>
        private void MoveToFolder_CanExecute(object sender, CanExecuteRoutedEventArgs e)
        {
            e.CanExecute = MoveToFolder_CanExecute();
        }

        private bool MoveToFolder_CanExecute()
        {
            var items = this.ListBox.SelectedItems.Cast<FolderItem>();
            return Config.Current.System.IsFileWriteAccessEnabled && items != null && items.All(x => x.IsEditable);
        }

        public async void MoveToFolder_Execute(object sender, ExecutedRoutedEventArgs e)
        {
            var folder = e.Parameter as DestinationFolder;
            if (folder == null) return;

            try
            {
                if (!Directory.Exists(folder.Path))
                {
                    throw new DirectoryNotFoundException();
                }

                var items = this.ListBox.SelectedItems.Cast<FolderItem>();
                if (items != null && items.Any())
                {
                    // 開いている本であるならば閉じる
                    if (items.Any(x => x.TargetPath.SimplePath == BookHub.Current.Address))
                    {
                        await BookHub.Current.RequestUnload(this, true).WaitAsync();
                        await ArchiverManager.Current.UnlockAllArchivesAsync();
                    }

                    ////Debug.WriteLine($"MoveToFolder: to {folder.Path}");
                    FileIO.MoveToFolder(items.Select(x => x.TargetPath.SimplePath), folder.Path);
                }
            }
            catch (OperationCanceledException)
            {
            }
            catch (Exception ex)
            {
                ToastService.Current.Show(new Toast(ex.Message, Properties.Resources.Bookshelf_Message_MoveToFolderFailed, ToastIcon.Error));
            }
        }


        public void Remove_CanExecute(object sender, CanExecuteRoutedEventArgs e)
        {
            var items = this.ListBox.SelectedItems.Cast<FolderItem>();
            e.CanExecute = items != null && !(_vm.FolderCollection is PlaylistFolderCollection) && items.All(x => x.CanRemove());
        }

        /// <summary>
        /// 削除コマンド実行
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        public async void Remove_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            var items = this.ListBox.SelectedItems.Cast<FolderItem>().ToList();
            await _vm.RemoveAsync(items);
            FocusSelectedItem(true);
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
            if (_vm.FolderCollection is PlaylistFolderCollection)
            {
                return false;
            }
            else if (item.IsFileSystem())
            {
                if (item.TargetPath.SimplePath.StartsWith(Temporary.Current.TempDirectory))
                {
                    return false;
                }
                else
                {
                    return Config.Current.System.IsFileWriteAccessEnabled;
                }
            }
            else if (item.Attributes.HasFlag(FolderItemAttribute.Bookmark | FolderItemAttribute.Directory))
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

                    if (item.IsFileSystem())
                    {
                        rename.IsSeleftFileNameBody = !item.IsDirectory;
                        rename.IsInvalidFileNameChars = true;
                    }
                    else if (item.Attributes.HasFlag(FolderItemAttribute.Bookmark | FolderItemAttribute.Directory))
                    {
                        rename.IsInvalidSeparatorChars = true;
                    }

                    rename.Closing += async (s, ev) =>
                    {
                        if (item.Source is TreeListNode<IBookmarkEntry> bookmarkNode)
                        {
                            BookmarkCollectionService.Rename(bookmarkNode, ev.NewValue);
                        }
                        else if (ev.OldValue != ev.NewValue)
                        {
                            var newName = item.IsHideExtension() ? ev.NewValue + System.IO.Path.GetExtension(item.Name) : ev.NewValue;
                            //Debug.WriteLine($"{ev.OldValue} => {newName}");
                            var src = item.TargetPath;
                            var dst = await FileIO.Current.RenameAsync(item, newName);
                            if (dst != null)
                            {
                                _vm.FolderCollection?.RequestRename(src, new QueryPath(dst));
                            }
                        }
                    };
                        rename.Closed += (s, ev) =>
                    {
                        RenameTools.RestoreFocus(listViewItem, ev.IsFocused);
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
                    RenameTools.GetRenameManager(this)?.Open(rename);
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
        /// エクスプローラーで開くコマンド
        /// </summary>
        private void OpenExplorer_CanExecute(object sender, CanExecuteRoutedEventArgs e)
        {
            var item = (sender as ListBox)?.SelectedItem as FolderItem;
            e.CanExecute = item != null;
        }

        public void OpenExplorer_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            var item = (sender as ListBox)?.SelectedItem as FolderItem;
            if (item != null)
            {
                var path = item.TargetPath.SimplePath;
                path = item.Attributes.AnyFlag(FolderItemAttribute.Bookmark | FolderItemAttribute.ArchiveEntry | FolderItemAttribute.Empty) ? ArchiverManager.Current.GetExistPathName(path) : path;
                ExternalProcess.Start("explorer.exe", "/select,\"" + path + "\"");
            }
        }

        /// <summary>
        /// 外部アプリで開く
        /// </summary>
        private void OpenExternalApp_CanExecute(object sender, CanExecuteRoutedEventArgs e)
        {
            e.CanExecute = CopyToFolder_CanExecute();
        }

        private bool OpenExternalApp_CanExecute()
        {
            var items = this.ListBox.SelectedItems.Cast<FolderItem>();
            return items != null && items.All(x => x.IsEditable);
        }

        public void OpenExternalApp_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            var externalApp = e.Parameter as ExternalApp;
            if (externalApp == null) return;

            var items = this.ListBox.SelectedItems.Cast<FolderItem>();
            if (items != null && items.Any())
            {
                var paths = items.Select(x => x.TargetPath.SimplePath).ToList();
                externalApp.Execute(paths);
            }
        }

        private string GetExistPathName(FolderItem item)
        {
            var path = item.TargetPath.SimplePath;
            return item.Attributes.AnyFlag(FolderItemAttribute.Bookmark | FolderItemAttribute.ArchiveEntry | FolderItemAttribute.Empty) ? ArchiverManager.Current.GetExistPathName(path) : path;
        }

        public void Open_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            var item = (sender as ListBox)?.SelectedItem as FolderItem;
            if (item != null)
            {
                _vm.MoveToSafety(item);
            }
        }

        public void OpenBook_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            var item = (sender as ListBox)?.SelectedItem as FolderItem;
            if (item != null && !item.IsEmpty())
            {
                _vm.Model.LoadBook(item);
            }
        }

        private void OpenDestinationFolderDialog_Execute(object sender, ExecutedRoutedEventArgs e)
        {
            DestinationFolderDialog.ShowDialog(Window.GetWindow(this));
        }

        private void OpenExternalAppDialog_Execute(object sender, ExecutedRoutedEventArgs e)
        {
            ExternalAppDialog.ShowDialog(Window.GetWindow(this));
        }

        private void OpenInPlaylistCommand_Execute(object sender, ExecutedRoutedEventArgs e)
        {
            var item = (sender as ListBox)?.SelectedItem as FolderItem;
            if (item != null && item.IsPlaylist)
            {
                Config.Current.Playlist.CurrentPlaylist = item.EntityPath.SimplePath;
                SidePanelFrame.Current.IsVisiblePlaylist = true;
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

        private async Task DragStartBehavior_DragBeginAsync(object sender, DragStartEventArgs e, CancellationToken token)
        {
            var items = this.ListBox.SelectedItems
                .Cast<FolderItem>()
                .Where(x => !x.Attributes.HasFlag(FolderItemAttribute.Empty))
                .ToList();

            if (!items.Any())
            {
                e.Cancel = true;
                return;
            }

            // List<QueryPath>
            e.Data.SetData(items.Select(x => x.TargetPath).ToQueryPathCollection());

            // bookmark?
            if (items.Any(x => x.Attributes.AnyFlag(FolderItemAttribute.Bookmark)))
            {
                var collection = items.Select(x => x.Source).OfType<TreeListNode<IBookmarkEntry>>().ToBookmarkNodeCollection();
                e.Data.SetData(collection);
                e.AllowedEffects |= DragDropEffects.Move;
            }
            // files only
            else
            {
                var collection = new System.Collections.Specialized.StringCollection();
                foreach (var path in items.Where(x => x.IsFileSystem()).Select(x => x.TargetPath.SimplePath).Distinct())
                {
                    collection.Add(path);
                }
                if (collection.Count > 0)
                {
                    e.Data.SetFileDropList(collection);

                    // 右クリックドラッグは移動を許可
                    if (Config.Current.System.IsFileWriteAccessEnabled && e.MouseEventArgs.RightButton == MouseButtonState.Pressed)
                    {
                        e.AllowedEffects |= DragDropEffects.Move;
                    }
                }
            }

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

            // bookmark
            if (_vm.FolderCollection is BookmarkFolderCollection bookmarkFolderCollection)
            {
                TreeListNode<IBookmarkEntry> bookmarkNode;
                if (listBoxItem?.Content is FolderItem target && target.Attributes.HasFlag(FolderItemAttribute.Bookmark | FolderItemAttribute.Directory))
                {
                    bookmarkNode = target.Source as TreeListNode<IBookmarkEntry>;
                }
                else
                {
                    bookmarkNode = bookmarkFolderCollection.BookmarkPlace;
                }

                if (bookmarkNode != null)
                {
                    DropToBookmark(sender, e, isDrop, bookmarkNode, e.Data.GetData<BookmarkNodeCollection>());
                    if (e.Handled) return;

                    DropToBookmark(sender, e, isDrop, bookmarkNode, e.Data.GetData<QueryPathCollection>());
                    if (e.Handled) return;

                    DropToBookmark(sender, e, isDrop, bookmarkNode, e.Data.GetFileDrop());
                    if (e.Handled) return;
                }
            }
        }

        private void DropToBookmark(object sender, DragEventArgs e, bool isDrop, TreeListNode<IBookmarkEntry> node, IEnumerable<TreeListNode<IBookmarkEntry>> bookmarkEntries)
        {
            if (bookmarkEntries == null || !bookmarkEntries.Any())
            {
                return;
            }

            e.Effects = bookmarkEntries.All(x => CanDropToBookmark(node, x)) ? DragDropEffects.Move : DragDropEffects.None;
            e.Handled = true;

            if (isDrop && e.Effects == DragDropEffects.Move)
            {
                foreach (var bookmarkEntry in bookmarkEntries)
                {
                    DropToBookmarkExecute(node, bookmarkEntry);
                }
            }
        }

        private void DropToBookmark(object sender, DragEventArgs e, bool isDrop, TreeListNode<IBookmarkEntry> node, TreeListNode<IBookmarkEntry> bookmarkEntry)
        {
            if (bookmarkEntry == null)
            {
                return;
            }

            e.Effects = CanDropToBookmark(node, bookmarkEntry) ? DragDropEffects.Move : DragDropEffects.None;
            e.Handled = true;

            if (isDrop && e.Effects == DragDropEffects.Move)
            {
                DropToBookmarkExecute(node, bookmarkEntry);
            }
        }

        private bool CanDropToBookmark(TreeListNode<IBookmarkEntry> node, TreeListNode<IBookmarkEntry> bookmarkEntry)
        {
            return !node.Children.Contains(bookmarkEntry) && !node.ParentContains(bookmarkEntry) && node != bookmarkEntry;
        }

        private void DropToBookmarkExecute(TreeListNode<IBookmarkEntry> node, TreeListNode<IBookmarkEntry> bookmarkEntry)
        {
            _vm.Model.SelectBookmark(node, true);
            BookmarkCollection.Current.MoveToChild(bookmarkEntry, node);
        }

        private void DropToBookmark(object sender, DragEventArgs e, bool isDrop, TreeListNode<IBookmarkEntry> node, IEnumerable<QueryPath> queries)
        {
            if (queries == null || !queries.Any())
            {
                return;
            }

            foreach (var query in queries)
            {
                DropToBookmark(sender, e, isDrop, node, query);
            }
        }

        private void DropToBookmark(object sender, DragEventArgs e, bool isDrop, TreeListNode<IBookmarkEntry> node, QueryPath query)
        {
            if (query == null)
            {
                return;
            }

            if (node.Value is BookmarkFolder && CanDropToBookmark(query))
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

        private bool CanDropToBookmark(QueryPath query)
        {
            if (query.Search != null)
            {
                return false;
            }

            switch (query.Scheme)
            {
                case QueryScheme.File:
                    return CanDropToBookmark(query.SimplePath);

                default:
                    return false;
            }
        }

        private void DropToBookmark(object sender, DragEventArgs e, bool isDrop, TreeListNode<IBookmarkEntry> node, IEnumerable<string> fileNames)
        {
            if (fileNames == null)
            {
                return;
            }
            if ((e.AllowedEffects & DragDropEffects.Copy) != DragDropEffects.Copy)
            {
                return;
            }

            foreach (var fileName in fileNames)
            {
                if (CanDropToBookmark(fileName))
                {
                    if (isDrop)
                    {
                        var bookmark = BookmarkCollectionService.AddToChild(node, new QueryPath(fileName));
                        _vm.Model.SelectBookmark(bookmark, true);
                    }
                    e.Effects = DragDropEffects.Copy;
                    e.Handled = true;
                }
            }
        }

        private bool CanDropToBookmark(string path)
        {
            return ArchiverManager.Current.IsSupported(path, true, true) || System.IO.Directory.Exists(path);
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


        private void FolderListBox_Loaded(object sender, RoutedEventArgs e)
        {
            SetBusy(false);

            _jobClient = new PageThumbnailJobClient("FolderList", JobCategories.BookThumbnailCategory);
            _thumbnailLoader = new ListBoxThumbnailLoader(this, _jobClient);

            _vm.SelectedChanged += SelectedChanged;
            _vm.BusyChanged += BusyChanged;
            _vm.RenameManager = RenameTools.GetRenameManager(this);

            Config.Current.Panels.ContentItemProfile.PropertyChanged += PanelListtemProfile_PropertyChanged;
            Config.Current.Panels.BannerItemProfile.PropertyChanged += PanelListtemProfile_PropertyChanged;
            Config.Current.Panels.ThumbnailItemProfile.PropertyChanged += PanelListtemProfile_PropertyChanged;
        }

        private void FolderListBox_Unloaded(object sender, RoutedEventArgs e)
        {
            _jobClient?.Dispose();

            _vm.SelectedChanged -= SelectedChanged;
            _vm.BusyChanged -= BusyChanged;
            _vm.RenameManager = null;

            Config.Current.Panels.ContentItemProfile.PropertyChanged -= PanelListtemProfile_PropertyChanged;
            Config.Current.Panels.BannerItemProfile.PropertyChanged -= PanelListtemProfile_PropertyChanged;
            Config.Current.Panels.ThumbnailItemProfile.PropertyChanged -= PanelListtemProfile_PropertyChanged;
        }

        /// <summary>
        /// サムネイルパラメーターが変化したらアイテムをリフレッシュする
        /// </summary>
        private void PanelListtemProfile_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            this.ListBox.Items?.Refresh();
        }

        /// <summary>
        /// フォーカス取得
        /// </summary>
        /// <param name="isFocus"></param>
        public void FocusSelectedItem(bool isFocus)
        {
            if (!this.ListBox.IsVisible)
            {
                return;
            }

            if (this.ListBox.SelectedIndex < 0)
            {
                this.ListBox.SelectedIndex = 0;
            }

            var needToFocus = (isFocus && this.IsFocusEnabled) || _vm.IsFocusAtOnce;

            if (this.ListBox.SelectedIndex < 0 && needToFocus)
            {
                _vm.IsFocusAtOnce = false;
                this.ListBox.Focus();
                return;
            }

            // 選択項目が表示されるようにスクロール
            this.ListBox.ScrollIntoView(this.ListBox.SelectedItem);

            if (needToFocus)
            {
                _vm.IsFocusAtOnce = false;
                ListBoxItem lbi = (ListBoxItem)(this.ListBox.ItemContainerGenerator.ContainerFromIndex(this.ListBox.SelectedIndex));
                lbi?.Focus();
            }
        }


        public void SelectedChanged(object sender, FolderListSelectedChangedEventArgs e)
        {
            if (this.ListBox.IsFocused)
            {
                FocusSelectedItem(true);
            }

            this.ListBox.ScrollIntoView(this.ListBox.SelectedItem);

            _thumbnailLoader.Load();

            if (e.IsNewFolder)
            {
                Rename();
            }
        }

        /// <summary>
        /// スクロール変更イベント処理
        /// </summary>
        private void ListBox_ScrollChanged(object sender, ScrollChangedEventArgs e)
        {
            // リネームキャンセル
            RenameTools.GetRenameManager(this)?.Stop();
        }

        private void FolderList_Loaded(object sender, RoutedEventArgs e)
        {
        }

        private async void FolderList_IsVisibleChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            if ((bool)e.NewValue == true)
            {
                _vm.IsVisibleChanged(true);
                // NOTE: ListBoxItemの表示を確定？
                await Task.Yield();
                FocusSelectedItem(false);
            }
        }

        private void FolderList_PreviewKeyDown(object sender, System.Windows.Input.KeyEventArgs e)
        {
            if (Keyboard.Modifiers == ModifierKeys.Alt)
            {
                Key key = e.Key == Key.System ? e.SystemKey : e.Key;

                if (key == Key.Home)
                {
                    _vm.MoveToHome();
                    e.Handled = true;
                }
                else if (key == Key.Up)
                {
                    _vm.MoveToUp();
                    e.Handled = true;
                }
                else if (key == Key.Down)
                {
                    var item = (sender as ListBox)?.SelectedItem as FolderItem;
                    if (item != null)
                    {
                        _vm.MoveToSafety(item);
                        e.Handled = true;
                    }
                }
                else if (key == Key.Left)
                {
                    _vm.MoveToPrevious();
                    e.Handled = true;
                }
                else if (key == Key.Right)
                {
                    _vm.MoveToNext();
                    e.Handled = true;
                }
            }
        }

        private void FolderList_KeyDown(object sender, System.Windows.Input.KeyEventArgs e)
        {
            bool isLRKeyEnabled = _vm.IsLRKeyEnabled();
            if (isLRKeyEnabled && e.Key == Key.Left) // ←
            {
                _vm.MoveToUp();
                e.Handled = true;
            }
        }

        private void FolderList_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
        }

        //
        private void FolderListItem_MouseLeftButtonDown(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            if (Keyboard.Modifiers != ModifierKeys.None) return;

            var item = (sender as ListBoxItem)?.Content as FolderItem;
            if (!Config.Current.Panels.OpenWithDoubleClick && item != null && !item.IsEmpty())
            {
                _vm.Model.LoadBook(item);
            }
        }

        //
        private void FolderListItem_MouseDoubleClick(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            var item = (sender as ListBoxItem)?.Content as FolderItem;
            if (Config.Current.Panels.OpenWithDoubleClick && item != null && !item.IsEmpty())
            {
                _vm.Model.LoadBook(item);
            }

            _vm.MoveToSafety(item);

            e.Handled = true;
        }

        //
        private void FolderListItem_KeyDown(object sender, System.Windows.Input.KeyEventArgs e)
        {
            bool isLRKeyEnabled = _vm.IsLRKeyEnabled();
            var item = (sender as ListBoxItem)?.Content as FolderItem;

            if (Keyboard.Modifiers == ModifierKeys.None)
            {
                if (e.Key == Key.Return)
                {
                    _vm.Model.LoadBook(item);
                    e.Handled = true;
                }
                else if (isLRKeyEnabled && e.Key == Key.Right) // →
                {
                    _vm.MoveToSafety(item);
                    e.Handled = true;
                }
                else if (isLRKeyEnabled && e.Key == Key.Left) // ←
                {
                    if (item != null)
                    {
                        _vm.MoveToUp();
                    }
                    e.Handled = true;
                }
            }
        }


        private void FolderListItem_MouseDown(object sender, MouseButtonEventArgs e)
        {
        }

        private void FolderListItem_MouseUp(object sender, MouseButtonEventArgs e)
        {
        }

        private void FolderListItem_MouseMove(object sender, MouseEventArgs e)
        {
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
                contextMenu.Items.Add(new MenuItem() { Header = Properties.Resources.BookshelfItem_Menu_Open, Command = OpenCommand });
            }
            else if (item.Attributes.HasFlag(FolderItemAttribute.Bookmark))
            {
                if (item.IsDirectory)
                {
                    contextMenu.Items.Add(new MenuItem() { Header = Properties.Resources.BookshelfItem_Menu_Open, Command = OpenCommand });
                    contextMenu.Items.Add(new Separator());
                    contextMenu.Items.Add(new MenuItem() { Header = Properties.Resources.BookshelfItem_Menu_Delete, Command = RemoveCommand });
                    contextMenu.Items.Add(new MenuItem() { Header = Properties.Resources.BookshelfItem_Menu_Rename, Command = RenameCommand });
                }
                else
                {
                    contextMenu.Items.Add(new MenuItem() { Header = Properties.Resources.BookshelfItem_Menu_OpenBook, Command = OpenBookCommand });
                    contextMenu.Items.Add(new Separator());
                    contextMenu.Items.Add(new MenuItem() { Header = Properties.Resources.BookshelfItem_Menu_Explorer, Command = OpenExplorerCommand });
                    contextMenu.Items.Add(ExternalAppCollectionUtility.CreateExternalAppItem(OpenExternalApp_CanExecute(), OpenExternalAppCommand, OpenExternalAppDialogCommand));
                    contextMenu.Items.Add(new MenuItem() { Header = Properties.Resources.BookshelfItem_Menu_Copy, Command = CopyCommand });
                    contextMenu.Items.Add(DestinationFolderCollectionUtility.CreateDestinationFolderItem(Properties.Resources.BookshelfItem_Menu_CopyToFolder, CopyToFolder_CanExecute(), CopyToFolderCommand, OpenDestinationFolderCommand));
                    contextMenu.Items.Add(DestinationFolderCollectionUtility.CreateDestinationFolderItem(Properties.Resources.BookshelfItem_Menu_MoveToFolder, false, MoveToFolderCommand, OpenDestinationFolderCommand));
                    contextMenu.Items.Add(new Separator());
                    contextMenu.Items.Add(new MenuItem() { Header = Properties.Resources.BookshelfItem_Menu_DeleteBookmark, Command = RemoveCommand });
                }
            }
            else if (item.Attributes.HasFlag(FolderItemAttribute.Empty))
            {
                bool canExplorer = !(_vm.FolderCollection is BookmarkFolderCollection);
                contextMenu.Items.Add(new MenuItem() { Header = Properties.Resources.BookshelfItem_Menu_Explorer, Command = OpenExplorerCommand, IsEnabled = canExplorer });
                contextMenu.Items.Add(new MenuItem() { Header = Properties.Resources.BookshelfItem_Menu_Copy, Command = CopyCommand, IsEnabled = false });
            }
            else if (item.IsFileSystem())
            {
                if (item.IsDirectory || Config.Current.System.ArchiveRecursiveMode != ArchiveEntryCollectionMode.IncludeSubArchives)
                {
                    contextMenu.Items.Add(new MenuItem() { Header = Properties.Resources.BookshelfItem_Menu_Open, Command = OpenCommand });
                    contextMenu.Items.Add(new Separator());
                }
                contextMenu.Items.Add(new MenuItem() { Header = Properties.Resources.BookshelfItem_Menu_OpenBook, Command = OpenBookCommand });
                contextMenu.Items.Add(new MenuItem() { Header = Properties.Resources.BookshelfItem_Menu_Subfolder, Command = LoadWithRecursiveCommand, IsChecked = item.IsRecursived });
                contextMenu.Items.Add(new Separator());
                contextMenu.Items.Add(new MenuItem() { Header = Properties.Resources.Word_Bookmark, Command = ToggleBookmarkCommand, IsChecked = BookmarkCollection.Current.Contains(item.EntityPath.SimplePath) });
                contextMenu.Items.Add(new MenuItem() { Header = Properties.Resources.BookshelfItem_Menu_DeleteHistory, Command = RemoveHistoryCommand });
                contextMenu.Items.Add(new Separator());
                contextMenu.Items.Add(new MenuItem() { Header = Properties.Resources.BookshelfItem_Menu_Explorer, Command = OpenExplorerCommand });
                contextMenu.Items.Add(ExternalAppCollectionUtility.CreateExternalAppItem(OpenExternalApp_CanExecute(), OpenExternalAppCommand, OpenExternalAppDialogCommand));
                contextMenu.Items.Add(new MenuItem() { Header = Properties.Resources.BookshelfItem_Menu_Copy, Command = CopyCommand });
                contextMenu.Items.Add(DestinationFolderCollectionUtility.CreateDestinationFolderItem(Properties.Resources.BookshelfItem_Menu_CopyToFolder, CopyToFolder_CanExecute(), CopyToFolderCommand, OpenDestinationFolderCommand));
                contextMenu.Items.Add(DestinationFolderCollectionUtility.CreateDestinationFolderItem(Properties.Resources.BookshelfItem_Menu_MoveToFolder, MoveToFolder_CanExecute(), MoveToFolderCommand, OpenDestinationFolderCommand));
                contextMenu.Items.Add(new Separator());
                contextMenu.Items.Add(new MenuItem() { Header = Properties.Resources.BookshelfItem_Menu_Delete, Command = RemoveCommand });
                contextMenu.Items.Add(new MenuItem() { Header = Properties.Resources.BookshelfItem_Menu_Rename, Command = RenameCommand });
                if (item.IsPlaylist)
                {
                    contextMenu.Items.Add(new Separator());
                    contextMenu.Items.Add(new MenuItem() { Header = Properties.Resources.BookshelfItem_Menu_OpenInPlaylist, Command = OpenInPlaylistCommand });
                }
            }
        }


        /// <summary>
        /// リスト更新中
        /// </summary>
        private void BusyChanged(object sender, FolderListBusyChangedEventArgs e)
        {
            SetBusy(e.IsBusy);
        }

        private void SetBusy(bool isBusy)
        {
            if (isBusy)
            {
                this.IsHitTestVisible = false;

                this.ListBox.BeginAnimation(UIElement.OpacityProperty, new DoubleAnimation(0.0, TimeSpan.FromSeconds(0.5)) { BeginTime = TimeSpan.FromSeconds(1.0) });

                this.BusyFadeContent.Content = new BusyFadeView();
            }
            else
            {
                this.IsHitTestVisible = true;

                this.ListBox.BeginAnimation(UIElement.OpacityProperty, null);
                this.ListBox.Opacity = 1.0;

                this.BusyFadeContent.Content = null;
            }
        }


        public void Refresh()
        {
            this.ListBox.Items.Refresh();
        }


        #region UI Accessor

        public List<FolderItem> GetItems()
        {
            return this.ListBox.Items?.Cast<FolderItem>().ToList();
        }

        public List<FolderItem> GetSelectedItems()
        {
            return this.ListBox.SelectedItems.Cast<FolderItem>().ToList();
        }

        public void SetSelectedItems(IEnumerable<FolderItem> selectedItems)
        {
            var items = selectedItems?.Intersect(GetItems()).ToList();
            this.ListBox.SetSelectedItems(items);
            this.ListBox.ScrollItemsIntoView(items);
        }

        #endregion UI Accessor
    }




    public class FolderItemToNoteConverter : IMultiValueConverter
    {
        public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
        {
            if (values[0] is FolderItem item && values[1] is FolderOrder order)
            {
                return item.GetNote(order);
            }

            return null;
        }

        public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
