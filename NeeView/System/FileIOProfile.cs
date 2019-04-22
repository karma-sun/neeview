using NeeLaboratory.ComponentModel;
using NeeView.Windows.Property;
using System.ComponentModel;
using System.IO;
using System.Runtime.Serialization;

namespace NeeView
{
    public class FileIOProfile : BindableBase
    {
        static FileIOProfile() => Current = new FileIOProfile();
        public static FileIOProfile Current { get; }

        private bool _isEnabled = true;
        private bool _isHiddenFileVisibled;


        private FileIOProfile()
        {
        }


        [PropertyMember("@ParamIsRemoveConfirmed")]
        public bool IsRemoveConfirmed { get; set; } = true;

        [PropertyMember("@ParamIsFileOperationEnabled")]
        public bool IsEnabled
        {
            get { return _isEnabled; }
            set { SetProperty(ref _isEnabled, value); }
        }

        [PropertyMember("@ParamIsHiddenFileVisibled")]
        public bool IsHiddenFileVisibled
        {
            get { return _isHiddenFileVisibled; }
            set { SetProperty(ref _isHiddenFileVisibled, value); }
        }



        /// <summary>
        /// ファイルは項目として有効か？
        /// </summary>
        public bool IsFileValid(FileAttributes attributes)
        {
            return IsHiddenFileVisibled || (attributes & FileAttributes.Hidden) == 0;
        }

        #region Memento
        [DataContract]
        public class Memento
        {
            [DataMember, DefaultValue(true)]
            public bool IsRemoveConfirmed { get; set; }

            [DataMember, DefaultValue(true)]
            public bool IsEnabled { get; set; }

            [DataMember]
            public bool IsHiddenFileVisibled { get; set; }


            [OnDeserializing]
            private void Deserializing(StreamingContext c)
            {
                this.InitializePropertyDefaultValues();
            }
        }

        public Memento CreateMemento()
        {
            var memento = new Memento();
            memento.IsRemoveConfirmed = this.IsRemoveConfirmed;
            memento.IsEnabled = this.IsEnabled;
            memento.IsHiddenFileVisibled = this.IsHiddenFileVisibled;
            return memento;
        }

        public void Restore(Memento memento)
        {
            if (memento == null) return;
            this.IsRemoveConfirmed = memento.IsRemoveConfirmed;
            this.IsEnabled = memento.IsEnabled;
            this.IsHiddenFileVisibled = memento.IsHiddenFileVisibled;
        }
        #endregion

    }
}
