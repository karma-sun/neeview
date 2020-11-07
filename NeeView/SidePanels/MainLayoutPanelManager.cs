using NeeView.Native;
using NeeView.Runtime.LayoutPanel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Windows.Media;

namespace NeeView
{
    public class MainLayoutPanelManager : LayoutPanelManager
    {
        private static MainLayoutPanelManager _instance;
        public static MainLayoutPanelManager Current => _instance ?? (_instance = new MainLayoutPanelManager());


        private bool _isStoreEnabled = true;

        // TODO: Initializeを分け、Restore()メソッドがいつ呼ばれても良いようにする
        private MainLayoutPanelManager()
        {
            if (_instance != null) throw new InvalidOperationException();
            _instance = this;

            // NOTE: To be on the safe side, initialize the floating point processor.
            Interop.NVFpReset();

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
        }


        public  Dictionary<string, IPanel> PanelsSource { get; private set; }
        public LayoutDockPanelContent LeftDock { get; private set; }
        public LayoutDockPanelContent RightDock { get; private set; }



        internal void SelectPanel(string key, bool isSelected)
        {
            if (isSelected)
            {
                Open(this.Panels[key]);
            }
            else
            {
                Close(this.Panels[key]);
            }
        }

        internal bool IsPanelSelected(string key)
        {
            return IsPanelVisible(this.Panels[key]);
        }


        public void SetIsStoreEnabled(bool allow)
        {
            _isStoreEnabled = allow;
        }

        public void Store()
        {
            if (_isStoreEnabled)
            {
                Config.Current.Panels.Layout = CreateMemento();
            }
        }

        public void Restore()
        {
            Restore(Config.Current.Panels.Layout);
        }

    }
}
