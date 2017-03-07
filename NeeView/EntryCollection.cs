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
        public bool IsAutoRecursive { get; set; }
        
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
            var collection = await CollectAsync(_root, token);

            // sort
            Collection = EntrySort.SortEntries(collection, PageSortMode.FileName);
        }


        /// <summary>
        /// 収拾 (ROOT)
        /// </summary>
        /// <param name="entry"></param>
        /// <returns></returns>
        private async Task<List<ArchiveEntry>> CollectAsync(Archiver archiver, CancellationToken token)
        {
            if (_isRecursived || !archiver.IsFileSystem)
            {
                return await CollectRecursiveAsync(archiver, token);
            }
            else
            {
                var entries = archiver.GetEntries()
                    .Where(e => !ModelContext.BitmapLoaderManager.IsExcludedPath(e.EntryName))
                    .ToList();

                if (IsAutoRecursive && entries.Count == 1 && entries.First().IsArchive())
                {
                    return await CollectAsync(await CreateArchiverAsync(entries.First(), token), token);
                }
                else
                {
                    return entries.Where(e => e.IsImage()).ToList();
                }
            }
        }


        /// <summary>
        /// 収集 (再帰)
        /// </summary>
        /// <param name="archiver"></param>
        /// <param name="token"></param>
        /// <returns></returns>
        private async Task<List<ArchiveEntry>> CollectRecursiveAsync(Archiver archiver, CancellationToken token)
        {
            var collection = new List<ArchiveEntry>();

            var entries = (await archiver.GetEntriesAsync(token))
                .Where(e => !ModelContext.BitmapLoaderManager.IsExcludedPath(e.EntryName))
                .ToList();

            foreach (var entry in entries)
            {
                if (entry.IsArchive())
                {
                    collection.AddRange(await CollectRecursiveAsync(await CreateArchiverAsync(entry, token), token));
                }
                else if (entry.IsImage())
                {
                    collection.Add(entry);
                }
            }

            return collection;
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
