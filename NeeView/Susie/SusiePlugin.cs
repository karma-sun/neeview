using NeeLaboratory.ComponentModel;
using NeeLaboratory.Windows.Input;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Interop;
using System.Windows.Media.Imaging;

namespace NeeView.Susie
{
    /// <summary>
    /// Susieプラグインの種類
    /// </summary>
    public enum SusiePluginType
    {
        None,
        Image,
        Archive,
    }


    /// <summary>
    /// Susie Plugin Accessor
    /// </summary>
    public class SusiePlugin : BindableBase, IDisposable
    {
        private object _lock = new object();
        private SusiePluginApi _module;
        private bool _isCacheEnabled;

        // 一連の処理をロックするときに使用
        public object GlobalLock = new object();

        // 有効/無効
        private bool _isEnabled = true;
        public bool IsEnabled
        {
            get { return _isEnabled; }
            set { SetProperty(ref _isEnabled, value); }
        }

        // 事前展開。AMプラグインのみ有効
        private bool _isPreExtract;
        public bool IsPreExtract
        {
            get { return _isPreExtract; }
            set { SetProperty(ref _isPreExtract, value); }
        }


        // プラグインファイルのパス
        public string FileName { get; private set; }

        // プラグイン名
        public string Name { get { return FileName != null ? Path.GetFileName(FileName) : null; } }

        // APIバージョン
        public string ApiVersion { get; private set; }

        // プラグインバージョン
        public string PluginVersion { get; private set; }

        // 詳細テキスト
        public string DetailText { get { return $"{Name} ( {string.Join(" ", Extensions)} )"; } }

        // 設定ダイアログの有無
        public bool HasConfigurationDlg { get; private set; }

        // サポートするファイルの種類
        public class SupportFileType
        {
            public string Extension; // ファイルの種類の拡張子
            public string Note; // ファイルの種類の情報
        }
        public List<SupportFileType> SupportFileTypeList { get; private set; }

        // プラグインの種類
        public SusiePluginType PluginType
        {
            get
            {
                switch (this.ApiVersion)
                {
                    default:
                        return SusiePluginType.None;
                    case "00IN":
                        return SusiePluginType.Image;
                    case "00AM":
                        return SusiePluginType.Archive;
                }
            }
        }

        // サポートするファイルの拡張子リスト
        public List<string> Extensions { get; private set; }


        // プラグインDLLをキャッシュする?
        public bool IsCacheEnabled
        {
            get { return _isCacheEnabled; }
            set
            {
                _isCacheEnabled = value;
                if (!_isCacheEnabled)
                {
                    UnloadModule();
                }
            }
        }


        #region Commands

        private RelayCommand<Window> _openConfigurationDlg;

        /// <summary>
        /// コンフィグダイアログを表示するコマンド
        /// </summary>
        public RelayCommand<Window> OpenConfigurationDlg
        {
            get { return _openConfigurationDlg = _openConfigurationDlg ?? new RelayCommand<Window>(OpenConfigurationDlg_Executed); }
        }

        public void OpenConfigurationDlg_Executed(Window owner)
        {
            int result = 0;

            try
            {
                result = ConfigurationDlg(owner);
            }
            catch
            {
                result = -1;
            }

            // 設定ウィンドウが呼び出せなかった場合はアバウト画面でお茶を濁す
            if (result < 0)
            {
                try
                {
                    AboutDlg(owner);
                }
                catch
                {
                }
            }
        }

        #endregion



        // 文字列変換
        public override string ToString()
        {
            return Name ?? "(none)";
        }

        /// <summary>
        /// プラグインアクセサ作成
        /// </summary>
        /// <param name="fileName">プラグインファイルのパス</param>
        /// <returns>プラグイン。失敗したらnullを返す</returns>
        public static SusiePlugin Create(string fileName)
        {
            var spi = new SusiePlugin();
            return spi.Initialize(fileName) ? spi : null;
        }


