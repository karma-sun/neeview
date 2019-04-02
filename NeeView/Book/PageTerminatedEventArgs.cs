using System;

namespace NeeView
{
    // ページ終端イベント
    public class PageTerminatedEventArgs : EventArgs
    {
        public PageTerminatedEventArgs(int direction)
        {
            Direction = direction;
        }

        public int Direction { get; set; }
    }


}

