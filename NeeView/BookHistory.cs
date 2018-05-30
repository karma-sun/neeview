using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.Serialization;
using System.Text;
using System.Windows.Data;

namespace NeeView
{
    [DataContract]
    public class BookHistory : IEquatable<BookHistory>, IHasPage
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


        public BookMementoUnit Unit { get; set; }

        #region IBookListItem support

        public string Name => Unit.Memento.Name;

        public ArchivePage ArchivePage => Unit.ArchivePage;

        #endregion

        #region IHasPage Support

        public Page GetPage()
        {
            return ArchivePage;
        }

        #endregion

        #region IEquatable support

        public bool Equals(BookHistory other)
        {
            if (other == null)
            {
                return false;
            }

            return this.Place == other.Place;
        }

        public override bool Equals(object obj)
        {
            if (obj == null || this.GetType() != obj.GetType())
            {
                return false;
            }

            return this.Equals((BookHistory)obj);
        }

        //Equalsがtrueを返すときに同じ値を返す
        public override int GetHashCode()
        {
            return this.Place.GetHashCode();
        }

        #endregion
    }
}
