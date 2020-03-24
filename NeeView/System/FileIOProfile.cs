using NeeLaboratory.ComponentModel;
using NeeView.Windows.Property;
using System;
using System.ComponentModel;
using System.IO;
using System.Runtime.Serialization;

namespace NeeView
{
    public class FileIOProfile : BindableBase
    {
        static FileIOProfile() => Current = new FileIOProfile();
        public static FileIOProfile Current { get; }



        private FileIOProfile()
        {
        }


#if false
        [PropertyMember("@ParamIsRemoveConfirmed")]
        public bool IsRemoveConfirmed { get; set; } = true;

        [PropertyMember("@ParamIsRemoveExplorerDialogEnabled", Tips = "@ParamIsRemoveExplorerDialogEnabledTips")]
        public bool IsRemoveExplorerDialogEnabled { get; set; } = false;

        private bool _isEnabled = true;
        [PropertyMember("@ParamIsFileOperationEnabled")]
        public bool IsEnabled
        {
            get { return _isEnabled; }
            set { SetProperty(ref _isEnabled, value); }
        }

        private bool _isHiddenFileVisibled;
        [PropertyMember("@ParamIsHiddenFileVisibled")]
        public bool IsHiddenFileVisibled
        {
            get { return _isHiddenFileVisibled; }
            set { SetProperty(ref _isHiddenFileVisibled, value); }
        }
#endif

        /// <summary>
        /// ファイルは項目として有効か？
        /// </summary>
        public bool IsFileValid(FileAttributes attributes)
        {
            return Config.Current.System.IsHiddenFileVisibled || (attributes & FileAttributes.Hidden) == 0;
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

            public void RestoreConfig(Config config)
            {
                config.System.IsRemoveConfirmed = IsRemoveConfirmed;
                config.System.IsRemoveExplorerDialogEnabled = IsRemoveExplorerDialogEnabled;
                config.System.IsFileWriteAccessEnabled = IsEnabled;
                config.System.IsHiddenFileVisibled = IsHiddenFileVisibled;
            }
        }

        public Memento CreateMemento()
        {
            var memento = new Memento();
            memento.IsRemoveConfirmed = Config.Current.System.IsRemoveConfirmed;
            memento.IsRemoveExplorerDialogEnabled = Config.Current.System.IsRemoveExplorerDialogEnabled;
            memento.IsEnabled = Config.Current.System.IsFileWriteAccessEnabled;
            memento.IsHiddenFileVisibled = Config.Current.System.IsHiddenFileVisibled;
            return memento;
        }

        [Obsolete]
        public void Restore(Memento memento)
        {
            if (memento == null) return;
            ////this.IsRemoveConfirmed = memento.IsRemoveConfirmed;
            ////this.IsRemoveExplorerDialogEnabled = memento.IsRemoveExplorerDialogEnabled;
            ////this.IsEnabled = memento.IsEnabled;
            ////this.IsHiddenFileVisibled = memento.IsHiddenFileVisibled;
        }
        #endregion

    }
}
