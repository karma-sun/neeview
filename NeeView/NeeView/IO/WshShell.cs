using System;

namespace NeeView.IO
{
    /// <summary>
    /// Windows の Shell アクセスインターフェイス (dynamic)
    /// </summary>
    public class WshShell
    {
        public static WshShell Current { get; private set; } = new WshShell();

        public WshShell()
        {
            var type = Type.GetTypeFromProgID("WScript.Shell");
            Shell = Activator.CreateInstance(type);
        }

        public dynamic Shell { get; }

        ~WshShell()
        {
            if (Shell != null)
            {
                System.Runtime.InteropServices.Marshal.FinalReleaseComObject(Shell);
            }
        }
    }
}
