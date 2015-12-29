using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Threading;

namespace NeeView
{
    /// <summary>
    /// App.xaml の相互作用ロジック
    /// </summary>
    public partial class App : Application
    {
        public void Application_DispatcherUnhandledException(object sender, DispatcherUnhandledExceptionEventArgs e)
        {
            string message = $"エラーが発生しました。アプリを終了します。\n\n理由 : {e.Exception.Message}";
            MessageBox.Show(message, "強制終了", MessageBoxButton.OK, MessageBoxImage.Error);

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

            e.Handled = true;

            this.Shutdown();
        }
    }
}
