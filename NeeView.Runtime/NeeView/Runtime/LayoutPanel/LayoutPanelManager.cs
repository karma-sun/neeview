using NeeView.Windows;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text.Encodings.Web;
using System.Text.Json;
using System.Windows;

namespace NeeView.Runtime.LayoutPanel
{
    public class LayoutPanelManager
    {
        public Dictionary<string, LayoutPanel> Panels { get; protected set; } = new Dictionary<string, LayoutPanel>();

        public Dictionary<string, LayoutDockPanelContent> Docks { get; protected set; } = new Dictionary<string, LayoutDockPanelContent>();

        public LayoutPanelWindowManager Windows { get; protected set; }

        public Dictionary<string, string> Resources { get; private set; } = new Dictionary<string, string>()
        {
            ["Floating"] = "_Floating",
            ["Docking"] = "Doc_king",
            ["Close"] = "_Close",
        };

        public ILayoutPanelWindowBuilder WindowBuilder { get; set; }

        public LayoutPanelManager()
        {
            Windows = new LayoutPanelWindowManager(this);
        }

        public event EventHandler DragBegin;
        public event EventHandler DragEnd;


        public (LayoutDockPanelContent dock, LayoutPanelCollection collection) FindPanelContains(LayoutPanel panel)
        {
            foreach (var dock in Docks.Values)
            {
                var item = dock.FirstOrDefault(e => e.Contains(panel));
                if (item != null)
                {
                    return (dock, item);
                }
            }

            return (null, null);
        }

        public LayoutDockPanelContent FindPanelListCollection(LayoutPanelCollection collection)
        {
            return Docks.Values.FirstOrDefault(e => e.Contains(collection));
        }

        public void Toggle(LayoutPanel panel)
        {
            if (IsPanelSelected(panel))
            {
                Close(panel);
            }
            else
            {
                Open(panel);
            }
        }

        public bool IsPanelSelected(LayoutPanel panel)
        {
            if (Windows.Contains(panel)) return true;

            foreach (var dock in Docks.Values)
            {
                if (dock.SelectedItem?.Contains(panel) == true)
                {
                    return true;
                }
            }

            return false;
        }

        public bool IsPanelVisible(LayoutPanel panel)
        {
            return (panel?.Content as UIElement)?.IsVisible == true;
        }

        public bool IsPanelFloating(LayoutPanel panel)
        {
            return panel.WindowPlacement.IsValid() || Windows.Contains(panel);
        }

        public void Open(LayoutPanel panel)
        {
            if (panel is null) throw new ArgumentNullException(nameof(panel));

            if (panel.WindowPlacement.IsValid() || Windows.Contains(panel))
            {
                OpenWindow(panel);
            }
            else
            {
                OpenDock(panel);
            }
        }

        public void OpenWindow(LayoutPanel panel)
        {
            OpenWindow(panel, WindowPlacement.None);
        }

        public void OpenWindow(LayoutPanel panel, WindowPlacement placement)
        {
            if (panel is null) throw new ArgumentNullException(nameof(panel));

            StandAlone(panel);
            CloseDock(panel);
            Windows.Open(panel, placement);
        }

        public void OpenDock(LayoutPanel panel)
        {
            if (panel is null) throw new ArgumentNullException(nameof(panel));

            Windows.Close(panel);
            (var collection, var list) = FindPanelContains(panel);
            if (collection == null) throw new InvalidOperationException($"This panel is not registered.: {panel}");
            collection.SelectedItem = list;
        }

        public void Close(LayoutPanel panel)
        {
            if (panel is null) throw new ArgumentNullException(nameof(panel));

            Windows.Close(panel);
            CloseDock(panel);
        }

        private void CloseDock(LayoutPanel panel)
        {
            if (panel is null) throw new ArgumentNullException(nameof(panel));

            foreach (var dock in Docks.Values)
            {
                if (dock.SelectedItem?.Contains(panel) == true)
                {
                    dock.SelectedItem = null;
                }
            }
        }

        public void Remove(LayoutPanel panel)
        {
            Windows.Close(panel);

            foreach (var dock in Docks.Values)
            {
                dock.RemovePanel(panel);
            }
        }


        // パネルの独立
        public void StandAlone(LayoutPanel panel)
        {
            (var collection, var list) = FindPanelContains(panel);
            if (collection == null) throw new InvalidOperationException($"This panel is not registered.: {panel}");

            if (list.IsStandAlone(panel)) return;

            list.Remove(panel);
            collection.Insert(collection.IndexOf(list) + 1, new LayoutPanelCollection() { panel });
        }

        public void RaiseDragBegin()
        {
            DragBegin?.Invoke(this, null);
        }

        public void RaiseDragEnd()
        {
            DragEnd?.Invoke(this, null);
        }


        #region Memento

        public class Memento
        {
            public Dictionary<string, LayoutPanel.Memento> Panels { get; set; }

            public Dictionary<string, LayoutDockPanelContent.Memento> Docks { get; set; }

            public LayoutPanelWindowManager.Memento Windows { get; set; }
        }


        public Memento CreateMemento()
        {
            this.Windows.Snap();

            var memento = new Memento();
            memento.Panels = this.Panels.ToDictionary(e => e.Key, e => e.Value.CreateMemento());
            memento.Docks = Docks.ToDictionary(e => e.Key, e => e.Value.CreateMemento());
            memento.Windows = this.Windows.CreateMemento();
            return memento;
        }

        public void Restore(Memento memento)
        {
            if (memento == null) return;

            this.Windows.CloseAll();

            foreach (var item in memento.Panels.Where(e => Panels.ContainsKey(e.Key)))
            {
                Panels[item.Key].Restore(item.Value);
            }

            foreach(var dock in memento.Docks.Where(e => Docks.ContainsKey(e.Key)))
            {
                Docks[dock.Key].Restore(dock.Value);
            }

            // すべてのパネル登録を保証する
            var excepts = Panels.Keys.Except(Docks.Values.SelectMany(e => e.Items).SelectMany(e => e).Select(e => e.Key)).ToList();
            foreach (var except in excepts)
            {
                Docks.Last().Value.AddPanel(Panels[except]);
            }

            this.Windows.Restore(memento.Windows);
        }

        #endregion
    }


}
