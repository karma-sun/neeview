// Copyright (c) 2016-2017 Mitsuhiro Ito (nee)
//
// This software is released under the MIT License.
// http://opensource.org/licenses/mit-license.php

using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace Susie
{
    public static class Win32Api
    {
        private const string KERNEL32 = "kernel32";

        [DllImport(KERNEL32)]
        public extern static IntPtr LoadLibrary(string lpFileName);

        [DllImport(KERNEL32)]
        public extern static bool FreeLibrary(IntPtr hModule);

        [DllImport(KERNEL32, CharSet = CharSet.Ansi)]
        public extern static IntPtr GetProcAddress(IntPtr hModule, string lpProcName);

        [DllImport(KERNEL32)]
        public extern static IntPtr LocalLock(IntPtr hMem);

        [DllImport(KERNEL32)]
        public extern static bool LocalUnlock(IntPtr hMem);

        [DllImport(KERNEL32)]
        public extern static IntPtr LocalFree(IntPtr hMem);

        [DllImport(KERNEL32)]
        public extern static uint LocalSize(IntPtr hMem);

        [DllImport(KERNEL32, CharSet = CharSet.Auto)]
        public extern static int GetShortPathName(string longPath, StringBuilder shortPathBuffer, int bufferSize);

        // ショートパス名を求める
        public static string GetShortPathName(string longPath)
        {
            int bufferSize = 260;
            StringBuilder shortPathBuffer = new StringBuilder(bufferSize);
            Win32Api.GetShortPathName(longPath, shortPathBuffer, bufferSize);
            string shortPath = shortPathBuffer.ToString();
            return shortPath;
        }
    }


    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public struct BitmapFileHeader
    {
        public ushort bfType;
        public uint bfSize;
        public ushort bfReserved1;
        public ushort bfReserved2;
        public uint bfOffBits;
    }

    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public struct BitmapInfoHeader
    {
        public uint biSize;
        public int biWidth;
        public int biHeight;
        public ushort biPlanes;
        public ushort biBitCount;
        public uint biCompression;
        public uint biSizeImage;
        public int biXPelsPerMeter;
        public int biYPelsPerMeter;
        public uint biClrUsed;
        public uint biClrImportant;
    }
}
