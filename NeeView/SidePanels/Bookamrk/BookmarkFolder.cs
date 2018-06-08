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

        public string Detail => null;

        public Thumbnail Thumbnail => ConstPage.Thumbnail;

        public BookMementoUnit Unit { get => null; set => throw new NotImplementedException(); }

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
