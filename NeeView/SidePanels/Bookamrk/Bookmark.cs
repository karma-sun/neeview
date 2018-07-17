using NeeLaboratory.ComponentModel;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Runtime.Serialization;

namespace NeeView
{
    public interface IHasName
    {
        string Name { get; }
    }

    public interface IBookmarkEntry : IBookListItem, IHasName
    {
    }


    [DataContract]
    public class Bookmark : BindableBase, IBookmarkEntry, IVirtualItem
    {
        public static string Scheme => "bookmark:";

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

        public Thumbnail Thumbnail
        {
            get
            {
                if (BookmarkList.Current.IsThumbnailVisibled)
                {
                    BookmarkListVertualCollection.Current.Attach(this);
                }
                return Unit.ArchivePage.Thumbnail;
            }
        }

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


        #region IVirtualItem

        // TODO: これはPageで保持するべきか？
        private JobRequest _jobRequest;

        public int DetachCount { get; set; }

        public void Attached()
        {
            /// Debug.WriteLine($"Attach: {Name}");
            _jobRequest?.Cancel();
            _jobRequest = Unit.ArchivePage.LoadThumbnail(QueueElementPriority.BookmarkThumbnail);
        }

        public void Detached()
        {
            ////Debug.WriteLine($"Detach: {Name}");
            _jobRequest?.Cancel();
            _jobRequest = null;
        }

        #endregion

        public override string ToString()
        {
            return base.ToString() + " Name:" + Name;
        }

    }

}
