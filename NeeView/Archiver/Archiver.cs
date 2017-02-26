// Copyright (c) 2016 Mitsuhiro Ito (nee)
//
// This software is released under the MIT License.
// http://opensource.org/licenses/mit-license.php

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

namespace NeeView
{
    /// <summary>
    /// アーカイバの種類
    /// </summary>
    public enum ArchiverType
    {
        None,

        FolderFiles,
        ZipArchiver,
        SevenZipArchiver,
        SusieArchiver,

        DefaultArchiver = ZipArchiver
    }


    /// <summary>
    /// アーカイバ基底クラス
    /// </summary>
    public abstract class Archiver : IDisposable
    {
        // アーカイブのパス
        public string FileName { get; protected set; }

        // ファイルシステムの場合はtrue
        public virtual bool IsFileSystem { get; } = false;

        // ファイルシステムでのパスを取得
        public virtual string GetFileSystemPath(ArchiveEntry entry) { return null; }

        // 対応判定
        public abstract bool IsSupported();

        // 親アーカイブ
        public Archiver Parent { get; set; }

        // エントリリストを取得
        public abstract List<ArchiveEntry> GetEntries();

        // エントリのストリームを取得
        public abstract Stream OpenStream(ArchiveEntry entry);

        /// <summary>
        /// テンポラリにアーカイブを解凍する
        /// このテンポラリはアーカイブ廃棄時に自動的に削除される
        /// </summary>
        /// <param name="entry"></param>
        /// <returns>テンポラリファイル名</returns>
        public string ExtractToTemp(ArchiveEntry entry)
        {
            var ext = Path.GetExtension(entry.EntryName);
            string tempFileName = Temporary.CreateCountedTempFileName("entry", ext);
            ExtractToFile(entry, tempFileName, false);
            TrashBox.Add(new TrashFile(tempFileName));
            return tempFileName;
        }

        // エントリをファイルとして出力
        public abstract void ExtractToFile(ArchiveEntry entry, string exportFileName, bool isOverwrite);


        /// <summary>
        /// 所属している場所を得る
        /// 再帰圧縮フォルダの場合は最上位のアーカイブの場所になる
        /// </summary>
        /// <returns>ファイルパス</returns>
        public string GetPlace()
        {
            return (Parent == null || Parent is FolderFiles) ? FileName : Parent.GetPlace();
        }

        // 廃棄用ゴミ箱
        public TrashBox TrashBox { get; private set; } = new TrashBox();

        // 廃棄処理
        public virtual void Dispose()
        {
            TrashBox.Dispose();
        }

        // エントリをファイルとして出力
        public async Task ExtractToFileAsync(ArchiveEntry entry, string exportFileName, bool isOverwrite, CancellationToken token)
        {
            var extractor = new AsyncronizedExtractor();
            await extractor.ExtractToFileAsync(this, entry, exportFileName, isOverwrite, token);
        }
    }


    /// <summary>
    /// 開発中：非同期Extractor
    /// </summary>
    public class AsyncronizedExtractor
    {
        private ManualResetEventSlim _completed = new ManualResetEventSlim(false);

        public async Task ExtractToFileAsync(Archiver archiver, ArchiveEntry entry, string exportFileName, bool isOverwrite, CancellationToken token)
        {
            token.ThrowIfCancellationRequested();

            Debug.WriteLine($"ExtractToFile: start {exportFileName}");

            try
            {
                // 非同期で実行
                Task.Run(() =>
                {
                    archiver.ExtractToFile(entry, exportFileName, isOverwrite);
                    _completed.Set();

                    if (token.IsCancellationRequested)
                    {
                        Debug.WriteLine($"ExtractToFile: delete {exportFileName} with cancel.");
                        System.IO.File.Delete(exportFileName);
                    }
                });

                // ここで待機
                _completed.Wait(token);
                Debug.WriteLine($"ExtractToFile: complete.");
            }
            catch (OperationCanceledException)
            {
                Debug.WriteLine("ExtractToFile: canceled!");
                throw;
            }
        }
    }

}
