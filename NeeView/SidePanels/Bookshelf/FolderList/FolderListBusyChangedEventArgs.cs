using System;

namespace NeeView
{
    public class FolderListBusyChangedEventArgs : EventArgs
    {
        public bool IsBusy { get; set; }

        public FolderListBusyChangedEventArgs(bool isBusy)
        {
            this.IsBusy = isBusy;
        }
    }

}
