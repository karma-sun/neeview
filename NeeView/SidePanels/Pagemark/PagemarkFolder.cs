using NeeLaboratory.ComponentModel;
using System.Runtime.Serialization;

namespace NeeView
{
    [DataContract]
    public class PagemarkFolder : BindableBase, IPagemarkEntry
    {
        private string _name;

        [DataMember(EmitDefaultValue = false)]
        public string Name
        {
            get { return _name; }
            set { SetProperty(ref _name, value); }
        }

        public string Note => null;
        public string Detail => null;

        public IThumbnail Thumbnail
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
