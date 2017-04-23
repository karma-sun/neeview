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

namespace NeeView
{
    /// <summary>
    /// BookmarkPanelView.xaml の相互作用ロジック
    /// </summary>
    public partial class BookmarkPanelView : UserControl
    {
        public BookmarkPanelView()
        {
            InitializeComponent();
        }

        public void Initialize(MainWindowVM vm)
        {
            this.DataContext = vm;
            this.BookmarkControl.Initialize(vm.BookHub);
        }
    }
}
