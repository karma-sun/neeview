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
    /// アプリの環境
    /// </summary>
    public static class Environment
    {
        private static string _localApplicationDataPath;
        private static string _librariesPath;
        private static string _packageType;
        private static bool? _isUseLocalApplicationDataFolder;


        static Environment()
        {
            ProcessId = Process.GetCurrentProcess().Id;

            var assembly = Assembly.GetEntryAssembly();
            ValidateProductInfo(assembly);

            // Windows7では標準でTLS1.1,TLS1.2に対応していないので対応させる。バージョンチェック通信用。
            if (IsWindows7)
            {
                ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls | SecurityProtocolType.Tls11 | SecurityProtocolType.Tls12;
            }
        }


        public static event EventHandler DpiChanged;
        public static event EventHandler LocalApplicationDataRemoved;


        /// <summary>
        /// DPI(アプリ値)
        /// </summary>
        public static DpiScale Dpi => App.Current.IsIgnoreImageDpi ? RawDpi : OneDpi;

        /// <summary>
        /// DPI(システム値)
        /// </summary>
        public static DpiScale RawDpi { get; private set; } = new DpiScale(1, 1);

        /// <summary>
        /// 等倍DPI値
        /// </summary>
        public static DpiScale OneDpi { get; private set; } = new DpiScale(1, 1);

        /// <summary>
        /// DPIのXY比率が等しい？
        /// </summary>
        public static bool IsDpiSquare => Dpi.DpiScaleX == Dpi.DpiScaleY;

        /// <summary>
        /// DPI設定
        /// </summary>
        /// <param name="dpi"></param>
        public static bool SetDip(DpiScale dpi)
        {
            if (RawDpi.DpiScaleX != dpi.DpiScaleX || RawDpi.DpiScaleY != dpi.DpiScaleY)
            {
                RawDpi = dpi;
                DpiChanged?.Invoke(null, null);
                return true;
            }
            else
            {
                return false;
            }
        }

        // Windows7?
        public static bool IsWindows7
        {
            get
            {
                var os = System.Environment.OSVersion;
                return os.Version.Major < 6 || (os.Version.Major == 6 && os.Version.Minor <= 1); // Windows7 = 6.1
            }
        }

        // Windows10?
        public static bool IsWindows10
        {
            get
            {
                var os = System.Environment.OSVersion;
                return os.Version.Major == 10;
            }
        }

        /// <summary>
        /// プロセスID
        /// </summary>
        public static int ProcessId { get; private set; }

        /// <summary>
        /// マルチ起動での2番目以降のプロセス
        /// </summary>
        public static bool IsSecondProcess { get; set; }

        /// <summary>
        /// 
        /// </summary>
        public static string AssemblyLocation { get; private set; }

        /// <summary>
        /// 会社名
        /// </summary>
        public static string CompanyName { get; private set; }

        /// <summary>
        /// ソリューション名
        /// </summary>
        public static string SolutionName => "NeeView";

        /// <summary>
        /// タイトル名
        /// </summary>
        public static string AssemblyTitle { get; private set; }

        /// <summary>
        /// プロダクト名
        /// </summary>
        public static string AssemblyProduct { get; private set; }

        /// <summary>
        /// アプリ名
        /// </summary>
        public static string ApplicationName => AssemblyTitle;

        /// <summary>
        /// プロダクトバージョン
        /// </summary>
        public static string ProductVersion { get; private set; }

        /// <summary>
        /// 表示用バージョン
        /// </summary>
        public static string DispVersion
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
        public static int ProductVersionNumber { get; private set; }

        /// <summary>
        /// ユーザデータフォルダー
        /// </summary>
        public static string LocalApplicationDataPath
        {
            get
            {
                if (_localApplicationDataPath == null)
                {
                    // configファイルの設定で LocalApplicationData を使用するかを判定。インストール版用
                    if (IsUseLocalApplicationDataFolder)
                    {
                        _localApplicationDataPath = GetFileSystemPath(System.Environment.SpecialFolder.LocalApplicationData, true);
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
        /// ライブラリーパス
        /// </summary>
        public static string LibrariesPath
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
        public static string LibrariesPlatformPath
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
        public static bool IsUseLocalApplicationDataFolder
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
        public static string PackageType
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

        public static bool IsZipPackage => PackageType == ".zip";
        public static bool IsMsiPackage => PackageType == ".msi";
        public static bool IsAppxPackage => PackageType == ".appx";
        public static bool IsCanaryPackage => PackageType == ".canary";
        public static bool IsBetaPackage => PackageType == ".beta";

        public static bool IsZipLikePackage => IsZipPackage || IsCanaryPackage || IsBetaPackage;


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
        private static void ValidateProductInfo(Assembly asm)
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
        /// マイドキュメントのアプリ専用フォルダー
        /// </summary>
        /// <param name="createFolder"></param>
        /// <returns></returns>
        public static string GetMyDocumentPath(bool createFolder)
        {
            string path = System.IO.Path.Combine(System.Environment.GetFolderPath(System.Environment.SpecialFolder.MyDocuments), CompanyName, SolutionName);

            if (createFolder && !Directory.Exists(path))
            {
                Directory.CreateDirectory(path);
            }
            return path;
        }

        /// <summary>
        /// フォルダーパス生成(特殊フォルダー用)
        /// </summary>
        /// <param name="folder"></param>
        /// <returns></returns>
        public static string GetFileSystemPath(System.Environment.SpecialFolder folder, bool createFolder)
        {
            string path = System.IO.Path.Combine(System.Environment.GetFolderPath(folder), CompanyName, SolutionName);

            if (IsAppxPackage)
            {
                path += ".a"; // 既存の設定を一切引き継がない
            }

            if (createFolder && !Directory.Exists(path))
            {
                Directory.CreateDirectory(path);
            }
            return path;
        }

        private static string GetFileSystemCompanyPath(System.Environment.SpecialFolder folder, bool createFolder)
        {
            string path = System.IO.Path.Combine(System.Environment.GetFolderPath(folder), CompanyName);
            if (createFolder && !Directory.Exists(path))
            {
                Directory.CreateDirectory(path);
            }
            return path;
        }

        // 全ユーザデータ削除
        public static void RemoveApplicationData(Window owner)
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
                    RemoveApplicationDataCore();
                    new MessageDialog(Resources.DialogDeleteApplicationDataComplete, Resources.DialogDeleteApplicationDataCompleteTitle).ShowDialog(owner);
                    LocalApplicationDataRemoved?.Invoke(null, null);
                }
                catch (Exception ex)
                {
                    new MessageDialog(ex.Message, Resources.DialogDeleteApplicationDataErrorTitle).ShowDialog(owner);
                }
            }
        }

        // 全ユーザデータ削除
        private static bool RemoveApplicationDataCore()
        {
            // LocalApplicationDataフォルダーを使用している場合のみ
            if (!IsUseLocalApplicationDataFolder)
            {
                throw new ApplicationException(Properties.Resources.ExceptionCannotDeleteData);
            }

            Debug.WriteLine("RemoveAllApplicationData ...");

            var productFolder = GetFileSystemPath(System.Environment.SpecialFolder.LocalApplicationData, false);
            Directory.Delete(LocalApplicationDataPath, true);
            System.Threading.Thread.Sleep(500);

            var companyFolder = GetFileSystemCompanyPath(System.Environment.SpecialFolder.LocalApplicationData, false);
            if (Directory.GetFileSystemEntries(companyFolder).Length == 0)
            {
                Directory.Delete(companyFolder);
            }

            Debug.WriteLine("RemoveAllApplicationData done.");
            return true;
        }
    }
}
