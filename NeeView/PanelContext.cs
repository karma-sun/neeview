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
    /// パネル共通
    /// </summary>
    public static class PanelContext
    {
        //
        public static event EventHandler<FolderListItemStyle> FolderListStyleChanged;

        //
        public static ThumbnailManager ThumbnailManager { get; private set; }

        //
        private static FolderListItemStyle s_folderListItemStyle;
        public static FolderListItemStyle FolderListItemStyle
        {
            get { return s_folderListItemStyle; }
            set
            {
                s_folderListItemStyle = value;
                ThumbnailManager.IsEnabled = s_folderListItemStyle.HasThumbnail();
                FolderListStyleChanged?.Invoke(null, s_folderListItemStyle);
            }
        }



        //
        public static event EventHandler<FolderListItemStyle> PageListStyleChanged;

        //
        public static ThumbnailManager PageThumbnailManager { get; private set; }

        /// <summary>
        /// PageListItemStyle property.
        /// </summary>
        private static FolderListItemStyle s_pageListItemStyle;
        public static FolderListItemStyle PageListItemStyle
        {
            get { return s_pageListItemStyle; }
            set
            {
                s_pageListItemStyle = value;
                PageThumbnailManager.IsEnabled = s_pageListItemStyle.HasThumbnail();
                PageListStyleChanged?.Invoke(null, s_pageListItemStyle);
            }
        }



        //
        static PanelContext()
        {
            ThumbnailManager = new ThumbnailManager(QueueElementPriority.FolderThumbnail);
            PageThumbnailManager = new ThumbnailManager(QueueElementPriority.PageListThumbnail);
        }
    }
}
