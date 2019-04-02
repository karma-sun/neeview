using System;

namespace NeeView
{
    // ページ関係のイベントパラメータ
    public class PageChangedEventArgs : EventArgs
    {
        public PageChangedEventArgs(Page page)
        {
            Page = page;
        }

        public Page Page { get; set; }
    }


}

