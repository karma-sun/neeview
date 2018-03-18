using NeeLaboratory.ComponentModel;
using NeeView.Windows.Property;
using System.ComponentModel;
using System.Runtime.Serialization;

namespace NeeView
{
    //
    public class SevenZipArchiverProfile : BindableBase
    {
        public static SevenZipArchiverProfile Current { get; private set; }

        private bool _isEnabled = true;

        public SevenZipArchiverProfile()
        {
            Current = this;
        }

        [PropertyMember("@ParamSevenZipArchiverIsEnabled")]
        public bool IsEnabled
        {
            get { return _isEnabled; }
            set { if (_isEnabled != value) { _isEnabled = value; RaisePropertyChanged(); } }
        }

        [PropertyPath("@ParamSevenZipArchiverX86DllPath", Tips = "@ParamSevenZipArchiverX86DllPathTips", Filter = "DLL|*.dll")]
        public string X86DllPath { get; set; } = "";

        [PropertyPath("@ParamSevenZipArchiverX64DllPath", Tips = "@ParamSevenZipArchiverX64DllPathTips", Filter = "DLL|*.dll")]
        public string X64DllPath { get; set; } = "";

        [PropertyMember("@ParamSevenZipArchiverSupportFileTypes")]
        public FileTypeCollection SupportFileTypes { get; set; } = new FileTypeCollection(".7z;.cb7;.cbr;.cbz;.lzh;.rar;.zip");

        [PropertyMember("@ParamSevenZipArchiverLockTime", Tips = "@ParamSevenZipArchiverLockTimeTips")]
        public double LockTime { get; set; } = -1.0;

        // 強制アンロックモード
        public bool IsUnlockMode { get; set; }

        // 事前展開
        [PropertyMember("@ParamSevenZipArchiverIsPreExtract", Tips = "@ParamSevenZipArchiverIsPreExtractTips")]
        public bool IsPreExtract { get; set; }

        // 事前展開サイズ上限
        [PropertyMember("@ParamSevenZipArchiverPreExtractSolidSize", Tips = "@ParamSevenZipArchiverPreExtractSolidSizeTips")]
        public int PreExtractSolidSize { get; set; } = 1000;

        #region Memento
        [DataContract]
        public class Memento
        {
            [DataMember]
            public int _Version { get; set; } = Config.Current.ProductVersionNumber;

            [DataMember, DefaultValue(true)]
            public bool IsEnabled { get; set; }

            [DataMember, DefaultValue("")]
            public string X86DllPath { get; set; }

            [DataMember, DefaultValue("")]
            public string X64DllPath { get; set; }

            [DataMember, DefaultValue(".7z;.cb7;.cbr;.cbz;.lzh;.rar;.zip")]
            public string SupportFileTypes { get; set; }

            [DataMember, DefaultValue(-1.0)]
            public double LockTime { get; set; }

            [DataMember, DefaultValue(false)]
            public bool IsPreExtract { get; set; }

            [DataMember, DefaultValue(1000)]
            public int PreExtractSolidSize { get; set; }

            [OnDeserializing]
            private void Deserializing(StreamingContext context)
            {
                this.InitializePropertyDefaultValues();
            }
        }

        //
        public Memento CreateMemento()
        {
            var memento = new Memento();
            memento.IsEnabled = this.IsEnabled;
            memento.X86DllPath = this.X86DllPath;
            memento.X64DllPath = this.X64DllPath;
            memento.LockTime = this.LockTime;
            memento.SupportFileTypes = this.SupportFileTypes.ToString();
            memento.IsPreExtract = this.IsPreExtract;
            memento.PreExtractSolidSize = this.PreExtractSolidSize;
            return memento;
        }

        //
        public void Restore(Memento memento)
        {
            if (memento == null) return;
            this.IsEnabled = memento.IsEnabled;
            this.X86DllPath = memento.X86DllPath;
            this.X64DllPath = memento.X64DllPath;
            this.LockTime = memento.LockTime;
            this.SupportFileTypes.FromString(memento.SupportFileTypes);
            this.IsPreExtract = memento.IsPreExtract;
            this.PreExtractSolidSize = memento.PreExtractSolidSize;

            // compatible before ver.25
            if (memento._Version < Config.GenerateProductVersionNumber(1, 25, 0))
            {
                this.SupportFileTypes.Add(".cbr");
                this.SupportFileTypes.Add(".cbz");
            }

            // compatible before ver.29
            if (memento._Version < Config.GenerateProductVersionNumber(1, 29, 0))
            {
                // .zipファイル展開を優先するため、標準の圧縮ファイル展開機能を無効にする
                if (this.SupportFileTypes.Contains(".zip"))
                {
                    ZipArchiverProfile.Current.IsEnabled = false;
                }

                this.SupportFileTypes.Add(".cb7");
                this.SupportFileTypes.Add(".zip");
            }
        }
        #endregion

    }
}
