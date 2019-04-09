using System;
using System.Threading;

namespace NeeView
{
    public class ViewContentSourceCollectionChangedEventArgs : EventArgs
    {
        public ViewContentSourceCollectionChangedEventArgs(ViewContentSourceCollection viewPageCollection)
        {
            ViewPageCollection = viewPageCollection;
        }

        public ViewContentSourceCollection ViewPageCollection { get; set; }
        public bool IsForceResize { get; set; }
        public CancellationToken CancellationToken { get; set; }
    }

}

