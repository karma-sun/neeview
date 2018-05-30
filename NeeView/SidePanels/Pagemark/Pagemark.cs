using System;
using System.Runtime.Serialization;

namespace NeeView
{
    /// <summary>
    /// ページマーカー
    /// </summary>
    [DataContract]
    public class Pagemark : IEquatable<Pagemark>, IBookListItem
    {
        [DataMember]
        public string Place { get; set; }

        [DataMember]
        public string EntryName { get; private set; }

        public string PlaceShort => LoosePath.GetFileName(Place);

        public string PageShort => LoosePath.GetFileName(EntryName);

        public string Detail => Place + "\n" + EntryName;

        [OnDeserialized]
        private void Deserialized(StreamingContext c)
        {
            this.EntryName = LoosePath.NormalizeSeparator(this.EntryName);
        }

        #region Constructroes

        public Pagemark()
        { }

        public Pagemark(string place, string page)
        {
            Place = place;
            EntryName = page;
        }

        public Pagemark(BookMementoUnit unit, string page)
        {
            Place = unit.Place;
            EntryName = page;
            Unit = unit;
        }

        #endregion

        #region IEqualable Support

        //otherと自分自身が等価のときはtrueを返す
        public bool Equals(Pagemark other)
        {
            //objがnullのときは、等価でない
            if (other == null)
            {
                return false;
            }

            //Numberで比較する
            return (this.Place == other.Place && this.EntryName == other.EntryName);
        }

        //objと自分自身が等価のときはtrueを返す
        public override bool Equals(object obj)
        {
            //objがnullか、型が違うときは、等価でない
            if (obj == null || this.GetType() != obj.GetType())
            {
                return false;
            }

            return this.Equals((Pagemark)obj);
        }

        //Equalsがtrueを返すときに同じ値を返す
        public override int GetHashCode()
        {
            return (Place == null ? 0 : Place.GetHashCode()) ^ (EntryName == null ? 0 : EntryName.GetHashCode());
        }

        #endregion

        #region IBookListItem Support

        public BookMementoUnit Unit { get; set; }

        public Page GetPage()
        {
            return ArchivePage;
        }

        #region for Page Thumbnail

        private volatile ArchivePage _archivePage;
        public ArchivePage ArchivePage
        {
            get
            {
                if (_archivePage == null)
                {
                    _archivePage = new ArchivePage(LoosePath.Combine(Place, EntryName));
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

        #endregion

        #endregion
    }

}
