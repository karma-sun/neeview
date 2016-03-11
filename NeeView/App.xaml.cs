using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.Remoting;
using System.Runtime.Remoting.Channels;
using System.Runtime.Remoting.Channels.Ipc;
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
        private int _ExceptionCount = 0;

        // 起動持の引数として渡されたパス
        public static string StartupPlace { get; set; }

        // ユーザー設定ファイル名
        public static string UserSettingFileName { get; set; }


        /// <summary>
        /// Startup
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Application_Startup(object sender, StartupEventArgs e)
        {
            ShutdownMode = ShutdownMode.OnMainWindowClose;


            // カレントフォルダをアプリの場所に再設定
            var assembly = Assembly.GetEntryAssembly();
            Environment.CurrentDirectory = Path.GetDirectoryName(assembly.Location);

            // 引数チェック
            foreach (string arg in e.Args)
            {
                StartupPlace = arg.Trim();
            }


            // 設定ファイル名
            UserSettingFileName = Path.Combine(Environment.CurrentDirectory, "UserSetting.xml");

            // 設定読み込み
            bool isDisableMultiBoot = false;
            try
            {
                var setting = Setting.Load(UserSettingFileName);
                isDisableMultiBoot = setting.ViewMemento.IsDisableMultiBoot;
            }
            catch { }

            // 多重起動チェック
            Process currentProcess = Process.GetCurrentProcess();
            if (isDisableMultiBoot && (Keyboard.Modifiers & ModifierKeys.Shift) != ModifierKeys.Shift)
            {
                // 自身と異なるプロセスを見つけ、サーバとする
                Process[] processes = Process.GetProcessesByName(currentProcess.ProcessName);

#if DEBUG
                processes = processes.Concat(Process.GetProcessesByName(Path.GetFileNameWithoutExtension(currentProcess.ProcessName))).ToArray();
#endif

                var serverProcess = processes.OrderBy((p) => p.StartTime).Reverse().FirstOrDefault((p) => p.Id != currentProcess.Id);

                if (serverProcess != null)
                {
                    // IPCクライアント送信
                    IpcRemote.LoadAs(serverProcess.Id, StartupPlace);

                    // 起動を中止してプログラムを終了
                    this.Shutdown();
                    return;
                }
            }

            // IPCサーバ起動
            IpcRemote.BootServer(currentProcess.Id);

            // メインウィンドウ起動
            var mainWindow = new MainWindow();
            mainWindow.Show();
        }


        /// <summary>
        /// クリティカルなエラーの処理
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        public void Application_DispatcherUnhandledException(object sender, DispatcherUnhandledExceptionEventArgs e)
        {
            if (++_ExceptionCount >= 2)
            {
                Debug.WriteLine($"AfterException({_ExceptionCount}): {e.Exception.Message}");
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
    }
}
