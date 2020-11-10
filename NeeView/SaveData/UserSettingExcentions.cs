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

                    // 古い設定を無効化
                    self.Config.Panels.PanelDocks = null;
                    self.Config.Panels.LeftPanelSeleted = null;
                    self.Config.Panels.RightPanelSeleted = null;
                    self.Config.Bookshelf.IsPageListDocked = false;
                    self.Config.Bookshelf.IsPageListVisible = false;
                    self.Config.Bookshelf.GridLength0 = default;
                    self.Config.Bookshelf.GridLength2 = default;
                }
                Debug.WriteLine($"PanelLayout done");
            }

            return self;
#pragma warning restore CS0612 // 型またはメンバーが旧型式です
        }
    }


}