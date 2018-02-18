// Copyright (c) 2016-2018 Mitsuhiro Ito (nee)
//
// This software is released under the MIT License.
// http://opensource.org/licenses/mit-license.php

using NeeView.Threading.Tasks;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace NeeView
{
    /// <summary>
    /// アーカイバーの種類
    /// </summary>
    public enum ArchiverType
    {
        None,

        FolderArchive,
        ZipArchiver,
        SevenZipArchiver,
        PdfArchiver,
        SusieArchiver,
        MediaArchiver,

        DefaultArchiver = ZipArchiver
    }


    /// <summary>
    /// アーカイバー基底クラス
    /// </summary>
    public abstract class Archiver : ITrash
    {
        // アーカイブ実体のパス
        public string Path { get; protected set; }

        /// <summary>
        /// 内部アーカイブのテンポラリファイル。インスタンス保持用
        /// </summary>
        public TempFile TempFile { get; set; }

        // ファイルシステムの場合はtrue
        public virtual bool IsFileSystem { get; } = false;

        // ファイルシステムでのパスを取得
        public virtual string GetFileSystemPath(ArchiveEntry entry) { return null; }

        // 本来のファイルシスでのパスを取得
        public string GetSourceFileSystemPath()
        {
            if (IsCompressedChild())
            {
                return this.Parent.GetSourceFileSystemPath();
            }
            else
            {
                return this.Path;
            }
        }

        // 圧縮ファイルの一部？
        public bool IsCompressedChild()
        {
            if (this.Parent != null)
            {
                if (this.Parent is FolderArchive)
                {
                    return this.Parent.IsCompressedChild();
                }
                else
                {
                    return true;
                }
            }
            else
            {
                return false;
            }
        }


        // 対応判定
        public abstract bool IsSupported();

        /// <summary>
        /// 親アーカイブ
        /// </summary>
        public Archiver Parent { get; private set; }

        /// <summary>
        /// 親アーカイブのエントリ表記
        /// </summary>
        public ArchiveEntry Source { get; private set; }


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
        /// ルートフラグ
        /// このフラグを立てたアーカイブがあればこれをルートとする
        /// </summary>
        public bool RootFlag { get; private set; }

        /// <summary>
        /// ルート判定
        /// </summary>
        public bool IsRoot => Parent == null || RootFlag;

        /// <summary>
        /// ルートアーカイバー取得
        /// </summary>
        public Archiver RootArchiver => IsRoot ? this : Parent.RootArchiver;

        /// <summary>
        /// ルートアーカイバーを基準としたエントリ名
        /// </summary>
        public string EntryFullName => IsRoot ? "" : LoosePath.Combine(Parent.EntryFullName, EntryName);

        /// <summary>
        /// ルートアーカイバー名を含んだエントリ名
        /// </summary>
        public string FullName => IsRoot ? EntryName : LoosePath.Combine(Parent.FullName, EntryName);

        /// <summary>
        /// エクスプローラーで指定可能な絶対パス
        /// </summary>
        public string FullPath => Parent == null ? Path : LoosePath.Combine(Parent.FullPath, EntryName);


        /// <summary>
        /// 識別名
        /// </summary>
        public string Ident => (Parent == null || Parent is FolderArchive) ? Path : LoosePath.Combine(Parent.Ident, $"{Id}.{EntryName}");


        /// <summary>
        /// コンストラクタ
        /// </summary>
        /// <param name="path">アーカイブ実体へのパス</param>
        /// <param name="source">基となるエントリ</param>
        /// <param name="isRoot">ルートアーカイバとする</param>
        public Archiver(string path, ArchiveEntry source, bool isRoot)
        {
            Path = path;
            RootFlag = isRoot;

            if (source != null)
            {
                Parent = source.Archiver;
                EntryName = source.EntryName;
                Id = source.Id;
                LastWriteTime = source.LastWriteTime;
                Length = source.Length;

                this.Source = source;
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
        /// ファイルロック解除
        /// </summary>
        public virtual void Unlock()
        {
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
                var entry = await TaskUtils.FuncAsync(GetEntriesFunc, token);
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
        /// アーカイブエントリのみ取得(同期)
        /// </summary>
        /// <returns></returns>
        public List<ArchiveEntry> GetArchives()
        {
            // エントリ取得
            var entries = GetEntries();

            // アーカイブ群収集
            var archives = entries
                .Where(e => e.IsArchive())
                .ToList();

            return archives;
        }

        /// <summary>
        /// アーカイブエントリのみ取得(非同期)
        /// </summary>
        /// <param name="archiver"></param>
        /// <param name="token"></param>
        /// <returns></returns>
        private async Task<List<ArchiveEntry>> GetArchivesAsync(CancellationToken token)
        {
            // エントリ取得
            var entries = await GetEntriesAsync(token);

            // アーカイブ群収集
            var archives = entries
                .Where(e => e.IsArchive())
                .ToList();

            return archives;
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


        /// <summary>
        /// フォルダーリスト上での親フォルダーを取得
        /// </summary>
        /// <returns></returns>
        public string GetParentPlace()
        {
            if (this.Parent != null)
            {
                return this.Parent.FullPath;
            }
            else
            {
                return LoosePath.GetDirectoryName(this.FullPath);
            }
        }


        /// <summary>
        /// ルートフラグ設定
        /// </summary>
        /// <param name="flag"></param>
        public virtual void SetRootFlag(bool flag)
        {
            this.RootFlag = flag;
        }



        public virtual bool IsDisposed => true;

        // 廃棄処理
        public virtual void Dispose()
        {
            this.TempFile = null;
        }
    }

    /// <summary>
    /// 拡張メソッド
    /// </summary>
    public static class ArchiverExtensions
    {
        /// <summary>
        /// 空のディレクトリエントリを抽出して追加
        /// </summary>
        /// <param name="list"></param>
        /// <param name="directoryEntries"></param>
        public static void AddDirectoryEntries(this List<ArchiveEntry> list, List<ArchiveEntry> directoryEntries)
        {
            // 空のディレクトリエントリを抽出
            var entries = directoryEntries
                .Where(entry => directoryEntries.All(e => e == entry || !e.EntryName.StartsWith(entry.EntryName)))
                .Where(entry => list.All(e => !e.EntryName.StartsWith(entry.EntryName)))
                .ToList();

            //foreach (var entry in entries) Debug.WriteLine($"DirectoryEntry!: {entry.EntryName}");

            list.AddRange(entries);
        }

    }



}

