// Copyright (c) 2016 Mitsuhiro Ito (nee)
//
// This software is released under the MIT License.
// http://opensource.org/licenses/mit-license.php

using System;
using System.Collections.Generic;
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
    public class SusieContext
    {
        public static SusieContext Current { get; private set; }

        /// <summary>
        /// constructor
        /// </summary>
        public SusieContext()
        {
            Current = this;
        }

        /// <summary>
        /// 機能サポート判定
        /// </summary>
#if SUSIE
        public bool IsSupportedSusie => true;
#else
        public bool IsSupportedSusie => false;
#endif

        public Susie.Susie Susie { get; private set; }

        // Susie 有効/無効フラグ
        public bool _IsEnableSusie;
        public bool IsEnableSusie
        {
            get { return _IsEnableSusie; }
            set
            {
                _IsEnableSusie = value;
                SusieArchiver.IsEnable = _IsEnableSusie;
                SusieBitmapLoader.IsEnable = _IsEnableSusie && IsSupportedSusie;
            }
        }

        // Susie プラグインパス
        public string _SusiePluginPath = "";
        public string SusiePluginPath
        {
            get { return _SusiePluginPath; }
            set { _SusiePluginPath = value; }
        }

        // Susie 画像プラグイン 優先フラグ
        public bool _IsFirstOrderSusieImage;
        public bool IsFirstOrderSusieImage
        {
            get { return _IsFirstOrderSusieImage; }
            set
            {
                _IsFirstOrderSusieImage = value;
                BitmapLoaderManager.Current.OrderType = _IsFirstOrderSusieImage ? BitmapLoaderType.Susie : BitmapLoaderType.Default;
            }
        }

        // Susie 書庫プラグイン 優先フラグ
        public bool _IsFirstOrderSusieArchive;
        public bool IsFirstOrderSusieArchive
        {
            get { return _IsFirstOrderSusieArchive; }
            set
            {
                _IsFirstOrderSusieArchive = value;
                ArchiverManager.Current.OrderType = _IsFirstOrderSusieArchive ? ArchiverType.SusieArchiver : ArchiverType.DefaultArchiver;
            }
        }


        /// <summary>
        /// Spi Files
        /// </summary>
        public Dictionary<string, bool> SpiFiles
        {
            get { return _spiFiles; }
            set
            {
                if (_spiFiles != value)
                {
                    _spiFiles = value ?? new Dictionary<string, bool>();
                    Initialize();
                }
            }
        }

        private Dictionary<string, bool> _spiFiles = new Dictionary<string, bool>();



        // Susie 初期化
        private void Initialize()
        {
            if (!IsSupportedSusie) return;
            //Debug.Assert(IsSupportedSusie);

            var list = ListUpSpiFiles(_spiFiles.Keys.ToList());

            // 新規
            Susie = new Susie.Susie();
            Susie.Load(list);

            // プラグイン有効/無効反映
            foreach (var pair in _spiFiles)
            {
                var plugin = Susie.GetPlugin(pair.Key);
                if (plugin != null)
                {
                    plugin.IsEnable = pair.Value;
                }
            }

            // 有効なプラグインリストに更新
            _spiFiles = Memento.CreateSpiFiles(Susie);

            // Susie対応拡張子更新
            ArchiverManager.Current.UpdateSusieSupprtedFileTypes(Susie);
            BitmapLoaderManager.Current.UpdateSusieSupprtedFileTypes(Susie);
        }


        // Susie プラグイン リストアップ
        private List<string> ListUpSpiFiles(List<string> spiListSource)
        {
            // nullや空白は無効
            if (string.IsNullOrWhiteSpace(SusiePluginPath)) return null;

            // ディテクトリが存在しない場合も無効
            if (!System.IO.Directory.Exists(SusiePluginPath)) return null;

            // 現在のパスで有効なものをリストアップ
            var spiList = spiListSource.Where(e => Path.GetDirectoryName(e) == SusiePluginPath.TrimEnd('\\', '/')).ToList();

            // 新しいSPI追加
            try
            {
                foreach (string s in Directory.GetFiles(SusiePluginPath))
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


            private void Constructor()
            {
                SusiePluginPath = "";
                SpiFiles = new Dictionary<string, bool>();
            }

            public Memento()
            {
                Constructor();
            }

            [OnDeserializing]
            private void Deserializing(StreamingContext c)
            {
                Constructor();
            }

            // Susieインスタンスから SpiFiles を生成する
            public static Dictionary<string, bool> CreateSpiFiles(global::Susie.Susie susie)
            {
                var spiFiles = new Dictionary<string, bool>();

                if (susie != null)
                {
                    foreach (var plugin in susie.PluginCollection)
                    {
                        spiFiles.Add(plugin.FileName, plugin.IsEnable);
                    }
                }

                return spiFiles;
            }

            //
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
            memento.SusiePluginPath = this.SusiePluginPath;
            memento.IsFirstOrderSusieImage = this.IsFirstOrderSusieImage;
            memento.IsFirstOrderSusieArchive = this.IsFirstOrderSusieArchive;
            memento.SpiFiles = this.SpiFiles;
            return memento;
        }

        //
        public void Restore(Memento memento)
        {
            if (memento == null) return;

            this.IsEnableSusie = memento.IsEnableSusie;
            this.SusiePluginPath = memento.SusiePluginPath;
            this.IsFirstOrderSusieImage = memento.IsFirstOrderSusieImage;
            this.IsFirstOrderSusieArchive = memento.IsFirstOrderSusieArchive;
            this.SpiFiles = memento.SpiFiles;

#if false
            if (IsSupportedSusie)
            {
                // Susie使用可能な場合のみ初期化
                Initialize();
            }
            else
            {
                Debug.WriteLine("Not support Susie (x86 only).");
                return;
            }
#endif
        }

        #endregion
    }
}
