using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using System.Runtime.InteropServices;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;

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
        }

        #endregion

        public static new App Current => (App)Application.Current;

        private bool _isSplashScreenVisibled;

        // 多重起動盛業
        private MultbootService _multiBootService;

        // プロセス間セマフォ
        private const string _semaphoreLabel = "NeeView.s0001";
        private Semaphore _semaphore;

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
        private async void Application_Startup(object sender, StartupEventArgs e)
        {
            Stopwatch = Stopwatch.StartNew();

            // DLL 検索パスから現在の作業ディレクトリ (CWD) を削除
            NativeMethods.SetDllDirectory("");

            try
            {
                await InitializeAsync(e);
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
        private async Task InitializeAsync(StartupEventArgs e)
        {
            this.ShutdownMode = ShutdownMode.OnExplicitShutdown;

            // コマンドライン引数処理
            this.Option = ParseArguments(e.Args);
            this.Option.Validate();

            // シフトキー起動は新しいウィンドウで
            if (Keyboard.IsKeyDown(Key.LeftShift) || Keyboard.IsKeyDown(Key.RightShift))
            {
                Option.IsNewWindow = SwitchOption.on;
            }

            // プロセス間セマフォ取得
            if (!Semaphore.TryOpenExisting(_semaphoreLabel, out _semaphore))
            {
                _semaphore = new Semaphore(1, 1, _semaphoreLabel);
            }

            // 多重起動サービス起動
            _multiBootService = new MultbootService();

            // セカンドプロセス判定
            Config.Current.IsSecondProcess = _multiBootService.IsServerExists;

            Debug.WriteLine($"App.UserSettingLoading: {Stopwatch.ElapsedMilliseconds}ms");

            // 設定ファイルの読み込み
            var setting = SaveData.Current.LoadUserSetting();

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

            // 多重起動制限になる場合、サーバーにパスを送って終了
            if (!CanStart())
            {
                await _multiBootService.RemoteLoadAsAsync(Option.StartupPlace);
                throw new ApplicationException("Already started.");
            }
        }

        /// <summary>
        /// Semaphore Wait
        /// </summary>
        public void SemaphoreWait()
        {
            _semaphore.WaitOne();
        }

        /// <summary>
        /// Semapnore Release
        /// </summary>
        public void SemaphoreRelease()
        {
            _semaphore.Release();
        }

        /// <summary>
        /// Show SplashScreen
        /// </summary>
        public void ShowSplashScreen()
        {
            if (IsSplashScreenEnabled && CanStart())
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
        /// 多重起動用実行可能判定
        /// </summary>
        private bool CanStart()
        {
            return !_multiBootService.IsServerExists || (Option.IsNewWindow != null ? Option.IsNewWindow == SwitchOption.on : IsMultiBootEnabled);
        }

        /// <summary>
        /// 終了処理
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Application_Exit(object sender, ExitEventArgs e)
        {
            ApplicationDisposer.Current.Dispose();

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
            ApplicationDisposer.Current.Dispose();
            Models.Current?.StopEngine();

            // 設定保存
            WindowShape.Current.CreateSnapMemento();
            SaveDataSync.Current.Flush();
            SaveDataSync.Current.SaveUserSetting(false);
            SaveDataSync.Current.SaveHistory();

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
