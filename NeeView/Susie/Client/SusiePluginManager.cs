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
using NeeView.Susie.Client;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Windows;
using System.Windows.Interop;

namespace NeeView
{
    public class SusiePluginManager : BindableBase
    {
        static SusiePluginManager() => Current = new SusiePluginManager();
        public static SusiePluginManager Current { get; }

        private bool _isEnabled;
        private bool _isFirstOrderSusieImage;
        private bool _isFirstOrderSusieArchive;

        private SusiePluginClient _client;
        private string _susiePluginPath;

        private List<SusiePluginInfo> _unauthorizedPlugins;
        private ObservableCollection<SusiePluginInfo> _INPlugins;
        private ObservableCollection<SusiePluginInfo> _AMPlugins;

        private SusiePluginManager()
        {
            _unauthorizedPlugins = new List<SusiePluginInfo>();
            _INPlugins = new ObservableCollection<SusiePluginInfo>();
            _AMPlugins = new ObservableCollection<SusiePluginInfo>();
        }


        #region Properties

        public List<SusiePluginInfo> UnauthorizedPlugins
        {
            get { return _unauthorizedPlugins; }
            private set { _unauthorizedPlugins = value ?? new List<SusiePluginInfo>(); }
        }

        public ObservableCollection<SusiePluginInfo> INPlugins
        {
            get { return _INPlugins; }
            private set { SetProperty(ref _INPlugins, value); }
        }

        public ObservableCollection<SusiePluginInfo> AMPlugins
        {
            get { return _AMPlugins; }
            private set { SetProperty(ref _AMPlugins, value); }
        }

        public IEnumerable<SusiePluginInfo> Plugins
        {
            get
            {
                foreach (var plugin in UnauthorizedPlugins) yield return plugin;
                foreach (var plugin in INPlugins) yield return plugin;
                foreach (var plugin in AMPlugins) yield return plugin;
            }
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
                if (_susiePluginPath != value)
                {
                    CloseSusiePluginCollection();
                    _susiePluginPath = value;
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

        private void Plugins_CollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            if (e.Action == NotifyCollectionChangedAction.Move)
            {
                FlushSusiePluginOrder();
            }
        }


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

            _client = new SusiePluginClient();

            var settings = Plugins.Select(e => e.ToSusiePluginSetting()).ToList();
            _client.Initialize(_susiePluginPath, settings);

            var plugins = _client.GetPlugin(null);
            UnauthorizedPlugins = new List<SusiePluginInfo>();
            INPlugins = new ObservableCollection<SusiePluginInfo>(plugins.Where(e => e.PluginType == SusiePluginType.Image));
            INPlugins.CollectionChanged += Plugins_CollectionChanged;
            AMPlugins = new ObservableCollection<SusiePluginInfo>(plugins.Where(e => e.PluginType == SusiePluginType.Archive));
            AMPlugins.CollectionChanged += Plugins_CollectionChanged;

            UpdateImageExtensions();
            UpdateArchiveExtensions();
        }

        private void CloseSusiePluginCollection()
        {
            if (_client == null) return;

            _client.Dispose();
            _client = null;

            UnauthorizedPlugins = Plugins.ToList();
            INPlugins = new ObservableCollection<SusiePluginInfo>();
            AMPlugins = new ObservableCollection<SusiePluginInfo>();

            UpdateImageExtensions();
            UpdateArchiveExtensions();
        }


        // Susie画像プラグインのサポート拡張子を更新
        public void UpdateImageExtensions()
        {
            var extensions = INPlugins
                .Where(e => e.IsEnabled)
                .SelectMany(e => e.Extensions);

            ImageExtensions.Restore(extensions);

            Debug.WriteLine("SusieIN Support: " + string.Join(" ", this.ImageExtensions));
        }

        // Susies書庫プラグインのサポート拡張子を更新
        public void UpdateArchiveExtensions()
        {
            var extensions = AMPlugins
                .Where(e => e.IsEnabled)
                .SelectMany(e => e.Extensions);

            ArchiveExtensions.Restore(extensions);

            Debug.WriteLine("SusieAM Support: " + string.Join(" ", this.ArchiveExtensions));
        }

