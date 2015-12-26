using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media.Imaging;

namespace Susie
{
    public class SusiePluginApi : IDisposable
    {
        private IntPtr _hModule = IntPtr.Zero;
        public IntPtr hModule { get { return _hModule; } }

        private Dictionary<Type, object> _ApiDelegateList = new Dictionary<Type, object>();

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

        private IntPtr Open(string fileName)
        {
            Close();
            _hModule = Win32Api.LoadLibrary(fileName);
            return _hModule;
        }

        private void Close()
        {
            if (_hModule != IntPtr.Zero)
            {
                Win32Api.FreeLibrary(_hModule);
                _hModule = IntPtr.Zero;
                _ApiDelegateList.Clear();
            }
        }

        #region IDisposable Support
        private bool disposedValue = false; // 重複する呼び出しを検出するには

        protected virtual void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                if (disposing)
                {
                    // TODO: マネージ状態を破棄します (マネージ オブジェクト)。
                }

                // TODO: アンマネージ リソース (アンマネージ オブジェクト) を解放し、下のファイナライザーをオーバーライドします。
                // TODO: 大きなフィールドを null に設定します。
                Close();

                disposedValue = true;
            }
        }

        // TODO: 上の Dispose(bool disposing) にアンマネージ リソースを解放するコードが含まれる場合にのみ、ファイナライザーをオーバーライドします。
        // ~Dll() {
        //   // このコードを変更しないでください。クリーンアップ コードを上の Dispose(bool disposing) に記述します。
        //   Dispose(false);
        // }

        // このコードは、破棄可能なパターンを正しく実装できるように追加されました。
        public void Dispose()
        {
            // このコードを変更しないでください。クリーンアップ コードを上の Dispose(bool disposing) に記述します。
            Dispose(true);
            // TODO: 上のファイナライザーがオーバーライドされる場合は、次の行のコメントを解除してください。
            // GC.SuppressFinalize(this);
        }
        #endregion


        /// <summary>
        /// 関数の存在確認
        /// </summary>
        /// <param name="name">関数名</param>
        /// <returns>trueなら存在する</returns>
        public bool IsExistFunction(string name)
        {
            if (hModule == null) throw new InvalidOperationException();

            IntPtr add = Win32Api.GetProcAddress(hModule, name);
            return (add != IntPtr.Zero);
        }



        public T GetApiDelegate<T>(string procName)
        {
            if (!_ApiDelegateList.ContainsKey(typeof(T)))
            {
                IntPtr add = Win32Api.GetProcAddress(hModule, procName);
                if (add == IntPtr.Zero) throw new NotSupportedException("not support " + procName);
                _ApiDelegateList.Add(typeof(T), Marshal.GetDelegateForFunctionPointer<T>(add));
            }

            return (T)_ApiDelegateList[typeof(T)];
        }


        #region 00IN,00AM 必須 GetPluginInfo
        delegate int GetPluginInfoDelegate(int infono, StringBuilder buf, int len);

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
        delegate int ConfigurationDlgDelegate(IntPtr parent, int fnc);


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
        delegate bool IsSupportedFromFileDelegate(string filename, IntPtr dw);
        delegate bool IsSupportedFromMemoryDelegate(string filename, [In]byte[] dw);

        public bool IsSupported(string filename)
        {
            if (hModule == null) throw new InvalidOperationException();
            var isSupported = GetApiDelegate<IsSupportedFromFileDelegate>("IsSupported");

            using (FileStream fs = new FileStream(filename, FileMode.Open))
            {
                return isSupported(filename, fs.SafeFileHandle.DangerousGetHandle());
            }
        }

        public bool IsSupported(string filename, byte[] buff)
        {
            if (hModule == null) throw new InvalidOperationException();
            var isSupported = GetApiDelegate<IsSupportedFromMemoryDelegate>("IsSupported");

            return isSupported(filename, buff);
        }
        #endregion


        #region 00AM 必須 GetArchiveInfo()
        delegate int GetArchiveInfoFromFileDelegate(string filename, int offset, uint flag, out IntPtr hInfo);

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

                    IntPtr p = hInfo;
                    while (true)
                    {
                        ArchiveFileInfoRaw fileInfo = Marshal.PtrToStructure<ArchiveFileInfoRaw>(p);
                        if (String.IsNullOrEmpty(fileInfo.method)) break;
                        list.Add(fileInfo);
                        p += Marshal.SizeOf<ArchiveFileInfoRaw>();
                    }

                    return list;
                }
            }
            finally
            {
                Win32Api.LocalFree(hInfo);
            }

            return null;
        }
        #endregion

        // TODO: フラグ指定とか
        // TODO: コールバックとか
        #region 00AM 必須 GetFile()
        delegate int GetFileFromFileHandler(string filename, int position, out IntPtr hBuff, uint flag, int lpPrgressCallback, int lData);
        delegate int GetFileFromFileToFileHandler(string filename, int position, string dest, uint flag, int lpPrgressCallback, int lData);

        public byte[] GetFile(string file, ArchiveFileInfoRaw entry)
        {
            if (hModule == null) throw new InvalidOperationException();
            var getFile = GetApiDelegate<GetFileFromFileHandler>("GetFile");

            IntPtr hBuff = IntPtr.Zero;
            try
            {
                int ret = getFile(file, (int)entry.position, out hBuff, 0x0100, 0, 0); // 0x0100 > File To Memory
                if (ret == 0)
                {
                    byte[] buf = new byte[entry.filesize];
                    Marshal.Copy(hBuff, buf, (int)0, (int)entry.filesize);
                    return buf;
                }
                return null;
            }
            finally
            {
                Win32Api.LocalFree(hBuff);
            }

        }


        public int GetFile(string file, ArchiveFileInfoRaw entry, string extractFolder)
        {
            if (hModule == null) throw new InvalidOperationException();
            var getFile = GetApiDelegate<GetFileFromFileToFileHandler>("GetFile");

            return getFile(file, (int)entry.position, extractFolder, 0x0000, 0, 0); // 0x0100 > File To File
        }

        #endregion


        #region 00IN 必須 GetPicture()
        delegate int GetPictureFromMemoryDelegate([In]byte[] buf, int len, uint flag, out IntPtr pHBInfo, out IntPtr pHBm, int lpProgressCallback, int lData);

        public BitmapImage GetPicture(byte[] buff)
        {
            if (hModule == null) throw new InvalidOperationException();
            var getPicture = GetApiDelegate<GetPictureFromMemoryDelegate>("GetPicture");


            IntPtr pHBInfo = IntPtr.Zero;
            IntPtr pHBm = IntPtr.Zero;
            try
            {
                int ret = getPicture(buff, buff.Length, 0x01, out pHBInfo, out pHBm, 0, 0);
                if (ret == 0)
                {
                    IntPtr pBInfo = Win32Api.LocalLock(pHBInfo);
                    IntPtr pBm = Win32Api.LocalLock(pHBm);

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
                        BitmapImage bmpImage = new BitmapImage();

                        bmpImage.BeginInit();
                        bmpImage.CacheOption = BitmapCacheOption.OnLoad;
                        bmpImage.StreamSource = ms;
                        bmpImage.EndInit();
                        bmpImage.Freeze();

                        return bmpImage;
                    }
                }
                return null;
            }
            finally
            {
                Win32Api.LocalUnlock(pHBInfo);
                Win32Api.LocalFree(pHBInfo);
                Win32Api.LocalUnlock(pHBm);
                Win32Api.LocalFree(pHBm);
            }
        }

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
    /// アーカイブ内ファイル情報(Raw)
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
