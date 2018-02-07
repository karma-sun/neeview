// from Stack Overflow 
// URL: http://ja.stackoverflow.com/questions/5670/c%E3%81%AB%E3%81%A6%E3%82%A2%E3%83%97%E3%83%AA%E3%81%8B%E3%82%89%E3%83%89%E3%83%A9%E3%83%83%E3%82%B0%E3%83%89%E3%83%AD%E3%83%83%E3%83%97%E3%82%92%E5%8F%97%E3%81%91%E5%85%A5%E3%82%8C%E3%81%9F%E3%81%84%E3%81%AE%E3%81%A7%E3%81%99%E3%81%8C-filecontents%E3%81%AE%E7%B5%90%E6%9E%9C%E3%81%8Call-0%E3%81%AB%E3%81%AA%E3%81%A3%E3%81%A6%E3%81%97%E3%81%BE%E3%81%84%E3%81%BE%E3%81%99
// License: CC BY-SA 3.0
//
// 変更点：
//
// - System.Window.IDataObjectに対応
// - GetFileDescriptor() の format.lindex を -1 に変更

using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Runtime.InteropServices.ComTypes;
using FILETIME = System.Runtime.InteropServices.ComTypes.FILETIME;
using IComDataObject = System.Runtime.InteropServices.ComTypes.IDataObject;
using STATSTG = System.Runtime.InteropServices.ComTypes.STATSTG;

namespace NeeView.IO
{
    public class FileContents
    {
        private FileContents(string name, byte[] bytes)
        {
            Name = name;
            Bytes = bytes;
        }
        public string Name { get; private set; }
        public byte[] Bytes { get; private set; }

        private static readonly System.Windows.Forms.DataFormats.Format s_CFSTR_FILEDESCRIPTORW = System.Windows.Forms.DataFormats.GetFormat("FileGroupDescriptorW");
        private static readonly System.Windows.Forms.DataFormats.Format s_CFSTR_FILECONTENTS = System.Windows.Forms.DataFormats.GetFormat("FileContents");

        [DllImport("Kernel32.dll", SetLastError = true)]
        private static extern IntPtr GlobalAlloc(int uFlags, int dwBytes);
        [DllImport("Kernel32.dll")]
        private static extern IntPtr GlobalFree(IntPtr hMem);
        [DllImport("Kernel32.dll", SetLastError = true)]
        private static extern IntPtr GlobalLock(IntPtr hMem);
        [DllImport("Kernel32.dll", SetLastError = true)]
        private static extern IntPtr GlobalSize(IntPtr hMem);
        [DllImport("Kernel32.dll")]
        private static extern bool GlobalUnlock(IntPtr hMem);

        [StructLayout(LayoutKind.Sequential, Pack = 4, CharSet = CharSet.Unicode)]
        private struct FILEDESCRIPTORW
        {
            public int dwFlags;
            public Guid clsid;
            public long sizel;
            public long pointl;
            public int dwFileAttributes;
            public FILETIME ftCreationTime;
            public FILETIME ftLastAccessTime;
            public FILETIME ftLastWriteTime;
            public int nFileSizeHigh;
            public uint nFileSizeLow;
            [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 260)]
            public string cFileName;
        }

        [StructLayout(LayoutKind.Sequential, Pack = 4, CharSet = CharSet.Unicode)]
        private struct FILEGROUPDESCRIPTORW
        {
            public int cItems;
            [MarshalAs(UnmanagedType.ByValArray)]
            public FILEDESCRIPTORW[] fgd;
        }

        public static FileContents[] Get(System.Windows.IDataObject dataObject)
        {
            return Get((IComDataObject)dataObject);
        }

        public static FileContents[] Get(IComDataObject dataObject)
        {
            var fileDescriptor = GetFileDescriptor(dataObject);
            return fileDescriptor.fgd.Select((fgd, i) => new FileContents(fgd.cFileName, GetFileContent(dataObject, i))).ToArray();
        }

        private static FILEGROUPDESCRIPTORW GetFileDescriptor(IComDataObject dataObject)
        {
            var format = new FORMATETC
            {
                cfFormat = unchecked((short)s_CFSTR_FILEDESCRIPTORW.Id),
                dwAspect = DVASPECT.DVASPECT_CONTENT,
                ptd = IntPtr.Zero,
                lindex = -1,
                tymed = TYMED.TYMED_HGLOBAL
            };
            STGMEDIUM medium;
            dataObject.GetData(ref format, out medium);
            Debug.Assert(medium.tymed == TYMED.TYMED_HGLOBAL && medium.unionmember != IntPtr.Zero && medium.pUnkForRelease == null);
            try
            {
                return Marshal.PtrToStructure<FILEGROUPDESCRIPTORW>(GlobalLock(medium.unionmember));
            }
            finally
            {
                GlobalFree(medium.unionmember);
            }
        }

        private static byte[] GetFileContent(IComDataObject dataObject, int i)
        {
            var format = new FORMATETC
            {
                cfFormat = unchecked((short)s_CFSTR_FILECONTENTS.Id),
                dwAspect = DVASPECT.DVASPECT_CONTENT,
                ptd = IntPtr.Zero,
                lindex = i,
                tymed = TYMED.TYMED_HGLOBAL | TYMED.TYMED_ISTREAM
            };
            STGMEDIUM medium;
            dataObject.GetData(ref format, out medium);
            Debug.Assert(medium.unionmember != IntPtr.Zero && medium.pUnkForRelease == null);
            switch (medium.tymed)
            {
                case TYMED.TYMED_HGLOBAL:
                    {
                        var size = (long)GlobalSize(medium.unionmember);
                        Debug.Assert(size <= Int32.MaxValue);
                        var buffer = new byte[size];
                        Marshal.Copy(GlobalLock(medium.unionmember), buffer, 0, buffer.Length);
                        GlobalUnlock(medium.unionmember);
                        GlobalFree(medium.unionmember);
                        return buffer;
                    }
                case TYMED.TYMED_ISTREAM:
                    {
                        var stream = (IStream)Marshal.GetObjectForIUnknown(medium.unionmember);
                        Marshal.Release(medium.unionmember);
                        STATSTG statstg;
                        stream.Stat(out statstg, 0);
                        Debug.Assert(statstg.cbSize <= Int32.MaxValue);
                        var buffer = new byte[statstg.cbSize];
                        stream.Read(buffer, buffer.Length, IntPtr.Zero);
                        return buffer;
                    }
            }
            throw new Exception();
        }
    }
}

