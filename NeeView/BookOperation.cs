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

        //
        public event EventHandler BookChanged;

        //
        public event EventHandler<PageChangedEventArgs> PageChanged;

        #endregion


        //
        BookHub _bookHub;

        //
        public BookOperation(BookHub bookHub)
        {
            Current = this;

            _bookHub = bookHub;

            _bookHub.AddressChanged +=
                (s, e) => RaisePropertyChanged(nameof(IsPagemark));

            _bookHub.PageChanged +=
                (s, e) =>
                {
                    RaisePropertyChanged(nameof(IsPagemark));
                    PageChanged?.Invoke(this, e);
                };

            _bookHub.PagesSorted +=
                (s, e) => UpdatePageList();

            _bookHub.PageRemoved +=
                (s, e) => UpdatePageList();

            _bookHub.AddPropertyChanged(nameof(_bookHub.Current),
                (s, e) => { this.BookUnit = _bookHub.Current; });
        }


        // メッセージ通知
        // TODO: メッセージ系はグローバルなので専用モデルにして直接コール？
        public event EventHandler<string> InfoMessage;


        /// <summary>
        /// Book property.
        /// </summary>
        public BookUnit BookUnit
        {
            get { return _bookUnit; }
            set { if (_bookUnit != value) { _bookUnit = value; RaisePropertyChanged(null); BookChanged?.Invoke(this, null); } }
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
            if (_bookHub.IsLoading || this.Book == null) return;

            if (Current.Book.Place.StartsWith(Temporary.TempDirectory))
            {
                new MessageDialog($"原因: 一時フォルダーはページマークできません", "ページマークできません").ShowDialog();
            }

            // マーク登録/解除
            // TODO: 登録時にサムネイルキャッシュにも登録
            ModelContext.Pagemarks.Toggle(new Pagemark(this.Book.Place, this.Book.GetViewPage().FullPath));

            // 更新
            UpdatePagemark();
        }


        // マーカー削除
        public void RemovePagemark(Pagemark mark)
        {
            ModelContext.Pagemarks.Remove(mark);
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
            this.Book?.SetMarkers(ModelContext.Pagemarks.Collect(this.Book.Place).Select(e => e.EntryName));

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
            if (_bookHub.IsLoading || this.Book == null) return;
            var result = this.Book.RequestJumpToMarker(this, -1, param.IsLoop, param.IsIncludeTerminal);
            if (!result)
            {
                InfoMessage?.Invoke(this, "現在ページより前のページマークはありません");
            }
        }

        public void NextPagemarkInPlace(MovePagemarkCommandParameter param)
        {
            if (_bookHub.IsLoading || this.Book == null) return;
            var result = this.Book.RequestJumpToMarker(this, +1, param.IsLoop, param.IsIncludeTerminal);
            if (!result)
            {
                InfoMessage?.Invoke(this, "現在ページより後のページマークはありません");
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
                    _bookHub.JumpPage(page);
                    return true;
                }
            }

            return false;
        }

        #endregion
    }
}
