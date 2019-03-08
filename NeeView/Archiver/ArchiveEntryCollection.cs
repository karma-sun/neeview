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
    // TODO: AllowPreExtractフラグのちがいをアーカイバの違いとして認識できない問題
    public class ArchiveEntryCollection
    {
        public bool _isRecursive;
        private bool _arrowPreExtract;
        private List<ArchiveEntry> _entries;

        public ArchiveEntryCollection(string path, bool isRecursive, bool arrowPreExtract)
        {
            Path = path;
            _isRecursive = isRecursive;
            _arrowPreExtract = arrowPreExtract;
        }

        public string Path { get; private set; }
        public string Prefix { get; private set; }

        // 作れないときは例外発生
        public async Task<List<ArchiveEntry>> GetEntriesAsync(CancellationToken token)
        {
            if (_entries != null) return _entries;

            var rootEntry = await ArchiveFileSystem.CreateArchiveEntry_New(Path, token);

            Archiver rootArchiver;
            string rootArchiverPath;

            if (rootEntry.IsFileSystem)
            {
                if (rootEntry.IsDirectory)
                {
                    rootArchiver = new FolderArchive(Path, null, true);
                    rootArchiverPath = "";
                }
                else
                {
                    rootArchiver = await ArchiverManager.Current.CreateArchiverAsync(rootEntry, true, _arrowPreExtract, token);
                    rootArchiverPath = "";
                }
            }
            else
            {
                if (rootEntry.IsArchive())
                {
                    rootArchiver = await ArchiverManager.Current.CreateArchiverAsync(rootEntry, true, _arrowPreExtract, token);
                    rootArchiverPath = "";
                }
                else
                {
                    rootArchiver = rootEntry.Archiver;
                    rootArchiverPath = rootEntry.EntryName;
                }
            }

            Prefix = rootArchiverPath;
            var entries = await rootArchiver.GetEntriesAsync(rootArchiverPath, _isRecursive, token);

            if (_isRecursive)
            {
                entries = await GetExpandedEntriesAsync(entries, token);
            }

            _entries = entries;
            return _entries;
        }

        // TODO: このリストはサブアーカイブ展開してもサブアーカイブ自体のエントリがのこっているので、ブックとして使用するときには適切な除外処理が必要
        // - IsArchvieをEntry作成時に設定、メディアを含んだIsArchiveは別メソッドにする
        // - IsBook ... メディアを含めて本として開けるフラグ
        // よって
        // FolderArchiveのディレクトリ > IsDirectory=true, IsArchive=true, IsBook=true
        // 圧縮ファイルの内部フォルダ > IsDirectory=true, IsArchive=false, IsBook=true
        // 圧縮ファイル > IsDirectory=false, IsArchive=true, IsBook=true
        // PDF > IsDirectory=false, IsArchive=true, IsBook=true
        // メディアファイル > IsDirectory=false, IsArchive=false, IsBook=true
        // 画像ファイル > IsDirectory=false, IsArchive=false, IsBook=false, IsPicture=true
        // それ以外のファイル > IsDirectory=false, IsArchive=false, isBook=false, IsPicture=false
        private async Task<List<ArchiveEntry>> GetExpandedEntriesAsync(List<ArchiveEntry> entries, CancellationToken token)
        {
            var result = new List<ArchiveEntry>();

            foreach (var entry in entries)
            {
                result.Add(entry);

                if (entry.IsArchive())
                {
                    var subArchive = await ArchiverManager.Current.CreateArchiverAsync(entry, false, _arrowPreExtract, token);
                    var subEntries = await subArchive.GetEntriesAsync(token);
                    result.AddRange(await GetExpandedEntriesAsync(subEntries, token));
                }
            }

            return result;
        }


        #region 開発用

        // ##
        public static async Task TestAsync()
        {
            try
            {
                var path = @"E:\Work\Labo\サンプル\サブフォルダテストX.zip";
                //var path = @"E:\Work\Labo\サンプル\サブフォルダテストX.zip\サブフォルダテストX";
                //var path = @"E:\Work\Labo\サンプル\サブフォルダテストX.zip\圧縮再帰♥.zip\root";
                //var path = @"E:\Work\Labo\サンプル\サブフォルダテストX.zip\圧縮再帰♥.zip\root\dir2?.zip";

                var collection = new ArchiveEntryCollection(path, true, false);

                Debug.WriteLine($"\n> {collection.Path}");

                var entries = await collection.GetEntriesAsync(CancellationToken.None);

                var prefix = LoosePath.TrimDirectoryEnd(collection.Path);
                DumpEntries("Raw", entries, prefix);

                // filter: ページとして画像ファイルのみリストアップ
                var p1 = entries.Where(e => e.IsImage());
                DumpEntries("ImageFilter", p1, prefix);

                // filter: ページとしてすべてのファイルをリストアップ。フォルダーはk空フォルダーのみリストアップ
                var directories = entries.Select(e => LoosePath.GetDirectoryName(e.SystemPath)).Distinct();
                var p2 = entries.Where(e => !directories.Contains(e.SystemPath));
                DumpEntries("AllPageFilter", p2, prefix);
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex.Message);
            }
        }

        private static void DumpEntries(string label, IEnumerable<ArchiveEntry> entries, string prefix)
        {
            Debug.WriteLine($"\n[{label}]");
            foreach (var entry in entries)
            {
                var attribute = entry.IsDirectory ? "D" : entry.IsArchive() ? "A" : entry.IsImage() ? "I" : "?";
                var name = entry.SystemPath.Substring(prefix.Length);
                Debug.WriteLine(attribute + " " + name);
            }
        }

        #endregion
    }
}
