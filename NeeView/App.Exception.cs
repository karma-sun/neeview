using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Threading;

namespace NeeView
{
    public partial class App
    {
        // 例外発生数
        private int _exceptionCount = 0;

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

            var errorLogFileName = System.IO.Path.Combine(Config.Current.LocalApplicationDataPath, "ErrorLog.txt");

            using (var writer = new StreamWriter(new FileStream(errorLogFileName, FileMode.Create, FileAccess.Write)))
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
            string message = $"エラーが発生しました。アプリを終了します。\n\n理由 : {exceptionMessage}\n\n次のファイルにエラーの詳細が出力されています。\n{errorLogFileName}";
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
