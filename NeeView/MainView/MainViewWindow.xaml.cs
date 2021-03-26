using NeeView.ComponentModel;
using NeeView.Runtime.LayoutPanel;
using NeeView.Windows;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
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
    public partial class MainViewWindow : Window, INotifyPropertyChanged, IDpiScaleProvider, IHasWindowController, ITopmostControllable, INotifyMouseHorizontalWheelChanged
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

        private DpiScaleProvider _dpiProvider = new DpiScaleProvider();
        private WindowChromeAccessor _windowChrome;
        private WindowStateManager _windowStateManager;
        private WindowBorder _windowBorder;
        private bool _canHideMenu;
        private WindowController _windowController;
        private RoutedCommandBinding _routedCommandBinding;
        private WeakBindableBase<MainViewConfig> _mainViewConfig;

        public MainViewWindow()
        {
            InitializeComponent();

            this.DataContext = this;

            this.SetBinding(MainViewWindow.TitleProperty, new Binding(nameof(WindowTitle.Title)) { Source = WindowTitle.Current });

            _windowChrome = new WindowChromeAccessor(this);

            _windowStateManager = new WindowStateManager(this, new WindowStateManagerDependency(_windowChrome, TabletModeWatcher.Current));
            _windowStateManager.StateChanged += WindowStateManager_StateChanged;

            _windowBorder = new WindowBorder(this, _windowChrome);

            _windowController = new WindowController(_windowStateManager, this);

            _routedCommandBinding = new RoutedCommandBinding(this, RoutedCommandTable.Current);

            _mainViewConfig = new WeakBindableBase<MainViewConfig>(Config.Current.MainView);
            _mainViewConfig.AddPropertyChanged(nameof(MainViewConfig.IsHideTitleBar), (s, e) =>
            {
                RaisePropertyChanged(nameof(IsAutoHide));
                UpdateCaptionBar();
            });

            _mainViewConfig.AddPropertyChanged(nameof(MainViewConfig.IsTopmost), (s, e) =>
            {
                RaisePropertyChanged(nameof(IsTopmost));
            });

            MenuAutoHideDescription = new BasicAutoHideDescription(this.CaptionBar);

            this.SourceInitialized += MainViewWindow_SourceInitialized;
            this.Loaded += MainViewWindow_Loaded;
            this.DpiChanged += MainViewWindow_DpiChanged;
            this.Activated += MainViewWindow_Activated;
            this.Closed += MainViewWindow_Closed;

            UpdateCaptionBar();

            var mouseHorizontalWheel = new MouseHorizontalWheelService(this);
            mouseHorizontalWheel.MouseHorizontalWheelChanged += (s, e) => MouseHorizontalWheelChanged.Invoke(s, e);
        }


        public event MouseWheelEventHandler MouseHorizontalWheelChanged;


        public WindowController WindowController => _windowController;

        public WindowChromeAccessor WindowChrome => _windowChrome;

        public WindowStateManager WindowStateManager => _windowStateManager;

        public WindowBorder WindowBorder => _windowBorder;

        public AutoHideConfig AutoHideConfig => Config.Current.AutoHide;

        public InfoMessage InfoMessage => InfoMessage.Current;

        public BasicAutoHideDescription MenuAutoHideDescription { get; private set; }


        public bool IsTopmost
        {
            get { return Config.Current.MainView.IsTopmost; }
            set { Config.Current.MainView.IsTopmost = value; }
        }

        public bool IsAutoHide
        {
            get { return Config.Current.MainView.IsHideTitleBar; }
            set { Config.Current.MainView.IsHideTitleBar = value; }
        }

        public bool CanHideMenu
        {
            get { return _canHideMenu; }
            set { SetProperty(ref _canHideMenu, value); }
        }

        public bool IsFullScreen
        {
            get { return _windowStateManager.IsFullScreen; }
            set { _windowStateManager.SetFullScreen(value); }
        }


        private void MainViewWindow_SourceInitialized(object sender, EventArgs e)
        {
            _windowChrome.IsEnabled = true;

            var placement = Config.Current.MainView.WindowPlacement;
            if (placement.IsValid() && placement.WindowState == WindowState.Minimized)
            {
                placement = placement.WithState(WindowState.Normal);
            }

            RestoreWindowPlacement(placement);
        }

        private void MainViewWindow_Loaded(object sender, RoutedEventArgs e)
        {
            _dpiProvider.SetDipScale(VisualTreeHelper.GetDpi(this));
        }

        private void MainViewWindow_DpiChanged(object sender, DpiChangedEventArgs e)
        {
            _dpiProvider.SetDipScale(e.NewDpi);
        }

        private void MainViewWindow_Activated(object sender, EventArgs e)
        {
            RoutedCommandTable.Current.UpdateInputGestures();
        }

        private void WindowStateManager_StateChanged(object sender, EventArgs e)
        {
            UpdateCaptionBar();
            RaisePropertyChanged(nameof(IsFullScreen));
        }

        private void MainViewWindow_Closed(object sender, EventArgs e)
        {
            if (_routedCommandBinding != null)
            {
                _routedCommandBinding.Dispose();
                _routedCommandBinding = null;
            }
        }

        private void UpdateCaptionBar()
        {
            if (Config.Current.MainView.IsHideTitleBar || _windowStateManager.IsFullScreen)
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

        public DpiScale GetDpiScale()
        {
            return _dpiProvider.DpiScale;
        }

        public WindowPlacement StoreWindowPlacement()
        {
            return _windowStateManager.StoreWindowPlacement(withAeroSnap: true);
        }

        public void RestoreWindowPlacement(WindowPlacement placement)
        {
            _windowStateManager.RestoreWindowPlacement(placement);
        }

        public void ToggleTopmost()
        {
            Config.Current.MainView.IsTopmost = !Config.Current.MainView.IsTopmost;
        }

        private void StretchWindowCommand_Execute(object sender, ExecutedRoutedEventArgs e)
        {
            if (this.MainViewSocket.Content is MainView mainView)
            {
                mainView.StretchWindow();
            }
        }

    }
}
