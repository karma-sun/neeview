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
            get
            {
                var queryPath = new QueryPath(_path);
                var s = queryPath.Path == null ? queryPath.Scheme.ToAliasName() : LoosePath.GetFileName(queryPath.Path);
                if (queryPath.Search != null)
                {
                    s = s + " (" + queryPath.Search + ")";
                }
                return s;
            }
        }

        public string Detail
        {
            get
            {
                var queryPath = new QueryPath(_path);
                var s = queryPath.SimplePath;
                if (queryPath.Search != null)
                {
                    s = s + "\n" + Properties.Resources.WordSearchWord + ": " + queryPath.Search;
                }
                return s;
            }
        }

        public override string ToString()
        {
            return Name;
        }

    }


}
