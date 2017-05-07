using System;
using System.Collections.Generic;
using System.Diagnostics;
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
    /// MessageDialogTest.xaml の相互作用ロジック
    /// </summary>
    public partial class MessageDialogTest : Window
    {
        public MessageDialogTest()
        {
            InitializeComponent();
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            var dialog = new MessageDialog(@"E:\Users\local\OneDrive\画像\NekoNeko\test.jpg", "このファイルを削除しますか？");
            dialog.Commands.Add(new UICommand("削除"));
            dialog.Commands.Add(new UICommand("キャンセル"));
            dialog.CancelCommandIndex = 1;

            var result = dialog.ShowDialog();
            Debug.WriteLine($"MessageDialog: {result?.Label}");
        }
    }
}
