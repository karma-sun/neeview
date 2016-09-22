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
        private string _LocalApplicationDataPath;
        public string LocalApplicationDataPath
        {
            get
            {
                if (_LocalApplicationDataPath == null)
                {
                    // configファイルの設定で LocalApplicationData を使用するかを判定。インストール版用
                    if (IsUseLocalApplicationDataFolder)
                    {
                        _LocalApplicationDataPath = GetFileSystemPath(Environment.SpecialFolder.LocalApplicationData, true);
                    }
                    else
                    {
                        _LocalApplicationDataPath = AssemblyLocation;
                    }
                }
                return _LocalApplicationDataPath;
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


        //
        private bool? _IsUseLocalApplicationDataFolder;
        public bool IsUseLocalApplicationDataFolder
        {
            get
            {
                if (_IsUseLocalApplicationDataFolder == null)
                {
                    _IsUseLocalApplicationDataFolder = System.Configuration.ConfigurationManager.AppSettings["UseLocalApplicationData"] == "True";
                }
                return (bool)_IsUseLocalApplicationDataFolder;
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

            var text = "ユーザデータを削除します。よろしいですか？\n\n以下のデータが削除されます\n- 設定ファイル\n- 履歴ファイル\n- ブックマークファイル\n- ページマークファイル";
            var result = MessageBox.Show(text, "NeeView - データ削除確認", MessageBoxButton.OKCancel, MessageBoxImage.Warning);

            if (result == MessageBoxResult.OK)
            {
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
