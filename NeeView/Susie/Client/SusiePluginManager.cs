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
using System.Threading;
using NeeView.Properties;
using NeeLaboratory.Collections.Specialized;

namespace NeeView
{
    public class SusiePluginManager : BindableBase
    {
        static SusiePluginManager() => Current = new SusiePluginManager();
        public static SusiePluginManager Current { get; }

        private bool _isInitialized;
        private SusiePluginRemoteClient _remote;
        private SusiePluginClient _client;
        private List<SusiePluginInfo> _unauthorizedPlugins;
        private ObservableCollection<SusiePluginInfo> _INPlugins;
        private ObservableCollection<SusiePluginInfo> _AMPlugins;


        private SusiePluginManager()
        {
            _unauthorizedPlugins = new List<SusiePluginInfo>();
            _INPlugins = new ObservableCollection<SusiePluginInfo>();
            _AMPlugins = new ObservableCollection<SusiePluginInfo>();

            _remote = new SusiePluginRemoteClient();

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
        /// 対応画像ファイル拡張子
        /// </summary>
        public FileTypeCollection ImageExtensions = new FileTypeCollection();

        /// <summary>
        /// 対応圧縮ファイル拡張子
        /// </summary>
        public FileTypeCollection ArchiveExtensions = new FileTypeCollection();

        #endregion

        #region Methods

        public void Initialize()
        {
            if (_isInitialized) return;
            _isInitialized = true;

            Config.Current.Susie.AddPropertyChanged(nameof(SusieConfig.IsEnabled), (s, e) =>
            {
                UpdateSusiePluginCollection();
            });

            Config.Current.Susie.AddPropertyChanging(nameof(SusieConfig.SusiePluginPath), (s, e) =>
            {
                CloseSusiePluginCollection();
            });

            Config.Current.Susie.AddPropertyChanged(nameof(SusieConfig.SusiePluginPath), (s, e) =>
            {
                UpdateSusiePluginCollection();
            });

            UpdateSusiePluginCollection();
        }


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
            if (!_isInitialized) throw new InvalidOperationException();

            if (Config.Current.Susie.IsEnabled && Directory.Exists(Config.Current.Susie.SusiePluginPath))
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

            _client = new SusiePluginClient(_remote);
            _client.SetRecoveryAction(LoadSusiePlugins);

            try
            {
                LoadSusiePlugins();
            }
            catch (Exception)
            {
                ToastService.Current.Show(new Toast(Resources.SusieConnectError_Message, null, ToastIcon.Error));
            }
        }

        private void LoadSusiePlugins()
        {
            var settings = Plugins.Select(e => e.ToSusiePluginSetting()).ToList();
            _client.Initialize(Config.Current.Susie.SusiePluginPath, settings);

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
        public class Memento : IMemento
        {
            [DataMember]
            public int _Version { get; set; } = Environment.ProductVersionNumber;

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

            [Obsolete, DataMember(EmitDefaultValue = false), DefaultValue(true)]
            public bool IsPluginCacheEnabled { get; set; }

            #endregion Obsolete


            [OnDeserializing]
            private void OnDeserializing(StreamingContext c)
            {
                this.InitializePropertyDefaultValues();
            }

            [OnDeserialized]
            private void OnDeserialized(StreamingContext c)
            {
#pragma warning disable CS0612
                if (_Version < Environment.GenerateProductVersionNumber(33, 0, 0) && SpiFilesV1 != null)
                {
                    SpiFilesV2 = SpiFilesV1.ToDictionary(e => e.Key, e => new SusiePluginSetting(e.Value, false));
                }

                if (_Version < Environment.GenerateProductVersionNumber(34, 0, 0) && SpiFilesV2 != null)
                {
                    SpiFilesV3 = SpiFilesV2
                        .ToDictionary(e => e.Key, e => e.Value.ToPluginMemento());
                }

                if (_Version < Environment.GenerateProductVersionNumber(35, 0, 0) && SpiFilesV3 != null)
                {
                    Plugins = SpiFilesV3
                        .Select(e => e.Value.ToSusiePluginSetting(LoosePath.GetFileName(e.Key), IsPluginCacheEnabled))
                        .ToList();
                }
#pragma warning restore CS0612
            }

            public void RestoreConfig(Config config)
            {
                // Pluginsは可変のためConfigに向かない。フラグのみConfigに対応

                config.Susie.IsEnabled = false; // NOTE: 設定最後にIsEnabledを確定することにより更新タイミングを制御する
                config.Susie.IsFirstOrderSusieImage = IsFirstOrderSusieImage;
                config.Susie.IsFirstOrderSusieArchive = IsFirstOrderSusieArchive;
                config.Susie.SusiePluginPath = SusiePluginPath;
                config.Susie.IsEnabled = IsEnableSusie;
            }

            public SusiePluginCollection CreateSusiePluginCollection()
            {
                if (Plugins == null) return null;

                var collection = new SusiePluginCollection();
                foreach (var item in this.Plugins)
                {
                    collection.Add(item.Name, SusiePluginMemento.FromSusiePluginSetting(item));
                }
                return collection;
            }
        }

        #endregion

        #region MementoV2

        public SusiePluginCollection CreateSusiePluginCollection()
        {
            var collection = new SusiePluginCollection();
            foreach(var item in this.Plugins)
            {
                collection.Add(item.Name, SusiePluginMemento.FromSusiePluginInfo(item));
            }
            return collection;
        }

        public void RestoreSusiePluginCollection(SusiePluginCollection memento)
        {
            if (memento == null) return;
            this.UnauthorizedPlugins = memento.Select(e => e.Value.ToSusiePluginInfo(e.Key)).ToList();

            if (_isInitialized)
            {
                UpdateSusiePluginCollection();
            }
        }

        #endregion
    }

    public class SusiePluginCollection : Dictionary<string, SusiePluginMemento>
    {
    }


    public class SusiePluginMemento
    {
        public bool IsEnabled { get; set; }

        public bool IsCacheEnabled { get; set; }

        public bool IsPreExtract { get; set; }

        public string UserExtensions { get; set; }


        public static SusiePluginMemento FromSusiePluginInfo(SusiePluginInfo info)
        {
            var setting = new SusiePluginMemento();
            setting.IsEnabled = info.IsEnabled;
            setting.IsCacheEnabled = info.IsCacheEnabled;
            setting.UserExtensions = info.UserExtension?.ToOneLine();
            return setting;
        }

        public static SusiePluginMemento FromSusiePluginSetting(Susie.SusiePluginSetting setting)
        {
            var memento = new SusiePluginMemento();
            memento.IsEnabled = setting.IsEnabled;
            memento.IsCacheEnabled = setting.IsCacheEnabled;
            memento.UserExtensions = setting.UserExtensions;
            return memento;
        }

        public SusiePluginInfo ToSusiePluginInfo(string name)
        {
            var info = new SusiePluginInfo();
            info.Name = name;
            info.IsEnabled = IsEnabled;
            info.IsCacheEnabled = IsCacheEnabled;
            info.UserExtension = new FileExtensionCollection(UserExtensions);
            return info;
        }
    }

}
