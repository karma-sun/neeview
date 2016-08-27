// Copyright (c) 2016 Mitsuhiro Ito (nee)
//
// This software is released under the MIT License.
// http://opensource.org/licenses/mit-license.php

using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

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

        // DPI倍率
        public Point DpiScaleFactor { get; private set; } = new Point(1, 1);

        // DPIのXY比率が等しい？
        public bool IsDpiSquare { get; private set; } = false;

        // DPI設定
        public void UpdateDpiScaleFactor(System.Windows.Media.Visual visual)
        {
            var dpiScaleFactor = DragExtensions.WPFUtil.GetDpiScaleFactor(visual);
            DpiScaleFactor = dpiScaleFactor;
            IsDpiSquare = DpiScaleFactor.X == DpiScaleFactor.Y;
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
        /// いろいろ初期化
        /// </summary>
        public void Initialize()
        {
            var assembly = Assembly.GetEntryAssembly();
            ValidateProductInfo(assembly);

            // カレントフォルダ設定
            System.Environment.CurrentDirectory = LocalUserAppDataPath;
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
            ProductVersion = $"{version.Major}.{version.Minor}";
        }


        /// <summary>
        /// ユーザデータフォルダ
        /// </summary>
        private string _LocalUserAppDataPath;
        private string LocalUserAppDataPath
        {
            get
            {
                if (_LocalUserAppDataPath == null)
                {
#if NV_INSTALLER
                    _LocalUserAppDataPath = GetFileSystemPath(Environment.SpecialFolder.LocalApplicationData, true);
#else
                    _LocalUserAppDataPath = AssemblyLocation;
#endif
                }
                return _LocalUserAppDataPath;
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
    }
}
