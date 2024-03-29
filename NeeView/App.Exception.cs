﻿using NeeView.Threading;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Threading;

namespace NeeView
{
    public partial class App
    {
        // 未処理例外発生数
        private int _exceptionCount = 0;

        // 未処理例の外排他処理
        private object _exceptionLock = new object();

        // ダイアログ通知有効
        private bool _isExceptionDialogEnabled = true;

        /// <summary>
        /// ダイアログ通知の無効化
        /// </summary>
        private void DisableExceptionDialog()
        {
            _isExceptionDialogEnabled = false;
        }

        /// <summary>
        /// 全ての未処理例外をキャッチするハンドル登録
        /// </summary>
        private void InitializeUnhandledException()
        {
            AppDomain.CurrentDomain.UnhandledException += CurrentDomain_UnhandledException;
        }

        public void DisableUnhandledException()
        {
            AppDomain.CurrentDomain.UnhandledException -= CurrentDomain_UnhandledException;
        }

        /// <summary>
        /// 未処理例外の処理
        /// </summary>
        private void CurrentDomain_UnhandledException(object sender, UnhandledExceptionEventArgs e)
        {
            var exception = e.ExceptionObject as Exception;
            if (exception == null)
            {
                return;
            }

            lock (_exceptionLock)
            {
                _exceptionCount++;
                Debug.WriteLine($"UnhandledException({_exceptionCount}): {exception.Message}");

                if (_exceptionCount >= 2)
                {
                    return;
                }

                SaveData.Current.IsEnableSave = false; // 保存禁止

                string errorLog;
                using (var writer = new StringWriter())
                {
                    writer.WriteLine("OS Version: " + System.Environment.OSVersion + (Environment.IsX64 ? " (64bit)" : " (32bit)"));
                    writer.WriteLine("NeeView Version: " + Environment.DispVersion + $" ({Environment.PackageType})");
                    writer.WriteLine("");
                    writer.WriteLine(exception.ToStackString());
                    
                    errorLog = writer.ToString();
                }

                var errorLogFileName = System.IO.Path.Combine(Environment.LocalApplicationDataPath, "ErrorLog.txt");
                using (var writer = new StreamWriter(new FileStream(errorLogFileName, FileMode.Create, FileAccess.Write)))
                {
                    writer.Write(errorLog);
                }

                if (_isExceptionDialogEnabled)
                {
                    try
                    {
                        var task = new Task(() =>
                        {
                            var dialog = new CriticalErrorDialog(errorLog, errorLogFileName);
                            dialog.ShowInTaskbar = true;
                            dialog.ShowDialog();
                        });
                        task.Start(SingleThreadedApartment.TaskScheduler);
                        task.Wait();
                    }
                    catch
                    {
                        MessageBox.Show(errorLog, "Abort", MessageBoxButton.OK, MessageBoxImage.Hand);
                    }
                }
            }
        }
    }
}
