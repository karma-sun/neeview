using NeeLaboratory.ComponentModel;
using System;
using System.Runtime.Serialization;
using System.Text.Json.Serialization;

namespace NeeView
{
    [DataContract]
    public class QuickAccess : BindableBase, ICloneable
    {
        private string _path;

        [JsonInclude, JsonPropertyName(nameof(Name))]
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public string _name;


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

        [JsonIgnore]
        public string Name
        {
            get
            {
                return _name ?? DefaultName;
            }
            set
            {
                var name = value.Trim();
                SetProperty(ref _name, string.IsNullOrEmpty(name) || name == DefaultName ? null : name); 
            }
        }

        public string DefaultName
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

        public object Clone()
        {
            return (QuickAccess)MemberwiseClone();
        }


        #region Memento

        [DataContract]
        public class Memento
        {
            public string Path { get; set; }
            public string Name { get; set; }

            [OnDeserializing]
            private void Deserializing(StreamingContext c)
            {
                this.InitializePropertyDefaultValues();
            }
        }

        public Memento CreateMemento()
        {
            var memento = new Memento();
            memento.Path = Path;
            memento.Name = _name;
            return memento;
        }

        public void Restore(Memento memento)
        {
            if (memento == null) return;
            Path = memento.Path;
            _name = memento.Name;
        }

        #endregion

    }
}
