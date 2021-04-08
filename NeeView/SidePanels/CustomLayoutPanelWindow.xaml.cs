using NeeView.ComponentModel;
using NeeView.Runtime.LayoutPanel;
using NeeView.Windows;
using System;
using System.Collections.Generic;
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
    /// CustomLayoutPanelWindow.xaml の相互作用ロジック
    /// </summary>
    public partial class CustomLayoutPanelWindow : LayoutPanelWindow, IDisposable
    {
        private WindowChromeAccessor _windowChrome;
        private WindowStateManager _windowStateManager;
        private RoutedCommandBinding _routedCommandBinding;
        private bool _disposedValue;


        public CustomLayoutPanelWindow()
        {
            InitializeComponent();
            this.DataContext = this;

            _windowChrome = new WindowChromeAccessor(this);
            _windowStateManager = new WindowStateManager(this, new WindowStateManagerDependency(_windowChrome, TabletModeWatcher.Current));
        }

        public CustomLayoutPanelWindow(LayoutPanelWindowManager manager, LayoutPanel layoutPanel) : this()
        {
            this.LayoutPanelWindowManager = manager;
            this.LayoutPanel = layoutPanel;
            this.Title = layoutPanel.Title;

            this.CaptionBar.ContextMenu = CreateContextMenu();

            _routedCommandBinding = new RoutedCommandBinding(this, RoutedCommandTable.Current);
        }


        public WindowChromeAccessor WindowChrome => _windowChrome;



        protected override void OnSourceInitialized(EventArgs e)
        {
            _windowChrome.IsEnabled = true;

            base.OnSourceInitialized(e);
        }

        protected override void OnActivated(EventArgs e)
        {
            RoutedCommandTable.Current.UpdateInputGestures();

            base.OnActivated(e);
        }

        protected override void OnClosed(EventArgs e)
        {
            Dispose();

            base.OnClosed(e);
        }

        #region IDisposable

        protected virtual void Dispose(bool disposing)
        {
            if (!_disposedValue)
            {
                if (disposing)
                {
                    _routedCommandBinding?.Dispose();
                    _routedCommandBinding = null;
                }

                _disposedValue = true;
            }
        }

        public void Dispose()
        {
            Dispose(disposing: true);
            GC.SuppressFinalize(this);
        }

        #endregion IDisposable
    }
}
