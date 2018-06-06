using System;
using System.Collections.ObjectModel;
using System.Runtime.Serialization;

namespace NeeView
{
    public interface IBookmarkEntry : IBookListItem
    {
        string Name { get; }
        string Detail { get; }
    }

    [DataContract]
    public class Bookmark : IBookmarkEntry
    {
        public Bookmark()
        {
        }

        public Bookmark(BookMementoUnit unit)
        {
            Place = unit.Place;
            Unit = unit;
        }


        [DataMember]
        public string Place { get; set; }


        #region IBookListItem Support

        public string Name => Unit.Memento.Name;
        public string Detail => Unit.Memento.Place;

        private BookMementoUnit _unit;
        public BookMementoUnit Unit
        {
            get { return _unit = _unit ?? BookMementoCollection.Current.Get(Place); }
            set { _unit = value; }
        }

        public Page GetPage()
        {
            return Unit?.ArchivePage;
        }

        #endregion
    }


    [DataContract]
    public class BookmarkFolder : IBookmarkEntry
    {
        public string Name { get; set; }
        public string Detail => "TODO: tips";

        public BookMementoUnit Unit { get => null; set => throw new NotImplementedException(); }

        public Page GetPage()
        {
            return ArchivePage;
        }

        private volatile ArchivePage _archivePage;
        public ArchivePage ArchivePage
        {
            get
            {
                if (_archivePage == null)
                {
                    _archivePage = new ArchivePage(path: null); //// LoosePath.Combine(Place, EntryName));
                    _archivePage.Thumbnail.IsCacheEnabled = true;
                    _archivePage.Thumbnail.Touched += Thumbnail_Touched;
                }
                return _archivePage;
            }
        }

        private void Thumbnail_Touched(object sender, EventArgs e)
        {
            var thumbnail = (Thumbnail)sender;
            BookThumbnailPool.Current.Add(thumbnail);
        }
    }
}
