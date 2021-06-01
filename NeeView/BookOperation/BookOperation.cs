using NeeLaboratory.ComponentModel;
using NeeLaboratory.Threading.Jobs;
using NeeView.Collections;
using NeeView.Collections.Generic;
using NeeView.Properties;
using NeeView.Windows.Property;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace NeeView
{
    /// <summary>
    /// 本の操作
    /// </summary>
    public class BookOperation : BindableBase
    {
        // System Object
        static BookOperation() => Current = new BookOperation();
        public static BookOperation Current { get; }

        #region Fields

        private bool _isEnabled;
        private Book _book;
        private ObservableCollection<Page> _pageList;
        private ExportImageProceduralDialog _exportImageProceduralDialog;
        private int _pageTerminating;

        #endregion

        #region Constructors

        private BookOperation()
        {
            BookHub.Current.BookChanging += BookHub_BookChanging;
            BookHub.Current.BookChanged += BookHub_BookChanged;
            BookHub.Current.ViewContentsChanged += BookHub_ViewContentsChanged;
        }

        // NOTE: 応急処置。シングルトンコンストラクタで互いを参照してしまっているのを回避するため
        public void LinkPlaylistHub(PlaylistHub playlistHub)
        {
            playlistHub.PlaylistCollectionChanged += Playlist_CollectionChanged;
        }

        #endregion

        #region Events

        // ブックが変更される
        public event EventHandler<BookChangingEventArgs> BookChanging;

        // ブックが変更された
        public event EventHandler<BookChangedEventArgs> BookChanged;

        // ページが変更された
        public event EventHandler<ViewContentSourceCollectionChangedEventArgs> ViewContentsChanged;

        // ページがソートされた
        public event EventHandler PagesSorted;

        // ページリストが変更された
        public event EventHandler PageListChanged;

        // ページが削除された
        public event EventHandler<PageRemovedEventArgs> PageRemoved;

        #endregion

        #region Properties

        /// <summary>
        /// 操作の有効設定。ロード中は機能を無効にするために使用
        /// </summary>
        public bool IsEnabled
        {
            get { return _isEnabled; }
            set { if (_isEnabled != value) { _isEnabled = value; RaisePropertyChanged(); } }
        }

        public Book Book
        {
            get { return _book; }
            set
            {
                if (SetProperty(ref _book, value))
                {
                    RaisePropertyChanged(nameof(Address));
                    RaisePropertyChanged(nameof(IsValid));
                    RaisePropertyChanged(nameof(IsBusy));
                }
            }
        }


        public string Address => Book?.Address;

        public bool IsValid => Book != null;

        public bool IsBusy
        {
            get { return Book != null ? Book.Viewer.IsBusy : false; }
        }

        public ObservableCollection<Page> PageList
        {
            get { return _pageList; }
        }

        public PageSortModeClass PageSortModeClass
        {
            get { return Book != null ? Book.PageSortModeClass : PageSortModeClass.Full; }
        }

        #endregion

        #region Methods


        private void BookHub_BookChanging(object sender, BookChangingEventArgs e)
        {
            // ブック操作無効
            IsEnabled = false;

            BookChanging?.Invoke(sender, e);
        }

        /// <summary>
        /// 本の更新
        /// </summary>
        private void BookHub_BookChanged(object sender, BookChangedEventArgs e)
        {
            this.Book = BookHub.Current.Book;

            if (this.Book != null)
            {
                this.Book.Pages.PagesSorted += Book_PagesSorted;
                this.Book.Pages.PageRemoved += Book_PageRemoved;
                this.Book.Viewer.ViewContentsChanged += Book_ViewContentsChanged;
                this.Book.Viewer.PageTerminated += Book_PageTerminated;
                this.Book.Viewer.AddPropertyChanged(nameof(BookPageViewer.IsBusy), (s, e_) => RaisePropertyChanged(nameof(IsBusy)));
            }

            //
            RaisePropertyChanged(nameof(IsBookmark));

            // マーカー復元
            UpdateMarkers();

            // ページリスト更新
            UpdatePageList(false);

            // ブック操作有効
            IsEnabled = true;

            // ページリスト更新通知
            PageListChanged?.Invoke(this, null);

            // Script: OnBookLoaded
            CommandTable.Current.TryExecute(this, ScriptCommand.EventOnBookLoaded, null, CommandOption.None);
            // Script: OnPageChanged
            CommandTable.Current.TryExecute(this, ScriptCommand.EventOnPageChanged, null, CommandOption.None);

            BookChanged?.Invoke(sender, e);
        }

        private void BookHub_ViewContentsChanged(object sender, ViewContentSourceCollectionChangedEventArgs e)
        {
            if (e is null) return;

            if (e.ViewPageCollection.IsFixedContents())
            {
                // NOTE: ブックが有効なときだけ呼ぶ
                if (IsEnabled)
                {
                    // Script: OnPageChanged
                    CommandTable.Current.TryExecute(this, ScriptCommand.EventOnPageChanged, null, CommandOption.None);
                }
            }
        }

        //
        private void Book_PagesSorted(object sender, EventArgs e)
        {
            AppDispatcher.Invoke(() =>
            {
                UpdatePageList(true);
            });

            PagesSorted?.Invoke(this, e);
        }

        //
        private void Book_ViewContentsChanged(object sender, ViewContentSourceCollectionChangedEventArgs e)
        {
            if (!IsEnabled) return;

            AppDispatcher.Invoke(() =>
            {
                RaisePropertyChanged(nameof(IsMarked));
                ViewContentsChanged?.Invoke(sender, e);
            });
        }


        // ページリスト更新
        public void UpdatePageList(bool raisePageListChanged)
        {
            var pages = this.Book?.Pages;
            var pageList = pages != null ? new ObservableCollection<Page>(pages) : null;

            if (SetProperty(ref _pageList, pageList, nameof(PageList)))
            {
                if (raisePageListChanged)
                {
                    PageListChanged?.Invoke(this, null);
                }
            }

            RaisePropertyChanged(nameof(IsMarked));
        }

        // 現在ベージ取得
        public Page GetPage()
        {
            return this.Book?.Viewer.GetViewPage();
        }

        // 現在ページ番号取得
        public int GetPageIndex()
        {
            return this.Book == null ? 0 : this.Book.Viewer.DisplayIndex; // GetPosition().Index;
        }

        // 現在ページ番号を設定し、表示を切り替える (先読み無し)
        public void RequestPageIndex(object sender, int index)
        {
            this.Book?.Control.RequestSetPosition(sender, new PagePosition(index, 0), 1);
        }

        /// <summary>
        /// 最大ページ番号取得
        /// </summary>
        /// <returns></returns>
        public int GetMaxPageIndex()
        {
            var count = this.Book == null ? 0 : this.Book.Pages.Count - 1;
            if (count < 0) count = 0;
            return count;
        }

        /// <summary>
        /// ページ数取得
        /// </summary>
        /// <returns></returns>
        public int GetPageCount()
        {
            return this.Book == null ? 0 : this.Book.Pages.Count;
        }

        /// <summary>
        /// 表示ページ読み込み完了まで待機
        /// </summary>
        public void Wait(CancellationToken token)
        {
            if (!BookHub.Current.IsBusy && Book?.Control.IsViewContentsLoading != true)
            {
                return;
            }

            // BookHubのコマンド処理が終わるまで待機
            var eventFlag = new ManualResetEventSlim();
            BookHub.Current.IsBusyChanged += BookHub_IsBusyChanged;
            try
            {
                if (BookHub.Current.IsBusy)
                {
                    eventFlag.Wait(token);
                }
            }
            finally
            {
                BookHub.Current.IsBusyChanged -= BookHub_IsBusyChanged;
            }

            var book = this.Book;
            if (book is null)
            {
                return;
            }

            // 表示ページの読み込みが終わるまで待機
            eventFlag.Reset();
            bool _isBookChanged = false;
            this.BookChanged += BookOperation_BookChanged;
            book.Control.ViewContentsLoading += BookControl_ViewContentsLoading;
            try
            {
                if (book.Control.IsViewContentsLoading)
                {
                    eventFlag.Wait(token);
                }
            }
            finally
            {
                this.BookChanged -= BookOperation_BookChanged;
                book.Control.ViewContentsLoading -= BookControl_ViewContentsLoading;
            }

            // 待機中にブックが変更された場合はそのブックで再待機
            if (_isBookChanged)
            {
                Wait(token);
            }

            void BookHub_IsBusyChanged(object sender, JobIsBusyChangedEventArgs e)
            {
                if (!e.IsBusy)
                {
                    eventFlag.Set();
                }
            }

            void BookOperation_BookChanged(object sender, BookChangedEventArgs e)
            {
                _isBookChanged = true;
                eventFlag.Set();
            }

            void BookControl_ViewContentsLoading(object sender, ViewContentsLoadingEventArgs e)
            {
                if (!e.IsLoading)
                {
                    eventFlag.Set();
                }
            }
        }


        #endregion

        #region BookCommand : ページ削除

        // 現在表示しているページのファイル削除可能？
        public bool CanDeleteFile()
        {
            return CanDeleteFile(Book?.Viewer.GetViewPage());
        }

        // 現在表示しているページのファイルを削除する
        public async Task DeleteFileAsync()
        {
            await DeleteFileAsync(Book?.Viewer.GetViewPage());
        }

        // 指定ページのファル削除可能？
        public bool CanDeleteFile(Page page)
        {
            return Config.Current.System.IsFileWriteAccessEnabled && FileIO.Current.CanRemovePage(page);
        }

        // 指定ページのファルを削除する
        public async Task DeleteFileAsync(Page page)
        {
            if (CanDeleteFile(page))
            {
                var isSuccess = await FileIO.Current.RemovePageAsync(page);
                if (isSuccess)
                {
                    Book.Control.RequestRemove(this, page);
                }
            }
        }

        // 指定ページのファイルを削除する
        public async Task DeleteFileAsync(List<Page> pages)
        {
            var removes = pages.Where(e => CanDeleteFile(e)).ToList();
            if (removes.Any())
            {
                if (removes.Count == 1)
                {
                    await DeleteFileAsync(removes.First());
                    return;
                }

                await FileIO.Current.RemovePageAsync(removes);
                ValidateRemoveFile(removes);
            }
        }

        // 消えたファイルのページを削除
        public void ValidateRemoveFile(IEnumerable<Page> pages)
        {
            Book?.Control.RequestRemove(this, pages.Where(e => FileIO.Current.IsPageRemoved(e)).ToList());
        }


        #endregion

        #region BookCommand : ブック削除

        // 現在表示しているブックの削除可能？
        public bool CanDeleteBook()
        {
            return Config.Current.System.IsFileWriteAccessEnabled && Book != null && (Book.LoadOption & BookLoadOption.Undeliteable) == 0 && (File.Exists(Book.SourceAddress) || Directory.Exists(Book.SourceAddress));
        }

        // 現在表示しているブックを削除する
        public async void DeleteBook()
        {
            if (CanDeleteBook())
            {
                var item = BookshelfFolderList.Current.FindFolderItem(Book.SourceAddress);
                if (item != null)
                {
                    await BookshelfFolderList.Current.RemoveAsync(item);
                }
                else
                {
                    await FileIO.Current.RemoveFileAsync(Book.SourceAddress, Resources.FileDeleteBookDialog_Title, null);
                }
            }
        }

        #endregion

        #region BookCommand : ページ出力

        // ファイルの場所を開くことが可能？
        public bool CanOpenFilePlace()
        {
            return Book?.Viewer.GetViewPage() != null;
        }

        // ファイルの場所を開く
        public void OpenFilePlace()
        {
            if (CanOpenFilePlace())
            {
                string place = Book.Viewer.GetViewPage()?.GetFolderOpenPlace();
                if (place != null)
                {
                    ExternalProcess.Start("explorer.exe", "/select,\"" + place + "\"");
                }
            }
        }


        // 外部アプリで開く
        public void OpenApplication(OpenExternalAppCommandParameter parameter)
        {
            if (CanOpenFilePlace())
            {
                try
                {
                    var external = new ExternalAppUtility();
                    var pages = CollectPages(this.Book, parameter.MultiPagePolicy);
                    external.Call(pages, parameter, CancellationToken.None);
                }
                catch (OperationCanceledException)
                {
                }
                catch (Exception ex)
                {
                    new MessageDialog(ex.Message, Properties.Resources.OpenApplicationErrorDialog_Title).ShowDialog();
                }
            }
        }

        private List<Page> CollectPages(Book book, MultiPagePolicy policy)
        {
            if (book is null)
            {
                return new List<Page>();
            }

            var pages = book.Viewer.GetViewPages().Distinct();

            switch (policy)
            {
                case MultiPagePolicy.Once:
                    pages = pages.Take(1);
                    break;

                case MultiPagePolicy.AllLeftToRight:
                    if (book.Viewer.BookReadOrder == PageReadOrder.RightToLeft)
                    {
                        pages = pages.Reverse();
                    }
                    break;
            }

            return pages.ToList();
        }


        // クリップボードにコピー
        public void CopyToClipboard(CopyFileCommandParameter parameter)
        {
            if (CanOpenFilePlace())
            {
                try
                {
                    var pages = CollectPages(this.Book, parameter.MultiPagePolicy);
                    ClipboardUtility.Copy(pages, parameter);
                }
                catch (Exception e)
                {
                    new MessageDialog($"{Resources.Word_Cause}: {e.Message}", Resources.CopyErrorDialog_Title).ShowDialog();
                }
            }
        }

        /// <summary>
        /// ファイル保存可否
        /// </summary>
        /// <returns></returns>
        public bool CanExport()
        {
            var pages = Book?.Viewer.GetViewPages();
            if (pages == null || pages.Count == 0) return false;

            var imageSource = pages[0].GetContentImageSource();
            if (imageSource == null) return false;

            return true;
        }


        // ファイルに保存する (ダイアログ)
        // TODO: OutOfMemory対策
        public void ExportDialog(ExportImageAsCommandParameter parameter)
        {
            if (CanExport())
            {
                try
                {
                    _exportImageProceduralDialog = _exportImageProceduralDialog ?? new ExportImageProceduralDialog();
                    _exportImageProceduralDialog.Owner = MainViewComponent.Current.GetWindow();
                    _exportImageProceduralDialog.Show(parameter);
                }
                catch (Exception e)
                {
                    new MessageDialog($"{Resources.ImageExportErrorDialog_Message}\n{Resources.Word_Cause}: {e.Message}", Resources.ImageExportErrorDialog_Title).ShowDialog();
                    return;
                }
            }
        }

        // ファイルに保存する
        public void Export(ExportImageCommandParameter parameter)
        {
            if (CanExport())
            {
                try
                {
                    var process = new ExportImageProcedure();
                    process.Execute(parameter);
                }
                catch (Exception e)
                {
                    new MessageDialog($"{Resources.ImageExportErrorDialog_Message}\n{Resources.Word_Cause}: {e.Message}", Resources.ImageExportErrorDialog_Title).ShowDialog();
                    return;
                }
            }
        }

        #endregion

        #region BookCommand : ページ操作

        // ページ終端を超えて移動しようとするときの処理
        private void Book_PageTerminated(object sender, PageTerminatedEventArgs e)
        {
            if (_pageTerminating > 0) return;

            // TODO ここでSlideShowを参照しているが、引数で渡すべきでは？
            if (SlideShow.Current.IsPlayingSlideShow && Config.Current.SlideShow.IsSlideShowByLoop)
            {
                FirstPage(sender);
            }
            else
            {
                switch (Config.Current.Book.PageEndAction)
                {
                    case PageEndAction.Loop:
                        PageEndAction_Loop(sender, e);
                        break;

                    case PageEndAction.NextBook:
                        PageEndAction_NextBook(sender, e);
                        break;

                    case PageEndAction.Dialog:
                        PageEndAction_Dialog(sender, e);
                        break;

                    default:
                        PageEndAction_None(sender, e, true);
                        break;
                }
            }
        }

        private void PageEndAction_Loop(object sender, PageTerminatedEventArgs e)
        {
            if (e.Direction < 0)
            {
                LastPage(sender);
            }
            else
            {
                FirstPage(sender);
            }
            if (Config.Current.Book.IsNotifyPageLoop)
            {
                InfoMessage.Current.SetMessage(InfoMessageType.Notify, Properties.Resources.Notice_BookOperationPageLoop);
            }
        }

        private void PageEndAction_NextBook(object sender, PageTerminatedEventArgs e)
        {
            AppDispatcher.Invoke(async () =>
            {
                if (e.Direction < 0)
                {
                    await BookshelfFolderList.Current.PrevFolder(BookLoadOption.LastPage);
                }
                else
                {
                    await BookshelfFolderList.Current.NextFolder(BookLoadOption.FirstPage);
                }
            });
        }

        private void PageEndAction_None(object sender, PageTerminatedEventArgs e, bool notify)
        {
            if (SlideShow.Current.IsPlayingSlideShow)
            {
                // スライドショー解除
                SlideShow.Current.IsPlayingSlideShow = false;
            }

            // 通知。本の場合のみ処理。メディアでは不要
            else if (notify && this.Book != null && !this.Book.IsMedia)
            {
                if (e.Direction < 0)
                {
                    SoundPlayerService.Current.PlaySeCannotMove();
                    InfoMessage.Current.SetMessage(InfoMessageType.Notify, Properties.Resources.Notice_FirstPage);
                }
                else
                {
                    SoundPlayerService.Current.PlaySeCannotMove();
                    InfoMessage.Current.SetMessage(InfoMessageType.Notify, Properties.Resources.Notice_LastPage);
                }
            }
        }

        private void PageEndAction_Dialog(object sender, PageTerminatedEventArgs e)
        {
            Interlocked.Increment(ref _pageTerminating);

            AppDispatcher.BeginInvoke(() =>
            {
                try
                {
                    PageEndAction_DialogCore(sender, e);
                }
                finally
                {
                    Interlocked.Decrement(ref _pageTerminating);
                }
            });
        }

        private void PageEndAction_DialogCore(object sender, PageTerminatedEventArgs e)
        {
            var title = (e.Direction < 0) ? Resources.Notice_FirstPage : Resources.Notice_LastPage;
            var dialog = new MessageDialog(Resources.PageEndDialog_Message, title);
            var nextCommand = new UICommand(Properties.Resources.PageEndAction_NextBook);
            var loopCommand = new UICommand(Properties.Resources.PageEndAction_Loop);
            var noneCommand = new UICommand(Properties.Resources.PageEndAction_None);
            dialog.Commands.Add(nextCommand);
            dialog.Commands.Add(loopCommand);
            dialog.Commands.Add(noneCommand);
            var result = dialog.ShowDialog(App.Current.MainWindow);

            if (result == nextCommand)
            {
                PageEndAction_NextBook(sender, e);
            }
            else if (result == loopCommand)
            {
                PageEndAction_Loop(sender, e);
            }
            else
            {
                PageEndAction_None(sender, e, false);
            }
        }


        // ページ削除時の処理
        private void Book_PageRemoved(object sender, PageRemovedEventArgs e)
        {
            if (this.Book is null) return;

            // プレイリストから削除
            var bookPlaylist = new BookPlaylist(this.Book, PlaylistHub.Current.Playlist);
            bookPlaylist.Remove(e.Pages);

            UpdatePageList(true);
            PageRemoved?.Invoke(sender, e);
        }

        // ページ移動量をメディアの時間移動量に変換して移動
        private void MoveMediaPage(object sender, int delta)
        {
            if (MediaPlayerOperator.Current == null) return;

            var isTerminated = MediaPlayerOperator.Current.AddPosition(TimeSpan.FromSeconds(delta * Config.Current.Archive.Media.PageSeconds));

            if (isTerminated)
            {
                this.Book?.Viewer.RaisePageTerminatedEvent(sender, delta < 0 ? -1 : 1);
            }
        }

        // 前のページに移動
        public void PrevPage(object sender)
        {
            if (this.Book == null) return;

            if (this.Book.IsMedia)
            {
                MoveMediaPage(sender, -1);
            }
            else
            {
                this.Book.Control.PrevPage(sender, 0);
            }
        }

        // 次のページに移動
        public void NextPage(object sender)
        {
            if (this.Book == null) return;

            if (this.Book.IsMedia)
            {
                MoveMediaPage(sender, +1);
            }
            else
            {
                this.Book.Control.NextPage(sender, 0);
            }
        }

        // 1ページ前に移動
        public void PrevOnePage(object sender)
        {
            if (this.Book == null) return;

            if (this.Book.IsMedia)
            {
                MoveMediaPage(sender, -1);
            }
            else
            {
                this.Book.Control.PrevPage(sender, 1);
            }
        }

        // 1ページ後に移動
        public void NextOnePage(object sender)
        {
            if (this.Book == null) return;

            if (this.Book.IsMedia)
            {
                MoveMediaPage(sender, +1);
            }
            else
            {
                this.Book?.Control.NextPage(sender, 1);
            }
        }

        // 指定ページ数前に移動
        public void PrevSizePage(object sender, int size)
        {
            if (this.Book == null) return;

            if (this.Book.IsMedia)
            {
                MoveMediaPage(sender, -size);
            }
            else
            {
                this.Book.Control.PrevPage(sender, size);
            }
        }

        // 指定ページ数後に移動
        public void NextSizePage(object sender, int size)
        {
            if (this.Book == null) return;

            if (this.Book.IsMedia)
            {
                MoveMediaPage(sender, +size);
            }
            else
            {
                this.Book.Control.NextPage(sender, size);
            }
        }


        // 最初のページに移動
        public void FirstPage(object sender)
        {
            if (this.Book == null) return;

            if (this.Book.IsMedia)
            {
                MediaPlayerOperator.Current?.SetPositionFirst();
            }
            else
            {
                this.Book.Control.FirstPage(sender);
            }
        }

        // 最後のページに移動
        public void LastPage(object sender)
        {
            if (this.Book == null) return;

            if (this.Book.IsMedia)
            {
                MediaPlayerOperator.Current?.SetPositionLast();
            }
            else
            {
                this.Book.Control.LastPage(sender);
            }
        }


        // 前のフォルダーに移動
        public void PrevFolderPage(object sender, bool isShowMessage)
        {
            if (this.Book == null) return;

            if (this.Book.IsMedia)
            {
            }
            else
            {
                var index = this.Book.Control.PrevFolderPage(sender);
                ShowMoveFolderPageMessage(index, Properties.Resources.Notice_FirstFolderPage, isShowMessage);
            }
        }

        // 次のフォルダーに移動
        public void NextFolderPage(object sender, bool isShowMessage)
        {
            if (this.Book == null) return;

            if (this.Book.IsMedia)
            {
            }
            else
            {
                var index = this.Book.Control.NextFolderPage(sender);
                ShowMoveFolderPageMessage(index, Properties.Resources.Notice_LastFolderPage, isShowMessage);
            }
        }

        private void ShowMoveFolderPageMessage(int index, string termianteMessage, bool isShowMessage)
        {
            if (index < 0)
            {
                InfoMessage.Current.SetMessage(InfoMessageType.Notify, termianteMessage);
            }
            else if (isShowMessage)
            {
                var directory = this.Book.Pages[index].GetSmartDirectoryName();
                if (string.IsNullOrEmpty(directory))
                {
                    directory = "(Root)";
                }
                InfoMessage.Current.SetMessage(InfoMessageType.Notify, directory);
            }
        }


        // ページを指定して移動
        public void JumpPage(object sender, int number)
        {
            if (this.Book == null || this.Book.IsMedia) return;

            var page = this.Book.Pages.GetPage(number - 1);
            this.Book.Control.JumpPage(sender, page);
        }

        // パスを指定して移動
        public bool JumpPageWithPath(object sender, string path)
        {
            if (this.Book == null || this.Book.IsMedia) return false;

            var page = this.Book.Pages.GetPageWithEntryFullName(path);
            return this.Book.Control.JumpPage(sender, page);
        }

        // ページを指定して移動
        public void JumpPageAs(object sender)
        {
            if (this.Book == null || this.Book.IsMedia) return;

            var dialogModel = new PageSelecteDialogModel(this.Book.Viewer.GetViewPageIndex() + 1, 1, this.Book.Pages.Count);

            var dialog = new PageSelectDialog(dialogModel);
            dialog.Owner = MainViewComponent.Current.GetWindow();
            dialog.WindowStartupLocation = WindowStartupLocation.CenterOwner;

            var result = dialog.ShowDialog();

            if (result == true)
            {
                var page = this.Book.Pages.GetPage(dialogModel.Value - 1);
                this.Book.Control.JumpPage(sender, page);
            }
        }

        // 指定ページに移動
        public void JumpPage(object sender, Page page)
        {
            if (_isEnabled && page != null) this.Book?.Control.JumpPage(sender, page);
        }

        // ランダムページに移動
        public void JumpRandomPage(object sender)
        {
            if (this.Book == null || this.Book.IsMedia) return;
            if (this.Book.Pages.Count <= 1) return;

            var currentIndex = this.Book.Viewer.GetViewPageIndex();

            var random = new Random();
            var index = random.Next(this.Book.Pages.Count - 1);

            if (index == currentIndex)
            {
                index = this.Book.Pages.Count - 1;
            }

            var page = this.Book.Pages.GetPage(index);
            this.Book.Control.JumpPage(sender, page);
        }


        // 動画再生中？
        public bool IsMediaPlaying()
        {
            if (this.Book != null && this.Book.IsMedia)
            {
                return MediaPlayerOperator.Current.IsPlaying;
            }
            else
            {
                return false;
            }
        }

        // 動画再生ON/OFF
        public bool ToggleMediaPlay()
        {
            if (this.Book != null && this.Book.IsMedia)
            {
                if (MediaPlayerOperator.Current.IsPlaying)
                {
                    MediaPlayerOperator.Current.Pause();
                }
                else
                {
                    MediaPlayerOperator.Current.Play();
                }
                return true;
            }
            else
            {
                return false;
            }
        }

        // スライドショー用：次のページへ移動
        public void NextSlide(object sender)
        {
            if (SlideShow.Current.IsPlayingSlideShow)
            {
                NextPage(sender);
            }
        }

        #endregion

        #region BookCommand : ブックマーク

        // ブックマーク登録可能？
        public bool CanBookmark()
        {
            return (Book != null);
        }

        // ブックマーク設定
        public void SetBookmark(bool isBookmark)
        {
            if (CanBookmark())
            {
                var query = new QueryPath(Book.Address);

                if (isBookmark)
                {
                    // ignore temporary directory
                    if (Book.Address.StartsWith(Temporary.Current.TempDirectory))
                    {
                        ToastService.Current.Show(new Toast(Resources.Bookmark_Message_TemporaryNotSupportedError, null, ToastIcon.Error));
                        return;
                    }

                    BookmarkCollectionService.Add(query);
                }
                else
                {
                    BookmarkCollectionService.Remove(query);
                }

                RaisePropertyChanged(nameof(IsBookmark));
            }
        }

        // ブックマーク切り替え
        public void ToggleBookmark()
        {
            if (CanBookmark())
            {
                SetBookmark(!IsBookmark);
            }
        }

        // ブックマーク判定
        public bool IsBookmark
        {
            get
            {
                return BookmarkCollection.Current.Contains(Book?.Address);
            }
        }

        #endregion

        #region BookCommand : マーク

        // プレイリストに追加、削除された
        public event EventHandler MarkersChanged;

        //
        private void Playlist_CollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            if (!IsValid)
            {
                return;
            }

            var placeQuery = new QueryPath(Address);

            var newItems = e.NewItems?.Cast<PlaylistItem>();
            var oldItems = e.OldItems?.Cast<PlaylistItem>();

            switch (e.Action)
            {
                case NotifyCollectionChangedAction.Reset:
                    UpdateMarkers();
                    break;

                case NotifyCollectionChangedAction.Add:
                    if (newItems.Any(x => x.Path.StartsWith(Address)))
                    {
                        UpdateMarkers();
                    }
                    break;

                case NotifyCollectionChangedAction.Remove:
                    if (oldItems.Any(x => x.Path.StartsWith(Address)))
                    {
                        UpdateMarkers();
                    }
                    break;

                case NotifyCollectionChangedAction.Replace:
                    if (!oldItems.SequenceEqual(newItems) && oldItems.Union(newItems).Any(x => x.Path.StartsWith(Address)))
                    {
                        UpdateMarkers();
                    }
                    break;
            }
        }

        // 表示ページのマーク判定
        public bool IsMarked
        {
            get { return this.Book != null ? this.Book.Marker.IsMarked(this.Book.Viewer.GetViewPage()) : false; }
        }

        // ページマーク登録可能？
        public bool CanMark()
        {
            if (this.Book is null) return false;

            return CanMark(this.Book.Viewer.GetViewPage());
        }

        public bool CanMark(Page page)
        {
            if (this.Book is null) return false;

            var bookPlaylist = new BookPlaylist(this.Book, PlaylistHub.Current.Playlist);
            return bookPlaylist.IsEnabled(page);
        }

        // マーカー追加/削除
        public PlaylistItem SetMark(bool isMark)
        {
            if (!_isEnabled) return null;

            var page = this.Book.Viewer.GetViewPage();
            if (!CanMark(page))
            {
                return null;
            }

            var bookPlaylist = new BookPlaylist(this.Book, PlaylistHub.Current.Playlist);
            return bookPlaylist.Set(page, isMark);
        }

        // マーカー切り替え
        public PlaylistItem ToggleMark()
        {
            if (!_isEnabled) return null;

            var page = this.Book.Viewer.GetViewPage();
            if (!CanMark(page))
            {
                return null;
            }

            var bookPlaylist = new BookPlaylist(this.Book, PlaylistHub.Current.Playlist);
            return bookPlaylist.Toggle(page);
        }

        #region 開発用

        /// <summary>
        /// (開発用) たくさんのページマーク作成
        /// </summary>
        [Conditional("DEBUG")]
        public void Test_MakeManyMarkers()
        {
            if (Book == null) return;
            var bookPlaylist = new BookPlaylist(this.Book, PlaylistHub.Current.Playlist);

            for (int index = 0; index < Book.Pages.Count; index += 100)
            {
                var page = Book.Pages[index];
                bookPlaylist.Add(page);
            }
        }

        #endregion

        // マーカー表示更新
        public void UpdateMarkers()
        {
            if (Book == null) return;

            // 本にマーカを設定
            var bookPlaylist = new BookPlaylist(this.Book, PlaylistHub.Current.Playlist);
            var pages = bookPlaylist.Collect();

            this.Book.Marker.SetMarkers(pages);

            // 表示更新
            MarkersChanged?.Invoke(this, null);
            RaisePropertyChanged(nameof(IsMarked));
        }

        //
        public bool CanPrevMarkInPlace(MovePlaylsitItemInBookCommandParameter param)
        {
            return (this.Book?.Marker.Markers != null && Current.Book.Marker.Markers.Count > 0) || param.IsIncludeTerminal;
        }

        //
        public bool CanNextMarkInPlace(MovePlaylsitItemInBookCommandParameter param)
        {
            return (this.Book?.Marker.Markers != null && Current.Book.Marker.Markers.Count > 0) || param.IsIncludeTerminal;
        }

        // ページマークに移動
        public void PrevMarkInPlace(MovePlaylsitItemInBookCommandParameter param)
        {
            if (!_isEnabled || this.Book == null) return;
            var result = this.Book.Control.RequestJumpToMarker(this, -1, param.IsLoop, param.IsIncludeTerminal);
            if (result != null)
            {
                var bookPlaylist = new BookPlaylist(this.Book, PlaylistHub.Current.Playlist);
                var item = bookPlaylist.Find(result);
                PlaylistPresenter.Current.PlaylistListBox?.SetSelectedItem(item);
            }
            else
            {
                InfoMessage.Current.SetMessage(InfoMessageType.Notify, Properties.Resources.Notice_FirstPlaylistItem);
            }
        }

        // ページマークに移動
        public void NextMarkInPlace(MovePlaylsitItemInBookCommandParameter param)
        {
            if (!_isEnabled || this.Book == null) return;
            var result = this.Book.Control.RequestJumpToMarker(this, +1, param.IsLoop, param.IsIncludeTerminal);
            if (result != null)
            {
                var bookPlaylist = new BookPlaylist(this.Book, PlaylistHub.Current.Playlist);
                var item = bookPlaylist.Find(result);
                PlaylistPresenter.Current.PlaylistListBox?.SetSelectedItem(item);
            }
            else
            {
                InfoMessage.Current.SetMessage(InfoMessageType.Notify, Properties.Resources.Notice_LastPlaylistItem);
            }
        }

        #endregion

        #region BookCommand : 下位ブックに移動

        public bool CanMoveToChildBook()
        {
            var page = Book?.Viewer.GetViewPage();
            return page != null && page.PageType == PageType.Folder;
        }

        public void MoveToChildBook(object sender)
        {
            var page = Book?.Viewer.GetViewPage();
            if (page != null && page.PageType == PageType.Folder)
            {
                BookHub.Current.RequestLoad(sender, page.Entry.SystemPath, null, BookLoadOption.IsBook | BookLoadOption.SkipSamePlace, true);
            }
        }

        #endregion

        #region Memento
        [DataContract]
        public class Memento : IMemento
        {
            [DataMember]
            public PageEndAction PageEndAction { get; set; }

            [DataMember]
            public ExternalApplicationMemento ExternalApplication { get; set; }

            [DataMember]
            public ClipboardUtilityMemento ClipboardUtility { get; set; }

            [DataMember]
            public bool IsNotifyPageLoop { get; set; }

            public void RestoreConfig(Config config)
            {
                config.Book.PageEndAction = PageEndAction;
                config.Book.IsNotifyPageLoop = IsNotifyPageLoop;

                // NOTE: ExternalApplication と ClipboardUtility は上位で処理されている
            }
        }

        #endregion


        public class ExternalApplicationMemento
        {
            [Obsolete, DataMember]
            public ExternalProgramType ProgramType { get; set; }

            [DataMember]
            public string Command { get; set; }

            [DataMember]
            public string Parameter { get; set; }

            [Obsolete, DataMember]
            public string Protocol { get; set; }

            // 複数ページのときの動作
            [PropertyMember]
            public MultiPagePolicy MultiPageOption { get; set; }

            // 圧縮ファイルのときの動作
            [DataMember]
            public ArchivePolicy ArchiveOption { get; set; }

            [DataMember]
            public string ArchiveSeparater { get; set; }


            [OnDeserialized]
            private void OnDeserialized(StreamingContext context)
            {
#pragma warning disable CS0612 // 型またはメンバーが旧型式です
                if (ProgramType == ExternalProgramType.Protocol)
                {
                    Command = "";
                    Parameter = Protocol;
                }
#pragma warning restore CS0612 // 型またはメンバーが旧型式です
            }
        }

        [DataContract]
        public class ClipboardUtilityMemento
        {
            [DataMember]
            public MultiPagePolicy MultiPageOption { get; set; }
            [DataMember]
            public ArchivePolicy ArchiveOption { get; set; }
            [DataMember]
            public string ArchiveSeparater { get; set; }
        }
    }
}
