using System;
using System.Runtime.Serialization;

namespace NeeView
{
    [DataContract]
    public class Bookmark : IBookListItem
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
