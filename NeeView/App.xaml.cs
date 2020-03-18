using NeeLaboratory.Diagnostics;
using NeeView.Native;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.IO;
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

        public static new App Current { get; private set; }

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

        // 起動日時
        public DateTime StartTime { get; private set; }

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
            Current = this;

            StartTime = DateTime.Now;
            Stopwatch = Stopwatch.StartNew();

            // DLL 検索パスから現在の作業ディレクトリ (CWD) を削除
            NativeMethods.SetDllDirectory("");

#if TRACE_LOG
            var nowTime = DateTime.Now;
            var traceLogFilename = $"Trace{nowTime.ToString("yyMMdHHmmss")}.log";
            StreamWriter sw = new StreamWriter(traceLogFilename) { AutoFlush = true };
            TextWriterTraceListener twtl = new TextWriterTraceListener(TextWriter.Synchronized(sw), "MyListener");
            Trace.Listeners.Add(twtl);
            Trace.WriteLine($"Trace: Start ({nowTime})");
#endif

            // 未処理例外ハンドル
            InitializeUnhandledException();

            try
            {
                await InitializeAsync(e);
            }
            catch (OperationCanceledException ex)
            {
                Trace.WriteLine("InitializeCancelException: " + ex.Message);
                Shutdown();
                return;
            }

            Trace.WriteLine($"App.Initialized: {Stopwatch.ElapsedMilliseconds}ms");

            // メインウィンドウ起動
            var mainWindow = new MainWindow();
            mainWindow.Initialize();

            Interop.NVFpReset();
            mainWindow.Show();

            MessageDialog.IsShowInTaskBar = false;
            this.ShutdownMode = ShutdownMode.OnMainWindowClose;
        }


        /// <summary>
        /// 初期化 
        /// </summary>
        private async Task InitializeAsync(StartupEventArgs e)
        {
            Interop.TryLoadNativeLibrary(Environment.LibrariesPath);

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
            _semaphore = new Semaphore(1, 1, _semaphoreLabel, out bool isCreateNew);

            // 多重起動サービス起動
            _multiBootService = new MultbootService(isCreateNew);

            // セカンドプロセス判定
            Environment.IsSecondProcess = _multiBootService.IsServerExists;

            Debug.WriteLine($"App.UserSettingLoading: {Stopwatch.ElapsedMilliseconds}ms");

            // 設定ファイル(V2)の読み込み (V2)
            var settingV2 = SaveData.Current.LoadConfig();
            Config.Current.Merge(settingV2.Config);

            // 設定ファイル(V1)の先行読み込み
            var setting = SaveData.Current.LoasUserSettingTemp();
            // 設定ファイル(V1)をConfigに適用 (互換性処理)
            setting.RestoreConfig();

            Debug.WriteLine($"App.UserSettingLoaded: {Stopwatch.ElapsedMilliseconds}ms");

            // restore
            RestoreOnce(setting.App);
            Restore(setting.App);

            // スプラッシュスクリーン(予備)
            ShowSplashScreen();

            // 言語適用
            NeeView.Properties.Resources.Culture = CultureInfo.GetCultureInfo(Config.Current.System.Language.GetCultureName());

            // バージョン表示
            if (this.Option.IsVersion)
            {
                var dialog = new VersionWindow() { WindowStartupLocation = WindowStartupLocation.CenterScreen };
                dialog.ShowDialog();
                throw new OperationCanceledException("Disp Version Dialog");
            }

            // 多重起動制限になる場合、サーバーにパスを送って終了
            Trace.WriteLine($"CanStart: {CanStart()}: IsServerExists={_multiBootService.IsServerExists}, IsNewWindow={Option.IsNewWindow}, IsMultiBootEnabled={Config.Current.StartUp.IsMultiBootEnabled}");
            if (!CanStart())
            {
                await _multiBootService.RemoteLoadAsAsync(Option.Values);
                throw new OperationCanceledException("Already started.");
            }

            // テンポラリーの場所
            TemporaryDirectory = Temporary.Current.SetDirectory(TemporaryDirectory);

            // キャッシュの場所
            InitializeCacheDirectory();

            // コマンド設定反映
            CommandTable.Current.RestoreCommandCollection(settingV2.CommandCollection);
        }

        /// <summary>
        /// キャッシュの場所の初期化
        /// </summary>
        public void InitializeCacheDirectory()
        {
            CacheDirectory = ThumbnailCache.Current.SetDirectory(CacheDirectory);
            if (CacheDirectory != CacheDirectoryOld)
            {
                try
                {
                    ThumbnailCache.Current.MoveDirectory(CacheDirectoryOld);
                }
                catch (Exception ex)
                {
                    Debug.WriteLine(ex.Message);
                }
            }
        }

        /// <summary>
        /// 各種データの場所情報の確定
        /// </summary>
        public void UpdateLocation()
        {
            TemporaryDirectory = Temporary.Current.TempRootPath;

            CacheDirectory = ThumbnailCache.Current.CacheFolderPath;
            CacheDirectoryOld = ThumbnailCache.Current.CacheFolderPath;
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
            if (Config.Current.StartUp.IsSplashScreenEnabled && CanStart())
            {
                if (_isSplashScreenVisibled) return;
                _isSplashScreenVisibled = true;
#if NEEVIEW_S
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
            return !_multiBootService.IsServerExists || (Option.IsNewWindow != null ? Option.IsNewWindow == SwitchOption.on : Config.Current.StartUp.IsMultiBootEnabled);
        }

        /// <summary>
        /// 終了処理
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Application_Exit(object sender, ExitEventArgs e)
        {
            DisableExceptionDialog();

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
            DisableUnhandledException();
            DisableExceptionDialog();

            ApplicationDisposer.Current.Dispose();

            // 設定保存
            WindowShape.Current.CreateSnapMemento();
            SaveDataSync.Current.Flush();
            SaveDataSync.Current.SaveUserSetting(false);
            SaveDataSync.Current.SaveHistory();
            SaveDataSync.Current.SaveBookmark(false);
            SaveDataSync.Current.SavePagemark(false);
            SaveDataSync.Current.RemoveBookmarkIfNotSave();
            SaveDataSync.Current.RemovePagemarkIfNotSave();

            // キャッシュ等削除
            CloseTemporary();
        }

        /// <summary>
        /// テンポラリ削除
        /// </summary>
        public void CloseTemporary()
        {
            // テンポラリファイル破棄
            Temporary.Current.RemoveTempFolder();

            // キャッシュDBを閉じる
            ThumbnailCache.Current.Dispose();
        }

        #endregion
    }
}
