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
        private async void DebugTestAction()
        {
            try
            {
                await DebugCreateBookThumbnail.TestAsync();
                return;

                // 致命的エラーのテスト
                ////InnerExcepionTest();

                // アーカイブのアンロック
                ////await Task.Run(() => BookOperation.Current.Unlock());

                ////ページマーク多数登録テスト
                ////Models.Current.BookOperation.Test_MakeManyPagemark();

                GC.Collect();
                GC.WaitForPendingFinalizers();
                await Task.Delay(1000);
                Debugger.Break();
                //ModelContext.CommandTable.OpenCommandListHelp();
                //Config.Current.RemoveApplicationData();
            }
            catch (Exception ex)
            {
                throw new ApplicationException("DEBUG error", ex);
            }
        }

        [Conditional("DEBUG")]
        private void InnerExcepionTest()
        {
            throw new ApplicationException("Exception test");
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

}
