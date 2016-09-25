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
        public string Version { get; set; }

        [DataMember(Order = 1)]
        public WindowPlacement.Memento WindowPlacement { set; get; }

        [DataMember(Order = 1)]
        public MainWindowVM.Memento ViewMemento { set; get; }

        [DataMember(Order = 1)]
        public SusieContext.Memento SusieMemento { get; set; }

        [DataMember(Order = 1)]
        public BookHub.Memento BookHubMemento { set; get; }

        [DataMember(Order = 9998)]
        public CommandTable.Memento CommandMememto { set; get; }

        [DataMember(Order = 9998)]
        public DragActionTable.Memento DragActionMemento { set; get; }

        [DataMember(Order = 9999, EmitDefaultValue = false)]
        public BookHistory.Memento BookHistoryMemento { set; get; }

        [DataMember(Order = 4)]
        public Exporter.Memento ExporterMemento { set; get; }

        [DataMember(Order = 14)]
        public Preference.Memento PreferenceMemento { set; get; }

        //
        private void Constructor()
        {
            Version = $"{App.Config.ProductName} {App.Config.ProductVersion}";
            WindowPlacement = new WindowPlacement.Memento();
            ViewMemento = new MainWindowVM.Memento();
            SusieMemento = new SusieContext.Memento();
            BookHubMemento = new BookHub.Memento();
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
