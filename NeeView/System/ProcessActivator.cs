using System;
using System.Diagnostics;
using System.Linq;
using System.Runtime.InteropServices;

namespace NeeView
{
    public static class ProcessActivator
    {
        private static class NativeMethods
        {
            [DllImport("user32.dll")]
            [return: MarshalAs(UnmanagedType.Bool)]
            public static extern bool ShowWindow(IntPtr hWnd, int nCmdShow);

            [DllImport("user32.dll")]
            [return: MarshalAs(UnmanagedType.Bool)]
            public static extern bool ShowWindowAsync(IntPtr hWnd, int nCmdShow);

            public const int SW_SHOWNORMAL = 1;
            public const int SW_SHOW = 5;
            public const int SW_RESTORE = 9;

            [DllImport("user32.dll")]
            [return: MarshalAs(UnmanagedType.Bool)]
            public static extern bool IsIconic(IntPtr hWnd);
        }

        public static void NextActivate(int direction)
        {
            var currentProcess = Process.GetCurrentProcess();

            // collect NeeView processes
            var processes = Process.GetProcesses().Where(e => e.ProcessName == currentProcess.ProcessName).OrderBy(e => e.StartTime).ToList();

            // 自身を基準として並び替え。自身は削除する
            var index = processes.FindIndex(e => e.Id == currentProcess.Id);
            processes = processes.Skip(index).Concat(processes.Take(index)).Where(e => e.Id != currentProcess.Id).ToList();

            var process = (direction > 0) ? processes.FirstOrDefault() : processes.LastOrDefault();
            AppActivate(process);
        }

        public static void AppActivate(Process process)
        {
            if (process == null) return;

            // アクティブにする
            Microsoft.VisualBasic.Interaction.AppActivate(process.Id);

            // ウィンドウが最小化されている場合は元に戻す
            var hWnd = process.MainWindowHandle;
            if (NativeMethods.IsIconic(hWnd))
            {
                NativeMethods.ShowWindowAsync(hWnd, NativeMethods.SW_RESTORE);
            }
        }
    }

}
