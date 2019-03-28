using System.Collections.Generic;
using System.Linq;

namespace NeeView
{
    public interface IHasPictureSource
    {
        PictureSource PictureSource { get; }
        bool IsPictureSourceLocked { get; }
        void UnloadPictureSource();
    }

    public class PictureSourcePool
    {
        private List<IHasPictureSource> _collection = new List<IHasPictureSource>();
        private object _lock = new object();

        public long TotalSize { get; private set; }

        public void Add(IHasPictureSource content)
        {
            lock (_lock)
            {
                if (!_collection.Contains(content))
                {
                    _collection.Add(content);
                    TotalSize += GetPictureSourceMemorySize(content.PictureSource);
                }
            }
        }

        public void Cleanup()
        {
            lock (_lock)
            {
                ////var c0 = _collection.Count;
                ////var m0 = TotalSize;
                
                foreach (var element in _collection.Where(e => !e.IsPictureSourceLocked))
                {
                    element.UnloadPictureSource();
                }

                _collection = _collection.Where(e => e.PictureSource != null).ToList();
                TotalSize = _collection.Sum(e => GetPictureSourceMemorySize(e.PictureSource));

                ////var c1 = _collection.Count;
                ////var m1 = TotalSize;
                ////Debug.WriteLine($"Cleanup2: PageSourceMemory {m0 / 1024 / 1024}MB({c0}) -> {m1 / 1024 / 1024}MB({c1})");
            }
        }

        // NOTE: 不意のインスタンス喪失に対応
        private long GetPictureSourceMemorySize(PictureSource source)
        {
            return source != null ? source.GetMemorySize() : 0;
        }

        public void Clear()
        {
            lock (_lock)
            {
                _collection.Clear();
                TotalSize = 0;
            }
        }
    }
}
