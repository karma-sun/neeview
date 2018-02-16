// Copyright (c) 2016-2018 Mitsuhiro Ito (nee)
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
    /// フォルダーの並び
    /// </summary>
    public enum FolderOrder
    {
        [AliasName("ブック列は名前順")]
        FileName,

        [AliasName("ブック列は日付順")]
        TimeStamp,

        [AliasName("ブック列はサイズ順")]
        Size,

        [AliasName("ブック列はシャッフル")]
        Random,
    }

    public static class FolderOrderExtension
    {
        public static FolderOrder GetToggle(this FolderOrder mode)
        {
            return (FolderOrder)(((int)mode + 1) % Enum.GetNames(typeof(FolderOrder)).Length);
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
