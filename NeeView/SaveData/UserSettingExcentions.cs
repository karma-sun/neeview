using NeeView.Runtime.LayoutPanel;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;

namespace NeeView
{
    public static class UserSettingExcentions
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
            }

            // ver.39
            if (self.Format.CompareTo(new FormatVersion(Environment.SolutionName, 39, 0, 0)) < 0)
            {
                if (self.Config.Panels.FontName_Legacy != default)
                {
                    self.Config.Fonts.PanelFontName = self.Config.Panels.FontName_Legacy;
                }
                if (self.Config.Panels.FontSize_Legacy != default)
                {
                    self.Config.Fonts.PanelFontScale = self.Config.Panels.FontSize_Legacy / VisualParameters.SystemMessageFontSize;
                }
                if (self.Config.Panels.FolderTreeFontSize_Legacy != default)
                {
                    self.Config.Fonts.FolderTreeFontScale = self.Config.Panels.FolderTreeFontSize_Legacy / VisualParameters.SystemMessageFontSize;
                }
            }

            return self;
#pragma warning restore CS0612 // 型またはメンバーが旧型式です
        }
    }


}