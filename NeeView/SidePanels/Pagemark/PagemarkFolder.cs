using NeeLaboratory.ComponentModel;
using System;
using System.Runtime.Serialization;

namespace NeeView
{
    [DataContract]
    public class PagemarkFolder : BindableBase, IPagemarkEntry
    {
        private string _name;

        [DataMember(EmitDefaultValue = false)]
        public virtual string Name
        {
            get { return _name; }
            set
            {
                if (SetProperty(ref _name, value))
                {
                    RaisePropertyChanged(null);
                }
            }
        }

        public string DispName
        {
            get { return LoosePath.GetFileName(_name); }
        }


        public string Note => null;
        public string Detail => _name;

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


        public static string GetValidateName(string name)
        {
            return name.Trim().Replace('/', '_').Replace('\\', '_');
        }

        public bool IsEqual(IPagemarkEntry entry)
        {
            return entry is PagemarkFolder folder && this.Name == folder.Name;
        }
    }

    [DataContract]
    public class DefaultPagemarkFolder : PagemarkFolder
    {
        public override string Name
        {
            get { return "(Default Pagemark)"; }
            set { }
        }
    }

}
