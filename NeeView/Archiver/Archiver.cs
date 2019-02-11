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
    /// アーカイバー基底クラス
    /// </summary>
    public abstract class Archiver : ITrash
    {
        #region Constructors

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

            var query = new QueryPath(path);

            if (source != null)
            {
                Parent = source.Archiver;
                EntryName = source.EntryName;
                Id = source.Id;
                LastWriteTime = source.LastWriteTime;
                Length = source.Length;

                this.Source = source;
            }

            else if (query.Scheme == QueryScheme.Pagemark)
            {
                EntryName = LoosePath.GetFileName(Path);
                Length = -1;
                LastWriteTime = default;
                return;
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

        #endregion

        #region Properties

        // アーカイブ実体のパス
        public string Path { get; protected set; }

        // 内部アーカイブのテンポラリファイル。インスタンス保持用
        public TempFile TempFile { get; set; }

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
        public DateTime LastWriteTime { get; private set; }

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
        /// エクスプローラーで指定可能な絶対パス
        /// </summary>
        public string SystemPath => Parent == null ? Path : LoosePath.Combine(Parent.SystemPath, EntryName);

        /// <summary>
        /// 識別名
        /// </summary>
        public string Ident => (Parent == null || Parent is FolderArchive) ? Path : LoosePath.Combine(Parent.Ident, $"{Id}.{EntryName}");

        #endregion

        #region Methods

        // 本来のファイルシスでのパスを取得
        public string GetSourceFileSystemPath()
        {
            if (IsCompressedChild())
            {
                return this.Parent.GetSourceFileSystemPath();
            }
            else
            {
                return LoosePath.TrimEnd(this.Path);
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
        /// 多重圧縮フォルダーの場合は最上位のアーカイブの場所になる
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
                return this.Parent.SystemPath;
            }
            else
            {
                return LoosePath.GetDirectoryName(this.SystemPath);
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

        #endregion

        #region ITrush Support
        public bool IsDisposed => _disposedValue;
        #endregion

        #region IDisposable Support
        protected bool _disposedValue { get; private set; }

        protected virtual void Dispose(bool disposing)
        {
            if (!_disposedValue)
            {
                if (disposing)
                {
                }

                this.TempFile = null;

                _disposedValue = true;
            }
        }

        ~Archiver()
        {
            Dispose(false);
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }
        #endregion
    }

    /// <summary>
    /// Archiver 拡張メソッド
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

