using System.Collections;
using System.Collections.Generic;
using System.Runtime.InteropServices;

namespace NeeView.Text
{
    /// <summary>
    /// Win32API での自然ソート
    /// </summary>
    public class NativeNaturalComparer : IComparer<string>, IComparer
    {
        private static class NativeMethods
        {
            [DllImport("shlwapi.dll", CharSet = CharSet.Unicode)]
            public static extern int StrCmpLogicalW(string psz1, string psz2);
        }


        public int Compare(string x, string y)
        {
            return NativeMethods.StrCmpLogicalW(x, y);
        }

        public int Compare(object x, object y)
        {
            return Compare(x as string, y as string);
        }
    }

}
