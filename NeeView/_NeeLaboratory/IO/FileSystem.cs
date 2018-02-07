// Copyright (c) 2015-2018 Mitsuhiro Ito (nee)
//
// This software is released under the MIT License.
// http://opensource.org/licenses/mit-license.php

using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Windows.Media.Imaging;

namespace NeeLaboratory.IO
{
    /// <summary>
    /// ファイル情報静的メソッド
    /// </summary>
    public class FileSystem
    {
        /// <summary>
        /// アイコンサイズ
        /// </summary>
        public enum IconSize
        {
            Small,
            Normal,
        };

        #region Win32API SHGetFileInfo
        // SHGetFileInfo関数
        [DllImport("shell32.dll", CharSet = CharSet.Auto)]
        private static extern IntPtr SHGetFileInfo(string pszPath, uint dwFileAttributes, ref SHFILEINFO psfi, uint cbSizeFileInfo, uint uFlags);

        // DestroyIcon関数
        [DllImport("user32.dll", CharSet = CharSet.Auto)]
        [return: MarshalAs(UnmanagedType.Bool)]
        private static extern bool DestroyIcon(IntPtr hIcon);

        // SHObjectProperties関数
        [DllImport("shell32.dll", CharSet = CharSet.Auto)]
        private static extern bool SHObjectProperties(IntPtr hwnd, uint shopObjectType, [MarshalAs(UnmanagedType.LPWStr)] string pszObjectName, [MarshalAs(UnmanagedType.LPWStr)] string pszPropertyPage);

        private const uint SHOP_PRINTERNAME = 0x1;
        private const uint SHOP_FILEPATH = 0x2;
        private const uint SHOP_VOLUMEGUID = 0x4;

        // SHGetFileInfo関数で使用するフラグ
        private const uint SHGFI_ICON = 0x100; // アイコン・リソースの取得
        private const uint SHGFI_LARGEICON = 0x0; // 大きいアイコン
        private const uint SHGFI_SMALLICON = 0x1; // 小さいアイコン
        private const uint SHGFI_TYPENAME = 0x400; //ファイルの種類

        private const uint SHGFI_USEFILEATTRIBUTES = 0x10; // fileAttributeを使用する

        //
        private const uint FILE_ATTRIBUTE_READONLY = 0x0001;
        private const uint FILE_ATTRIBUTE_HIDDEN = 0x0002;
        private const uint FILE_ATTRIBUTE_SYSTEM = 0x0004;
        private const uint FILE_ATTRIBUTE_DIRECTORY = 0x0010;
        private const uint FILE_ATTRIBUTE_ARCHIVE = 0x0020;
        private const uint FILE_ATTRIBUTE_ENCRYPTED = 0x0040;
        private const uint FILE_ATTRIBUTE_NORMAL = 0x0080;
        private const uint FILE_ATTRIBUTE_TEMPORARY = 0x0100;
        private const uint FILE_ATTRIBUTE_SPARSE_FILE = 0x0200;
        private const uint FILE_ATTRIBUTE_REPARSE_POINT = 0x0400;
        private const uint FILE_ATTRIBUTE_COMPRESSED = 0x0800;
        private const uint FILE_ATTRIBUTE_OFFLINE = 0x1000;

