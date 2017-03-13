// Copyright (c) 2016 Mitsuhiro Ito (nee)
//
// This software is released under the MIT License.
// http://opensource.org/licenses/mit-license.php

using System;
using System.Collections.Generic;
using System.Configuration;
using System.Diagnostics;
using System.IO;
using System.Linq;
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
        /// <summary>
        /// コンストラクタ
        /// </summary>
        public Config()
        {
        }

        /// <summary>
        /// DPI(アプリ値)
        /// </summary>
        public DpiScale Dpi => Preference.Current.view_image_dotbydot ? RawDpi : OneDpi;

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
        public void SetDip(DpiScale dpi)
        {
            RawDpi = dpi;
        }


        /// <summary>
        /// 
        /// </summary>
        public string AssemblyLocation { get; private set; }

        /// <summary>
        /// 会社名
        /// </summary>
        public string CompanyName { get; private set; }

        /// <summary>
        /// プロダクト名
        /// </summary>
        public string ProductName { get; private set; }

        /// <summary>
        /// プロダクトバージョン
        /// </summary>
        public string ProductVersion { get; private set; }

        /// <summary>
        /// プロダクトバージョン(int)
        /// </summary>
        public int ProductVersionNumber { get; private set; }

        //
        public static int GenerateProductVersionNumber(int major, int minor, int build)
        {
            return major << 16 | minor << 8 | build;
        }

        /// <summary>
        /// いろいろ初期化
        /// </summary>
        public void Initialize()
        {
            var assembly = Assembly.GetEntryAssembly();
            ValidateProductInfo(assembly);
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
            ProductName = titleAttribute.Title;

            // バージョンの取得
            var version = asm.GetName().Version;
            if (version.Build == 0)
            {
                ProductVersion = $"{version.Major}.{version.Minor}";
                ProductVersionNumber = GenerateProductVersionNumber(version.Major, version.Minor, 0);
            }
            else
            {
                ProductVersion = $"{version.Major}.{version.Minor}.{version.Build}";
                ProductVersionNumber = GenerateProductVersionNumber(version.Major, version.Minor, version.Build);
            }
        }


        /// <summary>
        /// ユーザデータフォルダ
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
        /// フォルダパス生成(特殊フォルダ用)
        /// </summary>
        /// <param name="folder"></param>
        /// <returns></returns>
        private string GetFileSystemPath(Environment.SpecialFolder folder, bool createFolder)
        {
            string path = System.IO.Path.Combine(Environment.GetFolderPath(folder), CompanyName, ProductName);
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
                    _librariesPath = Path.Combine(AssemblyLocation, GetProbingPath());
#if DEBUG
                    // 開発中はLibrariesパスが存在しないので、カレントに設定しなおす
                    _librariesPath = AssemblyLocation;
#endif
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
        public bool IsX64
        {
            get { return IntPtr.Size == 8; }
        }


        // http://stackoverflow.com/questions/33353420/appdomain-currentdomain-setupinformation-privatebinpath-is-null
        private static string GetProbingPath()
        {
            var configFile = XElement.Load(AppDomain.CurrentDomain.SetupInformation.ConfigurationFile);
            var probingElement = (
                from runtime
                    in configFile.Descendants("runtime")
                from assemblyBinding
                    in runtime.Elements(XName.Get("assemblyBinding", "urn:schemas-microsoft-com:asm.v1"))
                from probing
                    in assemblyBinding.Elements(XName.Get("probing", "urn:schemas-microsoft-com:asm.v1"))
                select probing)
                .FirstOrDefault();

            return probingElement?.Attribute("privatePath").Value;
        }


        // データ保存にアプリケーションデータフォルダを使用するか
        private bool? _isUseLocalApplicationDataFolder;
        public bool IsUseLocalApplicationDataFolder
        {
            get
            {
                if (_isUseLocalApplicationDataFolder == null)
                {
                    _isUseLocalApplicationDataFolder = System.Configuration.ConfigurationManager.AppSettings["UseLocalApplicationData"] == "True";
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
                    if (_packageType != ".msi") _packageType = ".zip";
                }
                return _packageType;
            }
        }


        // 全ユーザデータ削除
        private bool RemoveApplicationDataCore()
        {
            // LocalApplicationDataフォルダを使用している場合のみ
            if (!IsUseLocalApplicationDataFolder) return false;

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
        public void RemoveApplicationData()
        {
            if (!this.IsUseLocalApplicationDataFolder)
            {
                MessageBox.Show("--removeオプションはインストーラー版でのみ機能します", "起動オプションエラー", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            var text = "ユーザデータを削除します。よろしいですか？\n\n以下のデータが削除されます\n- 設定ファイル\n- 履歴ファイル\n- ブックマークファイル\n- ページマークファイル\n- キャッシュファイル";
            var result = MessageBox.Show(text, "NeeView - データ削除確認", MessageBoxButton.OKCancel, MessageBoxImage.Warning);

            if (result == MessageBoxResult.OK)
            {
                // キャッシュDBを閉じる
                ThumbnailCache.Current.Close();

                // 削除できないのでカレントフォルダ移動
                var currentFolder = System.Environment.CurrentDirectory;
                System.Environment.CurrentDirectory = this.AssemblyLocation;

                try
                {
                    this.RemoveApplicationDataCore();
                    MessageBox.Show("ユーザデータを削除しました。NeeViewを終了します。", "NeeView - 完了");
                    LocalApplicationDataRemoved?.Invoke(this, null);
                }
                catch (Exception ex)
                {
                    MessageBox.Show(ex.Message, "NeeView - エラー", MessageBoxButton.OK, MessageBoxImage.Error);

                    // カレントフォルダ復帰
                    System.Environment.CurrentDirectory = currentFolder;
                }
            }
        }
    }
}
