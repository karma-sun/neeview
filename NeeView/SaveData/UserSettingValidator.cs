using NeeView.Runtime.LayoutPanel;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;

namespace NeeView
{
    public static class UserSettingValidator
    {
        // 互換性処理
        public static UserSetting Validate(this UserSetting self)
        {
#pragma warning disable CS0612 // 型またはメンバーが旧型式です
            if (self is null) throw new ArgumentNullException();

            // 画像拡張子初期化
            if (self.Config.Image.Standard.SupportFileTypes is null)
            {
                self.Config.Image.Standard.SupportFileTypes = PictureFileExtensionTools.CreateDefaultSupprtedFileTypes(self.Config.Image.Standard.UseWicInformation);
            }

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

                    self.Config.Panels.Layout = layout;

                    // 古い設定を無効化
                    self.Config.Panels.PanelDocks = null;
                    self.Config.Panels.LeftPanelSeleted = null;
                    self.Config.Panels.RightPanelSeleted = null;
                }
                Debug.WriteLine($"PanelLayout done");

                self.Commands?.ValidateRename(CommandNameValidator.RenameMap_38_0_0);
                self.ContextMenu?.ValidateRename(CommandNameValidator.RenameMap_38_0_0);
            }

            // ver.39
            if (self.Format.CompareTo(new FormatVersion(Environment.SolutionName, 39, 0, 0)) < 0)
            {
                if (self.Config.Panels.FontName_Legacy != default)
                {
                    self.Config.Fonts.FontName = self.Config.Panels.FontName_Legacy;
                }
                if (self.Config.Panels.FontSize_Legacy != default)
                {
                    self.Config.Fonts.PanelFontScale = self.Config.Panels.FontSize_Legacy / SystemVisualParameters.Current.MessageFontSize;
                }
                if (self.Config.Panels.FolderTreeFontSize_Legacy != default)
                {
                    self.Config.Fonts.FolderTreeFontScale = self.Config.Panels.FolderTreeFontSize_Legacy / SystemVisualParameters.Current.MessageFontSize;
                }

                if (self.Config.PagemarkLegacy != null)
                {
                    self.Config.Playlist.PanelListItemStyle = self.Config.PagemarkLegacy.PanelListItemStyle;
                }

                self.Config.Panels.Layout?.ValidateRename("PagemarkPanel", nameof(PlaylistPanel));

                switch (self.Config.System.Language)
                {
                    case "English":
                        self.Config.System.Language = "en";
                        break;
                    case "Japanese":
                        self.Config.System.Language = "ja";
                        break;
                }

                if (self.Config.BookSetting.SortMode == PageSortMode.FileName)
                {
                    self.Config.BookSetting.SortMode = PageSortMode.Entry;
                }
                if (self.Config.BookSetting.SortMode == PageSortMode.FileNameDescending)
                {
                    self.Config.BookSetting.SortMode = PageSortMode.EntryDescending;
                }

                self.Commands?.ValidateRename(CommandNameValidator.RenameMap_39_0_0);
                self.ContextMenu?.ValidateRename(CommandNameValidator.RenameMap_39_0_0);
            }

            return self;
#pragma warning restore CS0612 // 型またはメンバーが旧型式です
        }
    }


    public static class CommandNameValidator
    {
        public static Dictionary<string, string> RenameMap_38_0_0 { get; } = new Dictionary<string, string>()
        {
            ["TogglePermitFileCommand"] = "TogglePermitFile",
            ["FocusPrevAppCommand"] = "FocusPrevApp",
            ["FocusNextAppCommand"] = "FocusNextApp",
        };

        public static Dictionary<string, string> RenameMap_39_0_0 { get; } = new Dictionary<string, string>()
        {
            ["ToggleVisiblePagemarkList"] = "ToggleVisiblePlaylist",
            ["TogglePagemark"] = "TogglePlaylistMark",
            ["PrevPagemark"] = "PrevPlaylistItem",
            ["NextPagemark"] = "NextPlaylistItem",
            ["PrevPagemarkInBook"] = "PrevPlaylistItemInBook",
            ["NextPagemarkInBook"] = "NextPlaylistItemInBook",
        };

        public static void ValidateRename(this CommandCollection commandCollection, Dictionary<string, string> renameMap)
        {
            foreach (var pair in renameMap)
            {
                Rename(pair.Key, pair.Value);
            }

            void Rename(string oldName, string newName)
            {
                if (commandCollection.TryGetValue(oldName, out var element))
                {
                    commandCollection[newName] = element;
                    commandCollection.Remove(oldName);
                }
            }
        }

        public static void ValidateRename(this MenuNode contextMenu, Dictionary<string, string> renameMap)
        {
            foreach (var node in contextMenu.GetEnumerator())
            {
                if (node.CommandName != null && renameMap.TryGetValue(node.CommandName, out var newName))
                {
                    node.CommandName = newName;
                }
            }
        }
    }

    public static class LayoutPanelValidator
    {
        public static void ValidateRename(this LayoutPanelManager.Memento self, string oldName, string newName)
        {
            if (self is null) return;

            if (self.Panels != null && self.Panels.TryGetValue(oldName, out var value))
            {
                self.Panels.Remove(oldName);
                self.Panels.Add(newName, value);
            }

            if (self.Docks != null)
            {
                foreach (var dock in self.Docks.Values)
                {
                    dock.SelectedItem = dock.SelectedItem == oldName ? newName : dock.SelectedItem;

                    dock.Panels = dock.Panels
                        .Select(e => e.Select(x => x == oldName ? newName : x).ToList())
                        .ToList();
                }
            }

            if (self.Windows != null)
            {
                self.Windows.Panels = self.Windows.Panels
                    .Select(x => x == oldName ? newName : x)
                    .ToList();
            }
        }

    }

}