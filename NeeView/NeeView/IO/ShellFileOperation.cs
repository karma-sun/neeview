using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Windows;

namespace NeeView.IO
{
    public static class ShellFileOperation
    {
        private static class NativeMethods
        {
            public enum FileFuncFlags : uint
            {
                FO_MOVE = 0x1,
                FO_COPY = 0x2,
                FO_DELETE = 0x3,
                FO_RENAME = 0x4
            }

            [Flags]
            public enum FILEOP_FLAGS : ushort
            {
                FOF_MULTIDESTFILES = 0x1,
                FOF_CONFIRMMOUSE = 0x2,
                FOF_SILENT = 0x4,
                FOF_RENAMEONCOLLISION = 0x8,
                FOF_NOCONFIRMATION = 0x10,
                FOF_WANTMAPPINGHANDLE = 0x20,
                FOF_ALLOWUNDO = 0x40,
                FOF_FILESONLY = 0x80,
                FOF_SIMPLEPROGRESS = 0x100,
                FOF_NOCONFIRMMKDIR = 0x200,
                FOF_NOERRORUI = 0x400,
                FOF_NOCOPYSECURITYATTRIBS = 0x800,
                FOF_NORECURSION = 0x1000,
                FOF_NO_CONNECTED_ELEMENTS = 0x2000,
                FOF_WANTNUKEWARNING = 0x4000,
                FOF_NORECURSEREPARSE = 0x8000
            }

            //[StructLayout(LayoutKind.Sequential, CharSet=CharSet.Unicode)]
            //If you use the above you may encounter an invalid memory access exception (when using ANSI
            //or see nothing (when using unicode) when you use FOF_SIMPLEPROGRESS flag.
            [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Unicode)]
            public struct SHFILEOPSTRUCT
            {
                public IntPtr hwnd;
                public FileFuncFlags wFunc;
                [MarshalAs(UnmanagedType.LPWStr)]
                public string pFrom;
                [MarshalAs(UnmanagedType.LPWStr)]
                public string pTo;
                public FILEOP_FLAGS fFlags;
                [MarshalAs(UnmanagedType.Bool)]
                public bool fAnyOperationsAborted;
                public IntPtr hNameMappings;
                [MarshalAs(UnmanagedType.LPWStr)]
                public string lpszProgressTitle;
            }

            public const int DE_SAMEFILE = 0x71;
            public const int DE_MANYSRC1DEST = 0x72;
            public const int DE_DIFFDIR = 0x73;
            public const int DE_ROOTDIR = 0x74;
            public const int DE_OPCANCELLED = 0x75;
            public const int DE_DESTSUBTREE = 0x76;
            public const int DE_ACCESSDENIEDSRC = 0x78;
            public const int DE_PATHTOODEEP = 0x79;
            public const int DE_MANYDEST = 0x7A;
            public const int DE_INVALIDFILES = 0x7C;
            public const int DE_DESTSAMETREE = 0x7D;
            public const int DE_FLDDESTISFILE = 0x7E;
            public const int DE_FILEDESTISFLD = 0x80;
            public const int DE_FILENAMETOOLONG = 0x81;
            public const int DE_DEST_IS_CDROM = 0x82;
            public const int DE_DEST_IS_DVD = 0x83;
            public const int DE_DEST_IS_CDRECORD = 0x84;
            public const int DE_FILE_TOO_LARGE = 0x85;
            public const int DE_SRC_IS_CDROM = 0x86;
            public const int DE_SRC_IS_DVD = 0x87;
            public const int DE_SRC_IS_CDRECORD = 0x88;
            public const int DE_ERROR_MAX = 0xB7;
            public const int DE_ERROR_UNKNOWN = 0x402;
            public const int ERRORONDEST = 0x10000;
            public const int DE_DESTROOTDIR = 0x10074;

            public const int ERROR_CANCELLED = 0x04C7;

