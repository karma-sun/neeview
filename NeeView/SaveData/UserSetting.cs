using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Runtime.Serialization;
using System.Xml;
using NeeView.Effects;
using System.IO;
using System.Diagnostics;

namespace NeeView
{
    /// <summary>
    /// ユーザー設定
    /// このデータがユーザ設定として保存されます。
    /// </summary>
    [DataContract(Name = "Setting")]
    public class UserSetting
    {
        [DataMember]
        public int _Version { get; set; } = Environment.ProductVersionNumber;

        [DataMember(Order = 1)]
        public SusiePluginManager.Memento SusieMemento { get; set; }

        [DataMember(Order = 9998)]
        public CommandTable.Memento CommandMememto { set; get; }

        [DataMember(Order = 9998)]
        public DragActionTable.Memento DragActionMemento { set; get; }

        [DataMember]
        public Models.Memento Memento { get; set; }

        [DataMember]
        public WindowShape.Memento WindowShape { get; set; }

        [DataMember]
        public WindowPlacement.Memento WindowPlacement { get; set; }

        [DataMember]
        public App.Memento App { get; set; }


        // ファイルに保存
        public void Save(string path)
        {
            XmlWriterSettings settings = new XmlWriterSettings();
            settings.Encoding = new System.Text.UTF8Encoding(false);
            settings.Indent = true;
            using (XmlWriter xw = XmlWriter.Create(path, settings))
            {
                DataContractSerializer serializer = new DataContractSerializer(typeof(UserSetting));
                serializer.WriteObject(xw, this);
            }
        }

        // ファイルから読み込み
        public static UserSetting Load(string path)
        {
            using (var stream = new FileStream(path, FileMode.Open, FileAccess.Read))
            {
                stream.Seek(0, SeekOrigin.Begin);
                return Load(stream);
            }
        }

        // ストリームから読み込み
        public static UserSetting Load(Stream stream)
        {
            using (XmlReader xr = XmlReader.Create(stream))
            {
                DataContractSerializer serializer = new DataContractSerializer(typeof(UserSetting));
                UserSetting setting = (UserSetting)serializer.ReadObject(xr);
                return setting;
            }
        }

        public void RestoreConfig(UserSettingV2 setting)
        {
            App?.RestoreConfig(setting.Config);
            WindowPlacement?.RestoreConfig(setting.Config);
            WindowShape?.RestoreConfig(setting.Config);
            Memento?.RestoreConfig(setting.Config);
            SusieMemento?.RestoreConfig(setting.Config);
            CommandMememto?.RestoreConfig(setting.Config);
            DragActionMemento?.RestoreConfig(setting.Config);

            setting.ContextMenu = this.Memento?.MainWindowModel?.ContextMenuSetting?.SourceTreeRaw?.CreateMenuNode();
            setting.SusiePlugins = this.SusieMemento?.CreateSusiePluginCollection() ?? new SusiePluginCollection();
            setting.Commands = this.CommandMememto?.CreateCommandCollection() ?? new CommandCollection();
            setting.DragActions = this.DragActionMemento?.CreateDragActionCollectioin() ?? new DragActionCollection();
        }
    }


    // 一般化できなかったインターフェイス
    public interface IMemento
    {
    }
}
