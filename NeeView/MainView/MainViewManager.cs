using NeeView.Windows;
using System;
using System.ComponentModel;
using System.Diagnostics;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Shapes;

namespace NeeView
{
    public class MainViewManager
    {
        static MainViewManager() => Current = new MainViewManager();
        public static MainViewManager Current { get; }


        private MainViewWindow _window;

        private MainViewComponent _viewComponent;

        private MainView _mainView;
        private MainViewBay _mainViewBay;

        private ContentControl _defaultSocket;

        private bool _isStoreEnabled = true;

        private MainViewLockerMediator _mediator;
        private MainViewLocker _dockingLocker;
        private MainViewLocker _floatingLocker;

        public MainViewWindow Window => _window;
        public MainView MainView => _mainView;
        public MainViewBay MainViewBay => _mainViewBay;



        public void Initialize(MainViewComponent viewComponent, ContentControl defaultSocket)
        {
            _viewComponent = viewComponent;

            _mainView = _viewComponent.MainView;
            _defaultSocket = defaultSocket;
            _mainViewBay = new MainViewBay();

            _defaultSocket.Content = _mainView;

            BookHub.Current.BookChanging += BookHub_BookChanging;

            Config.Current.MainView.AddPropertyChanged(nameof(MainViewConfig.IsFloating), (s, e) => Update());

            _mediator = new MainViewLockerMediator(_mainView);
            _dockingLocker = new MainViewLocker(_mediator, MainWindow.Current);
            _dockingLocker.Activate();
        }

        private void BookHub_BookChanging(object sender, BookChangingEventArgs e)
        {
            _mainView.MouseInput.Cancel();
            _mainViewBay.MouseInput.Cancel();
        }

        public bool IsFloating()
        {
            return Config.Current.MainView.IsFloating;
        }

        public void SetFloating(bool isFloating)
        {
            if (!_isStoreEnabled) return;

            Config.Current.MainView.IsFloating = isFloating;
        }

        public void Update()
        {
            if (Config.Current.MainView.IsFloating)
            {
                Floating();
            }
            else
            {
                Docking();
            }
        }

        private void Floating()
        {
            if (_window != null)
            {
                _window.Focus();
                return;
            }

            if (!Config.Current.MainView.WindowPlacement.IsValid())
            {
                var point = _mainView.PointToScreen(new Point(0.0, 0.0));
                Config.Current.MainView.WindowPlacement = new WindowPlacement(WindowState.Normal, (int)point.X + 32, (int)point.Y + 32, (int)_mainView.ActualWidth, (int)_mainView.ActualHeight);
            }

            _dockingLocker.Deactivate();

            _defaultSocket.Content = _mainViewBay;

            InfoMessage.Current.ClearMessage(ShowMessageStyle.Normal);

            _window = new MainViewWindow();
            _window.MainViewSocket.Content = _mainView;

            _window.Closing += (s, e) => Store();
            _window.Closed += (s, e) => SetFloating(false);

            _window.Show();
            _window.Activate();

            _floatingLocker = new MainViewLocker(_mediator, _window);
            _floatingLocker.Activate();
        }


        private void Docking()
        {
            if (_window is null) return;

            if (_floatingLocker != null)
            {
                _floatingLocker.Deactivate();
                _floatingLocker.Dispose();
                _floatingLocker = null;
            }

            _window.Close();
            _window.Content = null;
            _window = null;

            // NOTE: コンテンツの差し替えでLoadedイベントが呼ばれないことがあるため、新規コントロールをはさむことで確実にLoadedイベントが呼ばれるようにする。
            _defaultSocket.Content = new ContentControl() { Content = _mainView, IsTabStop = false, Focusable = false };

            _dockingLocker.Activate();
        }



        public void SetIsStoreEnabled(bool allow)
        {
            _isStoreEnabled = allow;
        }

        public void Store()
        {
            if (!_isStoreEnabled) return;

            if (_window != null)
            {
                try
                {
                    Config.Current.MainView.WindowPlacement = _window.StoreWindowPlacement();
                }
                catch (Exception ex)
                {
                    Debug.WriteLine(ex.Message);
                }
            }
        }
    }



}
