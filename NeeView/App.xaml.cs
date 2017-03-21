using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Diagnostics;
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
        // 例外発生数
        private int _exceptionCount = 0;

        // コマンドラインオプション
        private static OptionParser _OptionParser { get; set; } = new OptionParser();
        public static Dictionary<string, OptionUnit> Options => _OptionParser.Options;

        // 起動持の引数として渡されたパス
        public static string StartupPlace { get; set; }

        // ユーザー設定ファイル名
        public static string UserSettingFileName { get; set; }

        // ユーザ設定
        public static Setting Setting { get; set; }

        // アプリの環境設定
        public static Config Config { get; set; }


        // コマンドラインヘルプ(未使用)
        private string HelpText
        {
            get
            {
                var assembly = System.Reflection.Assembly.GetExecutingAssembly();
                var ver = FileVersionInfo.GetVersionInfo(assembly.Location);

                string exe = System.IO.Path.GetFileName(assembly.Location);
                string text = "\n";
                text += $"{ assembly.GetName().Name} { ver.FileMajorPart}.{ ver.FileMinorPart}\n";
                text += $"\nUsage: {exe} [options...] [ImageFile]\n\n";
                text += _OptionParser.HelpText + "\n";
                text += $"例:\n  {exe} --setting=\"C:\\Hoge\\CustomUserSetting.xml\" --new-window=off\n";
                text += $"例:\n  {exe} --fullscreen --slideshow\n";
                text += "\n";

                return text;
            }
        }


        /// <summary>
        /// Startup
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Application_Startup(object sender, StartupEventArgs e)
        {
#if DEBUG
            ////InitializeException();
#endif

            // 環境初期化
            Config = new Config();
            Config.Initialize();

            //
            ShutdownMode = ShutdownMode.OnMainWindowClose;

            try
            {
                _OptionParser.AddOption("--setting", OptionType.FileName, "設定ファイル(UserSetting.xml)のパスを指定します");
                _OptionParser.AddOption("--reset-placement", OptionType.None, "ウィンドウ座標を初期化します");
                _OptionParser.AddOption("--blank", OptionType.None, "画像ファイルを開かずに起動します");
                _OptionParser.AddOption("--new-window", OptionType.Bool, "新しいウィンドウで起動するかを指定します");
                _OptionParser.AddOption("--fullscreen", OptionType.Bool, "フルスクリーンで起動するかを指定します");
                _OptionParser.AddOption("--slideshow", OptionType.Bool, "スライドショウを開始するかを指定します");
                _OptionParser.Parse(e.Args);
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "起動オプションエラー", MessageBoxButton.OK, MessageBoxImage.Error);
                this.Shutdown(1);
                return;
            }

            foreach (string arg in _OptionParser.Args)
            {
                StartupPlace = arg.Trim();
            }


            // カレントフォルダー設定
            System.Environment.CurrentDirectory = Config.LocalApplicationDataPath;

            // 設定ファイル名
            if (Options["--setting"].IsValid)
            {
                var filename = _OptionParser.Options["--setting"].Value;
                if (File.Exists(filename))
                {
                    UserSettingFileName = Path.GetFullPath(filename);
                }
                else
                {
                    MessageBox.Show("指定された設定ファイルが存在しません\n\n" + filename, "起動オプションエラー", MessageBoxButton.OK, MessageBoxImage.Error);
                    this.Shutdown(1);
                    return;
                }
            }
            else
            {
                UserSettingFileName = Path.Combine(System.Environment.CurrentDirectory, "UserSetting.xml");
            }


            // 設定読み込み
            LoadSetting();

            // 多重起動チェック
            Process currentProcess = Process.GetCurrentProcess();

            bool isNewWindow = (Keyboard.Modifiers & ModifierKeys.Shift) == ModifierKeys.Shift
                || Options["--new-window"].IsValid ? Options["--new-window"].Bool : !Setting.ViewMemento.IsDisableMultiBoot;

            if (!isNewWindow)
            {
                // 自身と異なるプロセスを見つけ、サーバとする

                // x64版は NeeView64 という名前の可能性がある
                var processName = currentProcess.ProcessName.Replace("64", "");
                var processes = Process.GetProcessesByName(processName)
                    .Concat(Process.GetProcessesByName(processName + "64"));
#if DEBUG
                processes = processes
                    .Concat(Process.GetProcessesByName(Path.GetFileNameWithoutExtension(currentProcess.ProcessName)));
#endif

                // 最も古いプロセスを残す
                var serverProcess = processes
                    .OrderByDescending((p) => p.StartTime)
                    .FirstOrDefault((p) => p.Id != currentProcess.Id);

                if (serverProcess != null)
                {
                    try
                    {
                        Win32Api.AllowSetForegroundWindow(serverProcess.Id);
                        // IPCクライアント送信
                        IpcRemote.LoadAs(serverProcess.Id, StartupPlace);

                        // 起動を中止してプログラムを終了
                        this.Shutdown();
                        return;
                    }
                    catch (Exception ex)
                    {
                        Debug.WriteLine("起動を継続します。\n 理由：" + ex.Message);
                    }
                }
            }

            // IPCサーバ起動
            IpcRemote.BootServer(currentProcess.Id);

            // アプリ共通資源初期化
            ModelContext.Initialize();

            // メインウィンドウ起動
            var mainWindow = new MainWindow();
            mainWindow.Show();
        }


        // 設定ファイル読み込み
        public static void LoadSetting()
        {
            // 設定の読み込み
            if (System.IO.File.Exists(UserSettingFileName))
            {
                try
                {
                    Setting = Setting.Load(UserSettingFileName);
                }
                catch (Exception e)
                {
                    Debug.WriteLine(e.Message);
                    MessageBox.Show("設定の読み込みに失敗しました。初期設定で起動します。", "エラー", MessageBoxButton.OK, MessageBoxImage.Warning);
                    Setting = new Setting();
                }
            }
            else
            {
                Setting = new Setting();
            }
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
                System.Environment.Exit(0);
            });

            Debug.WriteLine("Application_Exit");
        }


        /// <summary>
        /// クリティカルなエラーの処理
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        public void Application_DispatcherUnhandledException(object sender, DispatcherUnhandledExceptionEventArgs e)
        {
            if (++_exceptionCount >= 2)
            {
                Debug.WriteLine($"AfterException({_exceptionCount}): {e.Exception.Message}");
                e.Handled = true;
                return;
            }

            using (var stream = new FileStream("ErrorLog.txt", FileMode.Create, FileAccess.Write))
            using (var writer = new StreamWriter(stream))
            {
                writer.WriteLine($"{DateTime.Now}\n");

                Action<Exception, StreamWriter> WriteException = (exception, sw) =>
                {
                    sw.WriteLine($"ExceptionType:\n  {exception.GetType()}");
                    sw.WriteLine($"ExceptionMessage:\n  {exception.Message}");
                    sw.WriteLine($"ExceptionStackTrace:\n{exception.StackTrace}");
                };

                WriteException(e.Exception, writer);

                Exception ex = e.Exception.InnerException;
                while (ex != null)
                {
                    writer.WriteLine("\n\n-------- InnerException --------\n");
                    WriteException(ex, writer);
                    ex = ex.InnerException;
                }
            }

            string exceptionMessage = e.Exception is System.Reflection.TargetInvocationException ? e.Exception.InnerException?.Message : e.Exception.Message;
            string message = $"エラーが発生しました。アプリを終了します。\n\n理由 : {exceptionMessage}\n\nErrorLog.txtにエラーの詳細が出力されています。この内容を開発者に報告してください。";
            MessageBox.Show(message, "強制終了", MessageBoxButton.OK, MessageBoxImage.Error);

#if DEBUG
#else
            e.Handled = true;

            this.Shutdown();
#endif
        }



#if DEBUG
        /// <summary>
        /// 全ての最終例外をキャッチ
        /// </summary>
        private void InitializeException()
        {
            // 全ての最終例外をキャッチ
            AppDomain.CurrentDomain.UnhandledException += CurrentDomain_UnhandledException;
        }

        /// <summary>
        /// 例外取得
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void CurrentDomain_UnhandledException(object sender, UnhandledExceptionEventArgs e)
        {
            var exception = e.ExceptionObject as Exception;
            if (exception == null)
            {
                MessageBox.Show("System.Exceptionとして扱えない例外");
                return;
            }
            else
            {
                Debug.WriteLine($"*** {exception.Message}");
            }

        }
#endif
    }
}
