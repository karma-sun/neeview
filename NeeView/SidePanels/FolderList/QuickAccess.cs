using NeeLaboratory.ComponentModel;
using System;
using System.Runtime.Serialization;

namespace NeeView
{
    [DataContract]
    public class QuickAccess : BindableBase, IFolderTreeNode
    {
        private string _path;

        public QuickAccess(string path)
        {
            _path = path;
        }

        [DataMember]
        public string Path
        {
            get { return _path; }
            private set { SetProperty(ref _path, value); }
        }

        public string Name
        {
            get { return new QueryPath(_path).ToDispString(); }
        }

        public string Detail
        {
            get { return new QueryPath(_path).ToDetailString(); }
        }

        #region ITreeViewNode Support

        private bool _isSelected;
        public bool IsSelected
        {
            get { return _isSelected; }
            set { SetProperty(ref _isSelected, value); }
        }

        public bool IsExpanded
        {
            get { return false; }
            set { }
        }

        #endregion

        public override string ToString()
        {
            return Name;
        }

    }


}
