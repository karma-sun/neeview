// Copyright (c) 2016 Mitsuhiro Ito (nee)
//
// This software is released under the MIT License.
// http://opensource.org/licenses/mit-license.php

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;


namespace NeeView
{
    // TODO: エクスプローラーの圧縮zip内からのドロップ対応
    // TODO: 7zからのドロップ対応

    /// <summary>
    ///  Drop Exception
    ///  ユーザに知らせるべき例外
    /// </summary>
    public class DropException : Exception
    {
        public DropException()
        {
        }

        public DropException(string message)
            : base(message)
        {
        }

        public DropException(string message, Exception inner)
            : base(message, inner)
        {
        }
    }

    /// <summary>
    /// Drop Manager
    /// </summary>
    public class ContentDropManager
    {
        // ドロップ受付判定
        public bool CheckDragContent(object sender, DragEventArgs e)
        {
            return (e.Data.GetDataPresent(DataFormats.FileDrop, true) || (e.Data.GetDataPresent("FileContents") && e.Data.GetDataPresent("FileGroupDescriptorW")));
        }

        // ファイラーからのドロップ
        List<DropReciever> _FileDropRecievers = new List<DropReciever>()
            {
                new DropFileDrop(),
                new DropFileContents(),
                new DropInlineImage(),
            };

        // ブラウザからのドロップ
        List<DropReciever> _BrowserDropRecievers = new List<DropReciever>()
            {
                new DropFileContents(),
                new DropInlineImage(),
                new DropFileDropCopy(),
                new DropWebImage(),
            };


        // ファイルのドラッグ＆ドロップで処理を開始する
        public async Task<string> DropAsync(object sender, DragEventArgs e, string downloadPath, Action<string> nowloading)
        {
            var recievers = (e.Data.GetDataPresent("UniformResourceLocator") || e.Data.GetDataPresent("UniformResourceLocatorW"))
                ? _BrowserDropRecievers : _FileDropRecievers;

            string errorMessage = null;
            foreach (var reciever in recievers)
            {
                try
                {
                    var path = await reciever.DropAsync(sender, e, downloadPath, nowloading);
                    if (path != null)
                    {
                        Debug.WriteLine("Load by " + reciever.ToString());
                        return path;
                    }
                }
                catch (DropException ex)
                {
                    errorMessage = ex.Message;
                    break;
                }
                catch (Exception ex)
                {
                    errorMessage = ex.Message;
                }
            }

            // データタンプ
            NVUtility.DumpDragData(e.Data);

            //  読み込めなかったエラー表示
            throw new ApplicationException(errorMessage ?? "ドロップコンテンツの読み込みに失敗しました");
        }
    }


    /// <summary>
    /// ドロップ処理基底
    /// </summary>
    public abstract class DropReciever
    {
        /// <summary>
        /// ドロップ処理
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e">ドロップイベント引数</param>
        /// <param name="downloadPath">ファイル出力パス</param>
        /// <param name="nowloading">NowLoading表示用デリゲート</param>
        /// <returns>得られたファイルパス</returns>
        public abstract Task<string> DropAsync(object sender, DragEventArgs e, string downloadPath, Action<string> nowloading);

        /// <summary>
        /// バイナリを画像としてファイルに保存(Async)
        /// </summary>
        public async Task<string> DownloadToFileAsync(byte[] buff, string name, string downloadPath)
        {
            return await Task.Run(() => DownloadToFile(buff, name, downloadPath));
        }

        /// <summary>
        /// バイナリを画像としてファイルに保存
        /// </summary>
        /// <param name="buff">バイナリ</param>
        /// <param name="name">希望ファイル名。現在の実装では無視されます</param>
        /// <param name="downloadPath">保存先フォルダ</param>
        /// <returns>出力されたファイルパスを返す。バイナリが画像データ出なかった場合はnull</returns>
        public string DownloadToFile(byte[] buff, string name, string downloadPath)
        {
            //if (!System.IO.Directory.Exists(downloadPath)) throw new DropException("保存先フォルダが存在しません");

            // ファイル名は固定
            name = DateTime.Now.ToString("yyyyMMddHHmmss");
            string ext = "";

            // 画像ファイルチェック
            // 対応拡張子に変更する
            var exts = NVUtility.GetSupportImageExtensions(buff);
            if (exts == null) return null;
            if (!exts.Contains(ext))
            {
                var newExtension = exts[0];
                // 一部拡張子置き換え
                if (newExtension == ".jpeg") newExtension = ".jpg";
                if (newExtension == ".tiff") newExtension = ".tif";
                name = System.IO.Path.ChangeExtension(name, newExtension);
            }

            // ユニークなパスを作成
            string fileName = NVUtility.CreateUniquePath(System.IO.Path.Combine(downloadPath, name));

            try
            {
                // 保存
                using (var stream = new System.IO.FileStream(fileName, System.IO.FileMode.Create))
                {
                    stream.Write(buff, 0, buff.Length);
                }
            }
            catch (Exception e)
            {
                if (!System.IO.Directory.Exists(downloadPath)) throw new DropException("ファイルの出力に失敗しました。\n" + e.Message, e);
            }

            return fileName;
        }

        // ファイル名の修正
        private string ValidateFileName(string name)
        {
            string DefaultName = DateTime.Now.ToString("yyyyMMddHHmmss");

            // nullの場合はデフォルト名
            name = name ?? DefaultName;

            // ファイル名として使用可能な文字列にする
            name = LoosePath.ValidFileName(name);

            // 拡張子取得
            string ext = System.IO.Path.GetExtension(name).ToLower();

            // 名前が長すぎる場合は独自名にする。64文字ぐらい？
            if (string.IsNullOrWhiteSpace(name) || name.Length > 64)
            {
                name = DefaultName + ext;
            }

            return name;
        }
    }


