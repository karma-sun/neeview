using NeeLaboratory.ComponentModel;
using System;
using System.Collections.Generic;
using System.ComponentModel;
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
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace NeeView
{
    /// <summary>
    /// DebugMenu.xaml の相互作用ロジック
    /// </summary>
    public partial class DebugMenu : UserControl 
    {
        public DebugMenu()
        {
            InitializeComponent();
            this.MenuDev.DataContext = new DebugMenuViewModel();
        }


        // [開発用] テストボタン
        private void MenuItemDevButton_Click(object sender, RoutedEventArgs e)
        {
            DebugTestAction();
        }

        // 開発用コマンド：テンポラリフォルダーを開く
        private void MenuItemDevTempFolder_Click(object sender, RoutedEventArgs e)
        {
            DebugOpenFolder(Temporary.Current.TempDirectory);
        }

        // 開発用コマンド：アプリケーションフォルダーを開く
        private void MenuItemDevApplicationFolder_Click(object sender, RoutedEventArgs e)
        {
            DebugOpenFolder(System.IO.Path.GetDirectoryName(System.Reflection.Assembly.GetEntryAssembly().Location));
        }

        // 開発用コマンド：アプリケーションデータフォルダーを開く
        private void MenuItemDevApplicationDataFolder_Click(object sender, RoutedEventArgs e)
        {
            DebugOpenFolder(Config.Current.LocalApplicationDataPath);
        }

        // 開発用コマンド：カレントフォルダーを開く
        private void MenuItemDevCurrentFolder_Click(object sender, RoutedEventArgs e)
        {
            DebugOpenFolder(Environment.CurrentDirectory);
        }


        /// <summary>
        /// 開発用：テストボタンのアクション
        /// </summary>
        [Conditional("DEBUG")]
        private void DebugTestAction()
        {
#if DEBUG
            var async = DebugTest.ExecuteTestAsync();
#endif
        }

        /// <summary>
        /// 開発用：フォルダーを開く
        /// </summary>
        [Conditional("DEBUG")]
        private void DebugOpenFolder(string path)
        {
            Debug.WriteLine($"OpenFolder: {path}");
            System.Diagnostics.Process.Start("explorer.exe", path);
        }
    }

    public class DebugMenuViewModel : BindableBase
    {
        private DebugWindow _debugWindow;

        private bool _isDebugWindowVisibled;
        public bool IsDebugWindowVisibled
        {
            get { return _isDebugWindowVisibled; }
            set
            {
                if (SetProperty(ref _isDebugWindowVisibled, value))
                {
                    if (_isDebugWindowVisibled)
                    {
                        if (_debugWindow == null)
                        {
                            _debugWindow = new DebugWindow(MainWindow.Current.ViewModel);
                            _debugWindow.Closed += (s, e) =>
                            {
                                _debugWindow = null;
                                IsDebugWindowVisibled = false;
                            };
                            _debugWindow.Show();
                        }
                    }
                    else
                    {
                        _debugWindow?.Close();
                        _debugWindow = null;
                    }
                }
            }
        }
    }

}
