using NeeView.ComponentModel;
using NeeView.Native;
using NeeView.Runtime.LayoutPanel;
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
    public class MainLayoutPanelManager : LayoutPanelManager
    {
        public static MainLayoutPanelManager Current { get; }
        static MainLayoutPanelManager() => Current = new MainLayoutPanelManager();


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

            Resources["Floating"] = Properties.Resources.LayoutPanelMenuFloating;
            Resources["Docking"] = Properties.Resources.LayoutPanelMenuDocking;
            Resources["Close"] = Properties.Resources.LayoutPanelMenuClose;

            ContainerDecorator = new LayoutPanelContainerDecorator();
            WindowDecorator = new LayoutWindowDecorator();

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
        /// LayoutContainer装飾
        /// </summary>
        class LayoutPanelContainerDecorator : ILayoutPanelContainerDecorator
        {
            public void Decorate(LayoutPanelContainer container, Button closeButton)
            {
                closeButton.Style = (Style)App.Current.Resources["IconButton"];
            }
        }

        /// <summary>
        /// LayoutWindow装飾
        /// </summary>
        class LayoutWindowDecorator : ILayoutPanelWindowDecorator
        {
            class LayoutWindowDecorate : IDisposable
            {
                private WeakBindableBase<WindowConfig> _windowConfig; // GCされないように保持
                private RoutedCommandBinding _routedCommandBinding;
                private bool _disposedValue;

                public LayoutWindowDecorate(WeakBindableBase<WindowConfig> windowConfig, RoutedCommandBinding routedCommandBinding)
                {
                    _windowConfig = windowConfig;
                    _routedCommandBinding = routedCommandBinding;
                }

                protected virtual void Dispose(bool disposing)
                {
                    if (!_disposedValue)
                    {
                        if (disposing)
                        {
                            if (_routedCommandBinding != null)
                            {
                                _routedCommandBinding.Dispose();
                                _routedCommandBinding = null;
                            }
                        }

                        _disposedValue = true;
                    }
                }

                public void Dispose()
                {
                    Dispose(disposing: true);
                    GC.SuppressFinalize(this);
                }
            }

            public void Decorate(LayoutPanelWindow window)
            {
                window.Style = (Style)App.Current.Resources["DefaultWindowStyle"];

                var binding = new ThemeBinder(window);
                binding.SetMenuBackgroundBinding(LayoutPanelWindow.CaptionBackgroundProperty);
                binding.SetMenuForegroundBinding(LayoutPanelWindow.CaptionForegroundProperty);
                window.SetBinding(LayoutPanelWindow.BackgroundProperty, new Binding(nameof(ThemeBrushProvider.BackgroundBrushRaw)) { Source = ThemeBrushProvider.Current });

                var windowConfig = new WeakBindableBase<WindowConfig>(Config.Current.Window);
                windowConfig.AddPropertyChanged(nameof(WindowConfig.MaximizeWindowGapWidth), (s, e) => UpdateMaximizeWindowGapWidth(window));
                UpdateMaximizeWindowGapWidth(window);

                var windowBorder = new WindowBorder(window, window.WindowChrome);
                ((FrameworkElement)window.FindName("WindowBorder")).SetBinding(Border.BorderThicknessProperty, new Binding(nameof(WindowBorder.Thickness)) { Source = windowBorder });

                var routedCommandBinding = new RoutedCommandBinding(window, RoutedCommandTable.Current);

                // NOTE: Tagにインスタンスを保持
                window.Tag = new LayoutWindowDecorate(windowConfig, routedCommandBinding);

                window.Activated += Window_Activated;
                window.Closed += Window_Closed;
            }

            private void Window_Closed(object sender, EventArgs e)
            {
                var window = (LayoutPanelWindow)sender;
                if (window.Tag is LayoutWindowDecorate decorate)
                {
                    decorate.Dispose();
                    window.Tag = null;
                }
            }

            private static void Window_Activated(object sender, EventArgs e)
            {
                RoutedCommandTable.Current.UpdateInputGestures();
            }

            private static void UpdateMaximizeWindowGapWidth(LayoutPanelWindow window)
            {
                window.WindowChrome.MaximizeWindowGapWidth = Config.Current.Window.MaximizeWindowGapWidth;
            }
        }
    }
}
