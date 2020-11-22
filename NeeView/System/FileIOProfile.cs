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
                config.System.IsRemoveWantNukeWarning = IsRemoveExplorerDialogEnabled;
                config.System.IsFileWriteAccessEnabled = IsEnabled;
                config.System.IsHiddenFileVisibled = IsHiddenFileVisibled;
            }
        }

        #endregion

    }
}
