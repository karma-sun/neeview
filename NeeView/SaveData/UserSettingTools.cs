using NeeView.Text.Json;
using System;
using System.Globalization;
using System.IO;
using System.Reflection;
using System.Runtime.Serialization;
using System.Text;
using System.Text.Encodings.Web;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Text.Unicode;
using System.Windows;
using System.Windows.Media;

namespace NeeView
{
    public static class UserSettingTools
    {
        public static UserSetting CreateUserSetting()
        {
            // 情報の確定
            MainWindow.Current.StoreWindowPlacement();
            MainLayoutPanelManager.Current.Store();

            return new UserSetting()
            {
                Format = new FormatVersion(Environment.SolutionName),
                Config = Config.Current,
                ContextMenu = ContextMenuManager.Current.CreateContextMenuNode(),
                SusiePlugins = SusiePluginManager.Current.CreateSusiePluginCollection(),
                DragActions = DragActionTable.Current.CreateDragActionCollection(),
                Commands = CommandTable.Current.CreateCommandCollectionMemento(),
            };
        }

        public static void Save(string path)
        {
            Save(path, CreateUserSetting());
        }

        public static void Save(string path, UserSetting setting)
        {
            var json = JsonSerializer.SerializeToUtf8Bytes(setting, GetSerializerOptions());
            File.WriteAllBytes(path, json);
        }



        public static UserSetting Load(string path)
        {
            var json = File.ReadAllBytes(path);
            return Load(new ReadOnlySpan<byte>(json));
        }

        public static UserSetting Load(Stream stream)
        {
            using (var ms = new MemoryStream())
            {
                stream.CopyTo(ms);
                return Load(new ReadOnlySpan<byte>(ms.ToArray()));
            }
        }

        public static UserSetting Load(ReadOnlySpan<byte> json)
        {
            return JsonSerializer.Deserialize<UserSetting>(json, GetSerializerOptions()).Validate();
        }

        public static JsonSerializerOptions GetSerializerOptions()
        {
            var options = new JsonSerializerOptions();

            options.Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping;
            options.WriteIndented = true;
            options.IgnoreReadOnlyProperties = true;
            options.Converters.Add(new JsonEnumFuzzyConverter());
            options.Converters.Add(new JsonColorConverter());
            options.Converters.Add(new JsonSizeConverter());
            options.Converters.Add(new JsonPointConverter());
            options.Converters.Add(new JsonTimeSpanConverter());
            options.Converters.Add(new JsonGridLengthConverter());
            return options;
        }

        public static void Restore(UserSetting setting, ObjectMergeOption options = null)
        {
            if (setting == null) return;
            if (setting.Config == null) return;

            // コンフィグ反映
            ObjectMerge.Merge(Config.Current, setting.Config, options);

            // レイアウト反映
            MainLayoutPanelManager.Current.Restore();

            // コマンド設定反映
            CommandTable.Current.RestoreCommandCollection(setting.Commands);

            // ドラッグアクション反映
            DragActionTable.Current.RestoreDragActionCollection(setting.DragActions);

            // コンテキストメニュー設定反映
            ContextMenuManager.Current.Resotre(setting.ContextMenu);

            // SusiePlugins反映
            SusiePluginManager.Current.RestoreSusiePluginCollection(setting.SusiePlugins);
        }
    }


}