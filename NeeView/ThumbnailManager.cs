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
    /// ページ保持インターフェイス
    /// サムネイル管理で使用されるリストの項目に必要なインターフェイス
    /// </summary>
    public interface IHasPage
    {
        Page GetPage();
    }

    /// <summary>
    /// サムネイル管理
    /// リストから指定範囲のサムネイルをロードし、上限に達したら古いものから削除していきます
    /// </summary>
    public class ThumbnailManager
    {
        // サムネイル有効リスト
        private AliveThumbnailList _AliveThumbnailList = new AliveThumbnailList();

        private double ThumbnailSize = 256;
        private int ThumbnailMemorySize = 4;

        //
        public void InitializeThumbnailSystem()
        {
            FolderInfo.ThumbnailChanged += (s, e) => _AliveThumbnailList.Add(e);
            BookMementoUnit.ThumbnailChanged += (s, e) => _AliveThumbnailList.Add(e);
        }

        //
        #region Property: IsEnabled
        private bool _IsEnabled;
        public bool IsEnabled
        {
            get { return _IsEnabled; }
            set
            {
                _IsEnabled = value;
                if (!_IsEnabled) ClearThumbnail();
            }
        }
        #endregion

        // サムネイル要求
        public void RequestThumbnail<T>(ICollection<T> collection, int start, int count, int margin, int direction) where T : IHasPage
        {
            if (!_IsEnabled) return;

            //Debug.WriteLine($"{start}+{count}");

            if (collection == null || ThumbnailSize < 8.0) return;

            // 未処理の要求を解除
            ModelContext.JobEngine.Clear(QueueElementPriority.FolderThumbnail);

            // 有効サムネイル数制限
            LimitThumbnail();

            // 要求
            int center = start + count / 2;
            var pages = Enumerable.Range(start - margin, count + margin * 2 - 1)
                .Where(i => i >= 0 && i < collection.Count)
                .Select(e => collection.ElementAt(e));

            foreach (var page in direction < 0 ? pages.Reverse() : pages)
            {
                page.GetPage()?.OpenThumbnail(QueueElementPriority.FolderThumbnail, ThumbnailSize);
            }
        }

        // 有効サムネイル数制限
        private void LimitThumbnail()
        {
            int limit = (ThumbnailMemorySize * 1024 * 1024) / ((int)ThumbnailSize * (int)ThumbnailSize);
            _AliveThumbnailList.Limited(limit);
        }

        // サムネイル破棄
        private void ClearThumbnail()
        {
            _AliveThumbnailList.Clear();
        }
    }

}