        /// <summary>
        /// 初期化
        /// </summary>
        /// <param name="fileName">プラグインファイルのパス</param>
        /// <returns>成功したらtrue</returns>
        public bool Initialize(string fileName)
        {
            if (FileName != null) throw new InvalidOperationException();

            try
            {
                using (var api = SusiePluginApi.Create(fileName))
                {
                    ApiVersion = api.GetPluginInfo(0);
                    PluginVersion = api.GetPluginInfo(1);

                    if (string.IsNullOrEmpty(PluginVersion))
                    {
                        PluginVersion = Path.GetFileName(fileName);
                    }

                    SupportFileTypeList = new List<SupportFileType>();
                    while (true)
                    {
                        int index = SupportFileTypeList.Count() * 2 + 2;
                        var fileType = new SupportFileType()
                        {
                            Extension = api.GetPluginInfo(index + 0),
                            Note = api.GetPluginInfo(index + 1)
                        };
                        if (fileType.Extension == null || fileType.Note == null) break;
                        SupportFileTypeList.Add(fileType);
                    }

                    HasConfigurationDlg = api.IsExistFunction("ConfigurationDlg");
                }

                FileName = fileName;

                // create extensions
                Extensions = new List<string>();
                foreach (var supportType in this.SupportFileTypeList)
                {
                    foreach (var filter in supportType.Extension.Split(';', ',')) // ifjpeg2k.spi用に","を追加
                    {
                        string extension = filter.TrimStart('*').ToLower().Trim();
                        if (!string.IsNullOrEmpty(extension))
                        {
                            Extensions.Add(extension);
                        }
                    }
                }

                return true;
            }
            catch (Exception e)
            {
                Debug.WriteLine(e.Message);
                return false;
            }
        }


        // API使用開始
        private SusiePluginApi BeginSection()
        {
            if (FileName == null) throw new InvalidOperationException();

            if (_module == null)
            {
                _module = SusiePluginApi.Create(FileName);
            }

            return _module;
        }

        // API使用終了
        private void EndSection()
        {
            if (IsCacheEnabled)
            {
                return;
            }

            UnloadModule();
        }

        // DLL開放
        private void UnloadModule()
        {
            if (_module != null)
            {
                _module.Dispose();
                _module = null;
            }
        }

        /// <summary>
        /// 情報ダイアログを開く
        /// </summary>
        /// <param name="parent">親ウィンドウ</param>
        /// <returns>成功した場合は0</returns>
        public int AboutDlg(Window parent)
        {
            if (FileName == null) throw new InvalidOperationException();

            lock (_lock)
            {
                try
                {
                    var api = BeginSection();
                    IntPtr hwnd = parent != null ? new WindowInteropHelper(parent).Handle : IntPtr.Zero;
                    return api.ConfigurationDlg(hwnd, 0);
                }
                finally
                {
                    EndSection();
                    UnloadModule();
                }
            }
        }


        /// <summary>
        /// 設定ダイアログを開く
        /// </summary>
        /// <param name="parent">親ウィンドウ</param>
        /// <returns>成功した場合は0</returns>
        public int ConfigurationDlg(Window parent)
        {
            if (FileName == null) throw new InvalidOperationException();

            lock (_lock)
            {
                try
                {
                    var api = BeginSection();
                    IntPtr hwnd = parent != null ? new WindowInteropHelper(parent).Handle : IntPtr.Zero;
                    return api.ConfigurationDlg(hwnd, 1);
                }
                finally
                {
                    EndSection();
                    UnloadModule();
                }
            }
        }


        /// <summary>
        /// プラグイン対応判定
        /// </summary>
        /// <param name="fileName">ファイル名</param>
        /// <param name="head">ヘッダ(2KB)</param>
        /// <returns>プラグインが対応していればtrue</returns>
        public bool IsSupported(string fileName, byte[] head, bool isCheckExtension)
        {
            if (FileName == null) throw new InvalidOperationException();
            if (!IsEnabled) return false;

            // サポート拡張子チェック
            if (isCheckExtension && !Extensions.Contains(GetExtension(fileName))) return false;

            lock (_lock)
            {
                try
                {
                    var api = BeginSection();
                    string shortPath = NativeMethods.GetShortPathName(fileName);
                    return api.IsSupported(shortPath, head);
                }
                finally
                {
                    EndSection();
                }
            }
        }

        /// <summary>
        /// アーカイブ情報取得
        /// </summary>
        /// <param name="fileName">アーカイブファイル名</param>
        /// <returns></returns>
        public ArchiveEntryCollection GetArchiveInfo(string fileName)
        {
            lock (_lock)
            {
                try
                {
                    var api = BeginSection();
                    string shortPath = NativeMethods.GetShortPathName(fileName);
                    var entries = api.GetArchiveInfo(shortPath);
                    if (entries == null) throw new ApplicationException($"{this.Name}: Failed to read archive information.");
                    return new ArchiveEntryCollection(this, fileName, entries);
                }
                finally
                {
                    EndSection();
                }
            }
        }

