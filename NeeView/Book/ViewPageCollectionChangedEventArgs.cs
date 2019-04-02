using System;

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
    }


}

