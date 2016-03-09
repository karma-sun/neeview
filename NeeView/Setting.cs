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
        public WindowPlacement.Memento WindowPlacement { set; get; }

        [DataMember]
        public MainWindowVM.Memento ViewMemento { set; get; }

        [DataMember]
        public SusieContext.Memento SusieMemento { get; set; }

        [DataMember]
        public BookHub.Memento BookHubMemento { set; get; }

        [DataMember(Order = 9998)]
        public CommandTable.Memento CommandMememto { set; get; }

        [DataMember(Order = 9999)]
        public BookHistory.Memento BookHistoryMemento { set; get; }

        [DataMember(Order = 4)]
        public Exporter.Memento ExporterMemento { set; get; }

        //
        private void Constructor()
        {
            WindowPlacement = new WindowPlacement.Memento();
            ViewMemento = new MainWindowVM.Memento();
            SusieMemento = new SusieContext.Memento();
            BookHubMemento = new BookHub.Memento();
            CommandMememto = new CommandTable.Memento();
            BookHistoryMemento = new BookHistory.Memento();
            ExporterMemento = new Exporter.Memento();
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
