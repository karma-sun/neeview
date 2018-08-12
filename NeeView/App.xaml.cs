using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Runtime.Remoting;
using System.Runtime.Remoting.Channels;
using System.Runtime.Remoting.Channels.Ipc;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using System.Windows.Threading;

namespace NeeView
{
    /// <summary>
    /// App.xaml の相互作用ロジック
    /// </summary>
    public partial class App : Application
    {
        #region Native

        internal static class NativeMethods
        {
            [DllImport("kernel32", CharSet = CharSet.Unicode, SetLastError = true)]
            public static extern bool SetDllDirectory(string lpPathName);

            [DllImport("user32.dll")]
            public static extern bool AllowSetForegroundWindow(int dwProcessId);
        }

        #endregion

        public static new App Current => (App)Application.Current;

        private bool _isSplashScreenVisibled;

        #region Properties

        // オプション設定
        public CommandLineOption Option { get; private set; }

        // システムロック
        public object Lock { get; } = new object();

        // 開発用：ストップウォッチ
        public Stopwatch Stopwatch { get; private set; }

        #endregion

        #region TickCount

        private int _tickBase = System.Environment.TickCount;

        /// <summary>
        /// アプリの起動時間(ms)取得
        /// </summary>
        public int TickCount => System.Environment.TickCount - _tickBase;

        #endregion

        #region Methods

        /// <summary>
        /// Show SplashScreen
        /// </summary>
        private void ShowSplashScreen()
        {
            if (_isSplashScreenVisibled) return;
            _isSplashScreenVisibled = true;

            Debug.WriteLine($"App.ShowSplashScreen: {Stopwatch.ElapsedMilliseconds}ms");

#if SUSIE
            var resourceName = "Resources/SplashScreenS.png";
#else
            var resourceName = "Resources/SplashScreen.png";
#endif
            SplashScreen splashScreen = new SplashScreen(resourceName);
            splashScreen.Show(true);
        }

        /// <summary>
        /// Startup
        /// </summary>
        private void Application_Startup(object sender, StartupEventArgs e)
        {
            Stopwatch = Stopwatch.StartNew();

            try
            {
                // 初期化
                Initialize(e);
            }
            catch (Exception ex)
            {
                Debug.WriteLine("InitializeException: " + ex.Message);
                Shutdown();
                return;
            }

            Debug.WriteLine($"App.Initialized: {Stopwatch.ElapsedMilliseconds}ms");

            // メインウィンドウ起動
            var mainWindow = new MainWindow();
            mainWindow.Initialize();
            mainWindow.Show();

            MessageDialog.IsShowInTaskBar = false;
            this.ShutdownMode = ShutdownMode.OnMainWindowClose;
        }


        // 初期化
        private void Initialize(StartupEventArgs e)
        {
            this.ShutdownMode = ShutdownMode.OnExplicitShutdown;

            // DLL 検索パスから現在の作業ディレクトリ (CWD) を削除
            NativeMethods.SetDllDirectory("");

            // 環境初期化
            Config.Current.Initiallize();

            // コマンドライン引数処理
            this.Option = ParseArguments(e.Args);
            this.Option.Validate();

            Debug.WriteLine($"App.UserSettingLoading: {Stopwatch.ElapsedMilliseconds}ms");

            // 設定ファイルの読み込み
            new SaveData();
            SaveData.Current.LoadSetting(Option.SettingFilename);
            var setting = SaveData.Current.UserSetting;

            Debug.WriteLine($"App.UserSettingLoaded: {Stopwatch.ElapsedMilliseconds}ms");

            // restore
            Restore(setting.App);
            RestoreCompatible(setting);

            // 言語適用
            NeeView.Properties.Resources.Culture = CultureInfo.GetCultureInfo(Language.GetCultureName());

            // バージョン表示
            if (this.Option.IsVersion)
            {
                var dialog = new VersionWindow() { WindowStartupLocation = WindowStartupLocation.CenterScreen };
                dialog.ShowDialog();
                throw new ApplicationException("Disp Version Dialog");
            }


            // シフトキー起動は新しいウィンドウで
            if (Keyboard.IsKeyDown(Key.LeftShift) || Keyboard.IsKeyDown(Key.RightShift))
            {
                Option.IsNewWindow = SwitchOption.on;
            }

            bool isNewWindow = Option.IsNewWindow != null ? Option.IsNewWindow == SwitchOption.on : IsMultiBootEnabled;


            // 多重起動チェック
            Process currentProcess = Process.GetCurrentProcess();

            // 自身と異なるプロセスを見つけ、サーバとする
            var processName = currentProcess.ProcessName;
#if DEBUG
            var processes = Process.GetProcessesByName(processName)
                .Concat(Process.GetProcessesByName(Path.GetFileNameWithoutExtension(currentProcess.ProcessName)))
                .ToList();
#else
                var processes = Process.GetProcessesByName(processName)
                    .ToList();
#endif

            // 最も古いプロセスを残す
            var serverProcess = processes
                .OrderByDescending((p) => p.StartTime)
                .FirstOrDefault((p) => p.Id != currentProcess.Id);

            // セカンドプロセス判定
            Config.Current.IsSecondProcess = serverProcess != null;

            // Single起動
            if (!isNewWindow)
            {
                if (serverProcess != null)
                {
                    try
                    {
                        NativeMethods.AllowSetForegroundWindow(serverProcess.Id);
                        // IPCクライアント送信
                        IpcRemote.LoadAs(serverProcess.Id, Option.StartupPlace);
                    }
                    catch (Exception ex)
                    {
                        Debug.WriteLine(ex.Message);
                        serverProcess = null;
                    }
                }

                if (serverProcess != null)
                {
                    // 起動を中止してプログラムを終了
                    throw new ApplicationException("Already started.");
                }
            }

            ShowSplashScreen();

            // IPCサーバ起動
            IpcRemote.BootServer(currentProcess.Id);
        }



        /// <summary>
        /// 終了処理
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Application_Exit(object sender, ExitEventArgs e)
        {
            // IPC シャットダウン
            Debug.WriteLine("IpcServer_Shutdown");
            IpcRemote.Shutdown();

            // プロセスを確実に終了させるための保険
            Task.Run(() =>
            {
                System.Threading.Thread.Sleep(5000);
                Debug.WriteLine("Environment_Exit");
                lock (this.Lock)
                {
                    System.Environment.Exit(0);
                }
            });

            Debug.WriteLine("Application_Exit");
        }

#endregion
    }
}