        public void FlushSusiePluginSetting(string name)
        {
            var settings = Plugins
                .Where(e => e.Name == name)
                .Select(e => e.ToSusiePluginSetting())
                .ToList();

            _client.SetPlugin(settings);
        }

        public void UpdateSusiePlugin(string name)
        {
            var plugins = _client.GetPlugin(new List<string>() { name });
            if (plugins != null && plugins.Count == 1)
            {
                var collection = plugins[0].PluginType == SusiePluginType.Image ? INPlugins : AMPlugins;
                var index = collection.IndexOf(collection.FirstOrDefault(e => e.Name == name));
                if (index >= 0)
                {
                    collection[index] = plugins[0];
                }
            }
        }

        public void FlushSusiePluginOrder()
        {
            _client.SetPluginOrder(Plugins.Select(e => e.Name).ToList());
        }

        public SusieImagePluginAccessor GetImagePluginAccessor()
        {
            return new SusieImagePluginAccessor(_client, null);
        }

        public SusieImagePluginAccessor GetImagePluginAccessor(string fileName, byte[] buff, bool isCheckExtension)
        {
            var plugin = _client.GetImagePlugin(fileName, buff, isCheckExtension);
            return new SusieImagePluginAccessor(_client, plugin);
        }

        public SusieArchivePluginAccessor GetArchivePluginAccessor(string fileName, byte[] buff, bool isCheckExtension)
        {
            var plugin = _client.GetArchivePlugin(fileName, buff, isCheckExtension);
            return new SusieArchivePluginAccessor(_client, plugin);
        }

        public void ShowPluginConfigulationDialog(string pluginName, Window owner)
        {
            var handle = new WindowInteropHelper(owner).Handle;
            _client.ShowConfigulationDlg(pluginName, handle.ToInt32());
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

            [DataMember(EmitDefaultValue = false)]
            public List<Susie.SusiePluginSetting> Plugins { get; set; }

            #region Obsolete



            [Obsolete, DataMember(Name = "SpiFiles", EmitDefaultValue = false)]
            public Dictionary<string, bool> SpiFilesV1 { get; set; } // ver 33.0

            [Obsolete, DataMember(Name = "SpiFilesV2", EmitDefaultValue = false)]
            public Dictionary<string, SusiePluginSetting> SpiFilesV2 { get; set; } // ver 34.0 

            [Obsolete, DataMember(Name = "SusiePlugins", EmitDefaultValue = false)]
            public Dictionary<string, SusiePlugin.Memento> SpiFilesV3 { get; set; } // ver 35.0

            [Obsolete, DataMember, DefaultValue(true)]
            public bool IsPluginCacheEnabled { get; set; }

            #endregion Obsolete


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
                    SpiFilesV3 = SpiFilesV2
                        .ToDictionary(e => e.Key, e => e.Value.ToPluginMemento());
                }

                if (_Version < Config.GenerateProductVersionNumber(35, 0, 0) && SpiFilesV3 != null)
                {
                    Plugins = SpiFilesV3
                        .Select(e => e.Value.ToSusiePluginSetting(LoosePath.GetFileName(e.Key), IsPluginCacheEnabled))
                        .ToList();
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
            memento.Plugins = this.Plugins.Select(e => e.ToSusiePluginSetting()).ToList();
            return memento;
        }

        public void Restore(Memento memento)
        {
            if (memento == null) return;

            this.IsEnabled = false; // NOTE: 設定最後にIsEnabledを確定することにより更新タイミングを制御する
            this.IsFirstOrderSusieImage = memento.IsFirstOrderSusieImage;
            this.IsFirstOrderSusieArchive = memento.IsFirstOrderSusieArchive;
            this.UnauthorizedPlugins = memento.Plugins?.Select(e => e.ToSusiePluginInfo()).ToList();
            this.SusiePluginPath = memento.SusiePluginPath;
            this.IsEnabled = memento.IsEnableSusie;
        }

        #endregion
    }
}
