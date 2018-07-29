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

        public static string GetValidateName(string name)
        {
            return name.Trim().Replace('/', '_').Replace('\\', '_');
        }

        public bool IsEqual(IBookmarkEntry entry)
        {
            return entry is BookmarkFolder folder && this.Name == folder.Name;
        }
    }
}
