using NeeView.Runtime.LayoutPanel;
using NeeView.Text.Json;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
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


    public static class UserSettingExcentions
    {
#pragma warning disable CS0612 // 型またはメンバーが旧型式です
        // 互換性処理
        public static UserSetting Validate(this UserSetting self)
        {
            if (self is null) throw new ArgumentNullException();

            // ver.38
            if (self.Format.CompareTo(new FormatVersion(Environment.SolutionName, 38, 0, 0)) < 0)
            {
                Debug.WriteLine($"ValidateShortCutKey...");
                foreach (var command in self.Commands.Values)
                {
                    command.ValidateShortCutKey();
                }
                Debug.WriteLine($"ValidateShortCutKey done.");

                Debug.WriteLine($"PanelLayout...");
                if (self.Config.Panels.PanelDocks != null)
                {

                    var layout = new LayoutPanelManager.Memento();
                    layout.Panels = self.Config.Panels.PanelDocks.Keys.ToDictionary(e => e, e => LayoutPanel.Memento.Default);

                    layout.Docks = new Dictionary<string, LayoutDockPanelContent.Memento>();
                    layout.Docks.Add("Left", new LayoutDockPanelContent.Memento()
                    {
                        Panels = self.Config.Panels.PanelDocks.Where(e => e.Value == PanelDock.Left).Select(e => e.Key).Select(e => new List<string> { e }).ToList(),
                        SelectedItem = self.Config.Panels.LeftPanelSeleted,
                    });
                    layout.Docks.Add("Right", new LayoutDockPanelContent.Memento()
                    {
                        Panels = self.Config.Panels.PanelDocks.Where(e => e.Value == PanelDock.Right).Select(e => e.Key).Select(e => new List<string> { e }).ToList(),
                        SelectedItem = self.Config.Panels.RightPanelSeleted,
                    });

#if false // ページリストの本棚一体化設定は継承しない
                    const string pageListPanelName = "PageListPanel";
                    const string folderPanelName = "FolderPanel";

                    if (self.Config.Bookshelf.IsPageListDocked)
                    {
                        foreach (var dock in layout.Docks.Values)
                        {
                            dock.Panels.RemoveAll(e => e.First() == pageListPanelName);

                            var bookshelf = dock.Panels.FirstOrDefault(e => e.First() == folderPanelName);
                            bookshelf?.Add(pageListPanelName);
                        }

                        var folderListPane = layout.Panels.FirstOrDefault(e => e.Key == folderPanelName);
                        if (folderListPane.Key != null)
                        {
                            folderListPane.Value.GridLength = self.Config.Bookshelf.GridLength0;
                        }

                        var pageListPanel = layout.Panels.FirstOrDefault(e => e.Key == pageListPanelName);
                        if (pageListPanel.Key != null)
                        {
                            pageListPanel.Value.GridLength = self.Config.Bookshelf.GridLength2;
                        }
                    }
#endif
                    self.Config.Panels.Layout = layout;
                }
                Debug.WriteLine($"PanelLayout done");
            }

            return self;
        }
#pragma warning restore CS0612 // 型またはメンバーが旧型式です
    }


}