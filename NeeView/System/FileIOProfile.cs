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


#if false
        [PropertyMember("@ParamIsRemoveConfirmed")]
        public bool IsRemoveConfirmed { get; set; } = true;

        [PropertyMember("@ParamIsRemoveExplorerDialogEnabled", Tips = "@ParamIsRemoveExplorerDialogEnabledTips")]
        public bool IsRemoveExplorerDialogEnabled { get; set; } = false;
#endif

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
        public class Memento : IMemento
        {
            [DataMember, DefaultValue(true)]
            public bool IsRemoveConfirmed { get; set; }

            [DataMember, DefaultValue(false)]
            public bool IsRemoveExplorerDialogEnabled { get; set; }

            [DataMember, DefaultValue(true)]
            public bool IsEnabled { get; set; }

            [DataMember]
            public bool IsHiddenFileVisibled { get; set; }


            [OnDeserializing]
            private void OnDeserializing(StreamingContext c)
            {
                this.InitializePropertyDefaultValues();
            }

            [OnDeserialized]
            public void OnDeserialized(StreamingContext c)
            {
            }

            public void RestoreConfig()
            {
                Config.Current.System.IsRemoveConfirmed = IsRemoveConfirmed;
                Config.Current.System.IsRemoveExplorerDialogEnabled = IsRemoveExplorerDialogEnabled;
            }
        }

        public Memento CreateMemento()
        {
            var memento = new Memento();
            memento.IsRemoveConfirmed = Config.Current.System.IsRemoveConfirmed;
            memento.IsRemoveExplorerDialogEnabled = Config.Current.System.IsRemoveExplorerDialogEnabled;
            memento.IsEnabled = this.IsEnabled;
            memento.IsHiddenFileVisibled = this.IsHiddenFileVisibled;
            return memento;
        }

        public void Restore(Memento memento)
        {
            if (memento == null) return;
            ////this.IsRemoveConfirmed = memento.IsRemoveConfirmed;
            ////this.IsRemoveExplorerDialogEnabled = memento.IsRemoveExplorerDialogEnabled;
            this.IsEnabled = memento.IsEnabled;
            this.IsHiddenFileVisibled = memento.IsHiddenFileVisibled;
        }
        #endregion

    }
}
