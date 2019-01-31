using NeeLaboratory.IO;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace NeeView
{
    public class MultbootService
    {
        #region Native

        internal static class NativeMethods
        {
            [DllImport("user32.dll")]
            public static extern bool AllowSetForegroundWindow(int dwProcessId);
        }

        #endregion


        private Process _currentProcess;
        private Process _serverProcess;


        public MultbootService()
        {
            _currentProcess = Process.GetCurrentProcess();
            _serverProcess = GetServerProcess(_currentProcess);

            RemoteCommandService.Current.AddReciever("LoadAs", LoadAs);
        }

        /// <summary>
        /// サーバの存在チェック
        /// </summary>
        public bool IsServerExists => _serverProcess != null;


        /// <summary>
        /// サーバープロセスを検索
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

            // 自身以外の最も新しいプロセスをターゲットにする
            var serverProcess = processes
                .OrderByDescending((p) => p.StartTime)
                .FirstOrDefault((p) => p.Id != currentProcess.Id);

            return serverProcess;
        }

        /// <summary>
        /// サーバーにパスを送る
        /// </summary>
        public void RemoteLoadAs(string path)
        {
            try
            {
                NativeMethods.AllowSetForegroundWindow(_serverProcess.Id);
                RemoteCommandService.Current.Send(new RemoteCommand("LoadAs", path), new RemoteCommandDelivery(_serverProcess.Id));
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex);
            }
        }

        /// <summary>
        /// リモートコマンド(LoadAs)
        /// </summary>
        private void LoadAs(RemoteCommand command)
        {
            try
            {
                var path = command.Args[0];

                // ウィンドウをアクティブにする (準備)
                // 最小化されているならば解除する
                var window = Application.Current.MainWindow;
                if (window.WindowState == WindowState.Minimized) window.WindowState = WindowState.Normal;

                // ウィンドウをアクティブにする (準備)
                // 一瞬TOPMOSTにする
                var temp = window.Topmost;
                window.Topmost = true;
                window.Topmost = temp;

                // パスの指定があれば開く
                if (path != null)
                {
                    BookHub.Current.RequestLoad(path, null, BookLoadOption.None, true);
                }

                // ウィンドウをアクティブにする (実行)
                window.Activate();
            }
            catch { }
        }
    }
}
