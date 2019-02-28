using System;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace NeeView
{
    public static class DebugCreateBookThumbnail
    {
        /// <summary>
        /// ブックサムネイル作成負荷計測
        /// </summary>
        public static async Task TestAsync()
        {
            Thumbnail.DebugIgnoreCache = true;

            var mres = new ManualResetEventSlim(false);
            ////var sw = Stopwatch.StartNew();
            
            DebugTimer.Start("CreateBookThumbnail", isSilent: true);

            var items = BookshelfFolderList.Current.FolderCollection.Items.OfType<FileFolderItem>().Take(100);
            foreach (var item in items)
            {
                Debug.WriteLine($"{item.Path.DispName}...");
                DebugTimer.CheckRestart();
                item.ThumbnailLoaded += Item_ThumbnailLoaded;
                mres.Reset();

                var page = item.GetPage();
                page.Thumbnail.Clear();
                page.LoadThumbnail(QueueElementPriority.BookmarkThumbnail);
                await mres.WaitHandle.WaitOneAsync();

                item.ThumbnailLoaded -= Item_ThumbnailLoaded;
                DebugTimer.Check("Complete");
            }

            DebugTimer.Result();
            ////Debug.WriteLine($"TestTime: {sw.ElapsedMilliseconds:#,0}");
            ////Debug.WriteLine($"ItemCount: {items.Count()} thumb.");

            mres.Dispose();
            Thumbnail.DebugIgnoreCache = false;

            DebugTimer.Stop();

            void Item_ThumbnailLoaded(object sender, EventArgs e)
            {
                mres.Set();
            }
        }

        private static Task WaitOneAsync(this WaitHandle waitHandle)
        {
            if (waitHandle == null)
                throw new ArgumentNullException("waitHandle");

            var tcs = new TaskCompletionSource<bool>();
            var rwh = ThreadPool.RegisterWaitForSingleObject(waitHandle,
                delegate { tcs.TrySetResult(true); }, null, -1, true);
            var t = tcs.Task;
            t.ContinueWith((antecedent) => rwh.Unregister(null));
            return t;
        }
    }
}
