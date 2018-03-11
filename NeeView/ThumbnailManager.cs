using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NeeView
{
    /// <summary>
    /// ページ保持インターフェイス
    /// サムネイル管理で使用されるリストの項目に必要なインターフェイス
    /// </summary>
    public interface IHasPage
    {
        Page GetPage();
    }

    /// <summary>
    /// サムネイル管理
    /// リストから指定範囲のサムネイルをロードします
    /// </summary>
    public class ThumbnailManager
    {
        // system object
        public static ThumbnailManager _current;
        public static ThumbnailManager Current { get { return _current = _current ?? new ThumbnailManager(); } }

        // サムネイル要求
        public void RequestThumbnail(IEnumerable<IHasPage> collection, QueueElementPriority priority, int start, int count, int margin, int direction) //where T : IHasPage
        {
            if (collection == null) return;

            ////bool isCollection = collection is System.Collections.ICollection;
            ////Debug.WriteLine($"RequestThumbnail: {priority}, ({start}, {count}, {margin}) {collection.GetType().Name}");

            // 未処理の要求を解除
            JobEngine.Current.Clear(priority);

            // 要求
            int center = start + count / 2;
            int collectionCount = collection.Count();
            int rangeStart = Math.Max(0, start - margin);
            int rangeCount = Math.Min(count + margin * 2, collectionCount - rangeStart);

            // 更新数０以下ならば何もしない
            if (rangeCount <= 0) return;

            var pages = Enumerable.Range(rangeStart, rangeCount)
                .Where(i => i >= 0 && i < collectionCount)
                .Select(e => collection.ElementAt(e));

            foreach (var page in direction < 0 ? pages.Reverse() : pages)
            {
                page.GetPage()?.LoadThumbnail(priority);
            }
        }
    }



    /// <summary>
    /// ThumbnaulPool for Panel
    /// </summary>
    public class BookThumbnailPool : ThumbnailPool
    {
        public static BookThumbnailPool _current;
        public static BookThumbnailPool Current
        {
            get
            {
                _current = _current ?? new BookThumbnailPool();
                return _current;
            }
        }

        public override int Limit => ThumbnailProfile.Current.BookCapacity;
    }
}
