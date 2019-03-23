using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media.Imaging;

namespace NeeView
{
    public class PageContentPool
    {
        // TODO: Bookで持つ？
        static PageContentPool() => Current = new PageContentPool();
        public static PageContentPool Current { get; }


        private List<Page> _collection = new List<Page>();
        private object _lock = new object();
        private Page _referencePage;


        private PageContentPool()
        {
        }

        public void SetReference(Page page)
        {
            _referencePage = page;
        }

        public void Add(Page page)
        {
            lock (_lock)
            {
                _collection.Add(page);
            }

            long limitSize = (long)BookProfile.Current.CacheMemorySize * 1024 * 1024;
            Cleanup(limitSize);
        }


        public void Cleanup(long limitSize)
        {
            List<Page> pages = null;
            List<Page> removes = null;

            lock (_lock)
            {
                var sw = Stopwatch.StartNew();

                long totalMemory = 0;

                int referenceIndex = _referencePage != null ? _referencePage.Index : 0;

                pages = _collection
                    .Distinct()
                    .OrderBy(e => e.IsLocked)
                    .ThenByDescending(e => Math.Abs(e.Index - referenceIndex))
                    .ToList();

                for (int i = pages.Count - 1; i >= 0; --i)
                {
                    var page = pages[i];

                    totalMemory += page.Content.GetMemorySize();
                    if (totalMemory > limitSize && !page.IsLocked)
                    {
                        removes = pages.Take(i + 1).ToList();
                        pages = pages.Skip(i + 1).ToList();
                        break;
                    }
                }

                var removeCount = removes != null ? removes.Count : 0;
                var contentCount = pages.Count;

                Debug.WriteLine($"Cleanup: {totalMemory / 1024 / 1024}MB, {removeCount}/{contentCount}, {sw.ElapsedMilliseconds:#,0}ms");
            }

            _collection = pages;

            if (removes != null)
            {
                foreach (var page in removes)
                {
                    page.UnloadContent();
                }
            }
        }

        public void Clear()
        {
            lock (_lock)
            {
                _collection.Clear();
            }

            _referencePage = null;
        }
    }
}
