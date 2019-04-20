using NeeLaboratory.ComponentModel;
using System;
using System.Runtime.Serialization;

namespace NeeView
{
    [DataContract]
    public class PagemarkFolder : BindableBase, IPagemarkEntry
    {
        private static IThumbnail _thumbnail = new FolderThumbnail();
        private string _place;

        
        [Obsolete, DataMember(Name="Name", EmitDefaultValue =false)]
        private string ObsoleteName
        {
            get { return null; }
            set { _place = value; }
        }


        [DataMember(EmitDefaultValue = false)]
        public string Place
        {
            get { return _place; }
            set
            {
                if (SetProperty(ref _place, value))
                {
                    RaisePropertyChanged(null);
                }
            }
        }

        public string Name => _place;
        public string DispName => LoosePath.GetFileName(_place);
        public string Note => null;
        public string Detail => _place;

        public IThumbnail Thumbnail => _thumbnail;


        public static string GetValidateName(string name)
        {
            return name.Trim().Replace('/', '_').Replace('\\', '_');
        }

        public bool IsEqual(IPagemarkEntry entry)
        {
            return entry is PagemarkFolder folder && this.Place == folder.Place;
        }
    }

}
