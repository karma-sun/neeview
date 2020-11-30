using System;
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

        private ViewComponent _viewComponent;

        private MainView _mainView;
        private MainViewBay _mainViewBay;

        private ContentControl _defaultSocket;



        public MainViewWindow Window => _window;
        public MainView MainView => _mainView;
        public MainViewBay MainViewBay => _mainViewBay;



        public void Initialize(ViewComponent viewComponent, ContentControl defaultSocket)
        {
            _viewComponent = viewComponent;

            _mainView = _viewComponent.MainView;
            _defaultSocket = defaultSocket;
            _mainViewBay = new MainViewBay();

            _defaultSocket.Content = _mainView;

            Config.Current.MainView.AddPropertyChanged(nameof(MainViewConfig.IsFloating), (s, e) => Update());
        }

        public bool IsFloating()
        {
            return Config.Current.MainView.IsFloating;
        }

        public void SetFloating(bool isFloating)
        {
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

            _defaultSocket.Content = _mainViewBay;

            _window = new MainViewWindow();
            _window.MainViewSocket.Content = _mainView;
            //_window.Owner = Application.Current.MainWindow;

            // NOTE: Tagにインスタンスを保持して消えないようにする
            _window.Tag = new RoutedCommandBinding(_window, RoutedCommandTable.Current);

            _window.Closed += (s, e) => SetFloating(false);

            _window.Show();
        }

        private void Docking()
        {
            if (_window is null) return;

            _window.Close();
            _window.Content = null;
            _window = null;

            _defaultSocket.Content = _mainView;
        }
    }



}
