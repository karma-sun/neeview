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
    public interface IHasPageContent
    {
        PageContent ContentAccessor { get; }
        int Index { get; }

        void UnloadContent();
    }

    public class PageContentPool
    {
        private List<IHasPageContent> _collection = new List<IHasPageContent>();
        private object _lock = new object();
        private int _referenceIndex;


        public long TotalSize { get; private set; }


        public void SetReference(int index)
        {
            _referenceIndex = index;
        }

        public void Add(IHasPageContent element)
        {
            lock (_lock)
            {
                ////Debug.WriteLine($"Add: {page}");
                _collection.Add(element);
                TotalSize = TotalSize + element.ContentAccessor.GetContentMemorySize();
            }
        }

        public void Cleanup(long limitSize)
        {
            List<IHasPageContent> elements = null;
            List<IHasPageContent> removes = null;

            lock (_lock)
            {
                long totalMemory = 0;

                elements = _collection
                    .Distinct()
                    .OrderByDescending(e => e.ContentAccessor.IsContentLocked)
                    .ThenBy(e => Math.Abs(e.Index - _referenceIndex))
                    .ToList();

                foreach (var (element, index) in elements.ToTuples())
                {
                    var size = element.ContentAccessor.GetContentMemorySize();
                    if (totalMemory + size > limitSize && !element.ContentAccessor.IsContentLocked)
                    {
                        removes = elements.Skip(index).ToList();
                        elements = elements.Take(index).ToList();
                        break;
                    }

                    totalMemory += size;
                }

                ////var removeCount = removes != null ? removes.Count : 0;
                ////var contentCount = elements.Count;
                ////Debug.WriteLine($"Cleanup1: {totalMemory / 1024 / 1024}MB, {removeCount}/{contentCount}");

                _collection = elements;
                TotalSize = totalMemory;
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
                TotalSize = 0;
            }

            _referenceIndex = 0;
        }
    }

}
