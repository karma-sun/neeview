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
    /// ExportImageWindow.xaml の相互作用ロジック
    /// </summary>
    public partial class ExportImageWindow : Window
    {
        private ExportImageWindowViewModel _vm;

        public ExportImageWindow()
        {
            InitializeComponent();
        }

        public ExportImageWindow(ExportImageWindowViewModel vm) : this()
        {
            _vm = vm;
            this.DataContext = _vm;
        }

        private void SaveButton_Click(object sender, RoutedEventArgs e)
        {
            bool? result = _vm.ShowSelectSaveFileDialog(this);
            if (result == true)
            {
                this.DialogResult = true;
                this.Close();
            }
        }

        private void CancelButton_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }
    }
}
