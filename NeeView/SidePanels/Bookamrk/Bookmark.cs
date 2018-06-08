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

        public Thumbnail Thumbnail => Unit.ArchivePage.Thumbnail;

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
}
