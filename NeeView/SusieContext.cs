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
        public Susie.Susie Susie { get; private set; }

        // Susie 有効/無効フラグ
        public bool _IsEnableSusie;
        public bool IsEnableSusie
        {
            get { return _IsEnableSusie; }
            set
            {
                if (_IsEnableSusie == value) return;
                _IsEnableSusie = value;
                SusieArchiver.IsEnable = _IsEnableSusie;
                SusieBitmapLoader.IsEnable = _IsEnableSusie;
            }
        }

        // Susie プラグイン有効/無効フラグリスト
        public Dictionary<string, bool> SpiFiles { get; set; } = new Dictionary<string, bool>();

        // Susie プラグインパス
        public string _SusiePluginPath;
        public string SusiePluginPath
        {
            get { return _SusiePluginPath; }
            set
            {
                _SusiePluginPath = value;
                ListUpSpiFiles();
                Initialize();
            }
        }

        // Susie プラグイン リストアップ
        private void ListUpSpiFiles()
        {
            // フィルタ削除
            SpiFiles = SpiFiles.Where(e => Path.GetDirectoryName(e.Key) == SusiePluginPath).ToDictionary(e => e.Key, e => e.Value);

            // nullや空白は無効
            if (string.IsNullOrWhiteSpace(SusiePluginPath)) return;

            // ディテクトリが存在しない場合も無効
            if (!System.IO.Directory.Exists(SusiePluginPath)) return;

            // 新しいSPI追加
            try
            {
                foreach (string s in Directory.GetFiles(SusiePluginPath))
                {
                    if (Path.GetExtension(s).ToLower() == ".spi" && !SpiFiles.ContainsKey(s))
                    {
                        SpiFiles.Add(s, true);
                    }
                }
            }
            catch (Exception e)
            {
                Debug.WriteLine("SKIP: " + e.Message);
            }
        }

        // Susie 画像プラグイン 優先フラグ
        public bool _IsFirstOrderSusieImage;
        public bool IsFirstOrderSusieImage
        {
            get { return _IsFirstOrderSusieImage; }
            set
            {
                if (_IsFirstOrderSusieImage != value)
                {
                    _IsFirstOrderSusieImage = value;
                    ModelContext.BitmapLoaderManager.OrderType = _IsFirstOrderSusieImage ? BitmapLoaderType.Susie : BitmapLoaderType.Default;
                }
            }
        }

        // Susie 書庫プラグイン 優先フラグ
        public bool _IsFirstOrderSusieArchive;
        public bool IsFirstOrderSusieArchive
        {
            get { return _IsFirstOrderSusieArchive; }
            set
            {
                if (_IsFirstOrderSusieArchive != value)
                {
                    _IsFirstOrderSusieArchive = value;
                    ModelContext.ArchiverManager.OrderType = _IsFirstOrderSusieArchive ? ArchiverType.SusieArchiver : ArchiverType.DefaultArchiver;
                }
            }
        }


        // Susie 初期化
        public void Initialize()
        {
            // 新規
            Susie = new Susie.Susie();
            Susie.Load(SpiFiles.Keys);

            var removeList = new List<string>();

            // プラグイン有効/無効反映
            foreach (var pair in SpiFiles)
            {
                var plugin = Susie.GetPlugin(pair.Key);
                if (plugin != null)
                {
                    plugin.IsEnable = pair.Value;
                }
                else
                {
                    removeList.Add(pair.Key);
                }
            }

            // 使用できないブラグイン削除
            foreach(var key in removeList)
            {
                SpiFiles.Remove(key);
            }


            // Susie対応拡張子更新
            ModelContext.ArchiverManager.UpdateSusieSupprtedFileTypes(Susie);
            ModelContext.BitmapLoaderManager.UpdateSusieSupprtedFileTypes(Susie);
        }



        #region Memento

        [DataContract]
        public class SusieSetting
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
                SusiePluginPath = global::Susie.Susie.GetSusiePluginInstallPath();
                SpiFiles = new Dictionary<string, bool>();
            }

            public SusieSetting()
            {
                Constructor();
            }

            [OnDeserializing]
            private void Deserializing(StreamingContext c)
            {
                Constructor();
            }

            // Susieインスタンスから SpiFiles を再構成する
            public void SetSpiFiles(global::Susie.Susie susie)
            {
                SpiFiles.Clear();

                foreach (var plugin in susie.PluginCollection)
                {
                    SpiFiles.Add(plugin.FileName, plugin.IsEnable);
                }
            }
        }

        //
        public SusieSetting CreateMemento()
        {
            var memento = new SusieSetting();
            memento.IsEnableSusie = IsEnableSusie;
            memento.SpiFiles = SpiFiles;
            memento.SusiePluginPath = SusiePluginPath;
            memento.IsFirstOrderSusieImage = IsFirstOrderSusieImage;
            memento.IsFirstOrderSusieArchive = IsFirstOrderSusieArchive;
            return memento;
        }

        //
        public void Restore(SusieSetting memento)
        {
            IsEnableSusie = memento.IsEnableSusie;
            SpiFiles = memento.SpiFiles;
            SusiePluginPath = memento.SusiePluginPath;
            IsFirstOrderSusieImage = memento.IsFirstOrderSusieImage;
            IsFirstOrderSusieArchive = memento.IsFirstOrderSusieArchive;
        }

        #endregion
    }
}
