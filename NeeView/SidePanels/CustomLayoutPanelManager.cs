using NeeView.ComponentModel;
using NeeView.Native;
using NeeView.Runtime.LayoutPanel;
using NeeView.Windows;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Media;
using System.Windows.Threading;

namespace NeeView
{
    public class CustomLayoutPanelManager : LayoutPanelManager
    {
        public static CustomLayoutPanelManager Current { get; }
        static CustomLayoutPanelManager() => Current = new CustomLayoutPanelManager();


        private bool _initialized;
        private bool _isStoreEnabled = true;
        private SidePanelProfile _sidePanelProfile;


        public event EventHandler CollectionChanged;


        public Dictionary<string, IPanel> PanelsSource { get; private set; }
        public LayoutDockPanelContent LeftDock { get; private set; }
        public LayoutDockPanelContent RightDock { get; private set; }


        public void Initialize()
        {
            if (_initialized) return;
            _initialized = true;

            // NOTE: To be on the safe side, initialize the floating point processor.
            Interop.NVFpReset();

            _sidePanelProfile = new SidePanelProfile();
            _sidePanelProfile.Initialize();

            Resources["Floating"] = Properties.Resources.LayoutPanel_Menu_Floating;
            Resources["Docking"] = Properties.Resources.LayoutPanel_Menu_Docking;
            Resources["Close"] = Properties.Resources.LayoutPanel_Menu_Close;

            WindowBuilder = new LayoutPanelWindowBuilder();

            var panelKyes = new[] {
                nameof(FolderPanel),
                nameof(PageListPanel),
                nameof(HistoryPanel),
                nameof(FileInformationPanel),
                nameof(NavigatePanel),
                nameof(ImageEffectPanel),
                nameof(BookmarkPanel),
                nameof(PagemarkPanel),
            };

            var panelLeftKeys = new[] { nameof(FolderPanel), nameof(PageListPanel), nameof(HistoryPanel) };
            var panelRightKeys = panelKyes.Except(panelLeftKeys).ToArray();

            PanelsSource = SidePanelFactory.CreatePanels(panelKyes).ToDictionary(e => e.TypeCode, e => e);
            Panels = LayoutPanelFactory.CreatePanels(PanelsSource.Values).ToDictionary(e => e.Key, e => e);

            LeftDock = new LayoutDockPanelContent(this);
            LeftDock.AddPanelRange(panelLeftKeys.Select(e => Panels[e]));

            RightDock = new LayoutDockPanelContent(this);
            RightDock.AddPanelRange(panelRightKeys.Select(e => Panels[e]));

            Docks = new Dictionary<string, LayoutDockPanelContent>()
            {
                ["Left"] = LeftDock,
                ["Right"] = RightDock,
            };

            Windows.Owner = App.Current.MainWindow;

            LeftDock.CollectionChanged += (s, e) => RaiseCollectionChanged(s, e);
            RightDock.CollectionChanged += (s, e) => RaiseCollectionChanged(s, e);
            Windows.CollectionChanged += (s, e) => RaiseCollectionChanged(s, e);
        }


        private void RaiseCollectionChanged(object sender, EventArgs e)
        {
            CollectionChanged?.Invoke(sender, e);
        }

        public IPanel GetPanel(string key)
        {
            return PanelsSource[key];
        }

        public void SelectPanel(string key, bool isSelected)
        {
            if (!_initialized) throw new InvalidOperationException();

            if (isSelected)
            {
                Open(key, true);
            }
            else
            {
                Close(key);
            }
        }

        public void Open(string key, bool isFocus)
        {
            Open(Panels[key]);
            if (isFocus)
            {
                Focus(key);
            }
        }

        public void OpenDock(string key, bool isFocus)
        {
            OpenDock(Panels[key]);
            if (isFocus)
            {
                Focus(key);
            }
        }

        public void OpenWindow(string key, bool isFocus)
        {
            OpenWindow(Panels[key]);
            if (isFocus)
            {
                Focus(key);
            }
        }

        public void Close(string key)
        {
            Close(Panels[key]);
        }

        public void Focus(string key)
        {
            PanelsSource[key].Focus();
            SidePanelFrame.Current.VisibleAtOnce(key);
        }

        public bool IsPanelSelected(string key)
        {
            if (!_initialized) throw new InvalidOperationException();

            return IsPanelSelected(this.Panels[key]);
        }

        public bool IsPanelVisible(string key)
        {
            if (!_initialized) throw new InvalidOperationException();

            return IsPanelVisible(this.Panels[key]);
        }

        public bool IsPanelFloating(string key)
        {
            if (!_initialized) throw new InvalidOperationException();

            return IsPanelFloating(this.Panels[key]);
        }

        public void SetIsStoreEnabled(bool allow)
        {
            if (!_initialized) throw new InvalidOperationException();

            _isStoreEnabled = allow;
        }

        public void Store()
        {
            if (_initialized && _isStoreEnabled)
            {
                Config.Current.Panels.Layout = CreateMemento();
            }
        }

        public void Restore()
        {
            if (_initialized)
            {
                Restore(Config.Current.Panels.Layout);
            }
        }

        /// <summary>
        /// LayoutPanelWindow作成
        /// </summary>
        class LayoutPanelWindowBuilder : ILayoutPanelWindowBuilder
        {
            public LayoutPanelWindow CreateWindow(LayoutPanelWindowManager manager, LayoutPanel layoutPanel)
            {
                return new CustomLayoutPanelWindow(manager, layoutPanel);
            }
        }
    }
}
