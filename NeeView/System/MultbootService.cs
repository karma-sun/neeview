using NeeLaboratory.IO;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
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


        public MultbootService(bool isCreateNew)
        {
            _currentProcess = Process.GetCurrentProcess();
            _serverProcess = isCreateNew ? null : GetServerProcess(_currentProcess);

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
            Trace.WriteLine($"GetServerProcess: CurrentProcess: ProcessName={processName}, Id={currentProcess.Id}");

            for (int retry = 0; retry < 2; ++retry)
            {
                var processes = Process.GetProcessesByName(processName)
                    .ToList();

                foreach (var p in processes)
                {
                    Trace.WriteLine($"GetServerProcess: FindProcess: ProcessName={p.ProcessName}, Id={p.Id}");
                }

                try
                {
                    // 自身以外のプロセスをターゲットにする
                    var serverProcess = processes
                        .LastOrDefault((p) => p.Id != currentProcess.Id);

                    if (serverProcess == null)
                    {
                        Trace.WriteLine($"GetServerProcess: ServerProcess not found.");
                    }
                    else
                    {
                        Trace.WriteLine($"GetServerProcess: ServerProcess: ProcessName={serverProcess.ProcessName}, Id={serverProcess.Id}");
                    }

                    return serverProcess;
                }
                catch (Exception ex)
                {
                    Trace.WriteLine(ex.Message);
                    Thread.Sleep(500);
                }
            }

            Trace.WriteLine($"GetServerProcess: ServerProcess not found from exception.");
            return null;
        }

        /// <summary>
        /// サーバーにパスを送る
        /// </summary>
        public async Task RemoteLoadAsAsync(List<string> files)
        {
            try
            {
                NativeMethods.AllowSetForegroundWindow(_serverProcess.Id);
                await RemoteCommandService.Current.SendAsync(new RemoteCommand("LoadAs", files.ToArray()), new RemoteCommandDelivery(_serverProcess.Id));
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
                if (command.Args != null && command.Args[0] != null)
                {
                    PlaylistBookLoader.Load(command.Args, true);
                }

                // ウィンドウをアクティブにする (実行)
                window.Activate();
            }
            catch { }
        }
    }
}
