using NeeLaboratory.ComponentModel;
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
        private BookUnit _bookUnit;
        private ObservableCollection<Page> _pageList;
        private ExternalApplication _ExternalApplication = new ExternalApplication();
        private ClipboardUtility _ClipboardUtility = new ClipboardUtility();

        #endregion

        #region Constructors

        private BookOperation()
        {
            BookHub.Current.BookChanging += BookHub_BookChanging;
            BookHub.Current.BookChanged += BookHub_BookChanged;

            PagemarkCollection.Current.PagemarkChanged += PagemarkCollection_PagemarkChanged;
        }


        #endregion

        #region Events

        // ブックが変更される
        public event EventHandler BookChanging;

        // ブックが変更された
        public event EventHandler<BookChangedEventArgs> BookChanged;

        // ページが変更された
        public event EventHandler<ViewPageCollectionChangedEventArgs> ViewContentsChanged;

        // ページがソートされた
        public event EventHandler PagesSorted;

        // ページリストが変更された
        public event EventHandler PageListChanged;

        // ページが削除された
        public event EventHandler<PageChangedEventArgs> PageRemoved;

        #endregion

        #region Properties

        // ページ終端でのアクション
        [PropertyMember("@ParamBookOperationPageEndAction")]
        public PageEndAction PageEndAction { get; set; }

        [PropertyMember("@ParamBookOperationNotifyPageLoop")]
        public bool IsNotifyPageLoop { get; set; }

        /// <summary>
        /// 操作の有効設定。ロード中は機能を無効にするために使用
        /// </summary>
        public bool IsEnabled
        {
            get { return _isEnabled; }
            set { if (_isEnabled != value) { _isEnabled = value; RaisePropertyChanged(); } }
        }

        public BookUnit BookUnit
        {
            get { return _bookUnit; }
            private set { if (_bookUnit != value) { _bookUnit = value; RaisePropertyChanged(); } }
        }

        public Book Book => _bookUnit?.Book;

        public string Place => _bookUnit?.Book.Place;

        public bool IsValid => _bookUnit != null;

        public ObservableCollection<Page> PageList
        {
            get { return _pageList; }
        }

        /// <summary>
        /// 外部アプリ設定
        /// </summary>
        public ExternalApplication ExternalApplication
        {
            get { return _ExternalApplication; }
            set { if (_ExternalApplication != value) { _ExternalApplication = value ?? new ExternalApplication(); RaisePropertyChanged(); } }
        }

        /// <summary>
        /// クリップボード
        /// </summary>
        public ClipboardUtility ClipboardUtility
        {
            get { return _ClipboardUtility; }
            set { if (_ClipboardUtility != value) { _ClipboardUtility = value ?? new ClipboardUtility(); RaisePropertyChanged(); } }
        }

        #endregion

        #region Methods


        private void BookHub_BookChanging(object sender, EventArgs e)
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
            this.BookUnit = BookHub.Current.BookUnit;

            if (this.BookUnit != null)
            {
                this.Book.ViewContentsChanged += Book_ViewContentsChanged;
                this.Book.PagesSorted += Book_PagesSorted;
                this.Book.PageTerminated += Book_PageTerminated;
                this.Book.PageRemoved += Book_PageRemoved;
            }

            //
            RaisePropertyChanged(nameof(IsBookmark));

            // マーカー復元
            // TODO: PageMarkersのしごと？
            UpdatePagemark();

            // ページリスト更新
            UpdatePageList(false);

            // ブック操作有効
            IsEnabled = true;

            // ページリスト更新通知
            PageListChanged?.Invoke(this, null);

            BookChanged?.Invoke(sender, e);
        }

        //
        private void Book_PagesSorted(object sender, EventArgs e)
        {
            UpdatePageList(true);
            PagesSorted?.Invoke(this, e);
        }

        //
        private void Book_ViewContentsChanged(object sender, ViewPageCollectionChangedEventArgs e)
        {
            if (!IsEnabled) return;

            RaisePropertyChanged(nameof(IsPagemark));
            ViewContentsChanged?.Invoke(sender, e);
        }


        // ページリスト更新
        // TODO: クリアしてもサムネイルのListBoxは項目をキャッシュしてしまうので、なんとかせよ
        // サムネイル用はそれに特化したパーツのみ提供する？
        // いや、ListBoxを独立させ、それ自体を作り直す方向で？んー？
        // 問い合わせがいいな。
        // 問い合わせといえば、BitmapImageでOutOfMemoryが取得できない問題も。
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

            RaisePropertyChanged(nameof(IsPagemark));
        }


        // 現在ページ番号取得
        public int GetPageIndex()
        {
            return this.Book == null ? 0 : this.Book.DisplayIndex; // GetPosition().Index;
        }

        // 現在ページ番号を設定し、表示を切り替える (先読み無し)
        public void RequestPageIndex(object sender, int index)
        {
            this.Book?.RequestSetPosition(sender, new PagePosition(index, 0), 1, false);
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
        /// ファイルロック解除
        /// </summary>
        public void Unlock()
        {
            // NOTE: メインスレッドで行うとSevenZipSharpがCOM例外になるのであえてタスク化。なぜ！？
            Task.Run(() => this.Book?.Unlock());
        }

        #endregion

        #region BookCommand : ページ削除

        // 現在表示しているページのファイル削除可能？
        public bool CanDeleteFile()
        {
            return FileIOProfile.Current.IsEnabled && FileIO.Current.CanRemovePage(Book?.GetViewPage());
        }

        // 現在表示しているページのファイルを削除する
        public async void DeleteFile()
        {
            if (CanDeleteFile())
            {
                await FileIO.Current.RemovePageAsync(Book?.GetViewPage());
            }
        }

        #endregion

        #region BookCommand : ブック削除

        // 現在表示しているブックの削除可能？
        public bool CanDeleteBook()
        {
            return FileIOProfile.Current.IsEnabled && Book != null && (File.Exists(Book.Place) || Directory.Exists(Book.Place));
        }

        // 現在表示しているブックを削除する
        public async void DeleteBook()
        {
            if (CanDeleteBook())
            {
                await FileIO.Current.RemoveAsync(Book.Place, Resources.DialogFileDeleteBookTitle);
            }
        }

        #endregion


        #region BookCommand : ページ出力

        // ファイルの場所を開くことが可能？
        public bool CanOpenFilePlace()
        {
            return Book?.GetViewPage() != null;
        }

        // ファイルの場所を開く
        public void OpenFilePlace()
        {
            if (CanOpenFilePlace())
            {
                string place = Book.GetViewPage()?.GetFolderOpenPlace();
                if (place != null)
                {
                    System.Diagnostics.Process.Start("explorer.exe", "/select,\"" + place + "\"");
                }
            }
        }


        // 外部アプリで開く
        public void OpenApplication()
        {
            if (CanOpenFilePlace())
            {
                try
                {
                    this.ExternalApplication.Call(Book?.GetViewPages());
                }
                catch (Exception e)
                {
                    var message = "";
                    if (this.ExternalApplication.LastCall != null)
                    {
                        message += $"{Resources.WordCommand}: {this.ExternalApplication.LastCall}\n";
                    }
                    message += $"{Resources.WordCause}: {e.Message}";

                    new MessageDialog(message, Resources.DialogOpenApplicationErrorTitle).ShowDialog();
                }
            }
        }


        // クリップボードにコピー
        public void CopyToClipboard()
        {
            if (CanOpenFilePlace())
            {
                try
                {
                    this.ClipboardUtility.Copy(Book?.GetViewPages());
                }
                catch (Exception e)
                {
                    new MessageDialog($"{Resources.WordCause}: {e.Message}", Resources.DialogCopyErrorTitle).ShowDialog();
                }
            }
        }


        /// <summary>
        /// ファイル保存可否
        /// </summary>
        /// <returns></returns>
        public bool CanExport()
        {
            var pages = Book?.GetViewPages();
            if (pages == null || pages.Count == 0) return false;

            var bitmapSource = (pages[0].Content as BitmapContent)?.BitmapSource;
            if (bitmapSource == null) return false;

            return true;
        }

        // ファイルに保存する
        // TODO: OutOfMemory対策
        public void Export()
        {
            if (CanExport())
            {
                try
                {
                    var pages = Book.GetViewPages();
                    int index = Book.GetViewPageindex() + 1;
                    string name = $"{Path.GetFileNameWithoutExtension(Book.Place)}_{index:000}-{index + pages.Count - 1:000}.png";
                    var exporter = new Exporter();
                    exporter.Initialize(pages, Book.BookReadOrder, name);
                    exporter.Background = ContentCanvasBrush.Current.CreateBackgroundBrush();
                    exporter.BackgroundFront = ContentCanvasBrush.Current.CreateBackgroundFrontBrush(new DpiScale(1, 1));
                    if (exporter.ShowDialog() == true)
                    {
                        try
                        {
                            exporter.Export();
                        }
                        catch (Exception e)
                        {
                            new MessageDialog($"{Resources.WordCause}: {e.Message}", Resources.DialogExportErrorTitle).ShowDialog();
                        }
                    }
                }
                catch (Exception e)
                {
                    new MessageDialog($"{Resources.DialogImageExportError}\n{Resources.WordCause}: {e.Message}", Resources.DialogImageExportErrorTitle).ShowDialog();
                    return;
                }
            }
        }


        #endregion

        #region BookCommand : ページ操作

        // ページ終端を超えて移動しようとするときの処理
        private async void Book_PageTerminated(object sender, PageTerminatedEventArgs e)
        {
            // TODO ここでSlideShowを参照しているが、引数で渡すべきでは？
            if (SlideShow.Current.IsPlayingSlideShow && SlideShow.Current.IsSlideShowByLoop)
            {
                FirstPage();
            }

            else if (this.PageEndAction == PageEndAction.Loop)
            {
                if (e.Direction < 0)
                {
                    LastPage();
                }
                else
                {
                    FirstPage();
                }
                if (this.IsNotifyPageLoop)
                {
                    InfoMessage.Current.SetMessage(InfoMessageType.Notify, Properties.Resources.NotifyBookOperationPageLoop);
                }
            }
            else if (this.PageEndAction == PageEndAction.NextFolder)
            {
                if (e.Direction < 0)
                {
                    await BookshelfFolderList.Current.PrevFolder(BookLoadOption.LastPage);
                }
                else
                {
                    await BookshelfFolderList.Current.NextFolder(BookLoadOption.FirstPage);
                }
            }
            else
            {
                if (SlideShow.Current.IsPlayingSlideShow)
                {
                    // スライドショー解除
                    SlideShow.Current.IsPlayingSlideShow = false;
                }

                // 本の場合のみ処理。メディアでは不要
                else if (this.Book != null && !this.Book.IsMedia)
                {
                    if (e.Direction < 0)
                    {
                        SoundPlayerService.Current.PlaySeCannotMove();
                        InfoMessage.Current.SetMessage(InfoMessageType.Notify, Properties.Resources.NotifyFirstPage);
                    }
                    else
                    {
                        SoundPlayerService.Current.PlaySeCannotMove();
                        InfoMessage.Current.SetMessage(InfoMessageType.Notify, Properties.Resources.NotifyLastPage);
                    }
                }
            }
        }


        // ページ削除時の処理
        private void Book_PageRemoved(object sender, PageChangedEventArgs e)
        {
            // ページマーカーから削除
            RemovePagemark(this.Book.Place, e.Page.EntryFullName);

            UpdatePageList(true);
            PageRemoved?.Invoke(sender, e);
        }

        // ページ移動量をメディアの時間移動量に変換して移動
        private void MoveMediaPage(int delta)
        {
            if (MediaPlayerOperator.Current == null) return;

            var isTerminated = MediaPlayerOperator.Current.AddPosition(TimeSpan.FromSeconds(delta * MediaControl.Current.PageSeconds));

            if (isTerminated)
            {
                this.Book?.RaisePageTerminatedEvent(delta < 0 ? -1 : 1);
            }
        }

        // 前のページに移動
        public void PrevPage()
        {
            if (this.Book == null) return;

            if (this.Book.IsMedia)
            {
                MoveMediaPage(-1);
            }
            else
            {
                this.Book.PrevPage();
            }
        }

        // 次のページに移動
        public void NextPage()
        {
            if (this.Book == null) return;

            if (this.Book.IsMedia)
            {
                MoveMediaPage(+1);
            }
            else
            {
                this.Book.NextPage();
            }
        }

        // 1ページ前に移動
        public void PrevOnePage()
        {
            if (this.Book == null) return;

            if (this.Book.IsMedia)
            {
                MoveMediaPage(-1);
            }
            else
            {
                this.Book.PrevPage(1);
            }
        }

        // 1ページ後に移動
        public void NextOnePage()
        {
            if (this.Book == null) return;

            if (this.Book.IsMedia)
            {
                MoveMediaPage(+1);
            }
            else
            {
                this.Book?.NextPage(1);
            }
        }

        // 指定ページ数前に移動
        public void PrevSizePage(int size)
        {
            if (this.Book == null) return;

            if (this.Book.IsMedia)
            {
                MoveMediaPage(-size);
            }
            else
            {
                this.Book.PrevPage(size);
            }
        }

        // 指定ページ数後に移動
        public void NextSizePage(int size)
        {
            if (this.Book == null) return;

            if (this.Book.IsMedia)
            {
                MoveMediaPage(+size);
            }
            else
            {
                this.Book.NextPage(size);
            }
        }


        // 最初のページに移動
        public void FirstPage()
        {
            if (this.Book == null) return;

            if (this.Book.IsMedia)
            {
                MediaPlayerOperator.Current?.SetPositionFirst();
            }
            else
            {
                this.Book.FirstPage();
            }
        }

        // 最後のページに移動
        public void LastPage()
        {
            if (this.Book == null) return;

            if (this.Book.IsMedia)
            {
                MediaPlayerOperator.Current?.SetPositionLast();
            }
            else
            {
                this.Book.LastPage();
            }
        }

        // ページを指定して移動
        public void JumpPage()
        {
            if (this.Book == null || this.Book.IsMedia) return;

            var dialogModel = new PageSelecteDialogModel()
            {
                Value = this.Book.GetViewPageindex() + 1,
                Min = 1,
                Max = this.Book.Pages.Count
            };

            var dialog = new PageSelectDialog(dialogModel);
            dialog.Owner = MainWindow.Current;
            dialog.WindowStartupLocation = WindowStartupLocation.CenterOwner;

            var result = dialog.ShowDialog();

            if (result == true)
            {
                var page = this.Book.GetPage(dialogModel.Value - 1);
                this.Book.JumpPage(page);
            }
        }

        // 指定ページに移動
        public void JumpPage(Page page)
        {
            if (_isEnabled && page != null) this.Book?.JumpPage(page);
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
        public bool ToggleMediaPlay(object sender, ExecutedRoutedEventArgs e)
        {
            if (e.Handled) return false;

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
                e.Handled = true;
                return true;
            }
            else
            {
                return false;
            }
        }

        // スライドショー用：次のページへ移動
        public void NextSlide()
        {
            if (SlideShow.Current.IsPlayingSlideShow) NextPage();
        }

        #endregion

        #region BookCommand : ブックマーク

        // ブックマーク登録可能？
        public bool CanBookmark()
        {
            return (Book != null);
        }

        // ブックマーク切り替え
        public void ToggleBookmark()
        {
            if (CanBookmark())
            {
                var query = new QueryPath(Book.Place);

                if (IsBookmark)
                {
                    BookmarkCollectionService.Remove(query);
                }
                else
                {
                    // ignore temporary directory
                    if (Book.Place.StartsWith(Temporary.Current.TempDirectory))
                    {
                        ToastService.Current.Show(new Toast(Resources.DialogBookmarkError, null, ToastIcon.Error));
                        return;
                    }

                    BookmarkCollectionService.Add(query);
                }

                RaisePropertyChanged(nameof(IsBookmark));
            }
        }

        // ブックマーク判定
        public bool IsBookmark
        {
            get
            {
                return BookmarkCollection.Current.Contains(Book?.Place);
            }
        }

        #endregion

        #region BookCommand : ページマーク

        // ページマークにに追加、削除された
        public event EventHandler PagemarkChanged;

        //
        private void PagemarkCollection_PagemarkChanged(object sender, PagemarkCollectionChangedEventArgs e)
        {
            if (!IsValid)
            {
                return;
            }

            var placeQuery = new QueryPath(Place);
            if (placeQuery.Scheme == QueryScheme.Pagemark)
            {
                switch (e.Action)
                {
                    case EntryCollectionChangedAction.Replace:
                    case EntryCollectionChangedAction.Reset:
                        BookHub.Current.RequestUnload(true);
                        break;
                    case EntryCollectionChangedAction.Add:
                        {
                            // ブックに含まれるページが追加されたら再読込 ... シャッフルで再ソートされてしまう問題あり
                            var query = e.Item.CreateQuery(QueryScheme.Pagemark);
                            if (Book.IsRecursiveFolder && placeQuery.Include(query) || query.GetParent().Equals(placeQuery))
                            {
                                Debug.WriteLine($"BookOperation: Add pagemarks: {e.Item.Value.Name}");
                                BookHub.Current.ReLoad();
                            }
                        }
                        break;

                    case EntryCollectionChangedAction.Remove:
                        {
                            var parentQuery = e.Parent.CreateQuery(QueryScheme.Pagemark);

                            // 現在のページに存在している場合はその項目を削除
                            // 自身もしくは親フォルダーから削除された場合は本を閉じる
                            // 含まれるフォルダーが削除された場合は再読込
                            var page = Book.Pages.FirstOrDefault(i => i.Entry.Instance is TreeListNode<IPagemarkEntry> node && node == e.Item);
                            if (page != null)
                            {
                                Debug.WriteLine($"BookOperation: Remve pagemark: {e.Item.Value.Name}");
                                Book.RequestRemove(this, page);
                            }
                            else if (PagemarkCollection.Current.FindNode(placeQuery) == null) // 親が削除されていたら見つからない
                            {
                                Debug.WriteLine($"BookOperation: Remove parent pagemark: {e.Item.Value.Name}");
                                BookHub.Current.RequestUnload(true);
                            }
                            else if (e.Item.Value is PagemarkFolder && Book.IsRecursiveFolder && placeQuery.Include(parentQuery))
                            {
                                Debug.WriteLine($"BookOperation: Remove pagemarks: {e.Item.Value.Name}");
                                BookHub.Current.ReLoad();
                            }
                        }
                        break;

                    case EntryCollectionChangedAction.Rename:
                        {
                            var parentQuery = e.Parent.CreateQuery(QueryScheme.Pagemark);

                            // 自信か親の名前が変化する場合、閉じる
                            // 含まれるフォルダー名が変化する場合、再読込
                            if (PagemarkCollection.Current.FindNode(placeQuery) == null) // 自身もしくは親の名前が変わっていたら見つからない
                            {
                                Debug.WriteLine($"BookOperation: Rename parent pagemark: {e.Item.Value.Name}");
                                BookHub.Current.RequestUnload(true);
                            }
                            else if (e.Item.Value is PagemarkFolder && Book.IsRecursiveFolder && placeQuery.Include(parentQuery))
                            {
                                Debug.WriteLine($"BookOperation: Rename pagemarks: {e.Item.Value.Name}");
                                BookHub.Current.ReLoad();
                            }
                        }
                        break;
                }
            }

            else
            {
                switch (e.Action)
                {
                    case EntryCollectionChangedAction.Replace:
                    case EntryCollectionChangedAction.Reset:
                        UpdatePagemark();
                        break;
                    case EntryCollectionChangedAction.Add:
                    case EntryCollectionChangedAction.Remove:
                        if (e.Item.Value is Pagemark pagemark && pagemark.Place == Place)
                        {
                            UpdatePagemark();
                        }
                        break;
                }
            }
        }

        //
        public bool IsPagemark
        {
            get { return IsMarked(); }
        }

        // 表示ページのマーク判定
        public bool IsMarked()
        {
            return this.Book != null ? this.Book.IsMarked(this.Book.GetViewPage()) : false;
        }

        // ページマーク登録可能？
        public bool CanPagemark()
        {
            return this.Book != null && !this.Book.IsMedia && !this.Book.IsPagemarkFolder;
        }

        // マーカー切り替え
        public Pagemark TogglePagemark()
        {
            if (!_isEnabled || this.Book == null || this.Book.IsMedia || this.Book.IsPagemarkFolder) return null;

            var place = Book.Place;
            var page = Book.GetViewPage();
            if (page == null)
            {
                return null;
            }
            var entryName = page.EntryFullName;
            var node = PagemarkCollection.Current.FindNode(place, entryName);
            if (node == null)
            {
                return AddPagemark(place, entryName); ////, page.Length, page.LastWriteTime);
            }
            else
            {
                RemovePagemark(place, entryName);
                return null;
            }
        }

        //
        public Pagemark AddPagemark()
        {
            if (!_isEnabled || this.Book == null || this.Book.IsMedia || this.Book.IsPagemarkFolder) return null;

            var place = Book.Place;
            var page = Book.GetViewPage();
            if (page == null)
            {
                return null;
            }
            return AddPagemark(place, page.EntryFullName);
        }

        //
        public Pagemark AddPagemark(string place, string entryName)
        {
            // ignore temporary directory
            if (place.StartsWith(Temporary.Current.TempDirectory))
            {
                ToastService.Current.Show(new Toast(Resources.DialogPagemarkError, null, ToastIcon.Error));
                return null;
            }

            var node = PagemarkCollection.Current.FindNode(place, entryName);
            if (node == null)
            {
                // TODO: 登録時にサムネイルキャッシュにも登録
                var pagemark = new Pagemark(place, entryName);
                PagemarkCollection.Current.Add(new TreeListNode<IPagemarkEntry>(pagemark));
                return pagemark;
            }
            else
            {
                return node.Value as Pagemark;
            }
        }

        #region 開発用

        /// <summary>
        /// (開発用) たくさんのページマーク作成
        /// </summary>
        [Conditional("DEBUG")]
        public void Test_MakeManyPagemark()
        {
            if (Book == null) return;
            var place = Book.Place;
            for (int index = 0; index < Book.Pages.Count; index += 100)
            {
                var page = Book.Pages[index];
                AddPagemark(place, page.EntryFullName);
            }
        }

        #endregion

        // マーカー削除
        public bool RemovePagemark(string place, string entryName)
        {
            var node = PagemarkCollection.Current.FindNode(place, entryName);
            if (node != null)
            {
                return PagemarkCollection.Current.Remove(node);
            }
            else
            {
                return false;
            }
        }

        // マーカー表示更新
        public void UpdatePagemark()
        {
            // 本にマーカを設定
            // TODO: これはPagemarkerの仕事？
            this.Book?.SetMarkers(PagemarkCollection.Current.Collect(this.Book.Place).Select(e => e.EntryName));

            // 表示更新
            PagemarkChanged?.Invoke(this, null);
            RaisePropertyChanged(nameof(IsPagemark));
        }

        //
        public bool CanPrevPagemarkInPlace(MovePagemarkCommandParameter param)
        {
            return (this.Book?.Markers != null && Current.Book.Markers.Count > 0) || param.IsIncludeTerminal;
        }

        //
        public bool CanNextPagemarkInPlace(MovePagemarkCommandParameter param)
        {
            return (this.Book?.Markers != null && Current.Book.Markers.Count > 0) || param.IsIncludeTerminal;
        }

        // ページマークに移動
        public void PrevPagemarkInPlace(MovePagemarkCommandParameter param)
        {
            if (!_isEnabled || this.Book == null) return;
            var result = this.Book.RequestJumpToMarker(this, -1, param.IsLoop, param.IsIncludeTerminal);
            if (result != null)
            {
                // ページマーク更新
                PagemarkList.Current.Jump(this.Book.Place, result.EntryName);
            }
            else
            {
                InfoMessage.Current.SetMessage(InfoMessageType.Notify, Properties.Resources.NotifyFirstPagemark);
            }
        }

        // ページマークに移動
        public void NextPagemarkInPlace(MovePagemarkCommandParameter param)
        {
            if (!_isEnabled || this.Book == null) return;
            var result = this.Book.RequestJumpToMarker(this, +1, param.IsLoop, param.IsIncludeTerminal);
            if (result != null)
            {
                // ページマーク更新
                PagemarkList.Current.Jump(this.Book.Place, result.EntryName);
            }
            else
            {
                InfoMessage.Current.SetMessage(InfoMessageType.Notify, Properties.Resources.NotifyLastPagemark);
            }
        }

        // ページマークに移動
        public bool JumpPagemarkInPlace(Pagemark mark)
        {
            if (mark == null) return false;

            if (mark.Place == this.Book?.Place)
            {
                Page page = this.Book.GetPage(mark.EntryName);
                if (page != null)
                {
                    JumpPage(page);
                    return true;
                }
            }

            return false;
        }

        #endregion


        #region Memento
        [DataContract]
        public class Memento
        {
            [DataMember]
            public PageEndAction PageEndAction { get; set; }

            [DataMember]
            public ExternalApplication ExternalApplication { get; set; }

            [DataMember]
            public ClipboardUtility ClipboardUtility { get; set; }

            [DataMember]
            public bool IsNotifyPageLoop { get; set; }
        }

        //
        public Memento CreateMemento()
        {
            var memento = new Memento();
            memento.PageEndAction = this.PageEndAction;
            memento.ExternalApplication = ExternalApplication.Clone();
            memento.ClipboardUtility = ClipboardUtility.Clone();
            memento.IsNotifyPageLoop = this.IsNotifyPageLoop;
            return memento;
        }

        //
        public void Restore(Memento memento)
        {
            if (memento == null) return;
            this.PageEndAction = memento.PageEndAction;
            this.ExternalApplication = memento.ExternalApplication?.Clone();
            this.ClipboardUtility = memento.ClipboardUtility?.Clone();
            this.IsNotifyPageLoop = memento.IsNotifyPageLoop;
        }
        #endregion

    }

}
