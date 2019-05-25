using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace NeeView.Susie.Server
{
    /// <summary>
    /// Susie Plugin Accessor
    /// </summary>
    public class SusiePlugin : IDisposable
    {
        private object _lock = new object();
        private SusiePluginApi _module;
        private bool _isCacheEnabled = true;
        private FileExtensionCollection _userExtensions;


        // 一連の処理をロックするときに使用
        public object GlobalLock = new object();

        // 有効/無効
        public bool IsEnabled { get; set; } = true;

        // 事前展開。AMプラグインのみ有効
        public bool IsPreExtract { get; set; }

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


        // 標準拡張子
        public FileExtensionCollection DefaultExtensions { get; set; } = new FileExtensionCollection();
        public FileExtensionCollection UserExtensions
        {
            get { return _userExtensions; }
            set
            {
                if (value != null && !value.IsEmpty() && !value.Equals(DefaultExtensions))
                {
                    _userExtensions = value;
                }
                else
                {
                    _userExtensions = null;
                }
            }
        }

        // 対応拡張子
        public FileExtensionCollection Extensions
        {
            get { return UserExtensions ?? DefaultExtensions; }
        }

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
        /// 対応拡張子を標準に戻す
        /// </summary>
        public void ResetExtensions()
        {
            UserExtensions = null;
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

                    UpdateDefaultExtensions(api);

                    HasConfigurationDlg = api.IsExistFunction("ConfigurationDlg");
                }

                FileName = fileName;

                return true;
            }
            catch (Exception ex)
            {
                Trace.WriteLine($"SusiePlugin.Initialze: FileName={fileName}: {ex.Message}");
                return false;
            }
        }

        /// <summary>
        /// 標準拡張子の更新
        /// </summary>
        /// <param name="api"></param>
        private void UpdateDefaultExtensions(SusiePluginApi api)
        {
            var extensions = new List<string>();
            for (int index = 2; ; index += 2)
            {
                // 拡張子フィルター
                var filter = api.GetPluginInfo(index + 0);
                // 拡張子フィルターの説明（未使用）
                var extensionsNote = api.GetPluginInfo(index + 1);

                if (filter == null) break;

                // ifjpeg2k.spi用に区切り記号に","を追加
                // ワイルド拡張子は無効
                extensions.AddRange(filter.Split(';', ',').Select(e => e.TrimStart('*').ToLower().Trim()).Where(e => e != ".*"));
            }

            DefaultExtensions = new FileExtensionCollection(extensions);
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
            // reset FPU
            NativeMethods._fpreset();

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
#if false
        public int AboutDlg(Window parent)
        {
            IntPtr hwnd = parent != null ? new WindowInteropHelper(parent).Handle : IntPtr.Zero;
            return AboutDlg(hwnd);
        }
#endif

        public int AboutDlg(IntPtr hwnd)
        {
            if (FileName == null) throw new InvalidOperationException();

            lock (_lock)
            {
                try
                {
                    var api = BeginSection();
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
#if false
        public int ConfigurationDlg(Window parent)
        {
            IntPtr hwnd = parent != null ? new WindowInteropHelper(parent).Handle : IntPtr.Zero;
            return ConfigurationDlg(hwnd);
        }
#endif

        public int ConfigurationDlg(IntPtr hwnd)
        {
            if (FileName == null) throw new InvalidOperationException();

            lock (_lock)
            {
                try
                {
                    var api = BeginSection();
                    var result = api.ConfigurationDlg(hwnd, 1);
                    UpdateDefaultExtensions(api);
                    return result;
                }
                finally
                {
                    EndSection();
                    UnloadModule();
                }
            }
        }

        /// <summary>
        /// 設定ダイアログもしくは情報ダイアログを開く
        /// </summary>
#if false
        public void OpenConfigulationDialog(Window owner)
        {
            int result;
            try
            {
                result = ConfigurationDlg(owner);
            }
            catch
            {
                result = -1;
            }

            // 設定ウィンドウが呼び出せなかった場合は情報画面でお茶を濁す
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
#endif

        public void OpenConfigulationDialog(IntPtr hWnd)
        {
            int result;
            try
            {
                result = ConfigurationDlg(hWnd);
            }
            catch
            {
                result = -1;
            }

            // 設定ウィンドウが呼び出せなかった場合は情報画面でお茶を濁す
            if (result < 0)
            {
                try
                {
                    AboutDlg(hWnd);
                }
                catch
                {
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
        public ArchiveEntryCollection GetArchiveEntryCollection(string fileName)
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
        public ArchiveEntryCollection GetArchiveEntryCollection(string fileName, byte[] head)
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
            return LoadArchiveEntry(archiveFileName, (int)info.position);
        }

        public byte[] LoadArchiveEntry(string archiveFileName, int position)
        {
            if (_isDisposed)
            {
                throw new SusieException("Susie plugin already disposed", this.Name);
            }

            lock (_lock)
            {
                try
                {
                    var api = BeginSection();
                    var buff = api.GetFile(archiveFileName, position);
                    if (buff == null) throw new SusieException("Susie extraction failed (Type.M)", this.Name);
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
            ExtracArchiveEntrytToFolder(archiveFileName, (int)info.position, extractFolder);
        }

        public void ExtracArchiveEntrytToFolder(string archiveFileName, int position, string extractFolder)
        {
            if (_isDisposed)
            {
                throw new SusieException("Susie plugin already disposed", this.Name);
            }

            lock (_lock)
            {
                try
                {
                    var api = BeginSection();
                    int ret = api.GetFile(archiveFileName, position, extractFolder);
                    if (ret != 0) throw new SusieException("Susie extraction failed (Type.F)", this.Name);
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


        public void Restore(SusiePluginSetting setting)
        {
            if (setting == null) return;
            this.IsEnabled = setting.IsEnabled;
            this.IsCacheEnabled = setting.IsCacheEnabled;
            this.IsPreExtract = setting.IsPreExtract;
            this.UserExtensions = setting.UserExtensions != null ? new FileExtensionCollection(setting.UserExtensions) : null;
        }

        public SusiePluginInfo ToSusiePluginInfo()
        {
            var info = new SusiePluginInfo();

            info.FileName = this.FileName;
            info.Name = this.Name;
            info.ApiVersion = this.ApiVersion;
            info.PluginVersion = this.PluginVersion;
            info.PluginType = this.PluginType;
            info.DetailText = this.DetailText;
            info.HasConfigurationDlg = this.HasConfigurationDlg;
            info.IsEnabled = this.IsEnabled;
            info.IsCacheEnabled = this.IsCacheEnabled;
            info.IsPreExtract = this.IsPreExtract;
            info.DefaultExtension = this.DefaultExtensions;
            info.UserExtension = this.UserExtensions;

            return info;
        }

        public SusiePluginSetting ToSusiePluginSetting()
        {
            var setting = new SusiePluginSetting();
            setting.Name = this.Name;
            setting.IsEnabled = this.IsEnabled;
            setting.IsCacheEnabled = this.IsCacheEnabled;
            setting.IsPreExtract = this.IsPreExtract;
            setting.UserExtensions = this.UserExtensions?.ToOneLine();
            return setting;
        }
    }

}
