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
    /// アーカイブ内パスを含むパス記述に対応した処理
    /// </summary>
    public static class ArchiveEntryUtility
    {
        /// <summary>
        /// パスから完全なArcvhiveEntryを作成
        /// </summary>
        public static async Task<ArchiveEntry> CreateAsync(string path, CancellationToken token)
        {
            var query = new QueryPath(path);

            if (File.Exists(path) || Directory.Exists(path))
            {
                return ArchiveEntry.Create(path);
            }
            else if (query.Scheme == QueryScheme.Pagemark)
            {
                if (query.Path == null)
                {
                    return ArchiveEntry.Create(query);
                }
                else
                {
                    var archiver = await ArchiverManager.Current.CreateArchiverAsync(ArchiveEntry.Create(new QueryPath(QueryScheme.Pagemark)), false, token);
                    var entries = await archiver.GetEntriesAsync(token);
                    var entry = entries.FirstOrDefault(e => e.EntryName == query.FileName);
                    if (entry != null)
                    {
                        return entry;
                    }
                }
            }
            else
            {
                try
                {
                    var parts = LoosePath.Split(path);
                    string archivePath = null;

                    foreach (var part in parts)
                    {
                        archivePath = LoosePath.Combine(archivePath, part);

                        if (File.Exists(archivePath))
                        {
                            var archiver = await ArchiverManager.Current.CreateArchiverAsync(ArchiveEntry.Create(archivePath), false, token);
                            var entries = await archiver.GetEntriesAsync(token);

                            var entryName = path.Substring(archivePath.Length).TrimStart(LoosePath.Separator);
                            var entry = entries.FirstOrDefault(e => e.EntryName == entryName);
                            if (entry != null)
                            {
                                return entry;
                            }
                            else
                            {
                                return await CreateInnerAsync(archiver, entryName, token);
                            }
                        }
                    }
                }
                catch (Exception ex)
                {
                    throw new FileNotFoundException(string.Format(Properties.Resources.FileNotFoundException_Message, path), ex);
                }
            }

            throw new FileNotFoundException(string.Format(Properties.Resources.FileNotFoundException_Message, path));
        }

        /// <summary>
        /// アーカイブ内のエントリーを返す。
        /// 入れ子になったアーカイブの場合、再帰処理する。
        /// </summary>
        private static async Task<ArchiveEntry> CreateInnerAsync(Archiver archiver, string entryName, CancellationToken token)
        {
            var entries = await archiver.GetEntriesAsync(token);

            var entry = entries.FirstOrDefault(e => e.EntryName == entryName);
            if (entry != null) return entry;

            var parts = LoosePath.Split(entryName);
            string archivePath = null;

            foreach (var part in parts)
            {
                archivePath = LoosePath.Combine(archivePath, part);

                entry = entries.FirstOrDefault(e => e.EntryName == archivePath && e.IsArchive());
                if (entry != null)
                {
                    var subArchiver = await ArchiverManager.Current.CreateArchiverAsync(entry, false, token);
                    var subEntryName = entryName.Substring(archivePath.Length).TrimStart(LoosePath.Separator);
                    return await CreateInnerAsync(subArchiver, subEntryName, token);
                }
            }

            throw new FileNotFoundException();
        }

        /// <summary>
        /// ブックサムネイルに使用するエントリを取得
        /// </summary>
        /// <param name="source">対象のエントリ</param>
        /// <param name="depth">検索範囲</param>
        public static async Task<ArchiveEntry> CreateFirstImageArchiveEntryAsync(ArchiveEntry source, int depth, CancellationToken token)
        {
            try
            {
                List<ArchiveEntry> entries;
                if (!source.IsFileSystem && source.IsDirectory)
                {
                    entries = (await source.Archiver.GetEntriesAsync(token))
                        .Where(e => e.EntryName.StartsWith(LoosePath.TrimDirectoryEnd(source.EntryName)))
                        .ToList();
                }
                else
                {
                    var archiver = await ArchiverManager.Current.CreateArchiverAsync(source, false, token);
                    entries = await archiver.GetEntriesAsync(token);
                }
                entries = EntrySort.SortEntries(entries, PageSortMode.FileName);

                var select = entries.FirstOrDefault(e => e.IsImage());
                if (select != null)
                {
                    return select;
                }

                if (depth > 1)
                {
                    // NOTE: 検索サブディレクトリ数もdepthで制限
                    foreach (var entry in entries.Where(e => e.IsArchive()).Take(depth))
                    {
                        select = await CreateFirstImageArchiveEntryAsync(entry, depth - 1, token);
                        if (select != null)
                        {
                            return select;
                        }
                    }
                }
            }
            catch (OperationCanceledException)
            {
                throw;
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex.Message);
            }

            return null;
        }

        /// <summary>
        /// アーカイブパスの存在チェック
        /// </summary>
        /// <param name="path"></param>
        /// <param name="token"></param>
        /// <returns></returns>
        public static async Task<bool> ExistsAsync(string path, CancellationToken token)
        {
            try
            {
                var entry = await CreateAsync(path, token);
                return entry != null;
            }
            catch (FileNotFoundException)
            {
                return false;
            }
        }


        /// <summary>
        /// 実在するディレクトリまで遡る
        /// </summary>
        /// <param name="path"></param>
        /// <returns></returns>
        public static string GetExistDirectoryName(string path)
        {
            if (Directory.Exists(path))
            {
                return path;
            }

            while (!string.IsNullOrEmpty(path))
            {
                path = LoosePath.GetDirectoryName(path);
                if (Directory.Exists(path))
                {
                    return path;
                }
            }

            return null;
        }

        /// <summary>
        /// 実在するエントリーまで遡る
        /// </summary>
        /// <param name="path"></param>
        /// <returns></returns>
        public static string GetExistEntryName(string path)
        {
            if (Directory.Exists(path) || File.Exists(path))
            {
                return path;
            }

            while (!string.IsNullOrEmpty(path))
            {
                path = LoosePath.GetDirectoryName(path);
                if (Directory.Exists(path) || File.Exists(path))
                {
                    return path;
                }
            }

            return null;
        }

    }

}
