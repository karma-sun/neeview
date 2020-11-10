using NeeLaboratory.Windows.Input;
using NeeView.Windows;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace NeeView.Runtime.LayoutPanel
{

    /// <summary>
    /// LayoutPanelWindow.xaml の相互作用ロジック
    /// </summary>
    public partial class LayoutPanelWindow : Window, IHasDpiScale
    {
        private LayoutPanelWindowManager _layoutPanelWindowManager;
        private DpiWatcher _dpiWatcher;

        private WindowChromeAccessor _windowChrome;
        private LayoutPanelWindowCaptionEmulator _windowCaptionEmulator;

        public LayoutPanelWindow()
        {
            InitializeComponent();
            this.DataContext = this;

            _dpiWatcher = new DpiWatcher(this);

            _windowChrome = new WindowChromeAccessor(this);
            //_windowChromeBehavior.WindowChrome.CaptionHeight = 32;
            _windowChrome.IsEnabled = true;

            _windowCaptionEmulator = new LayoutPanelWindowCaptionEmulator(this, this.CaptionBar);
            _windowCaptionEmulator.IsEnabled = true;
        }

        public LayoutPanelWindow(LayoutPanelWindowManager manager, LayoutPanel layoutPanel, WindowPlacement placement) : this()
        {
            WindowTools.DisableMinimize(this);
            ShowInTaskbar = false;

            _layoutPanelWindowManager = manager;
            LayoutPanel = layoutPanel;
            Title = layoutPanel.Title;

            this.FloatingMenuItem.Header = manager.Resources["Floating"];
            this.DockingMenuItem.Header = manager.Resources["Docking"];
            this.CloseMenuItem.Header = manager.Resources["Close"];

            if (placement.IsValid())
            {
                LayoutPanel.WindowPlacement = placement;
            }

            //Content = layoutPanel.Content;

            this.SourceInitialized += LayoutPanelWindow_SourceInitialized;
        }


        public LayoutPanel LayoutPanel
        {
            get { return (LayoutPanel)GetValue(LayoutPanelProperty); }
            set { SetValue(LayoutPanelProperty, value); }
        }

        public static readonly DependencyProperty LayoutPanelProperty =
            DependencyProperty.Register("LayoutPanel", typeof(LayoutPanel), typeof(LayoutPanelWindow), new PropertyMetadata(null));


        public Brush CaptionBackground
        {
            get { return (Brush)GetValue(CaptionBackgroundProperty); }
            set { SetValue(CaptionBackgroundProperty, value); }
        }

        public static readonly DependencyProperty CaptionBackgroundProperty =
            DependencyProperty.Register("CaptionBackground", typeof(Brush), typeof(LayoutPanelWindow), new PropertyMetadata(Brushes.DarkGray));


        public Brush CaptionForeground
        {
            get { return (Brush)GetValue(CaptionForegroundProperty); }
            set { SetValue(CaptionForegroundProperty, value); }
        }

        public static readonly DependencyProperty CaptionForegroundProperty =
            DependencyProperty.Register("CaptionForeground", typeof(Brush), typeof(LayoutPanelWindow), new PropertyMetadata(Brushes.White));


        public WindowChromeAccessor WindowChrome => _windowChrome;


        public DpiScale GetDpiScale()
        {
            return _dpiWatcher.Dpi;
        }

        private void LayoutPanelWindow_SourceInitialized(object sender, EventArgs e)
        {
            WindowPlacementTools.RestoreWindowPlacement(this, LayoutPanel.WindowPlacement);
        }

        public void Snap()
        {
            LayoutPanel.WindowPlacement = WindowPlacementTools.StoreWindowPlacement(this, withAeroSnap: true);
        }

        protected override void OnClosing(CancelEventArgs e)
        {
            Snap();
            base.OnClosing(e);
        }

        protected override void OnClosed(EventArgs e)
        {
            _layoutPanelWindowManager.Closed(LayoutPanel);
            base.OnClosed(e);
        }


        private void OpenDockCommand_Execute(object sender, ExecutedRoutedEventArgs e)
        {
            _layoutPanelWindowManager.LayoutPanelManager.OpenDock(LayoutPanel);
        }

        #region Window state commands

        private void MinimizeWindowCommand_Execute(object sender, ExecutedRoutedEventArgs e)
        {
            SystemCommands.MinimizeWindow(this);
        }

        private void RestoreWindowCommand_Execute(object sender, ExecutedRoutedEventArgs e)
        {
            SystemCommands.RestoreWindow(this);
        }

        private void MaximizeWindowCommand_Execute(object sender, ExecutedRoutedEventArgs e)
        {
            SystemCommands.MaximizeWindow(this);
        }

        private void CloseWindowCommand_Execute(object sender, ExecutedRoutedEventArgs e)
        {
            SystemCommands.CloseWindow(this);
        }

        #endregion Window state commands
    }


    public interface ILayoutPanelWindowDecorater
    {
        void Decorate(LayoutPanelWindow window);
    }

}
