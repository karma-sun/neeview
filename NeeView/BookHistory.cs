using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.Serialization;
using System.Text;
using System.Windows.Data;

namespace NeeView
{
    public interface IBookListItem : IHasPage
    {
        Thumbnail Thumbnail { get; }
    }

    [DataContract]
    public class BookHistory : IBookListItem
    {
        public BookHistory()
        {
        }

        public BookHistory(BookMementoUnit unit, DateTime lastAccessTime)
        {
            Place = unit.Place;
            LastAccessTime = lastAccessTime;
            Unit = unit;
        }

        [DataMember]
        public string Place { get; set; }

        [DataMember]
        public DateTime LastAccessTime { get; set; }

        public string Detail => Place + "\n" + LastAccessTime;

        public string ShortName => Unit.Memento.Name;

        public override string ToString()
        {
            return Place ?? base.ToString();
        }

        #region IBookListItem Support

        public Thumbnail Thumbnail => Unit.ArchivePage.Thumbnail;

        private BookMementoUnit _unit;
        public BookMementoUnit Unit
        {
            get { return _unit = _unit ?? BookMementoCollection.Current.Get(Place); }
            set { _unit = value; }
        }

        public Page GetPage()
        {
            return Unit.ArchivePage;
        }

        #endregion
    }
}
