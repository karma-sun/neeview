using NeeLaboratory.ComponentModel;
using System;
using System.Runtime.Serialization;

namespace NeeView
{
    [DataContract]
    public class QuickAccess : BindableBase
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
            private set
            {
                if (SetProperty(ref _path, value))
                {
                    RaisePropertyChanged(nameof(Name));
                    RaisePropertyChanged(nameof(Detail));
                }
            }
        }

        public string Name
        {
            get { return new QueryPath(_path).ToDispString(); }
        }

        public string Detail
        {
            get { return new QueryPath(_path).ToDetailString(); }
        }

        public override string ToString()
        {
            return Name;
        }

    }


}
