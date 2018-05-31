using System;
using System.Runtime.Serialization;

namespace NeeView
{
    /// <summary>
    /// ページマーカー
    /// </summary>
    [DataContract]
    public class Pagemark : IBookListItem
    {
        public Pagemark()
        {
        }

        public Pagemark(BookMementoUnit unit, string entryName)
        {
            Place = unit.Place;
            EntryName = entryName;
            Unit = unit;
        }


        [DataMember]
        public string Place { get; set; }

        [DataMember]
        public string EntryName { get; set; }

        public string PlaceShort => LoosePath.GetFileName(Place);

        public string PageShort => LoosePath.GetFileName(EntryName);

        public string Detail => Place + "\n" + EntryName;


        [OnDeserialized]
        private void Deserialized(StreamingContext c)
        {
            this.EntryName = LoosePath.NormalizeSeparator(this.EntryName);
        }

        #region IBookListItem Support

        private BookMementoUnit _unit;
        public BookMementoUnit Unit
        {
            get { return _unit = _unit ?? BookMementoCollection.Current.Get(Place); }
            set { _unit = value; }
        }

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