            [DllImport("shell32.dll", CharSet = CharSet.Unicode)]
            public static extern int SHFileOperation([In] ref SHFILEOPSTRUCT lpFileOp);

            [DllImport("kernel32.dll")]
            public static extern uint FormatMessage(uint dwFlags, IntPtr lpSource, uint dwMessageId, uint dwLanguageId, StringBuilder lpBuffer, int nSize, IntPtr Arguments);

            public const uint FORMAT_MESSAGE_FROM_SYSTEM = 0x00001000;
        }


        public static void Delete(Window owner, IEnumerable<string> paths, bool wantNukeWarning)
        {
            if (paths == null || !paths.Any()) throw new ArgumentException("Empty paths");

            var hwnd = owner != null
                ? new System.Windows.Interop.WindowInteropHelper(owner).Handle
                : IntPtr.Zero;

            var flags = NativeMethods.FILEOP_FLAGS.FOF_ALLOWUNDO | NativeMethods.FILEOP_FLAGS.FOF_NOCONFIRMATION;
            if (wantNukeWarning)
            {
                flags |= NativeMethods.FILEOP_FLAGS.FOF_WANTNUKEWARNING;
            }

            NativeMethods.SHFILEOPSTRUCT shfos;
            shfos.hwnd = hwnd;
            shfos.wFunc = NativeMethods.FileFuncFlags.FO_DELETE;
            shfos.pFrom = string.Join("\0", paths) + "\0\0";
            shfos.pTo = null;
            shfos.fFlags = flags;
            shfos.fAnyOperationsAborted = true;
            shfos.hNameMappings = IntPtr.Zero;
            shfos.lpszProgressTitle = null;

            var result = NativeMethods.SHFileOperation(ref shfos);

            Debug.WriteLine($"DeleteFile: Code=0x{result:x4}");

            switch (result)
            {
                case 0:
                    return;
                case NativeMethods.DE_OPCANCELLED:
                case NativeMethods.ERROR_CANCELLED:
                    throw new OperationCanceledException();

                case NativeMethods.DE_SAMEFILE:
                case NativeMethods.DE_MANYSRC1DEST:
                case NativeMethods.DE_DIFFDIR:
                case NativeMethods.DE_ROOTDIR:
                case NativeMethods.DE_DESTSUBTREE:
                case NativeMethods.DE_ACCESSDENIEDSRC:
                case NativeMethods.DE_PATHTOODEEP:
                case NativeMethods.DE_MANYDEST:
                case NativeMethods.DE_INVALIDFILES:
                case NativeMethods.DE_DESTSAMETREE:
                case NativeMethods.DE_FLDDESTISFILE:
                case NativeMethods.DE_FILEDESTISFLD:
                case NativeMethods.DE_FILENAMETOOLONG:
                case NativeMethods.DE_DEST_IS_CDROM:
                case NativeMethods.DE_DEST_IS_DVD:
                case NativeMethods.DE_DEST_IS_CDRECORD:
                case NativeMethods.DE_FILE_TOO_LARGE:
                case NativeMethods.DE_SRC_IS_CDROM:
                case NativeMethods.DE_SRC_IS_DVD:
                case NativeMethods.DE_SRC_IS_CDRECORD:
                case NativeMethods.DE_ERROR_MAX:
                case NativeMethods.DE_ERROR_UNKNOWN:
                case NativeMethods.ERRORONDEST:
                case NativeMethods.DE_DESTROOTDIR:
                    throw new IOException($"Code=0x{result:x4}");

                default:
                    StringBuilder message = new StringBuilder(1024);
                    var length = NativeMethods.FormatMessage(NativeMethods.FORMAT_MESSAGE_FROM_SYSTEM, IntPtr.Zero, (uint)result, 0, message, message.Capacity, IntPtr.Zero);
                    if (length > 0)
                    {
                        throw new IOException(message.ToString());
                    }
                    else
                    {
                        throw new IOException($"Code=0x{result:x4}");
                    }
            }
        }
    }

}
