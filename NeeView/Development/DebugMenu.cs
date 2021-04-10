using NeeLaboratory.ComponentModel;
using System;
using System.Diagnostics;
using System.Runtime;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;

namespace NeeView
{
#if DEBUG
    public class DebugMenu : BindableBase
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

        public MenuItem CreateDevMenuItem()
        {
            var top = new MenuItem() { Header = Properties.Resources.MenuTree_Debug };
            var collection = top.Items;

            var item = new MenuItem() { Header = "Debug Window", IsCheckable = true };
            item.SetBinding(MenuItem.IsCheckedProperty, new Binding(nameof(IsDebugWindowVisibled)) { Source = this });
            collection.Add(item);

            collection.Add(new Separator());

            item = new MenuItem() { Header = "Open application folder" };
            item.Click += MenuItemDevApplicationFolder_Click;
            collection.Add(item);

            item = new MenuItem() { Header = "Open data folder" };
            item.Click += MenuItemDevApplicationDataFolder_Click;
            collection.Add(item);

            item = new MenuItem() { Header = "Open current folder" };
            item.Click += MenuItemDevCurrentFolder_Click;
            collection.Add(item);

            item = new MenuItem() { Header = "Open temp folder" };
            item.Click += MenuItemDevTempFolder_Click;
            collection.Add(item);

            collection.Add(new Separator());

            item = new MenuItem() { Header = "Export Colors.xaml" };
            item.Click += MenuItemDevExportColorsXaml_Click;
            collection.Add(item);

            item = new MenuItem() { Header = "GC" };
            item.Click += MenuItemDevGC_Click;
            collection.Add(item);

            item = new MenuItem() { Header = "Go TEST" };
            item.Click += MenuItemDevButton_Click;
            collection.Add(item);

            return top;
        }

        // [開発用] Colors.xaml 出力
        private void MenuItemDevExportColorsXaml_Click(object sender, RoutedEventArgs e)
        {
            ThemeProfileTools.SaveColorsXaml(ThemeManager.Current.ThemeProfile, "Colors.xaml");
        }

        // [開発用] GCボタン
        private void MenuItemDevGC_Click(object sender, RoutedEventArgs e)
        {
            DebugGC();
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
            DebugOpenFolder(Environment.LocalApplicationDataPath);
        }

        // 開発用コマンド：カレントフォルダーを開く
        private void MenuItemDevCurrentFolder_Click(object sender, RoutedEventArgs e)
        {
            DebugOpenFolder(System.Environment.CurrentDirectory);
        }


        /// <summary>
        /// 開発用：GC
        /// </summary>
        [Conditional("DEBUG")]
        private void DebugGC()
        {
            GCSettings.LargeObjectHeapCompactionMode = GCLargeObjectHeapCompactionMode.CompactOnce;
            GC.Collect();
            GC.WaitForPendingFinalizers();
            GC.Collect();
        }

        /// <summary>
        /// 開発用：テストボタンのアクション
        /// </summary>
        [Conditional("DEBUG")]
        private void DebugTestAction()
        {
            var async = DebugTest.ExecuteTestAsync();
        }

        /// <summary>
        /// 開発用：フォルダーを開く
        /// </summary>
        [Conditional("DEBUG")]
        private void DebugOpenFolder(string path)
        {
            Debug.WriteLine($"OpenFolder: {path}");
            ExternalProcess.Start("explorer.exe", path);
        }
    }
#endif // DEBUG
}
