// Copyright (c) 2016 Mitsuhiro Ito (nee)
//
// This software is released under the MIT License.
// http://opensource.org/licenses/mit-license.php

// 参考：
// http://myugaru.wankuma.com/

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media.Imaging;

namespace Susie
{
    /// <summary>
    /// Susie Plugin API
    /// アンマネージなDLLアクセスを行います。
    /// </summary>
    public class SusiePluginApi : IDisposable
    {
        [DllImport("msvcrt.dll", CallingConvention = CallingConvention.Cdecl)]
        private extern static uint _controlfp(uint newcw, uint mask);

        private const uint _MCW_EM = 0x0008001f;
        public const uint EM_INVALID = 0x00000010;
        public const uint EM_DENORMAL = 0x00080000;
        public const uint EM_ZERODIVIDE = 0x00000008;
        public const uint EM_OVERFLOW = 0x00000004;
        public const uint EM_UNDERFLOW = 0x00000002;
        public const uint EM_INEXACT = 0x00000001;

        // FPUのリセット
        private static void FixFPU()
        {
            // add desired values
            _controlfp(_MCW_EM, EM_INVALID);
        }

        // DLLハンドル
        public IntPtr hModule { get; private set; } = IntPtr.Zero;

        // APIデリゲートリスト
        private Dictionary<Type, object> _apiDelegateList = new Dictionary<Type, object>();

        /// <summary>
        /// プラグインをロードし、使用可能状態にする
        /// </summary>
        /// <param name="fileName">spiファイル名</param>
        /// <returns>プラグインインターフェイス</returns>
        public static SusiePluginApi Create(string fileName)
        {
            var lib = new SusiePluginApi();
            lib.Open(fileName);
            if (lib == null) throw new ArgumentException("not support " + fileName);
            return lib;
        }

        /// <summary>
        /// DLLロードし、使用可能状態にする
        /// </summary>
        /// <param name="fileName">spiファイル名</param>
        /// <returns>DLLハンドル</returns>
        private IntPtr Open(string fileName)
        {
            Close();
            hModule = Win32Api.LoadLibrary(fileName);
            return hModule;
        }

        /// <summary>
        /// DLLをアンロードする
        /// </summary>
        private void Close()
        {
            if (hModule != IntPtr.Zero)
            {
                _apiDelegateList.Clear();
                Win32Api.FreeLibrary(hModule);
                hModule = IntPtr.Zero;

                FixFPU();
            }
        }

        #region IDisposable Support
        private bool _disposedValue = false; // 重複する呼び出しを検出するには

        //
        protected virtual void Dispose(bool disposing)
        {
            if (!_disposedValue)
            {
                if (disposing)
                {
                    // ここで、マネージ状態を破棄します (マネージ オブジェクト)。
                }

                // ここで、アンマネージ リソース (アンマネージ オブジェクト) を解放し、下のファイナライザーをオーバーライドします。
                // ここで、大きなフィールドを null に設定します。
                Close();

                _disposedValue = true;
            }
        }

        // 上の Dispose(bool disposing) にアンマネージ リソースを解放するコードが含まれる場合にのみ、ファイナライザーをオーバーライドします。
        ~SusiePluginApi()
        {
            // このコードを変更しないでください。クリーンアップ コードを上の Dispose(bool disposing) に記述します。
            Dispose(false);
        }

        // このコードは、破棄可能なパターンを正しく実装できるように追加されました。
        public void Dispose()
        {
            // このコードを変更しないでください。クリーンアップ コードを上の Dispose(bool disposing) に記述します。
            Dispose(true);
            // 上のファイナライザーがオーバーライドされる場合は、次の行のコメントを解除してください。
            GC.SuppressFinalize(this);
        }
        #endregion


        /// <summary>
        /// APIの存在確認
        /// </summary>
        /// <param name="name">API名</param>
        /// <returns>trueなら存在する</returns>
        public bool IsExistFunction(string name)
        {
            if (hModule == null) throw new InvalidOperationException();

            IntPtr add = Win32Api.GetProcAddress(hModule, name);
            return (add != IntPtr.Zero);
        }


        /// <summary>
        /// API取得
        /// </summary>
        /// <typeparam name="T">APIのデリゲート</typeparam>
        /// <param name="procName">API名</param>
        /// <returns></returns>
        public T GetApiDelegate<T>(string procName)
        {
            if (!_apiDelegateList.ContainsKey(typeof(T)))
            {
                IntPtr add = Win32Api.GetProcAddress(hModule, procName);
                if (add == IntPtr.Zero) throw new NotSupportedException("not support " + procName);
                _apiDelegateList.Add(typeof(T), Marshal.GetDelegateForFunctionPointer<T>(add));
            }

            return (T)_apiDelegateList[typeof(T)];
        }



