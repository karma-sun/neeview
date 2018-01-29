// Copyright (c) 2016-2018 Mitsuhiro Ito (nee)
//
// This software is released under the MIT License.
// http://opensource.org/licenses/mit-license.php

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace NeeView
{
    // BookMementoCollectionChangedイベントの種類
    public enum BookMementoCollectionChangedType
    {
        Load,
        Clear,
        Add,
        Update,
        Remove,
    }

    // BookMementoCollectionChangedイベントの引数
    public class BookMementoCollectionChangedArgs : EventArgs
    {
        public BookMementoCollectionChangedType HistoryChangedType { get; set; }
        public string Key { get; set; }

        public BookMementoCollectionChangedArgs(BookMementoCollectionChangedType type, string key)
        {
            HistoryChangedType = type;
            Key = key;
        }
    }


    /// <summary>
    /// BookMementoUnit用ノード
    /// </summary>
    public class BookMementoUnitNode : IHasPage
    {
        public BookMementoUnit Value { get; set; }

        public BookMementoUnitNode(BookMementoUnit value)
        {
            Value = value;
        }

        #region IHasPage Support

        public Page GetPage()
        {
            return Value?.ArchivePage;
        }

        #endregion
    }

    /// <summary>
    /// 高速検索用BookMemento辞書
    /// 履歴、ブックマーク共有の辞書です
    /// SQL使いたくなってきた..
    /// </summary>
    public class BookMementoUnit : IHasPage
    {
        // 履歴用リンク
        public LinkedListNode<BookMementoUnit> HistoryNode { get; set; }

        // ブックマーク用リンク
        public BookMementoUnitNode BookmarkNode { get; set; }

        // ページマーク用リンク
        public BookMementoUnitNode PagemarkNode { get; set; }

        // 本体
        public Book.Memento Memento { get; set; }

        //
        public override string ToString()
        {
            return Memento?.Place ?? base.ToString();
        }

        #region for Thumbnail

        /// <summary>
        /// ArchivePage Property.
        /// サムネイル用
        /// </summary>
        private volatile ArchivePage _archivePage;
        public ArchivePage ArchivePage
        {
            get
            {
                if (_archivePage == null)
                {
                    _archivePage = new ArchivePage(new ArchiveEntry(Memento.Place));
                    _archivePage.Thumbnail.IsCacheEnabled = true;
                    _archivePage.Thumbnail.Touched += Thumbnail_Touched;
                }
                return _archivePage;
            }
        }

        //
        private void Thumbnail_Touched(object sender, EventArgs e)
        {
            var thumbnail = (Thumbnail)sender;
            BookThumbnailPool.Current.Add(thumbnail);
        }

        #endregion

        #region IHasPage Support

        public Page GetPage()
        {
            return ArchivePage;
        }

        #endregion
    }


    /// <summary>
    /// 履歴
    /// </summary>
    public class BookMementoCollection
    {
        public static BookMementoCollection Current { get; private set; }

        //
        public BookMementoCollection()
        {
            Current = this;
        }

        public Dictionary<string, BookMementoUnit> Items { get; set; } = new Dictionary<string, BookMementoUnit>();

        private BookMementoUnit _lastFindUnit;

        //
        public void Add(BookMementoUnit unit)
        {
            Debug.Assert(unit != null);
            Debug.Assert(unit.Memento != null);
            Debug.Assert(unit.Memento.Place != null);
            Debug.Assert(unit.HistoryNode != null || unit.BookmarkNode != null || unit.PagemarkNode != null);

            Items.Add(unit.Memento.Place, unit);
        }

        //
        public BookMementoUnit Find(string place)
        {
            if (place == null) return null;

            // 最後に検索されたユニットは再度検索される時に高速にする
            if (place == _lastFindUnit?.Memento.Place) return _lastFindUnit;

            BookMementoUnit unit;
            Items.TryGetValue(place, out unit);
            _lastFindUnit = unit;
            return unit;
        }

        //
        internal void Rename(string src, string dst)
        {
            if (src == null || dst == null) return;

            var unit = Find(src);
            if (unit != null)
            {
                this.Items.Remove(src);

                unit.Memento.Place = dst;
                this.Items.Add(dst, unit);
            }
        }
    }
}
