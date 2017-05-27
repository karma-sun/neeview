// Copyright (c) 2016 Mitsuhiro Ito (nee)
//
// This software is released under the MIT License.
// http://opensource.org/licenses/mit-license.php

using NeeView.ComponentModel;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace NeeView
{
    /// <summary>
    /// 本の操作
    /// </summary>
    public class BookOperation : BindableBase
    {
        // System Object
        public static BookOperation Current;

        #region events

        // ブックが変更された
        public event EventHandler BookChanged;

        // ページ表示内容の更新
        //public event EventHandler<ViewSource> ViewContentsChanged;

        // ページが変更された
        public event EventHandler<PageChangedEventArgs> PageChanged;

        // ページがソートされた
        public event EventHandler PagesSorted;

        // ページが削除された
        // TODO: EventArgs
        public event EventHandler<Page> PageRemoved;


        #endregion


        // ページ終端でのアクション
        public PageEndAction PageEndAction { get; set; }


        //
        public BookOperation()
        {
            Current = this;

            ////this.InfoMessage +=
            ////    (s, e) => NeeView.InfoMessage.Current.SetMessage(NoticeShowMessageStyle, e);
        }

        /// <summary>
        /// 本の更新
        /// </summary>
        /// <param name="bookUnit"></param>
        public void SetBook(BookUnit bookUnit)
        {
            this.BookUnit = bookUnit;

            if (this.BookUnit != null)
            {
                this.Book.PageChanged += Book_PageChanged;
                this.Book.PagesSorted += Book_PagesSorted;
                this.Book.PageTerminated += Book_PageTerminated;
                this.Book.PageRemoved += Book_PageRemoved;
            }

            // マーカー復元
            // TODO: PageMarkersのしごと？
            UpdatePagemark();

            // ページリスト更新
            UpdatePageList();

            // ブック操作有効
            IsEnabled = true;

            BookChanged?.Invoke(this, null);
        }

        //
        private void Book_PagesSorted(object sender, EventArgs e)
        {
            UpdatePageList();
            PagesSorted?.Invoke(this, e);
        }

        //
        private void Book_PageChanged(object sender, PageChangedEventArgs e)
        {
            RaisePropertyChanged(nameof(IsPagemark));
            PageChanged?.Invoke(this, e);
        }


        // メッセージ通知
        // TODO: メッセージ系はグローバルなので専用モデルにして直接コール？
        //public event EventHandler<string> InfoMessage;


        /// <summary>
        /// IsEnabled property.
        /// ロード中は機能を無効にするため
        /// </summary>
        public bool IsEnabled
        {
            get { return _isEnabled; }
            set { if (_isEnabled != value) { _isEnabled = value; RaisePropertyChanged(); } }
        }

        private bool _isEnabled;



        /// <summary>
        /// Book property.
        /// </summary>
        public BookUnit BookUnit
        {
            get { return _bookUnit; }
            set { if (_bookUnit != value) { _bookUnit = value; RaisePropertyChanged(); /*BookChanged?.Invoke(this, null);*/ } }
        }

        private BookUnit _bookUnit;

        //
        public Book Book => _bookUnit?.Book;

        //
        public bool IsValid => _bookUnit != null;


        /// <summary>
        /// PaageList
        /// </summary>
        public ObservableCollection<Page> PageList
        {
            get { return _pageList; }
            set
            {
                _pageList = value;
                RaisePropertyChanged();
            }
        }

        private ObservableCollection<Page> _pageList;


        // ページリスト更新
        // TODO: クリアしてもサムネイルのListBoxは項目をキャッシュしてしまうので、なんとかせよ
        // サムネイル用はそれに特化したパーツのみ提供する？
        // いや、ListBoxを独立させ、それ自体を作り直す方向で？んー？
        // 問い合わせがいいな。
        // 問い合わせといえば、BitmapImageでOutOfMemoryが取得できない問題も。
        public void UpdatePageList()
        {
            var pages = this.Book?.Pages;
            PageList = pages != null ? new ObservableCollection<Page>(pages) : null;

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



        #region BookCommand

        // ページ終端を超えて移動しようとするときの処理
        private void Book_PageTerminated(object sender, int e)
        {
            // TODO ここでSlideShowを参照しているが、引数で渡すべきでは？
            if (SlideShow.Current.IsPlayingSlideShow && SlideShow.Current.IsSlideShowByLoop)
            {
                FirstPage();
            }

            else if (this.PageEndAction == PageEndAction.Loop)
            {
                if (e < 0)
                {
                    LastPage();
                }
                else
                {
                    FirstPage();
                }
            }
            else if (this.PageEndAction == PageEndAction.NextFolder)
            {
                if (e < 0)
                {
                    FolderList.Current.PrevFolder(BookLoadOption.LastPage);
                }
                else
                {
                    FolderList.Current.NextFolder(BookLoadOption.FirstPage);
                }
            }
            else
            {
                if (SlideShow.Current.IsPlayingSlideShow)
                {
                    // スライドショー解除
                    SlideShow.Current.IsPlayingSlideShow = false;
                }

                else if (e < 0)
                {
                    InfoMessage.Current.SetMessage(InfoMessageType.Notify, "最初のページです");
                }
                else
                {
                    InfoMessage.Current.SetMessage(InfoMessageType.Notify, "最後のページです");
                }
            }
        }


        // ページ削除時の処理
        private void Book_PageRemoved(object sender, Page e)
        {
            // ページマーカーから削除
            RemovePagemark(new Pagemark(this.Book.Place, e.FullPath));

            UpdatePageList();
            PageRemoved?.Invoke(sender, e);
        }


        // 前のページに移動
        public void PrevPage()
        {
            this.Book?.PrevPage();
        }

        // 次のページに移動
        public void NextPage()
        {
            this.Book?.NextPage();
        }

        // 1ページ前に移動
        public void PrevOnePage()
        {
            this.Book?.PrevPage(1);
        }

        // 1ページ後に移動
        public void NextOnePage()
        {
            this.Book?.NextPage(1);
        }

        // 指定ページ数前に移動
        public void PrevSizePage(int size)
        {
            this.Book?.PrevPage(size);
        }

        // 指定ページ数後に移動
        public void NextSizePage(int size)
        {
            this.Book?.NextPage(size);
        }


        // 最初のページに移動
        public void FirstPage()
        {
            this.Book?.FirstPage();
        }

        // 最後のページに移動
        public void LastPage()
        {
            this.Book?.LastPage();
        }

        // 指定ページに移動
        public void JumpPage(Page page)
        {
            if (_isEnabled && page != null) this.Book?.JumpPage(page);
        }

        // スライドショー用：次のページへ移動
        public void NextSlide()
        {
            if (SlideShow.Current.IsPlayingSlideShow) NextPage();
        }

        #endregion




        #region Pagemark

        // ページマークにに追加、削除された
        public event EventHandler<PagemarkChangedEventArgs> PagemarkChanged;

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
            return (this.Book != null);
        }

        // マーカー切り替え
        public void TogglePagemark()
        {
            if (!_isEnabled || this.Book == null) return;

            if (Current.Book.Place.StartsWith(Temporary.TempDirectory))
            {
                new MessageDialog($"原因: 一時フォルダーはページマークできません", "ページマークできません").ShowDialog();
            }

            // マーク登録/解除
            // TODO: 登録時にサムネイルキャッシュにも登録
            PagemarkCollection.Current.Toggle(new Pagemark(this.Book.Place, this.Book.GetViewPage().FullPath));

            // 更新
            UpdatePagemark();
        }


        // マーカー削除
        public void RemovePagemark(Pagemark mark)
        {
            PagemarkCollection.Current.Remove(mark);
            UpdatePagemark(mark);
        }

        /// <summary>
        /// マーカー表示更新
        /// </summary>
        /// <param name="mark">変更や削除されたマーカー</param>
        public void UpdatePagemark(Pagemark mark)
        {
            // 現在ブックに影響のある場合のみ更新
            if (this.Book?.Place == mark.Place)
            {
                UpdatePagemark();
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

        public bool CanPrevPagemarkInPlace(MovePagemarkCommandParameter param)
        {
            return (this.Book?.Markers != null && Current.Book.Markers.Count > 0) || param.IsIncludeTerminal;
        }

        public bool CanNextPagemarkInPlace(MovePagemarkCommandParameter param)
        {
            return (this.Book?.Markers != null && Current.Book.Markers.Count > 0) || param.IsIncludeTerminal;
        }

        // ページマークに移動
        public void PrevPagemarkInPlace(MovePagemarkCommandParameter param)
        {
            if (!_isEnabled || this.Book == null) return;
            var result = this.Book.RequestJumpToMarker(this, -1, param.IsLoop, param.IsIncludeTerminal);
            if (!result)
            {
                InfoMessage.Current.SetMessage(InfoMessageType.Notify, "現在ページより前のページマークはありません");
            }
        }

        public void NextPagemarkInPlace(MovePagemarkCommandParameter param)
        {
            if (!_isEnabled || this.Book == null) return;
            var result = this.Book.RequestJumpToMarker(this, +1, param.IsLoop, param.IsIncludeTerminal);
            if (!result)
            {
                InfoMessage.Current.SetMessage(InfoMessageType.Notify, "現在ページより後のページマークはありません");
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
        }

        //
        public Memento CreateMemento()
        {
            var memento = new Memento();
            memento.PageEndAction = this.PageEndAction;
            return memento;
        }

        //
        public void Restore(Memento memento)
        {
            if (memento == null) return;
            this.PageEndAction = memento.PageEndAction;
        }
        #endregion

    }
}
