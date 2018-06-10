using NeeLaboratory.ComponentModel;
using System;
using System.Runtime.Serialization;

namespace NeeView
{
    public interface IPagemarkEntry : IBookListItem
    {
    }

    /// <summary>
    /// ページマーカー
    /// </summary>
    [DataContract]
    public class Pagemark : BindableBase, IPagemarkEntry
    {
        private string _place;


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
        public string Place
        {
            get { return _place; }
            set
            {
                if (SetProperty(ref _place, value))
                {
                    _unit = null;
                    RaisePropertyChanged(null);
                }
            }
        }


        [DataMember]
        public string EntryName { get; set; }


        [OnDeserialized]
        private void Deserialized(StreamingContext c)
        {
            this.EntryName = LoosePath.NormalizeSeparator(this.EntryName);
        }

        #region IBookListItem Support

        public string Name => LoosePath.GetFileName(EntryName);
        public string Note => LoosePath.GetFileName(Place);
        public string Detail => Place + "\n" + EntryName;

        public Thumbnail Thumbnail => ArchivePage.Thumbnail;

        private BookMementoUnit _unit;
        public BookMementoUnit Unit
        {
            get { return _unit = _unit ?? BookMementoCollection.Current.Set(Place); }
            private set { _unit = value; }
        }

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
    }

}
