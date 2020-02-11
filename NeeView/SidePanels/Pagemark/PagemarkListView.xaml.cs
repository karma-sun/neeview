using NeeView.Windows;
using NeeLaboratory.Windows.Input;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System;

namespace NeeView
{
    /// <summary>
    /// PagemarkListViewl.xaml の相互作用ロジック
    /// </summary>
    public partial class PagemarkListView : UserControl, IDisposable
    {
        private PagemarkListViewModel _vm;


        public PagemarkListView()
        {
            InitializeComponent();
        }

        public PagemarkListView(PagemarkList model) : this()
        {
            _vm = new PagemarkListViewModel(model);
            this.DockPanel.DataContext = _vm;
        }


        private void MoreButton_MouseRightButtonUp(object sender, MouseButtonEventArgs e)
        {
            MoreButton.IsChecked = !MoreButton.IsChecked;
            e.Handled = true;
        }

        private void MoreButton_Checked(object sender, RoutedEventArgs e)
        {
            ContextMenuWatcher.SetTargetElement((UIElement)sender);
        }

        public void Refresh()
        {
            _vm.ListBoxContent?.Refresh();
        }

        #region IDisposable Support
        private bool _disposedValue = false;

        protected virtual void Dispose(bool disposing)
        {
            if (!_disposedValue)
            {
                if (disposing)
                {
                    _vm.Dispose();
                }
                _disposedValue = true;
            }
        }

        public void Dispose()
        {
            Dispose(true);
        }
        #endregion
    }
}
