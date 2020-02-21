using NeeView.Properties;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Xml.Linq;

namespace NeeView
{
    /// <summary>
    /// アプリ全体の設定
    /// </summary>
    public class Config
    {
        static Config() => Current = new Config();
        public static Config Current { get; }


        private Config()
        {
            this.ProcessId = Process.GetCurrentProcess().Id;

            var assembly = Assembly.GetEntryAssembly();
            ValidateProductInfo(assembly);

            // Windows7では標準でTLS1.1,TLS1.2に対応していないので対応させる。バージョンチェック通信用。
            if (IsWindows7())
            {
                ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls | SecurityProtocolType.Tls11 | SecurityProtocolType.Tls12;
            }
        }

        public event EventHandler DpiChanged;

        /// <summary>
        /// DPI(アプリ値)
        /// </summary>
        public DpiScale Dpi => App.Current.IsIgnoreImageDpi ? RawDpi : OneDpi;

        /// <summary>
        /// DPI(システム値)
        /// </summary>
        public DpiScale RawDpi { get; private set; } = new DpiScale(1, 1);

        /// <summary>
        /// 等倍DPI値
        /// </summary>
        public DpiScale OneDpi { get; private set; } = new DpiScale(1, 1);

        /// <summary>
        /// DPIのXY比率が等しい？
        /// </summary>
        public bool IsDpiSquare => Dpi.DpiScaleX == Dpi.DpiScaleY;

        /// <summary>
        /// DPI設定
        /// </summary>
        /// <param name="dpi"></param>
        public bool SetDip(DpiScale dpi)
        {
            if (RawDpi.DpiScaleX != dpi.DpiScaleX || RawDpi.DpiScaleY != dpi.DpiScaleY)
            {
                RawDpi = dpi;
                DpiChanged?.Invoke(this, null);
                return true;
            }
            else
            {
                return false;
            }
        }


        // Windows7?
        public bool IsWindows7()
        {
            var os = System.Environment.OSVersion;
            return os.Version.Major < 6 || (os.Version.Major == 6 && os.Version.Minor <= 1); // Windows7 = 6.1
        }

        // Windows10?
        public bool IsWindows10()
        {
            var os = System.Environment.OSVersion;
            return os.Version.Major == 10;
        }


        /// <summary>
        /// プロセスID
        /// </summary>
        public int ProcessId { get; private set; }

        /// <summary>
        /// マルチ起動での2番目以降のプロセス
        /// </summary>
        public bool IsSecondProcess { get; set; }

        /// <summary>
        /// 
        /// </summary>
        public string AssemblyLocation { get; private set; }

        /// <summary>
        /// 会社名
        /// </summary>
        public string CompanyName { get; private set; }

        /// <summary>
        /// ソリューション名
        /// </summary>
        public string SolutionName => "NeeView";

        /// <summary>
        /// タイトル名
        /// </summary>
        public string AssemblyTitle { get; private set; }

        /// <summary>
        /// プロダクト名
        /// </summary>
        public string AssemblyProduct { get; private set; }

        /// <summary>
        /// アプリ名
        /// </summary>
        public string ApplicationName => AssemblyTitle;

        /// <summary>
        /// プロダクトバージョン
        /// </summary>
        public string ProductVersion { get; private set; }

        /// <summary>
        /// 表示用バージョン
        /// </summary>
        public string DispVersion
        {
            get
            {
                if (IsCanaryPackage)
                {
                    return "Canary";
                }
                else if (IsBetaPackage)
                {
                    return ProductVersion + ".Beta";
                }
                else
                {
                    return ProductVersion;
                }
            }
        }

        /// <summary>
        /// プロダクトバージョン(int)
        /// </summary>
        public int ProductVersionNumber { get; private set; }

        // ※ build は未使用
        public static int GenerateProductVersionNumber(int major, int minor, int build)
        {
            return major << 16 | minor << 8;
        }

        // プロダクトバージョン(int)からメジャーバージョンを取得
        public static int GetMajorVersionNumber(int versionNumber)
        {
            return (versionNumber >> 16) & 0xff;
        }

        // プロダクトバージョン(int)からマイナーバージョンを取得
        public static int GetMinorVersionNumber(int versionNumber)
        {
            return (versionNumber >> 8) & 0xff;
        }

        // PCメモリサイズ
        public static ulong GetTotalPhysicalMemory()
        {
            var info = new Microsoft.VisualBasic.Devices.ComputerInfo();
            return info.TotalPhysicalMemory;
        }

        /// <summary>
        /// アセンブリ情報収集
        /// </summary>
        /// <param name="asm"></param>
        private void ValidateProductInfo(Assembly asm)
        {
            // パス
            AssemblyLocation = Path.GetDirectoryName(asm.Location);

            // 会社名
            AssemblyCompanyAttribute companyAttribute = Attribute.GetCustomAttribute(asm, typeof(AssemblyCompanyAttribute)) as AssemblyCompanyAttribute;
            CompanyName = companyAttribute.Company;

            // タイトル
            AssemblyTitleAttribute titleAttribute = Attribute.GetCustomAttribute(asm, typeof(AssemblyTitleAttribute)) as AssemblyTitleAttribute;
            AssemblyTitle = titleAttribute.Title;

            // プロダクト
            AssemblyProductAttribute productAttribute = Attribute.GetCustomAttribute(asm, typeof(AssemblyProductAttribute)) as AssemblyProductAttribute;
            AssemblyProduct = productAttribute.Product;


            // バージョンの取得
            var version = asm.GetName().Version;
            ProductVersion = $"{version.Major}.{version.Minor}";
            ProductVersionNumber = GenerateProductVersionNumber(version.Major, version.Minor, 0);
        }


