using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace NeeView
{
    class WicDecoders
    {
        private class NativeMethods
        {
            [DllImport("kernel32", SetLastError = true, CharSet = CharSet.Auto)]
            private static extern IntPtr LoadLibrary([MarshalAs(UnmanagedType.LPTStr)] string lpFileName);

            [DllImport("NeeView.Interop.dll", CharSet = CharSet.Unicode)]
            [return: MarshalAs(UnmanagedType.I1)]
            public static extern bool NVGetImageCodecInfo(uint index, StringBuilder friendryName, StringBuilder fileExtensions);

            [DllImport("NeeView.Interop.dll")]
            public static extern void NVCloseImageCodecInfo();

            static NativeMethods()
            {
                TryLoadNativeLibrary(Config.Current.LibrariesPath);
            }

            private static bool TryLoadNativeLibrary(string path)
            {
                if (path == null)
                {
                    return false;
                }

                path = Path.Combine(path, IntPtr.Size == 4 ? "x86" : "x64");
                path = Path.Combine(path, "NeeView.Interop.dll");

                return File.Exists(path) && LoadLibrary(path) != IntPtr.Zero;
            }
        }

        /// <summary>
        /// Collect WIC Decoders
        /// </summary>
        /// <returns>friendlyName to fileExtensions dictionary</returns>
        public static Dictionary<string, string> ListUp()
        {
            var collection = new Dictionary<string, string>();

            var friendlyName = new StringBuilder(1024);
            var fileExtensions = new StringBuilder(1024);
            for (uint i = 0; NativeMethods.NVGetImageCodecInfo(i, friendlyName, fileExtensions); ++i)
            {
                ////Debug.WriteLine($"{friendryName}: {fileExtensions}");
                var key = friendlyName.ToString();
                if (collection.ContainsKey(key))
                {
                    collection[key] = collection[key].TrimEnd(',') + ',' + fileExtensions.ToString().ToLower();
                }
                else
                {
                    collection.Add(key, fileExtensions.ToString().ToLower());
                }
            }
            NativeMethods.NVCloseImageCodecInfo();

            return collection;
        }
    }
}
