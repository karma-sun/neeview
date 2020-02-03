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
    /// <summary>
    /// 高速検索用BookMemento辞書
    /// 履歴、ブックマーク共有の辞書です
    /// SQL使いたくなってきた..
    /// </summary>
    public class BookMementoUnit : IHasPage
    {
        private BookMementoUnit()
        {
        }

        public Book.Memento Memento { get; set; }

        public string Place => Memento?.Place;

        public override string ToString()
        {
            return Memento?.Place ?? base.ToString();
        }

        #region for Thumbnail

        /// <summary>
        /// ArchivePage Property.
        /// サムネイル用
        /// </summary>
        private Page _archivePage;
        public Page ArchivePage
        {
            get
            {
                if (_archivePage == null)
                {
                    _archivePage = new Page("", new ArchiveContent(Memento.Place));
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

        public static BookMementoUnit Create(Book.Memento memento)
        {
            return new BookMementoUnit() { Memento = memento };
        }
    }
}
