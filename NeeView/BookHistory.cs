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
        BookMementoUnit Unit { get; set; }
    }

    [DataContract]
    public class BookHistory : IBookListItem
    {
        public BookHistory()
        {
        }

        public BookHistory(BookMementoUnit unit)
        {
            Place = unit.Place;
            LastAccessTime = DateTime.Now;
            Unit = unit;
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

        public BookMementoUnit Unit { get; set; }

        public Page GetPage()
        {
            return Unit.ArchivePage;
        }

        #endregion
    }
}
