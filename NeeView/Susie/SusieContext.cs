using NeeLaboratory.ComponentModel;
using NeeView.Windows.Property;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace NeeView
{
    /// <summary>
    /// Susie Context
    /// </summary>
    public class SusieContext : BindableBase
    {
        public static SusieContext Current { get; private set; }

        #region Fields

        private Susie.Susie _susie;
        public bool _isEnableSusie;
        public string _susiePluginPath = "";
        public bool _isFirstOrderSusieImage;
        public bool _isFirstOrderSusieArchive;
        private bool _isPluginCacheEnabled = true;

        #endregion

        #region Constructoes

        public SusieContext()
        {
            Current = this;
            _susie = new Susie.Susie();
        }

        #endregion

        #region Properties

        /// <summary>
        /// Susieコア
        /// </summary>
        public Susie.Susie Susie
        {
            get { return _susie; }
        }

        /// <summary>
        /// 機能サポート判定
        /// </summary>
#if SUSIE
        public static bool IsSupportedSusie => true;
#else
        public static bool IsSupportedSusie => false;
#endif

        /// <summary>
        /// Susie 有効/無効フラグ
        /// 実際に有効かどうかはこのフラグを使用する
        /// </summary>
        public bool IsEnabled
        {
            get { return IsSupportedSusie && _isEnableSusie; }
        }

        /// <summary>
        /// Susie 有効/無効設定
        /// 設定のみ。実際に有効かどうかは IsEnabled で判定する
        /// </summary>
        [PropertyMember("@ParamSusieIsEnabled")]
        public bool IsEnableSusie
        {
            get { return _isEnableSusie; }
            set
            {
                if (_isEnableSusie != value)
                {
                    _isEnableSusie = value;
                    RaisePropertyChanged();
                    RaisePropertyChanged(nameof(IsEnabled));
                }
            }
        }

        // Susie プラグインフォルダー
        [PropertyPath("@ParamSusiePluginPath", IsDirectory = true)]
        public string SusiePluginPath
        {
            get { return _susiePluginPath; }
            set
            {
                if (_susiePluginPath != value)
                {
                    _susiePluginPath = value;
                    SetupSusie(_susiePluginPath, CreateSpiFiles());
                }
            }
        }

        // Susie 画像プラグイン 優先フラグ
        [PropertyMember("@ParamSusieIsFirstOrderSusieImage")]
        public bool IsFirstOrderSusieImage
        {
            get { return _isFirstOrderSusieImage; }
            set { if (_isFirstOrderSusieImage != value) { _isFirstOrderSusieImage = value; RaisePropertyChanged(); } }
        }

        // Susie 書庫プラグイン 優先フラグ
        [PropertyMember("@ParamSusieIsFirstOrderSusieArchive")]
        public bool IsFirstOrderSusieArchive
        {
            get { return _isFirstOrderSusieArchive; }
            set { if (_isFirstOrderSusieArchive != value) { _isFirstOrderSusieArchive = value; RaisePropertyChanged(); } }
        }

        /// <summary>
        /// 対応画像ファイル拡張子
        /// </summary>
        public FileTypeCollection ImageExtensions = new FileTypeCollection();

        /// <summary>
        /// 対応圧縮ファイル拡張子
        /// </summary>
        public FileTypeCollection ArchiveExtensions = new FileTypeCollection();

        // Susie プラグインキャッシュ有効フラグ
        [PropertyMember("@ParamSusieIsPluginCacheEnabled")]
        public bool IsPluginCacheEnabled
        {
            get { return _isPluginCacheEnabled; }
            set
            {
                if (SetProperty(ref _isPluginCacheEnabled, value))
                {
                    _susie?.SetPluginCahceEnabled(_isPluginCacheEnabled);
                }
            }
        }

        // 事前展開
        [PropertyMember("@ParamSusieIsPreExtract", Tips = "@ParamSusieIsPreExtractTips")]
        public bool IsPreExtract { get; set; }

        #endregion

        #region Methods

        /// <summary>
        /// 初期化
        /// </summary>
        /// <param name="spiFolder">プラグインフォルダー</param>
        /// <param name="spiFiles">プラグインリスト</param>
        public void Initialize(string spiFolder, Dictionary<string, bool> spiFiles)
        {
            _susiePluginPath = spiFolder;
            SetupSusie(_susiePluginPath, spiFiles);
        }

        // Susie プラグイン 初期化
        private void SetupSusie(string spiFolder, Dictionary<string, bool> spiFiles)
        {
            if (!IsSupportedSusie) return;

            spiFiles = spiFiles ?? new Dictionary<string, bool>();

            var list = ListUpSpiFiles(spiFolder, spiFiles.Keys.ToList());

            _susie.Load(list, _isPluginCacheEnabled);

            // プラグイン有効/無効反映
            foreach (var pair in spiFiles)
            {
                var plugin = _susie.GetPlugin(pair.Key);
                if (plugin != null)
                {
                    plugin.IsEnable = pair.Value;
                }
            }

            // Susie対応拡張子更新
            UpdateImageExtensions();
            UpdateArchiveExtensions();
        }


        // Susieインスタンスから SpiFiles を生成する
        public Dictionary<string, bool> CreateSpiFiles()
        {
            var spiFiles = new Dictionary<string, bool>();

            if (_susie != null)
            {
                foreach (var plugin in _susie.PluginCollection)
                {
                    spiFiles.Add(plugin.FileName, plugin.IsEnable);
                }
            }

            return spiFiles;
        }


        // Susie画像プラグインのサポート拡張子を更新
        public void UpdateImageExtensions()
        {
            var list = new List<string>();
            foreach (var plugin in _susie.INPluginList)
            {
                if (plugin.IsEnable)
                {
                    list.AddRange(plugin.Extensions);
                }
            }
            this.ImageExtensions.FromCollection(list.Distinct());

            Debug.WriteLine("SusieIN Support: " + string.Join(" ", this.ImageExtensions));
        }

        // Susies書庫プラグインのサポート拡張子を更新
        public void UpdateArchiveExtensions()
        {
            var list = new List<string>();
            foreach (var plugin in _susie.AMPluginList)
            {
                if (plugin.IsEnable)
                {
                    list.AddRange(plugin.Extensions);
                }
            }

            this.ArchiveExtensions.FromCollection(list.Distinct());

            Debug.WriteLine("SusieAM Support: " + string.Join(" ", this.ArchiveExtensions));
        }


        /// <summary>
        /// 有効なSusieプラグインをリストアップ
        /// </summary>
        /// <param name="spiFolder">プラグインフォルダー</param>
        /// <param name="spiListSource">期待されるリスト(これまでのリスト)</param>
        /// <returns></returns>
        private List<string> ListUpSpiFiles(string spiFolder, List<string> spiListSource)
        {
            // nullや空白は無効
            if (string.IsNullOrWhiteSpace(spiFolder)) return null;

            // ディテクトリが存在しない場合も無効
            if (!System.IO.Directory.Exists(spiFolder)) return null;

            // 現在のパスで有効なものをリストアップ
            var spiList = spiListSource.Where(e => Path.GetDirectoryName(e) == spiFolder.TrimEnd('\\', '/')).ToList();

            // 新しいSPI追加
            try
            {
                foreach (string s in Directory.GetFiles(spiFolder))
                {
                    if (Path.GetExtension(s).ToLower() == ".spi" && !spiList.Contains(s))
                    {
                        spiList.Add(s);
                    }
                }
            }
            catch (Exception e)
            {
                Debug.WriteLine("SKIP: " + e.Message);
            }

            return spiList;
        }

        #endregion

        #region Memento

        [DataContract]
        public class Memento
        {
            [DataMember]
            public bool IsEnableSusie { get; set; }

            [DataMember]
            public string SusiePluginPath { get; set; }

            [DataMember]
            public bool IsFirstOrderSusieImage { get; set; }

            [DataMember]
            public bool IsFirstOrderSusieArchive { get; set; }

            [DataMember]
            public Dictionary<string, bool> SpiFiles { get; set; }

            [DataMember, DefaultValue(true)]
            public bool IsPluginCacheEnabled { get; set; }

            [DataMember]
            public bool IsPreExtract { get; set; }


            [OnDeserializing]
            private void Deserializing(StreamingContext c)
            {
                this.InitializePropertyDefaultValues();
            }

            public Memento Clone()
            {
                using (var ms = new MemoryStream())
                {
                    DataContractSerializer serializer = new DataContractSerializer(typeof(Memento));
                    serializer.WriteObject(ms, this);
                    ms.Seek(0, SeekOrigin.Begin);
                    return (Memento)serializer.ReadObject(ms);
                }
            }
        }

        //
        public Memento CreateMemento()
        {
            var memento = new Memento();
            memento.IsEnableSusie = this.IsEnableSusie;
            memento.IsFirstOrderSusieImage = this.IsFirstOrderSusieImage;
            memento.IsFirstOrderSusieArchive = this.IsFirstOrderSusieArchive;
            memento.SusiePluginPath = this.SusiePluginPath;
            memento.SpiFiles = CreateSpiFiles();
            memento.IsPluginCacheEnabled = this.IsPluginCacheEnabled;
            memento.IsPreExtract = this.IsPreExtract;
            return memento;
        }

        //
        public void Restore(Memento memento)
        {
            if (memento == null) return;

            this.IsEnableSusie = memento.IsEnableSusie;
            this.IsFirstOrderSusieImage = memento.IsFirstOrderSusieImage;
            this.IsFirstOrderSusieArchive = memento.IsFirstOrderSusieArchive;
            this.IsPluginCacheEnabled = memento.IsPluginCacheEnabled;
            this.IsPreExtract = memento.IsPreExtract;
            Initialize(memento.SusiePluginPath, memento.SpiFiles);
        }

        #endregion
    }
}