        /// <summary>
        /// アーカイブ情報取得(IsSupport判定有)
        /// </summary>
        /// <param name="fileName">アーカイブファイル名</param>
        /// <returns>アーカイブ情報。失敗した場合はnull</returns>
        public ArchiveEntryCollection GetArchiveInfo(string fileName, byte[] head)
        {
            if (FileName == null) throw new InvalidOperationException();
            if (!IsEnabled) return null;

            // サポート拡張子チェック
            if (!Extensions.Contains(GetExtension(fileName))) return null;

            lock (_lock)
            {
                try
                {
                    var api = BeginSection();
                    string shortPath = NativeMethods.GetShortPathName(fileName);
                    if (!api.IsSupported(shortPath, head)) return null;
                    var entries = api.GetArchiveInfo(shortPath);
                    if (entries == null) throw new ApplicationException($"{this.Name}: Failed to read archive information.");
                    return new ArchiveEntryCollection(this, fileName, entries);
                }
                finally
                {
                    EndSection();
                }
            }
        }


        /// <summary>
        /// 画像取得(メモリ版)
        /// </summary>
        /// <param name="fileName">画像ファイル名(サポート判定用)</param>
        /// <param name="buff">画像データ</param>
        /// <param name="isCheckExtension">拡張子をチェックする</param>
        /// <returns>Bitmap。失敗した場合はnull</returns>
        public byte[] GetPicture(string fileName, byte[] buff, bool isCheckExtension)
        {
            if (FileName == null) throw new InvalidOperationException();
            if (!IsEnabled) return null;

            // サポート拡張子チェック
            if (isCheckExtension && !Extensions.Contains(GetExtension(fileName))) return null;

            lock (_lock)
            {
                try
                {
                    var api = BeginSection();
                    // string shortPath = Win32Api.GetShortPathName(fileName);
                    if (!api.IsSupported(fileName, buff)) return null;
                    return api.GetPicture(buff);
                }
                finally
                {
                    EndSection();
                }
            }
        }

        /// <summary>
        /// 画像取得(ファイル版)
        /// </summary>
        /// <param name="fileName">画像ファイルパス</param>
        /// <param name="fileName">ファイルヘッダ2KB</param>
        /// <param name="isCheckExtension">拡張子をチェックする</param>
        /// <returns>Bitmap。失敗した場合はnull</returns>
        public byte[] GetPictureFromFile(string fileName, byte[] head, bool isCheckExtension)
        {
            if (FileName == null) throw new InvalidOperationException();
            if (!IsEnabled) return null;

            // サポート拡張子チェック
            if (isCheckExtension && !Extensions.Contains(GetExtension(fileName))) return null;

            lock (_lock)
            {
                try
                {
                    var api = BeginSection();
                    string shortPath = NativeMethods.GetShortPathName(fileName);
                    if (!api.IsSupported(shortPath, head)) return null;
                    return api.GetPicture(shortPath);
                }
                finally
                {
                    EndSection();
                }
            }
        }

        //
        private static string GetExtension(string s)
        {
            return "." + s.Split('.').Last().ToLower();
        }


        /// <summary>
        /// アーカイブエントリをメモリにロード
        /// </summary>
        public byte[] LoadArchiveEntry(string archiveFileName, ArchiveFileInfoRaw info)
        {
            if (_isDisposed)
            {
                throw new SpiException("Susie plugin already disposed", this);
            }

            lock (_lock)
            {
                try
                {
                    var api = BeginSection();
                    var buff = api.GetFile(archiveFileName, info);
                    if (buff == null) throw new SpiException("Susie extraction failed (Type.M)", this);
                    return buff;
                }
                finally
                {
                    EndSection();
                }
            }
        }

        /// <summary>
        /// アーカイブエントリをフォルダーに出力
        /// </summary>
        /// <param name="extractFolder"></param>
        public void ExtracArchiveEntrytToFolder(string archiveFileName, ArchiveFileInfoRaw info, string extractFolder)
        {
            if (_isDisposed)
            {
                throw new SpiException("Susie plugin already disposed", this);
            }

            lock (_lock)
            {
                try
                {
                    var api = BeginSection();
                    int ret = api.GetFile(archiveFileName, info, extractFolder);
                    if (ret != 0) throw new SpiException("Susie extraction failed (Type.F)", this);
                }
                finally
                {
                    EndSection();
                }
            }
        }

        #region IDisposable Support
        private bool _isDisposed = false;

        protected virtual void Dispose(bool disposing)
        {
            if (!_isDisposed)
            {
                if (disposing)
                {
                    UnloadModule();
                }

                _isDisposed = true;
            }
        }

        public void Dispose()
        {
            Dispose(true);
        }
        #endregion
    }
}
