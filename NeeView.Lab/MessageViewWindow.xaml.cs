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

namespace NeeView.Lab
{
    /// <summary>
    /// MessageViewWindow.xaml の相互作用ロジック
    /// </summary>
    public partial class MessageViewWindow : Window
    {
        public MessageViewWindow()
        {
            InitializeComponent();
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            this.Target.VM.Message = "";
            this.Target.VM.Message = "oo";
        }
    }
}
