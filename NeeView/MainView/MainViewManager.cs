using NeeView.Windows;
using System;
using System.ComponentModel;
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

            _defaultSocket.Content = _mainViewBay;

            _window = new MainViewWindow();
            _window.MainViewSocket.Content = _mainView;

            // NOTE: Tagにインスタンスを保持して消えないようにする
            _window.Tag = new RoutedCommandBinding(_window, RoutedCommandTable.Current);

            _window.Closing += (s, e) => Store();
            _window.Closed += (s, e) => SetFloating(false);

            _window.Show();
            _window.Activate();
        }


        private void Docking()
        {
            if (_window is null) return;

            _window.Close();
            _window.Content = null;
            _window = null;

            // NOTE: コンテンツの差し替えでLoadedイベントが呼ばれないことがあるため、新規コントロールをはさむことで確実にLoadedイベントが呼ばれるようにする。
            _defaultSocket.Content = new ContentControl() { Content = _mainView, IsTabStop = false, Focusable = false };
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
                Config.Current.MainView.WindowPlacement = _window.StoreWindowPlacement();
            }
        }
    }



}
