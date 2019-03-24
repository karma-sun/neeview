using NeeView.Collections.Generic;
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


        public bool IsFull { get; private set; }


        private long GetLimitSize()
        {
            return (long)BookProfile.Current.CacheMemorySize * 1024 * 1024;
        }


        public void SetReference(Page page)
        {
            _referencePage = page;
            IsFull = false;
        }

        public void Add(Page page)
        {
            lock (_lock)
            {
                ////Debug.WriteLine($"Add: {page}");
                _collection.Add(page);
            }

            Cleanup(GetLimitSize());
        }

        private void Cleanup(long limitSize)
        {
            List<Page> pages = null;
            List<Page> removes = null;

            lock (_lock)
            {
                ////var sw = Stopwatch.StartNew();

                long totalMemory = 0;

                // level 1 cleanup
                {
                    int referenceIndex = _referencePage != null ? _referencePage.Index : 0;

                    pages = _collection
                        .Distinct()
                        .OrderByDescending(e => e.State)
                        .ThenBy(e => Math.Abs(e.Index - referenceIndex))
                        .ToList();

                    foreach (var (page, index) in pages.ToTuples())
                    {
                        var size = page.Content.GetMemorySize();
                        if (totalMemory + size > limitSize && page.State == PageState.None)
                        {
                            removes = pages.Skip(index).ToList();
                            pages = pages.Take(index).ToList();
                            break;
                        }

                        totalMemory += size;
                    }

                    ////var removeCount = removes != null ? removes.Count : 0;
                    ////var contentCount = pages.Count;
                    ////Debug.WriteLine($"Cleanup1: {totalMemory / 1024 / 1024}MB, {removeCount}/{contentCount}, {sw.ElapsedMilliseconds:#,0}ms");
                }

                IsFull = totalMemory > limitSize;

                _collection = pages;
            }

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
