using NeeLaboratory.ComponentModel;
using NeeView.Windows.Property;
using System.ComponentModel;
using System.Runtime.Serialization;

namespace NeeView
{
    public class FileIOProfile : BindableBase
    {
        static FileIOProfile() => Current = new FileIOProfile();
        public static FileIOProfile Current { get; }

        private bool _isEnabled = true;


        private FileIOProfile()
        {
        }


        [PropertyMember("@ParamIsRemoveConfirmed")]
        public bool IsRemoveConfirmed { get; set; } = true;

        [PropertyMember("@ParamIsFileOperationEnabled")]
        public bool IsEnabled
        {
            get { return _isEnabled; }
            set { if (_isEnabled != value) { _isEnabled = value; RaisePropertyChanged(); } }
        }


        #region Memento
        [DataContract]
        public class Memento
        {
            [DataMember, DefaultValue(true)]
            public bool IsRemoveConfirmed { get; set; }

            [DataMember, DefaultValue(true)]
            public bool IsEnabled { get; set; }
        }

        //
        public Memento CreateMemento()
        {
            var memento = new Memento();
            memento.IsRemoveConfirmed = this.IsRemoveConfirmed;
            memento.IsEnabled = this.IsEnabled;
            return memento;
        }

        //
        public void Restore(Memento memento)
        {
            if (memento == null) return;
            this.IsRemoveConfirmed = memento.IsRemoveConfirmed;
            this.IsEnabled = memento.IsEnabled;
        }
        #endregion

    }
}
