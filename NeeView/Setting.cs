using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using System.Runtime.Serialization;
using System.Xml;
using System.Windows;

namespace NeeView
{
    [DataContract]
    public class Setting
    {
        [DataMember]
        public WindowPlacement WindowPlacement { set; get; }

        [DataMember]
        public ViewSetting ViewSetting { set; get; }

        [DataMember]
        public SusieSetting SusieSetting { get; set; }

        [DataMember]
        public BookCommonSetting BookCommonSetting { set; get; }

        [DataMember]
        public BookSetting BookSetting { set; get; }

        [DataMember]
        public BookCommandShortcutSource GestureSetting { set; get; }

        [DataMember]
        public BookHistory BookHistory { set; get; }


        private void Constructor()
        {
            ViewSetting = new ViewSetting();
            SusieSetting = new SusieSetting();
            BookCommonSetting = new BookCommonSetting();
            BookSetting = new BookSetting();
            GestureSetting = new BookCommandShortcutSource();
            BookHistory = new BookHistory();
        }

        public Setting()
        {
            WindowPlacement = new WindowPlacement();
            Constructor();
        }


        [OnDeserializing]
        private void Deserializing(StreamingContext c)
        {
            Constructor();
        }

        //
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

        //
        public static Setting Load(string path)
        {
            using (XmlReader xr = XmlReader.Create(path))
            {
                DataContractSerializer serializer = new DataContractSerializer(typeof(Setting));
                Setting setting = (Setting)serializer.ReadObject(xr);
                return setting;
            }
        }

        //
        public void Store(Window window)
        {
            WindowPlacement.Store(window);
        }

        //
        public void Restore(Window window)
        {
            WindowPlacement.Restore(window);
        }
    }
}
