// Copyright (c) 2016-2018 Mitsuhiro Ito (nee)
//
// This software is released under the MIT License.
// http://opensource.org/licenses/mit-license.php

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

namespace Susie
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
    public class SusiePlugin
    {
        // 文字列変換
        public override string ToString()
        {
            return Name ?? "(none)";
        }

        // 有効/無効
        public bool IsEnable { get; set; } = true;

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

        // 排他処理用ロックオブジェクト
        public object Lock = new object();
        public static object GlobalLock = new object();


        #region OpenConfigurationDlg command

        //
        private RelayCommand<Window> _OpenConfigurationDlg;

        /// <summary>
        /// コンフィグダイアログを表示するコマンド
        /// </summary>
        public RelayCommand<Window> OpenConfigurationDlg
        {
            get { return _OpenConfigurationDlg = _OpenConfigurationDlg ?? new RelayCommand<Window>(OpenConfigurationDlg_Executed); }
        }

        //
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


        /// <summary>
        /// SusiePluginAPIを開く
        /// LoadLibraryを行うため、使用後はDisposeしなければいけない
        /// </summary>
        /// <returns>SusiePluginAPI</returns>
        public SusiePluginApi Open()
        {
            if (FileName == null) throw new InvalidOperationException();
            return SusiePluginApi.Create(FileName);
        }


        /// <summary>
        /// 情報ダイアログを開く
        /// </summary>
        /// <param name="parent">親ウィンドウ</param>
        /// <returns>成功した場合は0</returns>
        public int AboutDlg(Window parent)
        {
            if (FileName == null) throw new InvalidOperationException();

            lock (Lock)
            {
                using (var api = Open())
                {
                    IntPtr hwnd = parent != null ? new WindowInteropHelper(parent).Handle : IntPtr.Zero;
                    return api.ConfigurationDlg(hwnd, 0);
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

            lock (Lock)
            {
                using (var api = Open())
                {
                    IntPtr hwnd = parent != null ? new WindowInteropHelper(parent).Handle : IntPtr.Zero;
                    return api.ConfigurationDlg(hwnd, 1);
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
            if (!IsEnable) return false;

            // サポート拡張子チェック
            if (isCheckExtension && !Extensions.Contains(GetExtension(fileName))) return false;

            lock (Lock)
            {
                using (var api = Open())
                {
                    string shortPath = Win32Api.GetShortPathName(fileName);
                    return api.IsSupported(shortPath, head);
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
            lock (Lock)
            {
                using (var api = Open())
                {
                    string shortPath = Win32Api.GetShortPathName(fileName);
                    var entries = api.GetArchiveInfo(shortPath);
                    if (entries == null) throw new ApplicationException($"{this.Name}: 書庫情報の取得に失敗しました");
                    return new ArchiveEntryCollection(this, fileName, entries);
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
            if (!IsEnable) return null;

            // サポート拡張子チェック
            if (!Extensions.Contains(GetExtension(fileName))) return null;

            lock (Lock)
            {
                using (var api = Open())
                {
                    string shortPath = Win32Api.GetShortPathName(fileName);
                    if (!api.IsSupported(shortPath, head)) return null;
                    var entries = api.GetArchiveInfo(shortPath);
                    if (entries == null) throw new ApplicationException($"{this.Name}: 書庫情報の取得に失敗しました");
                    return new ArchiveEntryCollection(this, fileName, entries);
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
            if (!IsEnable) return null;

            // サポート拡張子チェック
            if (isCheckExtension && !Extensions.Contains(GetExtension(fileName))) return null;

            lock (Lock)
            {
                using (var api = Open())
                {
                    // string shortPath = Win32Api.GetShortPathName(fileName);
                    if (!api.IsSupported(fileName, buff)) return null;
                    return api.GetPicture(buff);
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
            if (!IsEnable) return null;

            // サポート拡張子チェック
            if (isCheckExtension && !Extensions.Contains(GetExtension(fileName))) return null;

            lock (Lock)
            {
                using (var api = Open())
                {
                    string shortPath = Win32Api.GetShortPathName(fileName);
                    if (!api.IsSupported(shortPath, head)) return null;
                    return api.GetPicture(shortPath);
                }
            }
        }

        //
        private static string GetExtension(string s)
        {
            return "." + s.Split('.').Last().ToLower();
        }
    }
}
