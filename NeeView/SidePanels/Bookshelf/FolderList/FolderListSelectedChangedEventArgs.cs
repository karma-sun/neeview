using System;

namespace NeeView
{
    public class FolderListSelectedChangedEventArgs : EventArgs
    {
        public bool IsFocus { get; set; }
        public bool IsNewFolder { get; set; }
    }

}
