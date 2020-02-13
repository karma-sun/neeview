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
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Diagnostics;

namespace NeeView
{
    /// <summary>
    /// HistoryListView.xaml の相互作用ロジック
    /// </summary>
    public partial class HistoryListView : UserControl
    {
        private HistoryListViewModel _vm;

        //
        public HistoryListView()
        {
            InitializeComponent();
        }

        //
        public HistoryListView(HistoryList model) : this()
        {
            _vm = new HistoryListViewModel(model);
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

        public void FocusAtOnce()
        {
            _vm.ListBoxContent?.FocusAtOnce();
        }
    }

}
