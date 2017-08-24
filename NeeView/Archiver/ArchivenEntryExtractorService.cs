// Copyright (c) 2016-2017 Mitsuhiro Ito (nee)
//
// This software is released under the MIT License.
// http://opensource.org/licenses/mit-license.php

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
    /// ArchiveEntryExtractor管理
    /// キャンセルされたがまだ処理が残っているインスタンスの再利用
    /// </summary>
    public class ArchivenEntryExtractorService
    {
        /// <summary>
        ///  現在のシステムオブジェクト
        /// </summary>
        public static ArchivenEntryExtractorService _current;
        public static ArchivenEntryExtractorService Current
        {
            get
            {
                _current = _current ?? new ArchivenEntryExtractorService();
                return _current;
            }
        }

        // lock object
        private object _lock = new object();

        /// <summary>
        /// キャンセルされたが処理中のインスタンス群
        /// </summary>
        private Dictionary<string, ArchiveEntryExtractor> _collection = new Dictionary<string, ArchiveEntryExtractor>();

        /// <summary>
        /// 指定したキーが存在するか
        /// </summary>
        /// <param name="key"></param>
        /// <returns></returns>
        private bool Contains(string key)
        {
            {
                return _collection.ContainsKey(key);
            }
        }

        /// <summary>
        /// 指定したキーの削除
        /// </summary>
        /// <param name="key"></param>
        /// <returns>削除されたオブジェクトを返す。ない場合はnull</returns>
        private ArchiveEntryExtractor Remove(string key)
        {
            lock (_lock)
            {
                ArchiveEntryExtractor extractor;
                if (_collection.TryGetValue(key, out extractor))
                {
                    _collection.Remove(key);
                    return extractor;
                }
                else
                {
                    return null;
                }
            }
        }

        /// <summary>
        /// キーの追加
        /// </summary>
        /// <param name="key"></param>
        /// <param name="extractor"></param>
        private void Add(string key, ArchiveEntryExtractor extractor)
        {
            lock (_lock)
            {
                _collection.Add(key, extractor);
            }
        }

        /// <summary>
        /// 展開
        /// ファイルはテンポラリに生成される
        /// </summary>
        /// <param name="entry"></param>
        /// <param name="token"></param>
        /// <returns>展開後されたファイル名</returns>
        public async Task<string> ExtractAsync(ArchiveEntry entry, CancellationToken token)
        {
            //Debug.WriteLine($"EXT: Collection = {_collection.Count}");
            Debug.WriteLine($"EXT: {entry.Ident}");

            ArchiveEntryExtractor extractor = null;

            try
            {
                extractor = this.Remove(entry.Ident);
                if (extractor == null || !extractor.IsActive)
                {
                    //Debug.WriteLine("EXT: Extract...");
                    var tempFileName = Temporary.CreateCountedTempFileName("arcv", Path.GetExtension(entry.EntryName));
                    extractor = new ArchiveEntryExtractor(entry);
                    extractor.Completed += Extractor_Completed;
                    return await extractor.ExtractAsync(tempFileName, token);
                }
                else
                {
                    //Debug.WriteLine("EXT: Continue...");
                    return await extractor.WaitAsync(token);
                }
            }
            catch (OperationCanceledException)
            {
                //Debug.WriteLine("EXT: Add to Reserver");
                this.Add(entry.Ident, extractor);
                throw;
            }
        }

        /// <summary>
        /// 展開後処理
        /// 不要ならば展開ファイルを削除
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Extractor_Completed(object sender, ArchiveEntryExtractorEventArgs e)
        {
            var key = (sender as ArchiveEntryExtractor)?.Entry.Ident;
            if (key == null) return;

            var extractor = Remove(key);
            if (extractor != null && e.CancellationToken.IsCancellationRequested)
            {
                //Debug.WriteLine($"EXT: delete {extractor.ExtractFileName} with cancel.");
                if (File.Exists(extractor.ExtractFileName))
                {
                    File.Delete(extractor.ExtractFileName);
                }
            }
        }
    }
    
}