        // callback delegate
        private delegate int ProgressCallback(int nNum, int nDenom, int lData);

        /// <summary>
        /// Dummy Callback
        /// </summary>
        /// <param name="nNum"></param>
        /// <param name="nDenom"></param>
        /// <param name="lData"></param>
        /// <returns></returns>
        private static int ProgressCallbackDummy(int nNum, int nDenom, int lData)
        {
            return 0;
        }


        #region 00IN,00AM 必須 GetPluginInfo
        private delegate int GetPluginInfoDelegate(int infono, StringBuilder buf, int len);

        /// <summary>
        /// Plug-inに関する情報を得る
        /// </summary>
        /// <param name="infono">取得する情報番号</param>
        /// <returns>情報の文字列。情報番号が無効の場合はnullを返す</returns>
        public string GetPluginInfo(int infono)
        {
            if (hModule == null) throw new InvalidOperationException();
            var getPluginInfo = GetApiDelegate<GetPluginInfoDelegate>("GetPluginInfo");

            StringBuilder strb = new StringBuilder(256);
            int ret = getPluginInfo(infono, strb, strb.Capacity);
            if (ret <= 0) return null;
            return strb.ToString();
        }
        #endregion


        #region 00IN,00AM 任意 ConfigurationDlg()
        private delegate int ConfigurationDlgDelegate(IntPtr parent, int fnc);


        /// <summary>
        /// Plug-in設定ダイアログの表示 
        /// </summary>
        /// <param name="parent">親ウィンドウのウィンドウハンドル</param>
        /// <param name="func">0:aboutダイアログ / 1:設定ダイアログ</param>
        /// <returns>0なら正常終了、それ以外はエラーコードを返す</returns>
        public int ConfigurationDlg(IntPtr parent, int func)
        {
            if (hModule == null) throw new InvalidOperationException();
            var configurationDlg = GetApiDelegate<ConfigurationDlgDelegate>("ConfigurationDlg");

            return configurationDlg(parent, func);
        }
        #endregion

        #region 00IN,00AM 必須 IsSupported()
        private delegate bool IsSupportedFromFileDelegate(string filename, IntPtr dw);
        private delegate bool IsSupportedFromMemoryDelegate(string filename, [In]byte[] dw);

        /// <summary>
        /// サポート判定(ファイル版)
        /// 注意：Susie本体はこの関数(ファイル版)を使用していないため、正常に動作しないプラグインが存在します！
        /// </summary>
        /// <param name="filename">ファイル名</param>
        /// <returns>サポートしていればtrue</returns>
        public bool IsSupported(string filename)
        {
            if (hModule == null) throw new InvalidOperationException();
            var isSupported = GetApiDelegate<IsSupportedFromFileDelegate>("IsSupported");

            using (FileStream fs = new FileStream(filename, FileMode.Open, FileAccess.Read))
            {
                return isSupported(filename, fs.SafeFileHandle.DangerousGetHandle());
            }
        }

        /// <summary>
        /// サポート判定(メモリ版)
        /// </summary>
        /// <param name="filename">ファイル名(判定用)</param>
        /// <param name="buff">対象データ</param>
        /// <returns>サポートしていればtrue</returns>
        public bool IsSupported(string filename, byte[] buff)
        {
            if (hModule == null) throw new InvalidOperationException();
            var isSupported = GetApiDelegate<IsSupportedFromMemoryDelegate>("IsSupported");

            return isSupported(filename, buff);
        }
        #endregion


        #region 00AM 必須 GetArchiveInfo()
        private delegate int GetArchiveInfoFromFileDelegate([In]string filename, int offset, uint flag, out IntPtr hInfo);
        private delegate int GetArchiveInfoFromMemoryDelegate([In]byte[] buf, int offset, uint flag, out IntPtr hInfo);

