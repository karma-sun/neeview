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
        private ArchiveEntryCollectionOption _option;
        private List<ArchiveEntry> _entries;
        private int _prefixLength;

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
            ModeIfArchive = modeIfArchive;
            _option = option;

            _prefixLength = LoosePath.TrimDirectoryEnd(Path).Length;
        }

        public string Path { get; }
        public ArchiveEntryCollectionMode Mode { get; }
        public ArchiveEntryCollectionMode ModeIfArchive { get; }
        public Archiver Archiver { get; private set; }

        /// <summary>
        /// ArchiveEntry収集
        /// </summary>
        public async Task<List<ArchiveEntry>> GetEntriesAsync(CancellationToken token)
        {
            if (_entries != null) return _entries;

            var rootEntry = await ArchiveEntryUtility.CreateArchiveEntryAsync(Path, token);

            Archiver rootArchiver;
            string rootArchiverPath;

            if (rootEntry.IsFileSystem)
            {
                if (rootEntry.IsDirectory)
                {
                    rootArchiver = await ArchiverManager.Current.CreateArchiverAsync(new ArchiveEntry(Path), token);
                    rootArchiverPath = "";
                }
                else
                {
                    rootArchiver = await ArchiverManager.Current.CreateArchiverAsync(rootEntry, token);
                    rootArchiverPath = "";
                }
            }
            else
            {
                if (rootEntry.IsArchive())
                {
                    rootArchiver = await ArchiverManager.Current.CreateArchiverAsync(rootEntry, token);
                    rootArchiverPath = "";
                }
                else
                {
                    rootArchiver = rootEntry.Archiver;
                    rootArchiverPath = rootEntry.EntryName;
                }
            }

            Archiver = rootArchiver;

            var mode = Archiver.IsFileSystem ? Mode : ModeIfArchive;

            var includeSubDirectories = mode == ArchiveEntryCollectionMode.IncludeSubDirectories || mode == ArchiveEntryCollectionMode.IncludeSubArchives;
            var entries = await rootArchiver.GetEntriesAsync(rootArchiverPath, includeSubDirectories, token);

            var includeAllSubDirectories = mode == ArchiveEntryCollectionMode.IncludeSubArchives;
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
                    var subArchive = await ArchiverManager.Current.CreateArchiverAsync(entry, token);
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

        // filter: ページとしてすべてのファイルをリストアップ。フォルダーは空きフォルダーのみリストアップ
        public async Task<List<ArchiveEntry>> GetEntriesWherePageAllAsync(CancellationToken token)
        {
            var entries = await GetEntriesAsync(token);
            var directories = entries.Select(e => LoosePath.GetDirectoryName(e.SystemPath)).Distinct();
            return entries.Where(e => !directories.Contains(e.SystemPath)).ToList();
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
            return entries.Where(e => e.IsBook()).ToList();
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

            var mode = Archiver.IsFileSystem ? Mode : ModeIfArchive;

            if (mode == ArchiveEntryCollectionMode.IncludeSubArchives)
            {
                return LoosePath.GetDirectoryName(Archiver.RootArchiver.SystemPath);
            }
            else if (mode == ArchiveEntryCollectionMode.IncludeSubDirectories)
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
}
