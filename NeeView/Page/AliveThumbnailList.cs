// Copyright (c) 2016 Mitsuhiro Ito (nee)
//
// This software is released under the MIT License.
// http://opensource.org/licenses/mit-license.php

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NeeView
{
    /// <summary>
    /// サムネイル有効ページ管理
    /// </summary>
    public class AliveThumbnailList : IDisposable
    {
        private LinkedList<Page> _list = new LinkedList<Page>();

        private object _lock = new object();

        // サムネイル有効ページを追加
        public void Add(Page page)
        {
            if (page.Thumbnail != null)
            {
                lock (_lock)
                {
                    _list.AddFirst(page);
                }
            }
        }

        // サムネイル全開放
        public void Clear()
        {
            lock (_lock)
            {
                foreach (var page in _list)
                {
                    page.CloseThumbnail();
                }
                _list.Clear();
            }
        }

        // 終了処理
        public void Dispose()
        {
            Clear();
        }

        // 有効数を超えるサムネイルは古いものから無効にする
        public void Limited(int limit)
        {
            lock (_lock)
            {
                while (_list.Count > limit)
                {
                    var page = _list.Last();
                    page.CloseThumbnail();

                    _list.RemoveLast();
                }
            }
        }
    }

}
