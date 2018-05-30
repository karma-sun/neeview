using System;
using System.Runtime.Serialization;

namespace NeeView
{
    // TODO: IEquatableが不完全
    [DataContract]
    public class Bookmark : IEquatable<Bookmark>, IHasPage
    {
        public Bookmark()
        {
        }

        public Bookmark(string place)
        {
            Place = place;
        }

        public Bookmark(BookMementoUnit unit)
        {
            Place = unit.Place;
            Unit = unit;
        }

        [DataMember]
        public string Place { get; set; }

        //
        public BookMementoUnit Unit { get; set; }

        #region IEquatable support

        public bool Equals(Bookmark other)
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

            return this.Equals((Bookmark)obj);
        }

        public override int GetHashCode()
        {
            return this.Place.GetHashCode();
        }

        #endregion

        #region IHasPage Support

        public Page GetPage()
        {
            return Unit?.ArchivePage;
        }

        #endregion
    }
}
