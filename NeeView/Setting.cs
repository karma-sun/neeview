// Copyright (c) 2016 Mitsuhiro Ito (nee)
//
// This software is released under the MIT License.
// http://opensource.org/licenses/mit-license.php

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Runtime.Serialization;
using System.Xml;
using NeeView.Effects;

namespace NeeView
{
    /// <summary>
    /// ユーザー設定
    /// このデータがユーザ設定として保存されます。
    /// </summary>
    [DataContract]
    public class Setting
    {
        [DataMember]
        public int _Version { get; set; } = NeeView.Config.Current .ProductVersionNumber;


        [DataMember(Order = 1)]
        public SusieContext.Memento SusieMemento { get; set; }


        [DataMember(Order = 9998)]
        public CommandTable.Memento CommandMememto { set; get; }

        [DataMember(Order = 9998)]
        public DragActionTable.Memento DragActionMemento { set; get; }

        [DataMember(Order = 4)]
        public Exporter.Memento ExporterMemento { set; get; }

        [DataMember(Order = 14)]
        public Preference.Memento PreferenceMemento { set; get; }

        [DataMember]
        public Models.Memento Memento { get; set; }

        [DataMember]
        public WindowShape.Memento WindowShape { get; set; }

        [DataMember]
        public App.Memento App { get; set; }


        // no used
        [Obsolete, DataMember(Order = 1, EmitDefaultValue = false)]
        public BookHub.Memento BookHubMemento { set; get; }

        [Obsolete, DataMember(Order = 1, EmitDefaultValue = false)]
        public MainWindowVM.Memento ViewMemento { set; get; } // no used (ver.23)

        [Obsolete, DataMember(Order = 9999, EmitDefaultValue = false)]
        public BookHistory.Memento BookHistoryMemento { set; get; } // no used

        [Obsolete, DataMember(Order = 17, EmitDefaultValue = false)]
        public ImageEffect.Memento ImageEffectMemento { get; set; } // no used (ver.22)

        //
        private void Constructor()
        {
            SusieMemento = new SusieContext.Memento();
            CommandMememto = new CommandTable.Memento();
            DragActionMemento = new DragActionTable.Memento();
            ExporterMemento = new Exporter.Memento();
            PreferenceMemento = new Preference.Memento();
        }

        //
        public Setting()
        {
            Constructor();
        }

        //
        [OnDeserializing]
        private void Deserializing(StreamingContext c)
        {
            Constructor();
        }

#pragma warning disable CS0612

        //
        [OnDeserialized]
        private void Deserialized(StreamingContext c)
        {
            // before 1.20
            if (_Version < Config.GenerateProductVersionNumber(1, 20, 0))
            {
                PreferenceMemento.Add("openbook_begin_current", BookHubMemento.IsEnarbleCurrentDirectory.ToString());
                BookHubMemento.IsEnarbleCurrentDirectory = false;
            }
        }

#pragma warning restore CS0612


        // ファイルに保存
        public void Save(string path)
        {
            XmlWriterSettings settings = new XmlWriterSettings();
            settings.Encoding = new System.Text.UTF8Encoding(false);
            settings.Indent = true;
            using (XmlWriter xw = XmlWriter.Create(path, settings))
            {
                DataContractSerializer serializer = new DataContractSerializer(typeof(Setting));
                serializer.WriteObject(xw, this);
            }
        }

        // ファイルから読み込み
        public static Setting Load(string path)
        {
            using (XmlReader xr = XmlReader.Create(path))
            {
                DataContractSerializer serializer = new DataContractSerializer(typeof(Setting));
                Setting setting = (Setting)serializer.ReadObject(xr);
                return setting;
            }
        }
    }
}
