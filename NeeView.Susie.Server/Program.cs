using NeeView.Native;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace NeeView.Susie.Server
{
    class Program
    {
        #region Native

        internal static class NativeMethods
        {
            [DllImport("kernel32", CharSet = CharSet.Unicode, SetLastError = true)]
            public static extern bool SetDllDirectory(string lpPathName);

            [DllImport("user32.dll", SetLastError = true, CharSet = CharSet.Auto)]
            public static extern int MessageBoxW(int hWnd, string text, string caption, uint type);
        }

        #endregion

        static void Main(string[] args)
        {

            // DLL 検索パスから現在の作業ディレクトリ (CWD) を削除
            NativeMethods.SetDllDirectory("");

            Interop.TryLoadNativeLibrary(".");


            Console.WriteLine($"I'm Server.");


            
            // TODO:
            new SusiePluginRemoteServer().Run();

            Console.WriteLine($"DONE.");

            ////NativeMethods.MessageBoxW(0, "This exe file is SusiePlugin server for NeeView. Don't run it.", "Caption", 0);
        }
    }

}
