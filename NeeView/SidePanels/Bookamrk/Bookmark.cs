using NeeLaboratory.ComponentModel;
using System.Collections.ObjectModel;
using System.Runtime.Serialization;

namespace NeeView
{
    public interface IBookmarkEntry : IBookListItem
    {
    }

    [DataContract]
    public class Bookmark : BindableBase, IBookmarkEntry
    {
        private string _place;

        public Bookmark()
        {
        }

        public Bookmark(BookMementoUnit unit)
        {
            Place = unit.Place;
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



        #region IBookListItem Support

        public string Name => Unit.Memento.Name;
        public string Note => Unit.ArchivePage.Content.Entry.RootArchiverName;
        public string Detail => Unit.Memento.Place;

        public Thumbnail Thumbnail => Unit.ArchivePage.Thumbnail;

        private BookMementoUnit _unit;
        public BookMementoUnit Unit
        {
            get { return _unit = _unit ?? BookMementoCollection.Current.Set(Place); }
            private set { _unit = value; }
        }

        public Page GetPage()
        {
            return Unit?.ArchivePage;
        }

        #endregion
    }
}