        /// <summary>
        /// ユーザデータフォルダー
        /// </summary>
        private string _localApplicationDataPath;
        public string LocalApplicationDataPath
        {
            get
            {
                if (_localApplicationDataPath == null)
                {
                    // configファイルの設定で LocalApplicationData を使用するかを判定。インストール版用
                    if (IsUseLocalApplicationDataFolder)
                    {
                        _localApplicationDataPath = GetFileSystemPath(Environment.SpecialFolder.LocalApplicationData, true);
                    }
                    else
                    {
                        _localApplicationDataPath = AssemblyLocation;
                    }
                }
                return _localApplicationDataPath;
            }
        }

        /// <summary>
        /// フォルダーパス生成(特殊フォルダー用)
        /// </summary>
        /// <param name="folder"></param>
        /// <returns></returns>
        private string GetFileSystemPath(Environment.SpecialFolder folder, bool createFolder)
        {
            string path = System.IO.Path.Combine(Environment.GetFolderPath(folder), CompanyName, SolutionName);

            if (this.IsAppxPackage)
            {
                path += ".a"; // 既存の設定を一切引き継がない
            }

            if (createFolder && !Directory.Exists(path))
            {
                Directory.CreateDirectory(path);
            }
            return path;
        }

        private string GetFileSystemCompanyPath(Environment.SpecialFolder folder, bool createFolder)
        {
            string path = System.IO.Path.Combine(Environment.GetFolderPath(folder), CompanyName);
            if (createFolder && !Directory.Exists(path))
            {
                Directory.CreateDirectory(path);
            }
            return path;
        }


        /// <summary>
        /// ライブラリーパス
        /// </summary>
        private string _librariesPath;
        public string LibrariesPath
        {
            get
            {
                if (_librariesPath == null)
                {
                    _librariesPath = Path.GetFullPath(Path.Combine(AssemblyLocation, ConfigurationManager.AppSettings["LibrariesPath"]));
                }
                return _librariesPath;
            }
        }

        /// <summary>
        /// ライブラリーパス(Platform別)
        /// </summary>
        public string LibrariesPlatformPath
        {
            get { return Path.Combine(LibrariesPath, IsX64 ? "x64" : "x86"); }
        }

        /// <summary>
        /// x86/x64判定
        /// </summary>
        public static bool IsX64
        {
            get { return IntPtr.Size == 8; }
        }

        // データ保存にアプリケーションデータフォルダーを使用するか
        private bool? _isUseLocalApplicationDataFolder;
        public bool IsUseLocalApplicationDataFolder
        {
            get
            {
                if (_isUseLocalApplicationDataFolder == null)
                {
                    _isUseLocalApplicationDataFolder = ConfigurationManager.AppSettings["UseLocalApplicationData"] == "True";
                }
                return (bool)_isUseLocalApplicationDataFolder;
            }
        }

        // パッケージの種類(拡張子)
        private string _packageType;
        public string PackageType
        {
            get
            {
                if (_packageType == null)
                {
                    _packageType = ConfigurationManager.AppSettings["PackageType"];
                    ////if (_packageType != ".msi") _packageType = ".zip";
                }
                return _packageType;
            }
        }

        public bool IsZipPackage => this.PackageType == ".zip";
        public bool IsMsiPackage => this.PackageType == ".msi";
        public bool IsAppxPackage => this.PackageType == ".appx";
        public bool IsCanaryPackage => this.PackageType == ".canary";
        public bool IsBetaPackage => this.PackageType == ".beta";
        
        public bool IsZipLikePackage => IsZipPackage || IsCanaryPackage || IsBetaPackage;


        // 全ユーザデータ削除
        private bool RemoveApplicationDataCore()
        {
            // LocalApplicationDataフォルダーを使用している場合のみ
            if (!IsUseLocalApplicationDataFolder)
            {
                throw new ApplicationException(Properties.Resources.ExceptionCannotDeleteData);
            }

            Debug.WriteLine("RemoveAllApplicationData ...");

            var productFolder = GetFileSystemPath(Environment.SpecialFolder.LocalApplicationData, false);
            Directory.Delete(LocalApplicationDataPath, true);
            System.Threading.Thread.Sleep(500);

            var companyFolder = GetFileSystemCompanyPath(Environment.SpecialFolder.LocalApplicationData, false);
            if (Directory.GetFileSystemEntries(companyFolder).Length == 0)
            {
                Directory.Delete(companyFolder);
            }

            Debug.WriteLine("RemoveAllApplicationData done.");
            return true;
        }

        //
        public event EventHandler LocalApplicationDataRemoved;

        //
        public void RemoveApplicationData(Window owner)
        {
            var dialog = new MessageDialog(Resources.DialogDeleteApplicationData, Resources.DialogDeleteApplicationDataTitle);
            dialog.Commands.Add(UICommands.Delete);
            dialog.Commands.Add(UICommands.Cancel);
            var result = dialog.ShowDialog(owner);

            if (result == UICommands.Delete)
            {
                // キャッシュDBを閉じる
                ThumbnailCache.Current.Close();

                try
                {
                    this.RemoveApplicationDataCore();
                    new MessageDialog(Resources.DialogDeleteApplicationDataComplete, Resources.DialogDeleteApplicationDataCompleteTitle).ShowDialog(owner);

                    LocalApplicationDataRemoved?.Invoke(this, null);
                }
                catch (Exception ex)
                {
                    new MessageDialog(ex.Message, Resources.DialogDeleteApplicationDataErrorTitle).ShowDialog(owner);
                }
            }
        }
    }
}
