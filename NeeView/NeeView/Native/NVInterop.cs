﻿using System;
using System.IO;
using System.Runtime.InteropServices;
using System.Text;

namespace NeeView.Native
{
    /// <summary>
    /// for NeeView.Interop.dll
    /// </summary>
    public class Interop
    {
        private class NativeMethods
        {
            [DllImport("kernel32", SetLastError = true, CharSet = CharSet.Unicode)]
            public static extern IntPtr LoadLibrary(string lpFileName);

            [DllImport("NeeView.Interop.dll", CharSet = CharSet.Unicode)]
            [return: MarshalAs(UnmanagedType.I1)]
            public static extern bool NVGetImageCodecInfo(uint index, StringBuilder friendryName, StringBuilder fileExtensions);

            [DllImport("NeeView.Interop.dll")]
            public static extern void NVCloseImageCodecInfo();

            [DllImport("NeeView.Interop.dll")]
            public static extern void NVFpReset();

            [DllImport("NeeView.Interop.dll", CharSet = CharSet.Unicode)]
            [return: MarshalAs(UnmanagedType.I1)]
            public static extern bool NVGetFullPathFromShortcut([MarshalAs(UnmanagedType.LPWStr)] string shortcut, StringBuilder fullPath);


#if false // FPU設定のテスト用
            [DllImport("msvcrt.dll", CallingConvention = CallingConvention.Cdecl)]
            public extern static uint _controlfp(uint newcw, uint mask);

            public const uint _MCW_EM = 0x0008001f;
            public const uint EM_INVALID = 0x00000010;
            public const uint EM_DENORMAL = 0x00080000;
            public const uint EM_ZERODIVIDE = 0x00000008;
            public const uint EM_OVERFLOW = 0x00000004;
            public const uint EM_UNDERFLOW = 0x00000002;
            public const uint EM_INEXACT = 0x00000001;

            // FPUのリセット
            public static void FixFPU()
            {
                //_controlfp(_MCW_EM, EM_INVALID | EM_ZERODIVIDE);
                _controlfp(EM_INEXACT | EM_ZERODIVIDE | EM_OVERFLOW | EM_OVERFLOW | EM_DENORMAL | EM_INVALID, _MCW_EM);
            }
#endif
        }

        public static bool TryLoadNativeLibrary(string path)
        {
            if (path == null)
            {
                return false;
            }

            path = Path.Combine(path, IntPtr.Size == 4 ? "x86" : "x64");
            path = Path.Combine(path, "NeeView.Interop.dll");

            return File.Exists(path) && NativeMethods.LoadLibrary(path) != IntPtr.Zero;
        }

        public static bool NVGetImageCodecInfo(uint index, StringBuilder friendryName, StringBuilder fileExtensions)
        {
            return NativeMethods.NVGetImageCodecInfo(index, friendryName, fileExtensions);
        }

        public static void NVCloseImageCodecInfo()
        {
            NativeMethods.NVCloseImageCodecInfo();
        }

        public static void NVFpReset()
        {
            NativeMethods.NVFpReset();
        }

        public static bool NVGetFullPathFromShortcut(string shortcut, StringBuilder fullPath)
        {
            return NativeMethods.NVGetFullPathFromShortcut(shortcut, fullPath);
        }

    }
}
