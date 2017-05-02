// Copyright (c) 2016 Mitsuhiro Ito (nee)
//
// This software is released under the MIT License.
// http://opensource.org/licenses/mit-license.php

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NeeView
{
    /// <summary>
    /// ページ保持インターフェイス
    /// サムネイル管理で使用されるリストの項目に必要なインターフェイス
    /// </summary>
    public interface IHasPage
    {
        Page GetPage();
    }

    /// <summary>
    /// サムネイル管理
    /// リストから指定範囲のサムネイルをロードします
    /// </summary>
    public class ThumbnailManager
    {


        // サムネイル要求
        public void RequestThumbnail<T>(ICollection<T> collection, QueueElementPriority priority, int start, int count, int margin, int direction) where T : IHasPage
        {
            if (collection == null) return;

            Debug.WriteLine($"RequestThumbnail: {priority} ({start} - {start + count})");

            // 未処理の要求を解除
            ModelContext.JobEngine.Clear(priority);

            // 要求
            int center = start + count / 2;
            var pages = Enumerable.Range(start - margin, count + margin * 2 - 1)
                .Where(i => i >= 0 && i < collection.Count)
                .Select(e => collection.ElementAt(e));

            foreach (var page in direction < 0 ? pages.Reverse() : pages)
            {
                page.GetPage()?.LoadThumbnail(priority);
            }
        }
    }



    /// <summary>
    /// ThumbnaulPool for Panel
    /// </summary>
    public class PanelThumbnailPool : ThumbnailPool
    {
        public static PanelThumbnailPool _current;
        public static PanelThumbnailPool Current
        {
            get
            {
                _current = _current ?? new PanelThumbnailPool();
                return _current;
            }
        }

        public override int Limit => Preference.Current.thumbnail_folder_capacity;
    }
}
