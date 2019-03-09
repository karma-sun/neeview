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
    [Flags]
    public enum ArchiveEntryCollectionOption
    {
        None,

        /// <summary>
        /// 事前展開許可
        /// </summary>
        AllowPreExtract,
    }

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

    // TODO: Archiver Pool
    // TODO: AllowPreExtractフラグのちがいをアーカイバの違いとして認識できない問題。あとでPreExtractできるようにする
    // TODO: ArchiverのIsRootフラグ不要？
    public class ArchiveEntryCollection
    {
        private ArchiveEntryCollectionMode _mode;
        private ArchiveEntryCollectionMode _archiveMode;
        private ArchiveEntryCollectionOption _option;
        private List<ArchiveEntry> _entries;
        private int _prefixLength;

        public ArchiveEntryCollection(string path, ArchiveEntryCollectionMode mode, ArchiveEntryCollectionMode archiveMode, ArchiveEntryCollectionOption option)
        {
            Path = LoosePath.TrimEnd(path);
            _mode = mode;
            _archiveMode = archiveMode;
            _option = option;

            _prefixLength = LoosePath.TrimDirectoryEnd(Path).Length;
        }

        public string Path { get; private set; }

        public Archiver RootArchiver { get; private set; }

        // 作れないときは例外発生
        public async Task<List<ArchiveEntry>> GetEntriesAsync(CancellationToken token)
        {
            if (_entries != null) return _entries;

            var allowPreExtract = _option.HasFlag(ArchiveEntryCollectionOption.AllowPreExtract);
            var rootEntry = await ArchiveFileSystem.CreateArchiveEntry_New(Path, false, token); // ## あとからallowPreExtractを実行できるようにする

            Archiver rootArchiver;
            string rootArchiverPath;

            if (rootEntry.IsFileSystem)
            {
                if (rootEntry.IsDirectory)
                {
                    rootArchiver = new FolderArchive(Path, null);
                    rootArchiverPath = "";
                }
                else
                {
                    rootArchiver = await ArchiverManager.Current.CreateArchiverAsync(rootEntry, allowPreExtract, token);
                    rootArchiverPath = "";
                }
            }
            else
            {
                if (rootEntry.IsArchive())
                {
                    rootArchiver = await ArchiverManager.Current.CreateArchiverAsync(rootEntry, allowPreExtract, token);
                    rootArchiverPath = "";
                }
                else
                {
                    rootArchiver = rootEntry.Archiver;
                    rootArchiverPath = rootEntry.EntryName;
                }
            }

            RootArchiver = rootArchiver;

            var mode = RootArchiver.IsFileSystem ? _mode : _archiveMode;

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

        // filter: 含まれるサブアーカイブ、メディアのみ抽出
        public async Task<List<ArchiveEntry>> GetEntriesWhereSubArchivesAsync(CancellationToken token)
        {
            var entries = await GetEntriesAsync(token);
            return entries.Where(e => e.IsBook()).ToList();
            // TODO: e.IsBook()はおかしい。今後は圧縮ファイルの中のフォルダーもブックになるから、新しい設計が必要。
        }


        // TODO: このリストはサブアーカイブ展開してもサブアーカイブ自体のエントリがのこっているので、ブックとして使用するときには適切な除外処理が必要
        private async Task<List<ArchiveEntry>> GetSubArchivesEntriesAsync(List<ArchiveEntry> entries, CancellationToken token)
        {
            var result = new List<ArchiveEntry>();

            foreach (var entry in entries)
            {
                result.Add(entry);

                if (entry.IsArchive())
                {
                    var subArchive = await ArchiverManager.Current.CreateArchiverAsync(entry, _option.HasFlag(ArchiveEntryCollectionOption.AllowPreExtract), token);
                    var subEntries = await subArchive.GetEntriesAsync(token);
                    result.AddRange(await GetSubArchivesEntriesAsync(subEntries, token));
                }
            }

            return result;
        }


        public string GetEntryName(ArchiveEntry entry)
        {
            Debug.Assert(_entries != null);
            Debug.Assert(_entries.Contains(entry));

            return entry.SystemPath.Substring(_prefixLength);
        }
    }
}
