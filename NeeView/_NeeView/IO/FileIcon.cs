using System;
using System.Windows.Media.Imaging;
using System.Runtime.InteropServices;
using System.Collections.Generic;
using System.Windows;
using System.Linq;

namespace NeeView.IO
{
    // from https://www.ipentec.com/document/csharp-shell-namespace-get-big-icon-from-file-path
    public class FileIcon
    {
        internal static class NativeMethods
        {
            [DllImport("shell32.dll")]
            public static extern IntPtr SHGetFileInfo(string pszPath, FILE_ATTRIBUTE dwFileAttribs, out SHFILEINFO psfi, uint cbFileInfo, SHGFI uFlags);

            [DllImport("shell32.dll", EntryPoint = "#727")]
            public static extern int SHGetImageList(int iImageList, ref Guid riid, ref IImageList ppv);

            //[DllImport("shell32.dll", EntryPoint = "#727")]
            //public static extern int SHGetImageList(SHIL iImageList, [MarshalAs(UnmanagedType.LPStruct)] Guid riid, ref IImageList ppv);

            [DllImport("shell32.dll", EntryPoint = "#727")]
            public static extern int SHGetImageList(SHIL iImageList, ref Guid riid, out IImageList ppv);

            //
            [DllImport("shell32.dll", EntryPoint = "#727")]
            public static extern int SHGetImageList(SHIL iImageList, ref Guid riid, out IntPtr ppv);

            [DllImport("comctl32.dll", SetLastError = true)]
            public static extern IntPtr ImageList_GetIcon(IntPtr himl, int i, int flags);

            // DestroyIcon関数
            [DllImport("user32.dll", CharSet = CharSet.Auto)]
            [return: MarshalAs(UnmanagedType.Bool)]
            public static extern bool DestroyIcon(IntPtr hIcon);

            //SHFILEINFO
            [Flags]
            public enum SHGFI
            {
                SHGFI_ICON = 0x000000100,
                SHGFI_DISPLAYNAME = 0x000000200,
                SHGFI_TYPENAME = 0x000000400,
                SHGFI_ATTRIBUTES = 0x000000800,
                SHGFI_ICONLOCATION = 0x000001000,
                SHGFI_EXETYPE = 0x000002000,
                SHGFI_SYSICONINDEX = 0x000004000,
                SHGFI_LINKOVERLAY = 0x000008000,
                SHGFI_SELECTED = 0x000010000,
                SHGFI_ATTR_SPECIFIED = 0x000020000,
                SHGFI_LARGEICON = 0x000000000,
                SHGFI_SMALLICON = 0x000000001,
                SHGFI_OPENICON = 0x000000002,
                SHGFI_SHELLICONSIZE = 0x000000004,
                SHGFI_PIDL = 0x000000008,
                SHGFI_USEFILEATTRIBUTES = 0x000000010,
                SHGFI_ADDOVERLAYS = 0x000000020,
                SHGFI_OVERLAYINDEX = 0x000000040
            };

            [Flags]
            public enum SHIL
            {
                SHIL_LARGE = 0x0000, // 32x32
                SHIL_SMALL = 0x0001, // 16x16
                SHIL_EXTRALARGE = 0x0002, // 48x48 maybe.
                ////SHIL_SYSSMALL = 0x0003, // ?
                SHIL_JUMBO = 0x0004, //256x256 maybe.
            }

            [Flags]
            public enum FILE_ATTRIBUTE
            {
                FILE_ATTRIBUTE_READONLY = 0x0001,
                FILE_ATTRIBUTE_HIDDEN = 0x0002,
                FILE_ATTRIBUTE_SYSTEM = 0x0004,
                FILE_ATTRIBUTE_DIRECTORY = 0x0010,
                FILE_ATTRIBUTE_ARCHIVE = 0x0020,
                FILE_ATTRIBUTE_ENCRYPTED = 0x0040,
                FILE_ATTRIBUTE_NORMAL = 0x0080,
                FILE_ATTRIBUTE_TEMPORARY = 0x0100,
                FILE_ATTRIBUTE_SPARSE_FILE = 0x0200,
                FILE_ATTRIBUTE_REPARSE_POINT = 0x0400,
                FILE_ATTRIBUTE_COMPRESSED = 0x0800,
                FILE_ATTRIBUTE_OFFLINE = 0x1000,
            }

            public struct SHFILEINFO
            {
                public IntPtr hIcon;
                public int iIcon;
                public uint dwAttributes;
                [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 260)]
                public string szDisplayName;
                [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 80)]
                public string szTypeName;
            }

            //IMAGE LIST
            public static Guid IID_IImageList = new Guid("46EB5926-582E-4017-9FDF-E8998DAA0950");
            public static Guid IID_IImageList2 = new Guid("192B9D83-50FC-457B-90A0-2B82A8B5DAE1");
            //Private Const IID_IImageList    As String = "{46EB5926-582E-4017-9FDF-E8998DAA0950}"
            //Private Const IID_IImageList2   As String = "{192B9D83-50FC-457B-90A0-2B82A8B5DAE1}"


            [StructLayout(LayoutKind.Sequential)]
            public struct POINT
            {
                int x;
                int y;
            }

            [StructLayout(LayoutKind.Sequential)]
            public struct RECT
            {
                public int left, top, right, bottom;
            }

            [StructLayout(LayoutKind.Sequential)]
            public struct IMAGEINFO
            {
                public IntPtr hbmImage;
                public IntPtr hbmMask;
                public int Unused1;
                public int Unused2;
                public RECT rcImage;
            }

