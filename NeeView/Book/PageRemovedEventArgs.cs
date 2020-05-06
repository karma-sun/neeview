using System;
using System.Collections.Generic;

namespace NeeView
{
    /// <summary>
    /// ページ削除のインベントパラメータ
    /// </summary>
    public class PageRemovedEventArgs : EventArgs
    {
        public PageRemovedEventArgs(List<Page> pages)
        {
            Pages = pages;
        }

        public List<Page> Pages { get; set; }
    }

}

