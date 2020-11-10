using NeeView.Native;
using NeeView.Runtime.LayoutPanel;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Data;
using System.Windows.Media;
using System.Windows.Threading;

namespace NeeView
{
    public class MainLayoutPanelManager : LayoutPanelManager
    {
        public static MainLayoutPanelManager Current { get; }
        static MainLayoutPanelManager() => Current = new MainLayoutPanelManager();


        private bool _initialized;
        private bool _isStoreEnabled = true;


        private MainLayoutPanelManager()
        {
        }

        public void Initialize()
        {
            if (_initialized) return;
            _initialized = true;

            // NOTE: To be on the safe side, initialize the floating point processor.
            Interop.NVFpReset();

            Resources["Floating"] = Properties.Resources.LayoutPanelMenuFloating;
            Resources["Docking"] = Properties.Resources.LayoutPanelMenuDocking;
            Resources["Close"] = Properties.Resources.LayoutPanelMenuClose;

            WindowDecorater = new LayoutWindowDecorater();

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

            // 設定の反映
            Restore();
        }


        public Dictionary<string, IPanel> PanelsSource { get; private set; }
        public LayoutDockPanelContent LeftDock { get; private set; }
        public LayoutDockPanelContent RightDock { get; private set; }

        public bool IsEnabled => true;


        public void SelectPanel(string key, bool isSelected)
        {
            var panel = this.Panels[key];
            if (isSelected)
            {
                Open(panel);

                var source = PanelsSource[key];
                source.Focus();
            }
            else
            {
                Close(panel);
            }
        }

        public bool IsPanelSelected(string key)
        {
            return IsPanelSelected(this.Panels[key]);
        }

        public bool IsPanelVisible(string key)
        {
            return IsPanelVisible(this.Panels[key]);
        }

        public void SetIsStoreEnabled(bool allow)
        {
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
        /// LayoutWindow装飾
        /// </summary>
        class LayoutWindowDecorater : ILayoutPanelWindowDecorater
        {
            public static PanelColorToBrushConverter _captionBackgroundConverter;
            public static PanelColorTobrushMultiConverter _captionForegroundConverter;

            static LayoutWindowDecorater()
            {
                _captionBackgroundConverter = new PanelColorToBrushConverter()
                {
                    Dark = (SolidColorBrush)App.Current.Resources["NVMenuBackgroundDark"],
                    Light = (SolidColorBrush)App.Current.Resources["NVMenuBackgroundLight"],
                };

                _captionForegroundConverter = new PanelColorTobrushMultiConverter()
                {
                    Dark = (SolidColorBrush)App.Current.Resources["NVMenuForegroundDark"],
                    Light = (SolidColorBrush)App.Current.Resources["NVMenuForegroundLight"],
                };
            }


            public void Decorate(LayoutPanelWindow window)
            {
                var background = new Binding(nameof(SidePanelProfile.BackgroundBrushRaw)) { Source = SidePanelProfile.Current };
                window.SetBinding(LayoutPanelWindow.BackgroundProperty, background);

                var captionBackground = new Binding(nameof(ThemeConfig.MenuColor))
                {
                    Source = Config.Current.Theme,
                    Converter = _captionBackgroundConverter,
                };
                window.SetBinding(LayoutPanelWindow.CaptionBackgroundProperty, captionBackground);

                var captionForeground = new MultiBinding() { Converter = _captionForegroundConverter };
                captionForeground.Bindings.Add(new Binding(nameof(ThemeConfig.MenuColor)) { Source = Config.Current.Theme });
                captionForeground.Bindings.Add(new Binding(nameof(LayoutPanelWindow.IsActive)) { Source = window });
                window.SetBinding(LayoutPanelWindow.CaptionForegroundProperty, captionForeground);
            }
        }
    }
}
