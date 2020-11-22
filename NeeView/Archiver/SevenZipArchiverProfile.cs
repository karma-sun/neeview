using NeeLaboratory.ComponentModel;
using NeeView.Windows.Property;
using System;
using System.ComponentModel;
using System.Runtime.Serialization;

namespace NeeView
{
    //
    public class SevenZipArchiverProfile : BindableBase
    {
        static SevenZipArchiverProfile() => Current = new SevenZipArchiverProfile();
        public static SevenZipArchiverProfile Current { get; }


        #region Memento
        [DataContract]
        public class Memento : IMemento
        {
            [DataMember]
            public int _Version { get; set; } = Environment.ProductVersionNumber;

            [DataMember, DefaultValue(true)]
            public bool IsEnabled { get; set; }

            [DataMember, DefaultValue("")]
            public string X86DllPath { get; set; }

            [DataMember, DefaultValue("")]
            public string X64DllPath { get; set; }

            [DataMember, DefaultValue(".7z;.cb7;.cbr;.cbz;.lzh;.rar;.zip")]
            public string SupportFileTypes { get; set; }

            [DataMember, DefaultValue(1000)]
            public int PreExtractSolidSize { get; set; }

            [DataMember]
            public bool IsPreExtractToMemory { get; set; }


            [OnDeserializing]
            private void OnDeserializing(StreamingContext context)
            {
                this.InitializePropertyDefaultValues();
            }

            [OnDeserialized]
            private void OnDeserialized(StreamingContext context)
            {
            }

            public void RestoreConfig(Config config)
            {
                config.Performance.PreExtractSolidSize = PreExtractSolidSize;
                config.Performance.IsPreExtractToMemory = IsPreExtractToMemory;
                config.Archive.SevenZip.IsEnabled = IsEnabled;
                config.Archive.SevenZip.X86DllPath = X86DllPath;
                config.Archive.SevenZip.X64DllPath = X64DllPath;
                config.Archive.SevenZip.SupportFileTypes.OneLine = SupportFileTypes;
            }
        }
        
        #endregion

    }
}
