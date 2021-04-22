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
    public partial class PlaylistView : UserControl
    {
        private PlaylistViewModel _vm;

        public PlaylistView()
        {
            InitializeComponent();
        }

        public PlaylistView(PlaylistModel model) : this()
        {
            _vm = new PlaylistViewModel(model);
            this.DockPanel.DataContext = _vm;
        }
    }

}
