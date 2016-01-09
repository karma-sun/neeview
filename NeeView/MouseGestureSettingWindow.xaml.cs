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
    /// MouseGestureSettingWindow.xaml の相互作用ロジック
    /// </summary>
    public partial class MouseGestureSettingWindow : Window
    {
        public MouseGestureEx MouseGesture { get; set; }

        public SettingWindow.CommandParam Command { get; set; }

        public MouseGestureSettingWindow(SettingWindow.CommandParam command)
        {
            Command = command;

            InitializeComponent();
            DataContext = this;

            MouseGesture = new MouseGestureEx(this.GestureBox);

            // ESCでウィンドウを閉じる
            this.InputBindings.Add(new KeyBinding(new RelayCommand(Close), new KeyGesture(Key.Escape)));
        }

        private void OkButton_Click(object sender, RoutedEventArgs e)
        {
            Command.MouseGesture = MouseGesture.GestureText; // this.GestureText.Text;

            this.DialogResult = true;
            this.Close();
        }

        private void CancelButton_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }
    }
}
