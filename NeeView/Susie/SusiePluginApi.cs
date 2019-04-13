using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media.Imaging;

namespace NeeView.Susie
{
    /// <summary>
    /// Susie Plugin API
    /// アンマネージなDLLアクセスを行います。
    /// </summary>
    public class SusiePluginApi : IDisposable
    {
        // DLLハンドル
        public IntPtr hModule { get; private set; } = IntPtr.Zero;

        // APIデリゲートリスト
        private Dictionary<Type, object> _apiDelegateList = new Dictionary<Type, object>();

        // 64bitプラグイン(.sph)
        private bool _is64bitPlugin;


        private SusiePluginApi(bool is64bitPlugin)
        {
            _is64bitPlugin = is64bitPlugin;
        }

        /// <summary>
        /// プラグインをロードし、使用可能状態にする
        /// </summary>
        /// <param name="fileName">spiファイル名</param>
        /// <returns>プラグインインターフェイス</returns>
        public static SusiePluginApi Create(string fileName, bool is64bitPlugin)
        {
            var lib = new SusiePluginApi(is64bitPlugin);
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
            this.hModule = NeeView.Susie.NativeMethods.LoadLibrary(fileName);
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
                NeeView.Susie.NativeMethods.FreeLibrary(this.hModule);
                hModule = IntPtr.Zero;

                // 浮動小数点演算プロセッサのリセット
                NeeView.NVInterop.NVFpReset();
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

            IntPtr add = NeeView.Susie.NativeMethods.GetProcAddress(this.hModule, name);
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
                IntPtr add = NeeView.Susie.NativeMethods.GetProcAddress(this.hModule, procName);
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

            StringBuilder strb = new StringBuilder(1024);
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

                    IntPtr p = NeeView.Susie.NativeMethods.LocalLock(hInfo);
                    while (true)
                    {
                        ArchiveFileInfoRaw fileInfo = _is64bitPlugin
                            ? Marshal.PtrToStructure<ArchiveFileInfoRawX64>(p).ToArchiveFileInfoRaw()
                            : Marshal.PtrToStructure<ArchiveFileInfoRaw>(p);
                        if (String.IsNullOrEmpty(fileInfo.method)) break;
                        list.Add(fileInfo);
                        p += Marshal.SizeOf<ArchiveFileInfoRaw>();
                    }

                    return list;
                }
            }
            finally
            {
                NeeView.Susie.NativeMethods.LocalUnlock(hInfo);
                NeeView.Susie.NativeMethods.LocalFree(hInfo);
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
                    IntPtr pBuff = NeeView.Susie.NativeMethods.LocalLock(hBuff);
                    var buffSize = (int)NeeView.Susie.NativeMethods.LocalSize(hBuff);
                    if (buffSize == 0) throw new ApplicationException("Memory error.");
                    if (buffSize != (int)entry.filesize)
                    {
                        Debug.WriteLine($"SusieWarning: illigal ArchiveFile size: request={entry.filesize}, real={buffSize}");
                    }
                    byte[] buf = new byte[buffSize];
                    Marshal.Copy(pBuff, buf, (int)0, (int)buffSize);
                    return buf;
                }
                return null;
            }
            finally
            {
                NeeView.Susie.NativeMethods.LocalUnlock(hBuff);
                NeeView.Susie.NativeMethods.LocalFree(hBuff);
            }
        }

        /// <summary>
        /// アーカイブエントリ取得(ファイル版)
        /// </summary>
        /// <param name="file">アーカイブファイル名</param>
        /// <param name="entry">アーカイブエントリ名</param>
        /// <param name="extractFolder">出力フォルダー</param>
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
        /// <returns>Bitmap。失敗した場合はnull</returns>
        public byte[] GetPicture(byte[] buff)
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
                    IntPtr pBInfo = NeeView.Susie.NativeMethods.LocalLock(pHBInfo);
                    int pBInfoSize = (int)NeeView.Susie.NativeMethods.LocalSize(pHBInfo);
                    IntPtr pBm = NeeView.Susie.NativeMethods.LocalLock(pHBm);
                    int pBmSize = (int)NeeView.Susie.NativeMethods.LocalSize(pHBm);
                    return CraeteBitmapImage(pBInfo, pBInfoSize, pBm, pBmSize);
                }
                return null;
            }
            finally
            {
                NeeView.Susie.NativeMethods.LocalUnlock(pHBInfo);
                NeeView.Susie.NativeMethods.LocalUnlock(pHBm);
                NeeView.Susie.NativeMethods.LocalFree(pHBInfo);
                NeeView.Susie.NativeMethods.LocalFree(pHBm);
            }
        }


        /// <summary>
        /// 画像取得(ファイル版)
        /// </summary>
        /// <param name="filename">入力ファイル名</param>
        /// <returns>Bitmap。失敗した場合はnull</returns>
        public byte[] GetPicture(string filename)
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
                    IntPtr pBInfo = NeeView.Susie.NativeMethods.LocalLock(pHBInfo);
                    int pBInfoSize = (int)NeeView.Susie.NativeMethods.LocalSize(pHBInfo);
                    IntPtr pBm = NeeView.Susie.NativeMethods.LocalLock(pHBm);
                    int pBmSize = (int)NeeView.Susie.NativeMethods.LocalSize(pHBm);
                    return CraeteBitmapImage(pBInfo, pBInfoSize, pBm, pBmSize);
                }
                return null;
            }
            finally
            {
                NeeView.Susie.NativeMethods.LocalUnlock(pHBInfo);
                NeeView.Susie.NativeMethods.LocalUnlock(pHBm);
                NeeView.Susie.NativeMethods.LocalFree(pHBInfo);
                NeeView.Susie.NativeMethods.LocalFree(pHBm);
            }
        }


        // Bitmap 作成
        private byte[] CraeteBitmapImage(IntPtr pBInfo, int pBInfoSize, IntPtr pBm, int pBmSize)
        {
            if (pBInfoSize == 0 || pBmSize == 0)
            {
                throw new ApplicationException("Memory error.");
            }

            var bi = Marshal.PtrToStructure<BitmapInfoHeader>(pBInfo);
            var bf = CreateBitmapFileHeader(bi);
            byte[] mem = new byte[bf.bfSize];
            GCHandle gch = GCHandle.Alloc(mem, GCHandleType.Pinned);
            try { Marshal.StructureToPtr<BitmapFileHeader>(bf, gch.AddrOfPinnedObject(), false); }
            finally { gch.Free(); }

            int infoSize = (int)bf.bfOffBits - Marshal.SizeOf(bf);
            int infoSizeReal = pBInfoSize;
            if (infoSizeReal < infoSize)
            {
                Debug.WriteLine($"SusieWarning: illigal pBInfo size: request={infoSize}, real={infoSizeReal}");
                infoSize = infoSizeReal;
                if (infoSize <= 0) throw new ApplicationException("Memory error.");
            }
            Marshal.Copy(pBInfo, mem, Marshal.SizeOf(bf), infoSize);

            int dataSize = (int)(bf.bfSize - bf.bfOffBits);
            int dataSizeReal = pBmSize;
            if (dataSizeReal < dataSize)
            {
                Debug.WriteLine($"SusieWarning: illigal pBm size: request={dataSize}, real={dataSizeReal}");
                dataSize = dataSizeReal;
                if (dataSize <= 0) throw new ApplicationException("Memory error.");
            }
            Marshal.Copy(pBm, mem, (int)bf.bfOffBits, dataSize);

            return mem;
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

    /// <summary>
    /// アーカイブエントリ情報(Raw) 64bit
    /// </summary>
    [StructLayout(LayoutKind.Sequential, Pack = 1, CharSet = CharSet.Ansi)]
    public struct ArchiveFileInfoRawX64
    {
        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 8)]
        public string method; // 圧縮法の種類
        public ulong position; // ファイル上での位置
        public ulong compsize; // 圧縮されたサイズ
        public ulong filesize; // 元のファイルサイズ
        public ulong timestamp; // ファイルの更新日時
        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 200)]
        public string path; // 相対パス
        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 200)]
        public string filename; // ファイルネーム
        public ulong crc; // CRC 
    }

    public static class ArchiveFileInfoRawExtensions
    {
        public static ArchiveFileInfoRaw ToArchiveFileInfoRaw(this ArchiveFileInfoRawX64 self)
        {
            // NOTE: 有効な値の範囲は32bitまで
            var info = new ArchiveFileInfoRaw();
            info.method = self.method;
            info.position = (uint)self.position;
            info.compsize = (uint)self.compsize;
            info.filesize = (uint)self.filesize;
            info.timestamp = (uint)self.timestamp;
            info.path = self.path;
            info.filename = self.filename;
            info.crc = (uint)self.crc; // ビットが切り捨てられているので無意味な値になっている。未使用につき現状維持
            return info;
        }

        public static ArchiveFileInfoRawX64 ToArchiveFileInfoRawX64(this ArchiveFileInfoRaw self)
        {
            var info = new ArchiveFileInfoRawX64();
            info.method = self.method;
            info.position = self.position;
            info.compsize = self.compsize;
            info.filesize = self.filesize;
            info.timestamp = self.timestamp;
            info.path = self.path;
            info.filename = self.filename;
            info.crc = self.crc;
            return info;
        }
    }

}
