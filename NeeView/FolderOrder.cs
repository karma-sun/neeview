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
    /// フォルダの並び
    /// </summary>
    public enum FolderOrder
    {
        FileName,
        TimeStamp,
        Size,
        Random,
    }

    public static class FolderOrderExtension
    {
        public static FolderOrder GetToggle(this FolderOrder mode)
        {
            return (FolderOrder)(((int)mode + 1) % Enum.GetNames(typeof(FolderOrder)).Length);
        }

        public static string ToDispString(this FolderOrder mode)
        {
            switch (mode)
            {
                case FolderOrder.FileName: return "フォルダ列は名前順";
                case FolderOrder.TimeStamp: return "フォルダ列は日付順";
                case FolderOrder.Size: return "フォルダ列はサイズ順";
                case FolderOrder.Random: return "フォルダ列はシャッフル";
                default:
                    throw new NotSupportedException();
            }
        }

        public static Dictionary<FolderOrder, string> FolderOrderList { get; } = new Dictionary<FolderOrder, string>
        {
            [FolderOrder.FileName] = "名前順",
            [FolderOrder.TimeStamp] = "日付順",
            [FolderOrder.Size] = "サイズ順",
            [FolderOrder.Random] = "シャッフル",
        };
    }
}
