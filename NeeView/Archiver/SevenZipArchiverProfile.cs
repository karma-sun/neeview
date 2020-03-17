using NeeLaboratory.ComponentModel;
using NeeView.Windows.Property;
using System.ComponentModel;
using System.Runtime.Serialization;

namespace NeeView
{
    //
    public class SevenZipArchiverProfile : BindableBase
    {
        static SevenZipArchiverProfile() => Current = new SevenZipArchiverProfile();
        public static SevenZipArchiverProfile Current { get; }


        private bool _isEnabled = true;

        private SevenZipArchiverProfile()
        {
        }

        [PropertyMember("@ParamSevenZipArchiverIsEnabled")]
        public bool IsEnabled
        {
            get { return _isEnabled; }
            set { if (_isEnabled != value) { _isEnabled = value; RaisePropertyChanged(); } }
        }

        [PropertyPath("@ParamSevenZipArchiverX86DllPath", Tips = "@ParamSevenZipArchiverX86DllPathTips", Filter = "DLL|*.dll", DefaultFileName = "7z.dll")]
        public string X86DllPath { get; set; } = "";

        [PropertyPath("@ParamSevenZipArchiverX64DllPath", Tips = "@ParamSevenZipArchiverX64DllPathTips", Filter = "DLL|*.dll", DefaultFileName = "7z.dll")]
        public string X64DllPath { get; set; } = "";

        [PropertyMember("@ParamSevenZipArchiverSupportFileTypes")]
        public FileTypeCollection SupportFileTypes { get; set; } = new FileTypeCollection(".7z;.cb7;.cbr;.cbz;.lzh;.rar;.zip");

#if false
        // 事前展開サイズ上限
        [PropertyMember("@ParamSevenZipArchiverPreExtractSolidSize", Tips = "@ParamSevenZipArchiverPreExtractSolidSizeTips")]
        public int PreExtractSolidSize { get; set; } = 1000;

        // 事前展開先をメモリにする
        [PropertyMember("@ParamSevenZipArchiverIsPreExtractToMemory", Tips = "@ParamSevenZipArchiverIsPreExtractToMemoryTips")]
        public bool IsPreExtractToMemory { get; set; }
#endif

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

            public void RestoreConfig()
            {
                Config.Current.Performance.PreExtractSolidSize = PreExtractSolidSize;
                Config.Current.Performance.IsPreExtractToMemory = IsPreExtractToMemory;
            }
        }

        public Memento CreateMemento()
        {
            var memento = new Memento();
            memento.IsEnabled = this.IsEnabled;
            memento.X86DllPath = this.X86DllPath;
            memento.X64DllPath = this.X64DllPath;
            memento.SupportFileTypes = this.SupportFileTypes.OneLine;
            memento.PreExtractSolidSize = Config.Current.Performance.PreExtractSolidSize;
            memento.IsPreExtractToMemory = Config.Current.Performance.IsPreExtractToMemory;
            return memento;
        }
        
        public void Restore(Memento memento)
        {
            if (memento == null) return;
            this.IsEnabled = memento.IsEnabled;
            this.X86DllPath = memento.X86DllPath;
            this.X64DllPath = memento.X64DllPath;
            this.SupportFileTypes.OneLine = memento.SupportFileTypes;
            ////this.PreExtractSolidSize = memento.PreExtractSolidSize;
            ////this.IsPreExtractToMemory = memento.IsPreExtractToMemory;
        }
        #endregion

    }
}