    /// <summary>
    /// Drop : FileContents
    /// </summary>
    public class DropFileContents : DropReciever
    {
        public override async Task<string> DropAsync(object sender, DragEventArgs e, string downloadPath, Action<string> nowloading)
        {
            //
            if (e.Data.GetDataPresent("FileContents") && e.Data.GetDataPresent("FileGroupDescriptorW"))
            {
                var fileNames = new List<string>();
                foreach (var file in Utility.FileContents.Get(e.Data))
                {
                    if (file.Bytes == null || file.Bytes.Length <= 0) continue;

                    string fileName = await DownloadToFileAsync(file.Bytes, file.Name, downloadPath);
                    if (fileName != null) fileNames.Add(fileName);
                }

                if (fileNames.Count > 0)
                {
                    return fileNames[0];
                }
            }

            return null;
        }
    }


    /// <summary>
    /// Drop : FileDrop
    /// </summary>
    public class DropFileDrop : DropReciever
    {
        public override async Task<string> DropAsync(object sender, DragEventArgs e, string downloadPath, Action<string> nowloading)
        {
            // File drop
            if (e.Data.GetDataPresent(DataFormats.FileDrop))
            {
                string[] files = e.Data.GetData(DataFormats.FileDrop) as string[];
                if (files != null) return files[0];
            }

            await Task.Yield();
            return null;
        }
    }

    /// <summary>
    /// Drop: FileDrop to Copy
    /// </summary>
    public class DropFileDropCopy : DropReciever
    {
        public override async Task<string> DropAsync(object sender, DragEventArgs e, string downloadPath, Action<string> nowloading)
        {
            // File drop (from browser)
            if (e.Data.GetDataPresent(DataFormats.FileDrop))
            {
                string[] files = e.Data.GetData(DataFormats.FileDrop) as string[];
                if (files != null)
                {
                    var fileNames = new List<string>();
                    foreach (var file in files)
                    {
                        // copy
                        var bytes = await Task.Run(async () => { await Task.Yield(); return System.IO.File.ReadAllBytes(file); });
                        string fileName = await DownloadToFileAsync(bytes, System.IO.Path.GetFileName(file), downloadPath);
                        if (fileName != null) fileNames.Add(fileName);
                    }
                    if (fileNames.Count > 0) return fileNames[0];
                }
            }

            return null;
        }
    }


    /// <summary>
    /// Drop : Inline Image
    /// インラインデータ(base64)のみ処理
    /// </summary>
    public class DropInlineImage : DropReciever
    {
        public override async Task<string> DropAsync(object sender, DragEventArgs e, string downloadPath, Action<string> nowloading)
        {
            if (e.Data.GetDataPresent("HTML Format"))
            {
                var fileNames = new List<string>();
                foreach (var url in NVUtility.ParseSourceUrl(e.Data.GetData("HTML Format").ToString()))
                {
                    //data:[<mediatype>][;base64],<data>
                    if (url.StartsWith("data:image/"))
                    {
                        // base64 to binary
                        const string keyword = "base64,";
                        var index = url.IndexOf(keyword);
                        if (index < 0) continue;  // base64の埋め込みのサポート
                        var crypt = url.Substring(index + keyword.Length);
                        var bytes = Convert.FromBase64String(crypt);

                        // ファイル化
                        string fileName = await DownloadToFileAsync(bytes, null, downloadPath);
                        if (fileName != null) fileNames.Add(fileName);
                    }
                }
                if (fileNames.Count > 0) return fileNames[0];
            }

            return null;
        }
    }


    /// <summary>
    /// Drop : Download Image
    /// Webからダウンロードする
    /// </summary>
    public class DropWebImage : DropReciever
    {
        public override async Task<string> DropAsync(object sender, DragEventArgs e, string downloadPath, Action<string> nowloading)
        {
            // Webアクセス時はNowLoading表示を行う
            nowloading("ドロップされたコンテンツ");

            using (var wc = new System.Net.WebClient())
            {
                // from HTML format
                if (e.Data.GetDataPresent("HTML Format"))
                {
                    var fileNames = new List<string>();
                    foreach (var url in NVUtility.ParseSourceUrl(e.Data.GetData("HTML Format").ToString()))
                    {
                        if (url.StartsWith("http://") || url.StartsWith("https://"))
                        {
                            // download
                            var bytes = await wc.DownloadDataTaskAsync(new Uri(url));

                            // ファイル化
                            string fileName = await DownloadToFileAsync(bytes, null, downloadPath);
                            if (fileName != null) fileNames.Add(fileName);
                        }
                    }
                    if (fileNames.Count > 0) return fileNames[0];
                }

                // from Text
                if (e.Data.GetDataPresent("UniformResourceLocator") || e.Data.GetDataPresent("UniformResourceLocatorW"))
                {
                    var url = e.Data.GetData(DataFormats.Text).ToString();
                    if (url.StartsWith("http://") || url.StartsWith("https://"))
                    {
                        // download
                        var bytes = await wc.DownloadDataTaskAsync(new Uri(url));

                        // ファイル化
                        return DownloadToFile(bytes, null, downloadPath);
                    }
                }

                return null;
            }
        }
    }
}
