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
        private static FolderListItemStyle _FolderListItemStyle;
        public static FolderListItemStyle FolderListItemStyle
        {
            get { return _FolderListItemStyle; }
            set
            {
                _FolderListItemStyle = value;
                ThumbnailManager.IsEnabled = _FolderListItemStyle == FolderListItemStyle.Picture;
                FolderListStyleChanged?.Invoke(null, _FolderListItemStyle);
            }
        }

        //
        static PanelContext()
        {
            ThumbnailManager = new ThumbnailManager();
            ThumbnailManager.InitializeThumbnailSystem();
        }
    }

}
