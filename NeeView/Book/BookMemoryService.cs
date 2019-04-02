using NeeLaboratory.ComponentModel;

namespace NeeView
{
    /// <summary>
    /// ブックのメモリ管理
    /// </summary>
    public class BookMemoryService : BindableBase
    {
        private PageContentPool _contentPool = new PageContentPool();
        private PictureSourcePool _pictureSourcePool = new PictureSourcePool();

        public long LimitSize => (long)BookProfile.Current.CacheMemorySize * 1024 * 1024;

        public long TotalSize => _contentPool.TotalSize + _pictureSourcePool.TotalSize;

        public bool IsFull => TotalSize > LimitSize;


        public void SetReference(int index)
        {
            _contentPool.SetReference(index);
        }

        public void AddPageContent(IHasPageContent content)
        {
            _contentPool.Add(content);

            _contentPool.Cleanup(LimitSize - _pictureSourcePool.TotalSize);
            if (IsFull)
            {
                _pictureSourcePool.Cleanup();
            }

            RaisePropertyChanged("");
        }

        public void AddPictureSource(IHasPictureSource pictureSource)
        {
            _pictureSourcePool.Add(pictureSource);

            RaisePropertyChanged("");
        }

        /// <summary>
        /// OutOfMemory発生時の不活性メモリ開放処理
        /// </summary>
        public void CleanupDeep()
        {
            _contentPool.Cleanup(0);
            _pictureSourcePool.Cleanup();
        }

        public void Clear()
        {
            _contentPool.Clear();
            _pictureSourcePool.Clear();

            RaisePropertyChanged("");
        }
    }
}