        /// <summary>
        /// アーカイブ情報取得
        /// </summary>
        /// <param name="file">アーカイブファイル名</param>
        /// <returns>アーカイブエントリ情報(RAW)。失敗した場合はnull</returns>
        public List<ArchiveFileInfoRaw> GetArchiveInfo(string file)
        {
            if (hModule == null) throw new InvalidOperationException();
            var getArchiveInfo = GetApiDelegate<GetArchiveInfoFromFileDelegate>("GetArchiveInfo");

            IntPtr hInfo = IntPtr.Zero;
            try
            {
                int ret = getArchiveInfo(file, 0, 0, out hInfo);
                if (ret == 0)
                {
                    var list = new List<ArchiveFileInfoRaw>();

                    IntPtr p = Win32Api.LocalLock(hInfo);
                    while (true)
                    {
                        ArchiveFileInfoRaw fileInfo = Marshal.PtrToStructure<ArchiveFileInfoRaw>(p);
                        if (String.IsNullOrEmpty(fileInfo.method)) break;
                        if (fileInfo.filesize > 0)
                        {
                            list.Add(fileInfo);
                        }
                        p += Marshal.SizeOf<ArchiveFileInfoRaw>();
                    }

                    return list;
                }
            }
            finally
            {
                Win32Api.LocalUnlock(hInfo);
                Win32Api.LocalFree(hInfo);
            }

            return null;
        }
        #endregion


        #region 00AM 必須 GetFile()
        private delegate int GetFileFromFileHandler(string filename, int position, out IntPtr hBuff, uint flag, ProgressCallback lpPrgressCallback, int lData);
        private delegate int GetFileFromFileToFileHandler(string filename, int position, string dest, uint flag, ProgressCallback lpPrgressCallback, int lData);

        /// <summary>
        /// アーカイブエントリ取得(メモリ版)
        /// </summary>
        /// <param name="file">アーカイブファイル名</param>
        /// <param name="entry">アーカイブエントリ名</param>
        /// <returns>出力されたバッファ。失敗した場合はnull</returns>
        public byte[] GetFile(string file, ArchiveFileInfoRaw entry)
        {
            if (hModule == null) throw new InvalidOperationException();
            var getFile = GetApiDelegate<GetFileFromFileHandler>("GetFile");

            IntPtr hBuff = IntPtr.Zero;
            try
            {
                int ret = getFile(file, (int)entry.position, out hBuff, 0x0100, ProgressCallbackDummy, 0); // 0x0100 > File To Memory
                if (ret == 0)
                {
                    IntPtr pBuff = Win32Api.LocalLock(hBuff);
                    byte[] buf = new byte[entry.filesize];
                    Marshal.Copy(pBuff, buf, (int)0, (int)entry.filesize);
                    return buf;
                }
                return null;
            }
            finally
            {
                Win32Api.LocalUnlock(hBuff);
                Win32Api.LocalFree(hBuff);
            }
        }

        /// <summary>
        /// アーカイブエントリ取得(ファイル版)
        /// </summary>
        /// <param name="file">アーカイブファイル名</param>
        /// <param name="entry">アーカイブエントリ名</param>
        /// <param name="extractFolder">出力フォルダ</param>
        /// <returns>成功した場合は0</returns>
        public int GetFile(string file, ArchiveFileInfoRaw entry, string extractFolder)
        {
            if (hModule == null) throw new InvalidOperationException();
            var getFile = GetApiDelegate<GetFileFromFileToFileHandler>("GetFile");

            return getFile(file, (int)entry.position, extractFolder, 0x0000, ProgressCallbackDummy, 0); // 0x0000 > File To File
        }

        #endregion


        #region 00IN 必須 GetPicture()
        private delegate int GetPictureFromMemoryDelegate([In]byte[] buf, int len, uint flag, out IntPtr pHBInfo, out IntPtr pHBm, ProgressCallback lpProgressCallback, int lData);
        private delegate int GetPictureFromFileDelegate([In]string filename, int offset, uint flag, out IntPtr pHBInfo, out IntPtr pHBm, ProgressCallback lpProgressCallback, int lData);



        /// <summary>
        /// 画像取得(メモリ版)
        /// </summary>
        /// <param name="buff">入力画像データ</param>
        /// <returns>BitmapSource。失敗した場合はnull</returns>
        public BitmapSource GetPicture(byte[] buff)
        {
            if (hModule == null) throw new InvalidOperationException();
            var getPicture = GetApiDelegate<GetPictureFromMemoryDelegate>("GetPicture");

            IntPtr pHBInfo = IntPtr.Zero;
            IntPtr pHBm = IntPtr.Zero;
            try
            {
                int ret = getPicture(buff, buff.Length, 0x01, out pHBInfo, out pHBm, ProgressCallbackDummy, 0);
                if (ret == 0)
                {
                    IntPtr pBInfo = Win32Api.LocalLock(pHBInfo);
                    IntPtr pBm = Win32Api.LocalLock(pHBm);
                    return CraeteBitmapImage(pBInfo, pBm);
                }
                return null;
            }
            finally
            {
                Win32Api.LocalUnlock(pHBInfo);
                Win32Api.LocalUnlock(pHBm);
                Win32Api.LocalFree(pHBInfo);
                Win32Api.LocalFree(pHBm);
            }
        }


