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
    public class ArchiveEntryExtractorService
    {
        static ArchiveEntryExtractorService() => Current = new ArchiveEntryExtractorService();
        public static ArchiveEntryExtractorService Current { get; }

        #region Fields

        // lock object
        private object _lock = new object();

        /// <summary>
        /// キャンセルされたが処理中のインスタンス群
        /// </summary>
        private Dictionary<string, ArchiveEntryExtractor> _collection = new Dictionary<string, ArchiveEntryExtractor>();

        #endregion

        #region Methods

        /// <summary>
        /// 指定したキーが存在するか
        /// </summary>
        private bool Contains(string key)
        {
            {
                return _collection.ContainsKey(key);
            }
        }

        /// <summary>
        /// 指定したキーの削除
        /// </summary>
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
        private void Add(string key, ArchiveEntryExtractor extractor)
        {
            lock (_lock)
            {
                _collection.Add(key, extractor);
            }
        }


        /// <summary>
        /// 展開
        /// テンポラリファイルはキャッシュを活用する
        /// </summary>
        public async Task<TempFile> ExtractAsync(ArchiveEntry entry, CancellationToken token)
        {
            var tempFile = TempFileCache.Current.Get(entry.Ident);
            if (tempFile != null) return tempFile;

            tempFile = new TempFile(await ExtractRawAsync(entry, token));
            TempFileCache.Current.Add(entry.Ident, tempFile);
            return tempFile;
        }

        /// <summary>
        /// 展開
        /// ファイルはテンポラリに生成される
        /// </summary>
        /// <returns>展開後されたファイル名</returns>
        public async Task<string> ExtractRawAsync(ArchiveEntry entry, CancellationToken token)
        {
            ////Debug.WriteLine($"EXT: {entry.Ident}");

            ArchiveEntryExtractor extractor = null;

            try
            {
                extractor = this.Remove(entry.Ident);
                if (extractor == null || !extractor.IsActive)
                {
                    //Debug.WriteLine("EXT: Extract...");
                    var tempFileName = Temporary.Current.CreateCountedTempFileName("arcv", Path.GetExtension(entry.EntryName));
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

        #endregion
    }
}
