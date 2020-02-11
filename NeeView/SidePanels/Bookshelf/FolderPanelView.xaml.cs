using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace NeeView
{
    /// <summary>
    /// FolderListPanel.xaml の相互作用ロジック
    /// </summary>
    public partial class FolderPanelView : UserControl
    {
        private FolderPanelViewModel _vm;
        private FolderListView _folderListView;


        public FolderPanelView()
        {
            InitializeComponent();
        }

        public FolderPanelView(FolderPanelModel model, FolderList folderList, PageList pageList) : this()
        {
            _vm = new FolderPanelViewModel(model);
            this.Root.DataContext = _vm;

            _folderListView = new FolderListView(folderList);
            this.FolderList.Content = _folderListView;

            PageListPlacementService.Current.Update();
        }


        private void Root_KeyDown(object sender, KeyEventArgs e)
        {
            bool isLRKeyEnabled = SidePanelProfile.Current.IsLeftRightKeyEnabled;

            if (e.Key == Key.Up || e.Key == Key.Down || (isLRKeyEnabled && (e.Key == Key.Left || e.Key == Key.Right)) || e.Key == Key.Return || e.Key == Key.Delete)
            {
                e.Handled = true;
            }
        }

        public void Refresh()
        {
            _folderListView.Refresh();
        }
    }
}
