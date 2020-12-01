using NeeView.Runtime.LayoutPanel;
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

namespace NeeView
{

    /// <summary>
    /// MainViewWindow.xaml の相互作用ロジック
    /// </summary>
    public partial class MainViewWindow : Window, IHasDpiScale, IWindowStateControllable, INotifyPropertyChanged
    {
        #region INotifyPropertyChanged Support

        public event PropertyChangedEventHandler PropertyChanged;

        protected bool SetProperty<T>(ref T storage, T value, [System.Runtime.CompilerServices.CallerMemberName] string propertyName = null)
        {
            if (object.Equals(storage, value)) return false;
            storage = value;
            this.RaisePropertyChanged(propertyName);
            return true;
        }

        protected void RaisePropertyChanged([System.Runtime.CompilerServices.CallerMemberName] string name = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
        }

        public void AddPropertyChanged(string propertyName, PropertyChangedEventHandler handler)
        {
            PropertyChanged += (s, e) => { if (string.IsNullOrEmpty(e.PropertyName) || e.PropertyName == propertyName) handler?.Invoke(s, e); };
        }

        #endregion

        private DpiWatcher _dpiWatcher;

        private WindowChromeAccessor _windowChrome;
        private LayoutPanelWindowCaptionEmulator _windowCaptionEmulator;
        private WindowStateManager _windowStateManager;

        public MainViewWindow()
        {
            InitializeComponent();

            var binding = new ThemeBrushBinding(this);
            binding.SetMenuBackgroundBinding(MainViewWindow.CaptionBackgroundProperty);
            binding.SetMenuForegroundBinding(MainViewWindow.CaptionForegroundProperty);

            this.SetBinding(MainViewWindow.TitleProperty, new Binding(nameof(WindowTitle.Title)) { Source = WindowTitle.Current });

            this.DataContext = this;

            _dpiWatcher = new DpiWatcher(this);

            _windowChrome = new WindowChromeAccessor(this);
            _windowChrome.IsEnabled = true;

            _windowCaptionEmulator = new LayoutPanelWindowCaptionEmulator(this, this.CaptionBar);
            _windowCaptionEmulator.IsEnabled = true;

            _windowStateManager = new WindowStateManager(this, new WindowStateManagerDependency(_windowChrome, TabletModeWatcher.Current));
            _windowStateManager.StateChanged += WindowStateManager_StateChanged;

            MenuAutoHideDescription = new BasicAutoHideDescription(this.CaptionBar);

            this.Activated += MainViewWindow_Activated;
        }

        private void MainViewWindow_Activated(object sender, EventArgs e)
        {
            RoutedCommandTable.Current.UpdateInputGestures();
        }

        private void WindowStateManager_StateChanged(object sender, EventArgs e)
        {
            if (this.WindowState == WindowState.Maximized && _windowStateManager.IsFullScreen)
            {
                this.CanHideMenu = true;
                Grid.SetRow(this.CaptionBar, 1);
            }
            else
            {
                this.CanHideMenu = false;
                Grid.SetRow(this.CaptionBar, 0);
            }
        }

        public Brush CaptionBackground
        {
            get { return (Brush)GetValue(CaptionBackgroundProperty); }
            set { SetValue(CaptionBackgroundProperty, value); }
        }

        public static readonly DependencyProperty CaptionBackgroundProperty =
            DependencyProperty.Register("CaptionBackground", typeof(Brush), typeof(MainViewWindow), new PropertyMetadata(Brushes.DarkGray));


        public Brush CaptionForeground
        {
            get { return (Brush)GetValue(CaptionForegroundProperty); }
            set { SetValue(CaptionForegroundProperty, value); }
        }

        public static readonly DependencyProperty CaptionForegroundProperty =
            DependencyProperty.Register("CaptionForeground", typeof(Brush), typeof(MainViewWindow), new PropertyMetadata(Brushes.White));


        public WindowChromeAccessor WindowChrome => _windowChrome;

        public AutoHideConfig AutoHideConfig => Config.Current.AutoHide;

        public InfoMessage InfoMessage => InfoMessage.Current;

        public BasicAutoHideDescription MenuAutoHideDescription { get; private set; }


        public bool IsPanelVisibleLocked => false;


        private bool _CanHideMenu;
        public bool CanHideMenu
        {
            get { return _CanHideMenu; }
            set { SetProperty(ref _CanHideMenu, value); }
        }




        public DpiScale GetDpiScale()
        {
            return _dpiWatcher.Dpi;
        }


        #region IWindowStateControllable

        public void ToggleMinimize()
        {
            SystemCommands.MinimizeWindow(Application.Current.MainWindow);
        }

        public void ToggleMaximize()
        {
            if (this.WindowState != WindowState.Maximized)
            {
                SystemCommands.MaximizeWindow(this);
            }
            else
            {
                SystemCommands.RestoreWindow(this);
            }
        }

        public void ToggleFullScreen()
        {
            _windowStateManager.ToggleFullScreen();
        }

        #endregion IWindowStateControllable

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