        // SHGetFileInfo関数で使用する構造体
        [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Auto)]
        private struct SHFILEINFO
        {
            public IntPtr hIcon;
            public IntPtr iIcon;
            public uint dwAttributes;
            [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 260)]
            public string szDisplayName;
            [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 80)]
            public string szTypeName;
        };
        #endregion

        #region Fields

        // 拡張子と対応するリソースはキャッシュして高速化する
        private static Dictionary<string, string> _typeNameDictionary = new Dictionary<string, string>();
        private static Dictionary<string, BitmapSource> _iconDictionary = new Dictionary<string, BitmapSource>();

        // フォルダーリソース
        private static string _folderTypeName;
        private static BitmapSource _folderIcon;

        // 標準ファイルリソース
        private static string _defaultTypeName;
        private static BitmapSource _defaultIcon;

        #endregion

        #region Methods

        /// <summary>
        /// ファイルの標準アイコンを準備
        /// </summary>
        private static void InitializeDefaultResource()
        {
            if (_folderTypeName != null) return;

            const string path = "__dummy_file__";
            _folderTypeName = GetTypeNameWithAttribute(path, FILE_ATTRIBUTE_DIRECTORY);
            _folderIcon = GetTypeIconSourceWithAttribute(path, IconSize.Small, FILE_ATTRIBUTE_DIRECTORY);
            _defaultTypeName = GetTypeNameWithAttribute(path, 0);
            _defaultIcon = GetTypeIconSourceWithAttribute(path, IconSize.Small, 0);
        }

        /// <summary>
        /// ファイルタイプ名取得 (高速版)
        /// </summary>
        /// <param name="path"></param>
        /// <param name="isDirectory"></param>
        /// <returns></returns>
        public static string CreateTypeName(string path, bool isDirectory)
        {
            InitializeDefaultResource();

            string typeName = null;

            if (isDirectory)
            {
                typeName = _folderTypeName;
            }
            else
            {
                string ext = System.IO.Path.GetExtension(path).ToLower();
                if (!string.IsNullOrEmpty(ext))
                {
                    if (!_typeNameDictionary.TryGetValue(ext, out typeName))
                    {
                        typeName = GetTypeNameWithAttribute(ext, 0);
                        if (typeName != null)
                        {
                            _typeNameDictionary.Add(ext, typeName);
                        }
                    }
                }
            }

            return typeName ?? _defaultTypeName;
        }


        /// <summary>
        /// アイコン取得  (高速版)
        /// 拡張子依存のアイコンを取得
        /// </summary>
        /// <param name="path"></param>
        /// <param name="isDirectory"></param>
        /// <returns></returns>
        public static BitmapSource CreateIcon(string path, bool isDirectory)
        {
            InitializeDefaultResource();

            BitmapSource icon = null;

            if (isDirectory)
            {
                icon = _folderIcon;
            }
            else
            {
                string ext = System.IO.Path.GetExtension(path).ToLower();
                if (!string.IsNullOrEmpty(ext))
                {
                    if (!_iconDictionary.TryGetValue(ext, out icon))
                    {
                        icon = GetTypeIconSourceWithAttribute(ext, IconSize.Small, 0);
                        if (icon != null)
                        {
                            _iconDictionary.Add(ext, icon);
                        }
                    }
                }
            }
            return icon ?? _defaultIcon;
        }


        /// <summary>
        /// ファイルの種類名を取得(Win32版)
        /// </summary>
        /// <param name="path"></param>
        /// <returns></returns>
        public static string GetTypeName(string path)
        {
            SHFILEINFO shfi = new SHFILEINFO();
            shfi.szDisplayName = "";
            shfi.szTypeName = "";

            IntPtr hSuccess = SHGetFileInfo(path, 0, ref shfi, (uint)Marshal.SizeOf(shfi), SHGFI_TYPENAME);
            if (!string.IsNullOrEmpty(shfi.szTypeName))
            {
                return shfi.szTypeName;
            }
            else
            {
                return null;
            }
        }

        /// <summary>
        /// ファイルの種類名を取得(Win32版)(USEFILEATTRIBUTES)
        /// </summary>
        /// <param name="path"></param>
        /// <returns></returns>
        public static string GetTypeNameWithAttribute(string path, uint attribute)
        {
            SHFILEINFO shfi = new SHFILEINFO();
            shfi.szDisplayName = "";
            shfi.szTypeName = "";

            IntPtr hSuccess = SHGetFileInfo(path, attribute, ref shfi, (uint)Marshal.SizeOf(shfi), SHGFI_TYPENAME | SHGFI_USEFILEATTRIBUTES);
            if (!string.IsNullOrEmpty(shfi.szTypeName))
            {
                return shfi.szTypeName;
            }
            else
            {
                return null;
            }
        }

        /// <summary>
        /// アプリケーション・アイコンを取得(Win32版)
        /// </summary>
        /// <param name="path"></param>
        /// <param name="iconSize"></param>
        /// <returns></returns>
        public static BitmapSource GetTypeIconSource(string path, IconSize iconSize)
        {
            SHFILEINFO shinfo = new SHFILEINFO();
            IntPtr hSuccess = SHGetFileInfo(path, 0, ref shinfo, (uint)Marshal.SizeOf(shinfo), SHGFI_ICON | (iconSize == IconSize.Small ? SHGFI_SMALLICON : SHGFI_LARGEICON));
            if (hSuccess != IntPtr.Zero)
            {
                BitmapSource bitmapSource = System.Windows.Interop.Imaging.CreateBitmapSourceFromHIcon(shinfo.hIcon, System.Windows.Int32Rect.Empty, BitmapSizeOptions.FromEmptyOptions());
                DestroyIcon(shinfo.hIcon);
                return bitmapSource;
            }
            else
            {
                return null;
            }
        }

        /// <summary>
        // アプリケーション・アイコンを取得(Win32版)(USEFILEATTRIBUTES)
        /// </summary>
        /// <param name="path"></param>
        /// <param name="iconSize"></param>
        /// <returns></returns>
        public static BitmapSource GetTypeIconSourceWithAttribute(string path, IconSize iconSize, uint attribute)
        {
            SHFILEINFO shinfo = new SHFILEINFO();
            IntPtr hSuccess = SHGetFileInfo(path, attribute, ref shinfo, (uint)Marshal.SizeOf(shinfo), SHGFI_ICON | (iconSize == IconSize.Small ? SHGFI_SMALLICON : SHGFI_LARGEICON) | SHGFI_USEFILEATTRIBUTES);
            if (hSuccess != IntPtr.Zero)
            {
                BitmapSource bitmapSource = System.Windows.Interop.Imaging.CreateBitmapSourceFromHIcon(shinfo.hIcon, System.Windows.Int32Rect.Empty, BitmapSizeOptions.FromEmptyOptions());
                DestroyIcon(shinfo.hIcon);
                return bitmapSource;
            }
            else
            {
                return null;
            }
        }

        /// <summary>
        /// ファイルサイズ取得
        /// </summary>
        /// <param name="path"></param>
        /// <returns></returns>
        public static long GetSize(string path)
        {
            var fileInfo = new System.IO.FileInfo(path);
            if ((fileInfo.Attributes & System.IO.FileAttributes.Directory) == System.IO.FileAttributes.Directory)
            {
                return -1;
            }
            else
            {
                return fileInfo.Length;
            }
        }

        /// <summary>
        /// ファイル更新日取得
        /// </summary>
        /// <param name="path"></param>
        /// <returns></returns>
        public static DateTime GetLastWriteTime(string path)
        {
            var fileInfo = new System.IO.FileInfo(path);
            return fileInfo.LastWriteTime;
        }

        /// <summary>
        /// プロパティウィンドウを開く
        /// </summary>
        /// <param name="path"></param>
        public static void OpenProperty(System.Windows.Window window, string path)
        {
            var handle = new System.Windows.Interop.WindowInteropHelper(window).Handle;

            if (!SHObjectProperties(handle, SHOP_FILEPATH, path, string.Empty))
            {
                throw new ApplicationException($"Cannot open file property window. {path}");
            }
        }

        #endregion
    }
}