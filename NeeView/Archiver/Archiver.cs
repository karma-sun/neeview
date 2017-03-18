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

        FolderArchive,
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
        // アーカイブ実体のパス
        public string Path { get; protected set; }

        // ファイルシステムの場合はtrue
        public virtual bool IsFileSystem { get; } = false;

        // ファイルシステムでのパスを取得
        public virtual string GetFileSystemPath(ArchiveEntry entry) { return null; }

        // 対応判定
        public abstract bool IsSupported();

        /// <summary>
        /// 親アーカイブ
        /// </summary>
        public Archiver Parent { get; private set; }


        /// <summary>
        /// エントリでの名前
        /// </summary>
        public string EntryName { get; private set; }

        /// <summary>
        /// エントリでのID
        /// </summary>
        public int Id { get; private set; }

        /// <summary>
        /// アーカイブのサイズ
        /// </summary>
        public long Length { get; private set; }

        /// <summary>
        /// アーカイブの最終更新日
        /// </summary>
        public DateTime? LastWriteTime { get; private set; }
        

        /// <summary>
        /// ルート判定
        /// </summary>
        public bool IsRoot => Parent == null;

        /// <summary>
        /// ルートアーカイバ取得
        /// </summary>
        public Archiver RootArchiver => IsRoot ? this : Parent.RootArchiver;

        /// <summary>
        /// ルートアーカイバを基準としたエントリ名
        /// </summary>
        public string EntryFullName => IsRoot ? "" : LoosePath.Combine(Parent.EntryFullName, EntryName);

        /// <summary>
        /// ルートアーカイバ名を含んだエントリ名
        /// </summary>
        public string FullName => IsRoot ? EntryName : LoosePath.Combine(Parent.FullName, EntryName);

        
        /// <summary>
        /// 識別名
        /// </summary>
        public string Ident => (Parent == null || Parent is FolderArchive) ? Path : LoosePath.Combine(Parent.Ident, $"{Id}.{EntryName}");


        /// <summary>
        /// コンストラクタ
        /// </summary>
        /// <param name="path">アーカイブ実体へのパス</param>
        /// <param name="source">基となるエントリ</param>
        public Archiver(string path, ArchiveEntry source)
        {
            Path = path;

            if (source != null)
            {
                Parent = source.Archiver;
                EntryName = source.EntryName;
                Id = source.Id;
                LastWriteTime = source.LastWriteTime;
                Length = source.Length;
            }

            else
            {
                EntryName = LoosePath.GetFileName(Path);

                var directoryInfo = new DirectoryInfo(Path);
                if (directoryInfo.Exists)
                {
                    Length = -1;
                    LastWriteTime = directoryInfo.LastWriteTime;
                    return;
                }

                var fileInfo = new FileInfo(Path);
                if (fileInfo.Exists)
                {
                    Length = fileInfo.Length;
                    LastWriteTime = fileInfo.LastWriteTime;
                    return;
                }
            }
        }


        /// <summary>
        /// エントリリストを取得
        /// </summary>
        /// <param name="token"></param>
        /// <returns></returns>
        public abstract List<ArchiveEntry> GetEntries(CancellationToken token);

        /// <summary>
        /// エントリリストを取得(同期)
        /// </summary>
        /// <returns></returns>
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
                var entry = await Utility.Process.FuncAsync(GetEntriesFunc, token);
                ////Debug.WriteLine($"Entry: done.: {this.Path}");
                return entry;
            }
            catch (OperationCanceledException)
            {
                ////Debug.WriteLine($"[CanceledException]: {this}.{nameof(GetEntriesAsync)}: Cabceled.");
                ////Debug.WriteLine($"Entry: Canceled!: {this.Path}");
                throw;
            }
        }

        /// <summary>
        /// (デリゲート用)
        /// </summary>
        /// <param name="token"></param>
        /// <returns></returns>
        private List<ArchiveEntry> GetEntriesFunc(CancellationToken token)
        {
            return GetEntries(token);
        }

        /// <summary>
        /// エントリのストリームを取得
        /// </summary>
        /// <param name="entry"></param>
        /// <returns></returns>
        public abstract Stream OpenStream(ArchiveEntry entry);

        /// <summary>
        /// エントリをファイルとして出力
        /// </summary>
        /// <param name="entry"></param>
        /// <param name="exportFileName"></param>
        /// <param name="isOverwrite"></param>
        public abstract void ExtractToFile(ArchiveEntry entry, string exportFileName, bool isOverwrite);


        /// <summary>
        /// 所属している場所を得る
        /// 再帰圧縮フォルダーの場合は最上位のアーカイブの場所になる
        /// </summary>
        /// <returns>ファイルパス</returns>
        public string GetPlace()
        {
            return (Parent == null || Parent is FolderArchive) ? Path : Parent.GetPlace();
        }

        public virtual bool IsDisposed => true;

        // 廃棄処理
        public virtual void Dispose()
        {
        }
    }




}