        /// <summary>
        /// 画像取得(ファイル版)
        /// </summary>
        /// <param name="filename">入力ファイル名</param>
        /// <returns>BitmapSource。失敗した場合はnull</returns>
        public BitmapSource GetPicture(string filename)
        {
            if (hModule == null) throw new InvalidOperationException();
            var getPicture = GetApiDelegate<GetPictureFromFileDelegate>("GetPicture");

            IntPtr pHBInfo = IntPtr.Zero;
            IntPtr pHBm = IntPtr.Zero;
            try
            {
                int ret = getPicture(filename, 0, 0x00, out pHBInfo, out pHBm, ProgressCallbackDummy, 0);
                if (ret == 0)
                {
                    IntPtr pBInfo = Win32Api.LocalLock(pHBInfo);
                    IntPtr pBm = Win32Api.LocalLock(pHBm);
                    return CraeteBitmapImage(pBInfo, pBm);
                }
                return null;
            }
            finally
            {
                Win32Api.LocalUnlock(pHBInfo);
                Win32Api.LocalUnlock(pHBm);
                Win32Api.LocalFree(pHBInfo);
                Win32Api.LocalFree(pHBm);
            }
        }

        // BitmapImage 作成
        private BitmapSource CraeteBitmapImage(IntPtr pBInfo, IntPtr pBm)
        {
            var bi = Marshal.PtrToStructure<BitmapInfoHeader>(pBInfo);
            var bf = CreateBitmapFileHeader(bi);

            byte[] mem = new byte[bf.bfSize];
            GCHandle gch = GCHandle.Alloc(mem, GCHandleType.Pinned);
            try { Marshal.StructureToPtr<BitmapFileHeader>(bf, gch.AddrOfPinnedObject(), false); }
            finally { gch.Free(); }
            Marshal.Copy(pBInfo, mem, Marshal.SizeOf(bf), (int)bf.bfOffBits - Marshal.SizeOf(bf));
            Marshal.Copy(pBm, mem, (int)bf.bfOffBits, (int)(bf.bfSize - bf.bfOffBits));

            using (MemoryStream ms = new MemoryStream(mem))
            {
                try
                {
                    var bitmapFrame = BitmapFrame.Create(ms, BitmapCreateOptions.None, BitmapCacheOption.OnLoad);
                    bitmapFrame.Freeze();
                    return bitmapFrame;
                }
                catch (OutOfMemoryException)
                {
                    throw;
                }
                catch (Exception e)
                {
                    Debug.WriteLine(e.Message);
                }

                BitmapImage bmpImage = new BitmapImage();
                bmpImage.BeginInit();
                bmpImage.CacheOption = BitmapCacheOption.OnLoad;
                bmpImage.StreamSource = ms;
                bmpImage.EndInit();
                bmpImage.Freeze();

                // out of memory?
                if (ms.Length > 100 * 1024 && bmpImage.PixelHeight == 1 && bmpImage.PixelWidth == 1)
                {
                    Debug.WriteLine("1x1!?");
                    throw new OutOfMemoryException();
                }

                return bmpImage;
            }
        }

        // BitmaiFileHeader作成
        private BitmapFileHeader CreateBitmapFileHeader(BitmapInfoHeader bi)
        {
            var bf = new BitmapFileHeader();
            bf.bfSize = (uint)((((bi.biWidth * bi.biBitCount + 0x1f) >> 3) & ~3) * bi.biHeight);
            bf.bfOffBits = (uint)(Marshal.SizeOf(bf) + Marshal.SizeOf(bi));
            if (bi.biBitCount <= 8)
            {
                uint palettes = bi.biClrUsed;
                if (palettes == 0)
                    palettes = 1u << bi.biBitCount;
                bf.bfOffBits += palettes << 2;
            }
            bf.bfSize += bf.bfOffBits;
            bf.bfType = 0x4d42;
            bf.bfReserved1 = 0;
            bf.bfReserved2 = 0;

            return bf;
        }
        #endregion
    }


    /// <summary>
    /// アーカイブエントリ情報(Raw)
    /// </summary>
    [StructLayout(LayoutKind.Sequential, Pack = 1, CharSet = CharSet.Ansi)]
    public struct ArchiveFileInfoRaw
    {
        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 8)]
        public string method; // 圧縮法の種類
        public uint position; // ファイル上での位置
        public uint compsize; // 圧縮されたサイズ
        public uint filesize; // 元のファイルサイズ
        public uint timestamp; // ファイルの更新日時
        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 200)]
        public string path; // 相対パス
        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 200)]
        public string filename; // ファイルネーム
        public uint crc; // CRC 
    }
}
