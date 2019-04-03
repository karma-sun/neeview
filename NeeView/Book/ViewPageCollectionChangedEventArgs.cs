using System;
using System.Threading;

namespace NeeView
{
    // 表示コンテンツ変更イベント
    public class ViewPageCollectionChangedEventArgs : EventArgs
    {
        public ViewPageCollectionChangedEventArgs(ViewPageCollection viewPageCollection)
        {
            ViewPageCollection = viewPageCollection;
        }

        public ViewPageCollection ViewPageCollection { get; set; }
        public bool IsForceResize { get; set; }
        public CancellationToken CancellationToken { get; set; }
    }


}

