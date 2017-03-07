// Copyright (c) 2016 Mitsuhiro Ito (nee)
//
// This software is released under the MIT License.
// http://opensource.org/licenses/mit-license.php

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace NeeView
{
    /// <summary>
    /// ページ収拾
    /// TODO: ページ指定時の最適化
    /// TODO: BOOKと共有
    /// </summary>
    public class EntryCollection : IDisposable
    {
        /// <summary>
        /// 収集されたエントリ群
        /// </summary>
        public List<ArchiveEntry> Collection { get; set; }

        // system property
        // TODO: preference化
        public static bool IsAutoRecursive { get; set; }

        /// <summary>
        /// ごみ箱
        /// </summary>
        public TrashBox _trashBox = new TrashBox();

        /// <summary>
        /// 基準アーカイブ
        /// </summary>
        private Archiver _root;

        /// <summary>
        /// 再帰フラグ
        /// </summary>
        private bool _isRecursived;

        /// <summary>
        /// コンストラクタ
        /// </summary>
        /// <param name="root">基準アーカイブ</param>
        /// <param name="isRecursived">再帰フラグ</param>
        public EntryCollection(Archiver root, bool isRecursived)
        {
            _root = root;
            _isRecursived = isRecursived;
        }

        /// <summary>
        /// 収拾
        /// </summary>
        /// <param name="token"></param>
        /// <returns></returns>
        public async Task CollectAsync(CancellationToken token)
        {
            // collect
            var param = new CollectParams(CollectType.All);
            var collection = await CollectAsync(_root, param, token);

            // sort
            // TODO: ここか？
            Collection = EntrySort.SortEntries(collection, PageSortMode.FileName);
        }

        /// <summary>
        /// 先頭ページのみ取得
        /// </summary>
        /// <param name="token"></param>
        /// <returns></returns>
        public async Task FirstOneAsync(CancellationToken token)
        {
            // first
            var param = new CollectParams(CollectType.FirstOne);
            var collection = await CollectAsync(_root, param, token);

            Collection = collection;
        }

        /// <summary>
        /// 指定したページのみ取得
        /// </summary>
        /// <param name="entryName"></param>
        /// <param name="token"></param>
        /// <returns></returns>
        public async Task SelectAsync(string entryName, CancellationToken token)
        {
            // select
            var param = new CollectParams(CollectType.Select) { SelectEntryName = entryName };
            var collection = await CollectAsync(_root, param, token);

            Collection = collection;
        }

        //
        private enum CollectType
        {
            All,
            FirstOne,
            Select,
        }

        //
        private class CollectParams
        {
            public CollectType Type { get; set; }
            public string SelectEntryName { get; set; }

            public CollectParams(CollectType type)
            {
                Type = type;
            }
        }


        /// <summary>
        /// 収拾 (ROOT)
        /// </summary>
        /// <param name="entry"></param>
        /// <returns></returns>
        private async Task<List<ArchiveEntry>> CollectAsync(Archiver archiver, CollectParams param, CancellationToken token)
        {
            if (_isRecursived || !archiver.IsFileSystem)
            {
                return await CollectRecursiveAsync(archiver, true, param, token);
            }
            else
            {
                var entries = archiver.GetEntries()
                    .Where(e => !ModelContext.BitmapLoaderManager.IsExcludedPath(e.EntryName))
                    .ToList();

                if (IsAutoRecursive && entries.Count == 1 && entries.First().IsArchive())
                {
                    return await CollectAsync(await CreateArchiverAsync(entries.First(), token), param, token);
                }
                else
                {
                    return await CollectRecursiveAsync(archiver, false, param, token);
                }
            }
        }


        /// <summary>
        /// 収拾
        /// </summary>
        /// <param name="archiver"></param>
        /// <param name="param"></param>
        /// <param name="token"></param>
        /// <returns></returns>
        private async Task<List<ArchiveEntry>> CollectRecursiveAsync(Archiver archiver, bool isRecursive, CollectParams param, CancellationToken token)
        {
            switch (param.Type)
            {
                default:
                case CollectType.All:
                    return await CollectRecursiveAsync(archiver, isRecursive, token);
                case CollectType.FirstOne:
                    return await FirstRecursiveAsync(archiver, isRecursive, token);
                case CollectType.Select:
                    return await SelectRecursiveAsync(archiver, isRecursive, param.SelectEntryName, token);
            }
        }
        

        /// <summary>
        /// 収集 (再帰)
        /// </summary>
        /// <param name="archiver"></param>
        /// <param name="token"></param>
        /// <returns></returns>
        private async Task<List<ArchiveEntry>> CollectRecursiveAsync(Archiver archiver, bool isRecursive, CancellationToken token)
        {
            var collection = new List<ArchiveEntry>();

            var entries = (await archiver.GetEntriesAsync(token))
                .Where(e => !ModelContext.BitmapLoaderManager.IsExcludedPath(e.EntryName))
                .ToList();

            foreach (var entry in entries)
            {
                if (entry.IsArchive() && isRecursive)
                {
                    collection.AddRange(await CollectRecursiveAsync(await CreateArchiverAsync(entry, token), isRecursive, token));
                }
                else if (entry.IsImage())
                {
                    collection.Add(entry);
                }
            }

            return collection;
        }


        /// <summary>
        /// 代表 (再帰)
        /// </summary>
        /// <param name="archiver"></param>
        /// <param name="entryName"></param>
        /// <param name="token"></param>
        /// <returns></returns>
        private async Task<List<ArchiveEntry>> FirstRecursiveAsync(Archiver archiver, bool isRecursive, CancellationToken token)
        {
            var collection = new List<ArchiveEntry>();

            var entries = (await archiver.GetEntriesAsync(token))
                .Where(e => !ModelContext.BitmapLoaderManager.IsExcludedPath(e.EntryName))
                .ToList();

            // sort
            entries = EntrySort.SortEntries(entries, PageSortMode.FileName);

            var select = entries.FirstOrDefault(e => e.IsImage());
            if (select != null) return new List<ArchiveEntry>() { select };

            if (isRecursive)
            {
                foreach (var entry in entries.Where(e => e.IsArchive()))
                {
                    var collect = await FirstRecursiveAsync(await CreateArchiverAsync(entry, token), isRecursive, token);
                    if (collect.Any()) return collect;
                }
            }

            return new List<ArchiveEntry>();
        }


        /// <summary>
        /// 選択 (再帰)
        /// </summary>
        /// <param name="archiver"></param>
        /// <param name="entryName"></param>
        /// <param name="token"></param>
        /// <returns></returns>
        private async Task<List<ArchiveEntry>> SelectRecursiveAsync(Archiver archiver, bool isRecursive, string entryName, CancellationToken token)
        {
            // エントリ取得
            var entries = await archiver.GetEntriesAsync(token);

            // 一致するエントリを探す
            var entry = entries.FirstOrDefault(e => e.EntryFullName == entryName);
            if (entry != null) return new List<ArchiveEntry>() { entry };

            // 一致しなかった場合、最長一致するサブフォルダで再帰
            if (isRecursive)
            {
                var folder = entries
                    .Where(e => e.IsArchive() && entryName.StartsWith(LoosePath.TrimEnd(e.EntryFullName)))
                    .OrderByDescending(e => e.EntryName.Length)
                    .FirstOrDefault();

                if (folder == null) return new List<ArchiveEntry>();

                return await SelectRecursiveAsync(await CreateArchiverAsync(folder, token), isRecursive, entryName, token);
            }

            return new List<ArchiveEntry>();
        }



        /// <summary>
        /// サブアーカイブ作成
        /// 作られたアーカイブはTrashBoxで寿命管理される
        /// </summary>
        /// <param name="entry"></param>
        /// <param name="token"></param>
        /// <returns></returns>
        private async Task<Archiver> CreateArchiverAsync(ArchiveEntry entry, CancellationToken token)
        {
            Archiver archiver;
            if (entry.IsFileSystem)
            {
                archiver = ModelContext.ArchiverManager.CreateArchiver(entry.GetFileSystemPath(), entry);
            }
            else
            {
                string tempFileName = await ArchivenEntryExtractorService.Current.ExtractAsync(entry, token);
                _trashBox.Add(new TempFile(tempFileName));
                archiver = ModelContext.ArchiverManager.CreateArchiver(tempFileName, entry);
            }

            _trashBox.Add(archiver);
            return archiver;
        }

        /// <summary>
        /// Dispose
        /// </summary>
        public void Dispose()
        {
            if (_trashBox.Any())
            {
                _trashBox.Clear();
            }
        }
    }
}
