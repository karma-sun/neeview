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
    public abstract class Archiver : ITrash
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
        public Archiver Parent => Source?.Archiver;

        /// <summary>
        /// 元となったアーカイブエントリ
        /// </summary>
        public ArchiveEntry Source { get; set; }

        /// <summary>
        /// 識別名
        /// </summary>
        public string Ident => (Parent == null || Parent is FolderFiles) ? FileName : LoosePath.Combine(Parent.Ident, $"{Source.Id}.{Source.EntryName}");

        /// <summary>
        /// エントリリストを取得
        /// </summary>
        /// <returns></returns>
        public abstract List<ArchiveEntry> GetEntries(CancellationToken token);

        //
        public List<ArchiveEntry> GetEntries()
        {
            return GetEntries(CancellationToken.None);
        }

        /// <summary>
        /// エントリリストを取得(非同期)
        /// ※キャンセルしても処理は続行されます
        /// </summary>
        /// <param name="token"></param>
        /// <returns></returns>
        public async Task<List<ArchiveEntry>> GetEntriesAsync(CancellationToken token)
        {
            token.ThrowIfCancellationRequested();

            try
            {
                var proc = new Utility.AsynchronousAction<List<ArchiveEntry>>();
                var entry = await proc.ExecuteAsync(GetEntriesFunc, token);
                Debug.WriteLine($"Entry: done.: {this.FileName}");
                return entry;
            }
            catch (OperationCanceledException)
            {
                Debug.WriteLine($"Entry: Canceled!: {this.FileName}");
                throw;
            }
        }

        private List<ArchiveEntry> GetEntriesFunc(CancellationToken token)
        {
            return GetEntries(token);
        }

        // エントリのストリームを取得
        public abstract Stream OpenStream(ArchiveEntry entry);

        /// <summary>
        /// テンポラリにアーカイブを解凍する
        /// このテンポラリはアーカイブ廃棄時に自動的に削除される
        /// </summary>
        /// <param name="entry"></param>
        /// <param name="isKeepFileName">エントリー名をファイル名にする</param>
        public FileProxy ExtractToTemp(ArchiveEntry entry, bool isKeepFileName = false)
        {
            if (entry.IsFileSystem)
            {
                return new FileProxy(entry.GetFileSystemPath());
            }
            else
            {
                string tempFileName = isKeepFileName
                    ? Temporary.CreateTempFileName(LoosePath.GetFileName(entry.EntryName))
                    : Temporary.CreateCountedTempFileName("entry", Path.GetExtension(entry.EntryName));
                ExtractToFile(entry, tempFileName, false);
                return new TempFile(tempFileName);
            }
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

        //
        public virtual bool IsDisposed => true;

        // 廃棄処理
        public virtual void Dispose()
        {
        }
    }
}