            [StructLayout(LayoutKind.Sequential)]
            public struct IMAGELISTDRAWPARAMS
            {
                public int cbSize;
                public IntPtr himl;
                public int i;
                public IntPtr hdcDst;
                public int x;
                public int y;
                public int cx;
                public int cy;
                public int xBitmap;    // x offest from the upperleft of bitmap
                public int yBitmap;    // y offset from the upperleft of bitmap
                public int rgbBk;
                public int rgbFg;
                public int fStyle;
                public int dwRop;
                public int fState;
                public int Frame;
                public int crEffect;
            }

            [Flags]
            public enum ImageListDrawItemConstants : int
            {
                /// <summary>
                /// Draw item normally.
                /// </summary>
                ILD_NORMAL = 0x0,
                /// <summary>
                /// Draw item transparently.
                /// </summary>
                ILD_TRANSPARENT = 0x1,
                /// <summary>
                /// Draw item blended with 25% of the specified foreground colour
                /// or the Highlight colour if no foreground colour specified.
                /// </summary>
                ILD_BLEND25 = 0x2,
                /// <summary>
                /// Draw item blended with 50% of the specified foreground colour
                /// or the Highlight colour if no foreground colour specified.
                /// </summary>
                ILD_SELECTED = 0x4,
                /// <summary>
                /// Draw the icon's mask
                /// </summary>
                ILD_MASK = 0x10,
                /// <summary>
                /// Draw the icon image without using the mask
                /// </summary>
                ILD_IMAGE = 0x20,
                /// <summary>
                /// Draw the icon using the ROP specified.
                /// </summary>
                ILD_ROP = 0x40,
                /// <summary>
                /// ?
                /// </summary>
                ILD_OVERLAYMASK = 0xF00,
                /// <summary>
                /// Preserves the alpha channel in dest. XP only.
                /// </summary>
                ILD_PRESERVEALPHA = 0x1000, // 
                /// <summary>
                /// Scale the image to cx, cy instead of clipping it.  XP only.
                /// </summary>
                ILD_SCALE = 0x2000,
                /// <summary>
                /// Scale the image to the current DPI of the display. XP only.
                /// </summary>
                ILD_DPISCALE = 0x4000
            }


            // interface COM IImageList
            [ComImportAttribute()]
            [GuidAttribute("46EB5926-582E-4017-9FDF-E8998DAA0950")]
            [InterfaceTypeAttribute(ComInterfaceType.InterfaceIsIUnknown)]
            public interface IImageList
            {
                [PreserveSig]
                int Add(IntPtr hbmImage, IntPtr hbmMask, ref int pi);

                [PreserveSig]
                int ReplaceIcon(int i, IntPtr hicon, ref int pi);

                [PreserveSig]
                int SetOverlayImage(int iImage, int iOverlay);

                [PreserveSig]
                int Replace(int i, IntPtr hbmImage, IntPtr hbmMask);

                [PreserveSig]
                int AddMasked(IntPtr hbmImage, int crMask, ref int pi);

                [PreserveSig]
                int Draw(ref IMAGELISTDRAWPARAMS pimldp);

                [PreserveSig]
                int Remove(int i);

                [PreserveSig]
                int GetIcon(int i, int flags, ref IntPtr picon);
            };
        }

        public enum IconSize
        {
            Large = NativeMethods.SHIL.SHIL_LARGE,
            Small = NativeMethods.SHIL.SHIL_SMALL,
            ExtraLarge = NativeMethods.SHIL.SHIL_EXTRALARGE,
            Jumbo = NativeMethods.SHIL.SHIL_JUMBO,
        };

        public static BitmapSource CreateDirectoryTypeIcon(string filename, IconSize iconSize)
        {
            var currentshil = (NativeMethods.SHIL)iconSize;
            return CreateFileIcon(filename, currentshil, NativeMethods.FILE_ATTRIBUTE.FILE_ATTRIBUTE_DIRECTORY, NativeMethods.SHGFI.SHGFI_USEFILEATTRIBUTES);
        }

        public static BitmapSource CreateFileTypeIcon(string filename, IconSize iconSize)
        {
            var currentshil = (NativeMethods.SHIL)iconSize;
            return CreateFileIcon(System.IO.Path.GetExtension(filename), currentshil, 0, NativeMethods.SHGFI.SHGFI_USEFILEATTRIBUTES);
        }

        public static BitmapSource CreateFileIcon(string filename, IconSize iconSize)
        {
            var currentshil = (NativeMethods.SHIL)iconSize;
            return CreateFileIcon(filename, currentshil, 0, 0);
        }

        private static BitmapSource CreateFileIcon(string filename, NativeMethods.SHIL currentshil, NativeMethods.FILE_ATTRIBUTE attribute, NativeMethods.SHGFI flags)
        {
            NativeMethods.SHFILEINFO shinfo = new NativeMethods.SHFILEINFO();

            IntPtr hImg = NativeMethods.SHGetFileInfo(filename, attribute, out shinfo, (uint)Marshal.SizeOf(typeof(NativeMethods.SHFILEINFO)), NativeMethods.SHGFI.SHGFI_SYSICONINDEX | flags);

            NativeMethods.IImageList imglist = null;
            int rsult = NativeMethods.SHGetImageList(currentshil, ref NativeMethods.IID_IImageList, out imglist);

            IntPtr hicon = IntPtr.Zero;
            imglist.GetIcon(shinfo.iIcon, (int)NativeMethods.ImageListDrawItemConstants.ILD_TRANSPARENT, ref hicon);
            BitmapSource bitmapSource = System.Windows.Interop.Imaging.CreateBitmapSourceFromHIcon(hicon, System.Windows.Int32Rect.Empty, BitmapSizeOptions.FromEmptyOptions());
            NativeMethods.DestroyIcon(hicon);

            return bitmapSource;
        }
    }

}
