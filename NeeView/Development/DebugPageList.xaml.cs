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
    /// DebugPageList.xaml の相互作用ロジック
    /// </summary>
    public partial class DebugPageList : UserControl
    {
        public DebugPageList()
        {
            InitializeComponent();
            this.Root.DataContext = new DevPageListViewModel();
        }
    }

    public class DevPageListViewModel
    {
        public Development Development => Development.Current;
        public BookOperation BookOperation => BookOperation.Current;
    }
}
