using NeeLaboratory.ComponentModel;
using NeeView.Windows.Controls;
using NeeView.Windows.Property;
using NeeView.Susie;
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
    public class SusieContext : BindableBase
    {
        static SusieContext() => Current = new SusieContext();
        public static SusieContext Current { get; }

        private SusiePluginCollection _pluginCollection;
        public bool _isEnabled;
        public string _susiePluginPath = "";
        public bool _isFirstOrderSusieImage;
        public bool _isFirstOrderSusieArchive;
        private bool _isPluginCacheEnabled = true;


        private SusieContext()
        {
        }


        #region Properties

        /// <summary>
        /// 64bit support?
        /// </summary>
        public static bool Is64bitPlugin { get; } = Environment.Is64BitProcess;

        /// <summary>
        /// Susie Plugin Collection
        /// </summary>
        public SusiePluginCollection PluginCollection
        {
            get { return _pluginCollection; }
            set { SetProperty(ref _pluginCollection, value); }
        }

        /// <summary>
        /// Susie 有効/無効設定
        /// </summary>
        [PropertyMember("@ParamSusieIsEnabled")]
        public bool IsEnabled
        {
            get { return _isEnabled; }
            set
            {
                if (SetProperty(ref _isEnabled, value))
                {
                    UpdateSusiePluginCollection();
                }
            }
        }

        // Susie プラグインフォルダー
        [PropertyPath("@ParamSusiePluginPath", FileDialogType = FileDialogType.Directory)]
        public string SusiePluginPath
        {
            get { return _susiePluginPath; }
            set
            {
                if (SetProperty(ref _susiePluginPath, value))
                {
                    UpdateSusiePluginCollection();
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

        // Susie プラグインキャッシュ有効フラグ
        [PropertyMember("@ParamSusieIsPluginCacheEnabled", Tips = "@ParamSusieIsPluginCacheEnabledTips")]
        public bool IsPluginCacheEnabled
        {
            get { return _isPluginCacheEnabled; }
            set
            {
                if (SetProperty(ref _isPluginCacheEnabled, value))
                {
                    _pluginCollection?.SetPluginCahceEnabled(_isPluginCacheEnabled);
                }
            }
        }

        /// <summary>
        /// プラグイン設定
        /// </summary>
        public Dictionary<string, SusiePlugin.Memento> PluginSettings { get; private set; }

        /// <summary>
        /// 対応画像ファイル拡張子
        /// </summary>
        public FileTypeCollection ImageExtensions = new FileTypeCollection();

        /// <summary>
        /// 対応圧縮ファイル拡張子
        /// </summary>
        public FileTypeCollection ArchiveExtensions = new FileTypeCollection();

        #endregion

        #region Methods

        // PluginCollectionのOpen/Close
        private void UpdateSusiePluginCollection()
        {
            if (_isEnabled && Directory.Exists(_susiePluginPath))
            {
                OpenSusiePluginCollection();
            }
            else
            {
                CloseSusiePluginCollection();
            }
        }

        private void OpenSusiePluginCollection()
        {
            CloseSusiePluginCollection();

            PluginCollection = new SusiePluginCollection(Is64bitPlugin, _isPluginCacheEnabled);
            PluginCollection.Initialize(_susiePluginPath);
            PluginCollection.RestorePlugins(PluginSettings);

            UpdateImageExtensions();
            UpdateArchiveExtensions();
        }

        private void CloseSusiePluginCollection()
        {
            if (PluginCollection == null) return;

            // store plugin settings
            PluginSettings = PluginCollection.StorePlugins();

            PluginCollection.Dispose();
            PluginCollection = null;

            UpdateImageExtensions();
            UpdateArchiveExtensions();
        }

        // 最新のプラグイン設定を取得
        private Dictionary<string, SusiePlugin.Memento> GetLatestPluginSettings()
        {
            return _pluginCollection?.StorePlugins() ?? PluginSettings;
        }

        // Susie画像プラグインのサポート拡張子を更新
        public void UpdateImageExtensions()
        {
            var extensions = _pluginCollection?.INPluginList
                .Where(e => e.IsEnabled)
                .SelectMany(e => e.Extensions.Items);

            ImageExtensions.Restore(extensions);

            Debug.WriteLine("SusieIN Support: " + string.Join(" ", this.ImageExtensions));
        }

        // Susies書庫プラグインのサポート拡張子を更新
        public void UpdateArchiveExtensions()
        {
            var extensions = _pluginCollection?.AMPluginList
                .Where(e => e.IsEnabled)
                .SelectMany(e => e.Extensions.Items);

            ArchiveExtensions.Restore(extensions);

            Debug.WriteLine("SusieAM Support: " + string.Join(" ", this.ArchiveExtensions));
        }

        #endregion

        #region Memento

        [DataContract]
        public class Memento
        {
            [DataMember]
            public int _Version { get; set; } = Config.Current.ProductVersionNumber;

            [DataMember]
            public bool IsEnableSusie { get; set; }

            [DataMember]
            public string SusiePluginPath { get; set; }

            [DataMember]
            public bool IsFirstOrderSusieImage { get; set; }

            [DataMember]
            public bool IsFirstOrderSusieArchive { get; set; }

            [Obsolete, DataMember(Name = "SpiFiles", EmitDefaultValue = false)]
            public Dictionary<string, bool> SpiFilesV1 { get; set; } // ver 33.0

            [Obsolete, DataMember(Name = "SpiFilesV2", EmitDefaultValue = false)]
            public Dictionary<string, SusiePluginSetting> SpiFilesV2 { get; set; } // ver 34.0 

            [DataMember]
            public Dictionary<string, SusiePlugin.Memento> SusiePlugins { get; set; }

            [DataMember, DefaultValue(true)]
            public bool IsPluginCacheEnabled { get; set; }


            [OnDeserializing]
            private void Deserializing(StreamingContext c)
            {
                this.InitializePropertyDefaultValues();
            }

            [OnDeserialized]
            private void Deserialized(StreamingContext c)
            {
#pragma warning disable CS0612
                if (_Version < Config.GenerateProductVersionNumber(33, 0, 0) && SpiFilesV1 != null)
                {
                    SpiFilesV2 = SpiFilesV1.ToDictionary(e => e.Key, e => new SusiePluginSetting(e.Value, false));
                }

                if (_Version < Config.GenerateProductVersionNumber(34, 0, 0) && SpiFilesV2 != null)
                {
                    SusiePlugins = SpiFilesV2
                        .ToDictionary(e => e.Key, e => e.Value.ToPluginMemento());
                }
#pragma warning restore CS0612
            }
        }

        public Memento CreateMemento()
        {
            var memento = new Memento();
            memento.IsEnableSusie = this.IsEnabled;
            memento.IsFirstOrderSusieImage = this.IsFirstOrderSusieImage;
            memento.IsFirstOrderSusieArchive = this.IsFirstOrderSusieArchive;
            memento.SusiePluginPath = this.SusiePluginPath;
            memento.SusiePlugins = GetLatestPluginSettings();
            memento.IsPluginCacheEnabled = this.IsPluginCacheEnabled;
            return memento;
        }

        public void Restore(Memento memento)
        {
            if (memento == null) return;

            this.IsFirstOrderSusieImage = memento.IsFirstOrderSusieImage;
            this.IsFirstOrderSusieArchive = memento.IsFirstOrderSusieArchive;
            this.IsPluginCacheEnabled = memento.IsPluginCacheEnabled;
            this.PluginSettings = memento.SusiePlugins;
            this.SusiePluginPath = memento.SusiePluginPath;
            this.IsEnabled = memento.IsEnableSusie;
        }

        #endregion
    }


    /// <summary>
    /// プラグイン単位の設定 (Obsolete)
    /// </summary>
    [Obsolete, DataContract]
    public class SusiePluginSetting
    {
        public SusiePluginSetting(bool isEnable, bool isPreExtract)
        {
            this.IsEnabled = isEnable;
            this.IsPreExtract = isPreExtract;
        }

        [DataMember]
        public bool IsEnabled { get; set; }

        [DataMember(EmitDefaultValue = false)]
        public bool IsPreExtract { get; set; }


        public SusiePlugin.Memento ToPluginMemento()
        {
            return new SusiePlugin.Memento()
            {
                IsEnabled = IsEnabled,
                IsPreExtract = IsPreExtract,
            };
        }
    }
}
