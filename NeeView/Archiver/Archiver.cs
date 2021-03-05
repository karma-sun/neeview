using NeeLaboratory.Threading.Tasks;
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
    public abstract class Archiver
    {
        #region Fields

        /// <summary>
        /// ArchiveEntry Cache
        /// </summary>
        private List<ArchiveEntry> _entries;

        /// <summary>
        /// 事前展開フォルダー
        /// </summary>
        private TempDirectory _preExtractDirectory;

        #endregion Fields

        #region Constructors

        /// <summary>
        /// コンストラクタ
        /// </summary>
        /// <param name="path">アーカイブ実体へのパス</param>
        /// <param name="source">基となるエントリ</param>
        public Archiver(string path, ArchiveEntry source)
        {
            Path = path;

            var query = new QueryPath(path);

            if (source != null)
            {
                Parent = source.Archiver;
                EntryName = source.EntryName;
                Id = source.Id;
                CreationTime = source.CreationTime;
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
        /// アーカイブの作成日時
        /// </summary>
        public DateTime CreationTime { get; private set; }

        /// <summary>
        /// アーカイブの最終更新日
        /// </summary>
        public DateTime LastWriteTime { get; private set; }

        /// <summary>
        /// ルート判定
        /// </summary>
        public bool IsRoot => Parent == null;

        /// <summary>
        /// ルートアーカイバー取得
        /// </summary>
        public Archiver RootArchiver => IsRoot ? this : Parent.RootArchiver;

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
        /// エントリリストを取得 (Archive内でのみ使用)
        /// </summary>
        protected abstract Task<List<ArchiveEntry>> GetEntriesInnerAsync(CancellationToken token);

        /// <summary>
        /// エントリリストを取得
        /// </summary>
        public async Task<List<ArchiveEntry>> GetEntriesAsync(CancellationToken token)
        {
            token.ThrowIfCancellationRequested();

            if (_entries != null)
            {
                return _entries;
            }

            // NOTE: MTAスレッドで実行。SevenZipSharpのCOM例外対策
            _entries = await Task.Run(async () =>
            {
                return (await GetEntriesInnerAsync(token))
                    .Where(e => !BookProfile.Current.IsExcludedPath(e.EntryName))
                    .ToList();
            });

            return _entries;
        }


        /// <summary>
        /// 指定階層のエントリのみ取得
        /// </summary>
        public async Task<List<ArchiveEntry>> GetEntriesAsync(string path, bool isRecursive, CancellationToken token)
        {
            path = LoosePath.TrimDirectoryEnd(path);

            var entries = (await GetEntriesAsync(token))
                .Where(e => path.Length < e.EntryName.Length && e.EntryName.StartsWith(path));

            if (!isRecursive)
            {
                entries = entries.Where(e => LoosePath.Split(e.EntryName.Substring(path.Length)).Length == 1);
            }

            return entries.ToList();
        }

        /// <summary>
        /// エントリーのストリームを取得
        /// </summary>
        public Stream OpenStream(ArchiveEntry entry)
        {
            if (entry.Id < 0) throw new ApplicationException("Cannot open this entry: " + entry.EntryName);

            if (entry.Data is byte[] rawData)
            {
                return new MemoryStream(rawData);
            }
            if (entry.Data is string fileName)
            {
                return new FileStream(fileName, FileMode.Open, FileAccess.Read);
            }
            else
            {
                return OpenStreamInner(entry);
            }
        }

        /// <summary>
        /// エントリのストリームを取得 (Inner)
        /// </summary>
        protected abstract Stream OpenStreamInner(ArchiveEntry entry);

        /// <summary>
        /// エントリーをファイルとして出力
        /// </summary>
        public void ExtractToFile(ArchiveEntry entry, string exportFileName, bool isOverwrite)
        {
            if (entry.Id < 0) throw new ApplicationException("Cannot open this entry: " + entry.EntryName);

            if (entry.Data is string fileName)
            {
                File.Copy(fileName, exportFileName, isOverwrite);
            }
            else
            {
                ExtractToFileInner(entry, exportFileName, isOverwrite);
            }
        }

        /// <summary>
        /// エントリをファイルとして出力 (Inner)
        /// </summary>
        protected abstract void ExtractToFileInner(ArchiveEntry entry, string exportFileName, bool isOverwrite);


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
        /// エントリ群からディレクトリエントリを生成する
        /// </summary>
        /// <param name="entries">アーカイブのエントリ群</param>
        /// <returns>ディレクトリエントリのリスト</returns>
        protected List<ArchiveEntry> CreateDirectoryEntries(IEnumerable<ArchiveEntry> entries)
        {
            var tree = new ArchiveEntryTree();
            tree.AddRange(entries);

            var directories = tree.GetDirectories()
                .Select(e => new ArchiveEntry()
                {
                    IsValid = true,
                    Archiver = this,
                    Id = -1,
                    Instance = null,
                    RawEntryName = e.Path,
                    Length = -1,
                    IsEmpty = !e.HasChild,
                    CreationTime = e.CreationTime,
                    LastWriteTime = e.LastWriteTime,
                })
                .ToList();

            return directories;
        }


        /// <summary>
        /// 事前展開する？
        /// </summary>
        public virtual async Task<bool> CanPreExtractAsync(CancellationToken token)
        {
            await Task.CompletedTask;
            return false;
        }

        /// <summary>
        /// 事前展開処理
        /// </summary>
        public async Task PreExtractAsync(CancellationToken token)
        {
            if (_preExtractDirectory != null) return;

            var directory = Temporary.Current.CreateCountedTempFileName("arc", "");
            Directory.CreateDirectory(directory);

            await PreExtractInnerAsync(directory, token);

            _preExtractDirectory = new TempDirectory(directory);
        }

        /// <summary>
        /// 事前展開 (Inner)
        /// </summary>
        public virtual async Task PreExtractInnerAsync(string directory, CancellationToken token)
        {
            var entries = await GetEntriesAsync(token);
            foreach (var entry in entries)
            {
                if (entry.IsDirectory) continue;
                var filename = $"{entry.Id:000000}{System.IO.Path.GetExtension(entry.EntryName)}";
                var path = System.IO.Path.Combine(directory, filename);
                entry.ExtractToFile(path, true);
                entry.Data = path;
            }
        }

        // エントリー実体のファイルシステム判定
        public virtual bool IsFileSystemEntry(ArchiveEntry entry)
        {
            return IsFileSystem || entry.Link != null;
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

