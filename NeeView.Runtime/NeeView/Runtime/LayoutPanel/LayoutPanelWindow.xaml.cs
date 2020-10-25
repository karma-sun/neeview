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

        private WindowChromeAttachment _windowChrome;
        private LayoutPanelWindowCaptionEmulator _windowCaptionEmulator;

        public LayoutPanelWindow()
        {
            InitializeComponent();
            this.DataContext = this;

            _dpiWatcher = new DpiWatcher(this);

            _windowChrome = new WindowChromeAttachment(this);
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



        public WindowChromeAttachment WindowChrome => _windowChrome;


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

        #region Commands

        private RelayCommand _CloseCommand;
        public RelayCommand CloseCommand
        {
            get { return _CloseCommand = _CloseCommand ?? new RelayCommand(CloseCommand_Executed); }
        }

        private void CloseCommand_Executed()
        {
            this.Close();
        }

        private RelayCommand _DockingCommand;
        public RelayCommand DockingCommand
        {
            get { return _DockingCommand = _DockingCommand ?? new RelayCommand(DockingCommand_Executed); }
        }

        private void DockingCommand_Executed()
        {
            _layoutPanelWindowManager.LayoutPanelManager.OpenDock(LayoutPanel);
        }


        #endregion Commands





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
}
