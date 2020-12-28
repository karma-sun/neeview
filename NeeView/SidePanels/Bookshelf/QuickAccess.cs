using NeeLaboratory.ComponentModel;
using System;
using System.Runtime.Serialization;

namespace NeeView
{
    [DataContract]
    public class QuickAccess : BindableBase
    {
        private string _path;

        public QuickAccess()
        { 
        }

        public QuickAccess(string path)
        {
            _path = path;
        }

        [DataMember]
        public string Path
        {
            get { return _path; }
            set
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
                var query = new QueryPath(_path);
                return query.DispName + (query.Search != null ? $" ({query.Search})" : null);
            }
        }

        public string Detail
        {
            get
            {
                var query = new QueryPath(_path);
                return query.SimplePath + (query.Search != null ? $"\n{Properties.Resources.Word_SearchWord}: {query.Search}" : null);
            }
        }

        public override string ToString()
        {
            return Name;
        }

    }


}
