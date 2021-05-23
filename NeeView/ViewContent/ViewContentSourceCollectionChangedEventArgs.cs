using System;
using System.Threading;

namespace NeeView
{
    public class ViewContentSourceCollectionChangedEventArgs : EventArgs
    {
        public ViewContentSourceCollectionChangedEventArgs(string bookAddress, ViewContentSourceCollection viewPageCollection)
        {
            BookAddress = bookAddress;
            ViewPageCollection = viewPageCollection ?? throw new ArgumentNullException(nameof(viewPageCollection));
        }

        public string BookAddress { get; set; }
        public ViewContentSourceCollection ViewPageCollection { get; set; }
        public bool IsForceResize { get; set; }
        public bool IsFirst { get; set; }
        public CancellationToken CancellationToken { get; set; }
    }

}

