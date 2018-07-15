using NeeLaboratory.ComponentModel;
using System;
using System.Runtime.Serialization;

namespace NeeView
{
    [DataContract]
    public class BookmarkFolder : BindableBase, IBookmarkEntry
    {
        private string _name;

        [DataMember]
        public string Name
        {
            get { return _name; }
            set { SetProperty(ref _name, value); }
        }

        public string Note => null;
        public string Detail => null;

        public Thumbnail Thumbnail
        {
            get
            {
                ConstPage.LoadThumbnail(QueueElementPriority.BookmarkThumbnail);
                return ConstPage.Thumbnail;
            }
        }


        public Page GetPage()
        {
            return ConstPage;
        }

        private volatile ConstPage _constPage;
        public ConstPage ConstPage
        {
            get
            {
                return _constPage != null ? _constPage : _constPage = new ConstPage(ThumbnailType.Folder);
            }
        }
    }
}
