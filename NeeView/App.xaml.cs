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

        Process _currentProcess;
        Process _serverProcess;
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

            NVInterop.NVFpReset();
            mainWindow.Show();

            MessageDialog.IsShowInTaskBar = false;
            this.ShutdownMode = ShutdownMode.OnMainWindowClose;
        }


        /// <summary>
        /// 初期化 
        /// </summary>
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

            // シフトキー起動は新しいウィンドウで
            if (Keyboard.IsKeyDown(Key.LeftShift) || Keyboard.IsKeyDown(Key.RightShift))
            {
                Option.IsNewWindow = SwitchOption.on;
            }

            // プロセス取得
            _currentProcess = Process.GetCurrentProcess();
            _serverProcess = GetServerProcess(_currentProcess);

            // セカンドプロセス判定
            Config.Current.IsSecondProcess = _serverProcess != null;

            Debug.WriteLine($"App.UserSettingLoading: {Stopwatch.ElapsedMilliseconds}ms");

            // 設定ファイルの読み込み
            new SaveData();
            SaveData.Current.LoadSetting(Option.SettingFilename);
            var setting = SaveData.Current.UserSetting;

            Debug.WriteLine($"App.UserSettingLoaded: {Stopwatch.ElapsedMilliseconds}ms");

            // restore
            Restore(setting.App);
            RestoreCompatible(setting);

            // スプラッシュスクリーン(予備)
            ShowSplashScreen();

            // 言語適用
            NeeView.Properties.Resources.Culture = CultureInfo.GetCultureInfo(Language.GetCultureName());

            // バージョン表示
            if (this.Option.IsVersion)
            {
                var dialog = new VersionWindow() { WindowStartupLocation = WindowStartupLocation.CenterScreen };
                dialog.ShowDialog();
                throw new ApplicationException("Disp Version Dialog");
            }

            // MultiBoot?
            if (!IsNewWindow())
            {
                try
                {
                    NativeMethods.AllowSetForegroundWindow(_serverProcess.Id);
                    // IPCクライアント送信
                    IpcRemote.LoadAs(_serverProcess.Id, Option.StartupPlace);
                }
                catch (Exception ex)
                {
                    Debug.WriteLine(ex.Message);
                    _serverProcess = null;
                }

                // 起動を中止してプログラムを終了
                throw new ApplicationException("Already started.");
            }

            // IPCサーバ起動
            IpcRemote.BootServer(_currentProcess.Id);
        }

        /// <summary>
        /// Show SplashScreen
        /// </summary>
        public void ShowSplashScreen()
        {
            if (IsSplashScreenEnabled && IsNewWindow())
            {
                if (_isSplashScreenVisibled) return;
                _isSplashScreenVisibled = true;
#if SUSIE
                var resourceName = "Resources/SplashScreenS.png";
#else
                var resourceName = "Resources/SplashScreen.png";
#endif
                SplashScreen splashScreen = new SplashScreen(resourceName);
                splashScreen.Show(true);
                Debug.WriteLine($"App.ShowSplashScreen: {Stopwatch.ElapsedMilliseconds}ms");
            }
        }

        /// <summary>
        /// 他のプロセスを検索
        /// </summary>
        private Process GetServerProcess(Process currentProcess)
        {
            var processName = currentProcess.ProcessName;

#if DEBUG
            var processes = Process.GetProcessesByName(processName)
                .Concat(Process.GetProcessesByName(Path.GetFileNameWithoutExtension(currentProcess.ProcessName)))
                .ToList();
#else
                var processes = Process.GetProcessesByName(processName)
                    .ToList();
#endif

            // 最も古いプロセスをターゲットにする
            var serverProcess = processes
                .OrderByDescending((p) => p.StartTime)
                .FirstOrDefault((p) => p.Id != currentProcess.Id);

            return serverProcess;
        }

        /// <summary>
        /// 新しいウィンドウの作成？
        /// </summary>
        private bool IsNewWindow()
        {
            return _serverProcess == null || (Option.IsNewWindow != null ? Option.IsNewWindow == SwitchOption.on : IsMultiBootEnabled);
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

        /// <summary>
        /// シャットダウン時に呼ばれる
        /// </summary>
        private void Application_SessionEnding(object sender, SessionEndingCancelEventArgs e)
        {
            // 設定保存
            WindowShape.Current.CreateSnapMemento();
            SaveData.Current.SaveAll();

            // キャッシュ等削除
            CloseTemporary();
        }


        /// <summary>
        /// テンポラリ削除
        /// </summary>
        public void CloseTemporary()
        {
            // テンポラリファイル破棄
            Temporary.RemoveTempFolder();

            // キャッシュDBを閉じる
            ThumbnailCache.Current.Dispose();
        }

        #endregion
    }
}
