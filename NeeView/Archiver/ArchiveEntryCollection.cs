using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace NeeView
{
    /// <summary>
    /// ArchiveEntryCollectionの展開オプション
    /// </summary>
    [Flags]
    public enum ArchiveEntryCollectionOption
    {
        None,
        IgnoreCache,
    }

    /// <summary>
    /// ArchiveEntryCollectionの展開範囲
    /// </summary>
    public enum ArchiveEntryCollectionMode
    {
        /// <summary>
        /// 指定ディレクトリのみ
        /// </summary>
        [AliasName("@EnumArchiveEntryCollectionModeCurrentDirectory")]
        CurrentDirectory,

        /// <summary>
        /// 指定ディレクトリを含む現在のアーカイブの範囲
        /// </summary>
        [AliasName("@EnumArchiveEntryCollectionModeIncludeSubDirectories")]
        IncludeSubDirectories,

        /// <summary>
        /// 指定ディレクトリ以下サブアーカイブ含むすべて
        /// </summary>
        [AliasName("@EnumArchiveEntryCollectionModeIncludeSubArchives")]
        IncludeSubArchives,
    }

    /// <summary>
    /// 指定パス以下のArchiveEntryの収集
    /// </summary>
    public class ArchiveEntryCollection
    {
        private ArchiveEntryCollectionMode _mode;
        private ArchiveEntryCollectionMode _modeIfArchive;
        private List<ArchiveEntry> _entries;
        private int _prefixLength;
        private bool _ignoreCache;

        /// <summary>
        /// コンストラクタ
        /// </summary>
        /// <param name="path">対象のパス</param>
        /// <param name="mode">標準再帰モード</param>
        /// <param name="modeIfArchive">圧縮ファイルの再帰モード</param>
        /// <param name="option"></param>
        public ArchiveEntryCollection(string path, ArchiveEntryCollectionMode mode, ArchiveEntryCollectionMode modeIfArchive, ArchiveEntryCollectionOption option)
        {
            Path = LoosePath.TrimEnd(path);
            Mode = mode;
            _mode = mode;
            _modeIfArchive = modeIfArchive;
            _ignoreCache = option.HasFlag(ArchiveEntryCollectionOption.IgnoreCache);

            _prefixLength = LoosePath.TrimDirectoryEnd(Path).Length;
        }

        public string Path { get; }
        public Archiver Archiver { get; private set; }

        public ArchiveEntryCollectionMode Mode { get; private set; }

        /// <summary>
        /// ArchiveEntry収集
        /// </summary>
        public async Task<List<ArchiveEntry>> GetEntriesAsync(CancellationToken token)
        {
            if (_entries != null) return _entries;

            var rootEntry = await ArchiveEntryUtility.CreateAsync(Path, token);

            Archiver rootArchiver;
            string rootArchiverPath;

            if (rootEntry.IsFileSystem)
            {
                if (rootEntry.IsDirectory)
                {
                    rootArchiver = await ArchiverManager.Current.CreateArchiverAsync(ArchiveEntry.Create(Path), _ignoreCache, token);
                    rootArchiverPath = "";
                }
                else
                {
                    rootArchiver = await ArchiverManager.Current.CreateArchiverAsync(rootEntry, _ignoreCache, token);
                    rootArchiverPath = "";
                }
            }
            else
            {
                if (rootEntry.IsArchive())
                {
                    rootArchiver = await ArchiverManager.Current.CreateArchiverAsync(rootEntry, _ignoreCache, token);
                    rootArchiverPath = "";
                }
                else
                {
                    rootArchiver = rootEntry.Archiver;
                    rootArchiverPath = rootEntry.EntryName;
                }
            }

            Archiver = rootArchiver;
            Mode = Archiver.IsFileSystem? _mode : _modeIfArchive;

            var includeSubDirectories = Mode == ArchiveEntryCollectionMode.IncludeSubDirectories || Mode == ArchiveEntryCollectionMode.IncludeSubArchives;
            var entries = await rootArchiver.GetEntriesAsync(rootArchiverPath, includeSubDirectories, token);

            var includeAllSubDirectories = Mode == ArchiveEntryCollectionMode.IncludeSubArchives;
            if (includeAllSubDirectories)
            {
                entries = await GetSubArchivesEntriesAsync(entries, token);
            }

            _entries = entries;
            return _entries;
        }


        private async Task<List<ArchiveEntry>> GetSubArchivesEntriesAsync(List<ArchiveEntry> entries, CancellationToken token)
        {
            var result = new List<ArchiveEntry>();

            foreach (var entry in entries)
            {
                result.Add(entry);

                if (entry.IsArchive())
                {
                    var subArchive = await ArchiverManager.Current.CreateArchiverAsync(entry, _ignoreCache, token);
                    var subEntries = await subArchive.GetEntriesAsync(token);
                    result.AddRange(await GetSubArchivesEntriesAsync(subEntries, token));
                }
            }

            return result;
        }


        // filter: ページとして画像ファイルのみリストアップ
        public async Task<List<ArchiveEntry>> GetEntriesWhereImageAsync(CancellationToken token)
        {
            var entries = await GetEntriesAsync(token);
            return entries.Where(e => e.IsImage()).ToList();
        }

        // filter: ページとして画像ファイルとアーカイブをリストアップ
        public async Task<List<ArchiveEntry>> GetEntriesWhereImageAndArchiveAsync(CancellationToken token)
        {
            var entries = await GetEntriesAsync(token);
            if (Mode == ArchiveEntryCollectionMode.CurrentDirectory)
            {
                return entries.Where(e => e.IsImage() || e.IsBook()).ToList();
            }
            else
            {
                return entries.WherePageAll().Where(e => e.IsImage() || e.IsBook()).ToList();
            }
        }

        // filter: ページとしてすべてのファイルをリストアップ。フォルダーは空きフォルダーのみリストアップ
        public async Task<List<ArchiveEntry>> GetEntriesWherePageAllAsync(CancellationToken token)
        {
            var entries = await GetEntriesAsync(token);
            return entries.WherePageAll().ToList();
        }

        // filter: 含まれるサブアーカイブのみ抽出
        public async Task<List<ArchiveEntry>> GetEntriesWhereSubArchivesAsync(CancellationToken token)
        {
            var entries = await GetEntriesAsync(token);
            return entries.Where(e => e.IsArchive()).ToList();
        }

        // filter: 含まれるブックを抽出
        public async Task<List<ArchiveEntry>> GetEntriesWhereBookAsync(CancellationToken token)
        {
            var entries = await GetEntriesAsync(token);
            if (Mode == ArchiveEntryCollectionMode.CurrentDirectory)
            {
                return entries.Where(e => e.IsBook()).ToList();
            }
            else
            {
                return entries.Where(e => e.IsBook() && !e.IsArchiveDirectory()).ToList();
            }
        }

        /// <summary>
        /// フォルダーリスト上での親フォルダーを取得
        /// </summary>
        public string GetFolderPlace()
        {
            if (Path == null || Archiver == null)
            {
                return null;
            }

            if (Archiver == null)
            {
                Debug.Assert(false, "Invalid operation");
                return null;
            }

            if (Mode == ArchiveEntryCollectionMode.IncludeSubArchives)
            {
                return LoosePath.GetDirectoryName(Archiver.RootArchiver.SystemPath);
            }
            else if (Mode == ArchiveEntryCollectionMode.IncludeSubDirectories)
            {
                if (Archiver.Parent != null)
                {
                    return Archiver.Parent.SystemPath;
                }
                else
                {
                    return LoosePath.GetDirectoryName(Archiver.SystemPath);
                }
            }
            else
            {
                return LoosePath.GetDirectoryName(Path);
            }
        }
    }

    public static class ArchiveEntryCollectionExtensions
    {
        // filter: ディレクトリとなるエントリをすべて除外
        public static IEnumerable<ArchiveEntry> WherePageAll(this IEnumerable<ArchiveEntry> source)
        {
            var directories = source.Select(e => LoosePath.GetDirectoryName(e.SystemPath)).Distinct().ToList();
            return source.Where(e => !directories.Contains(e.SystemPath));
        }
    }
}
