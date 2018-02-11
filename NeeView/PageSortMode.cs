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
    // ページ整列
    public enum PageSortMode
    {
        [AliasName("ファイル名昇順")]
        FileName,

        [AliasName("ファイル名降順")]
        FileNameDescending,

        [AliasName("日付昇順")]
        TimeStamp,

        [AliasName("日付降順")]
        TimeStampDescending,

        [AliasName("シャッフル")]
        Random,
    }

    public static class PageSortModeExtension
    {
        public static PageSortMode GetToggle(this PageSortMode mode)
        {
            return (PageSortMode)(((int)mode + 1) % Enum.GetNames(typeof(PageSortMode)).Length);
        }

        //
        public static Dictionary<PageSortMode, string> PageSortModeList { get; } = new Dictionary<PageSortMode, string>
        {
            [PageSortMode.FileName] = "ファイル名昇順",
            [PageSortMode.FileNameDescending] = "ファイル名降順",
            [PageSortMode.TimeStamp] = "日付昇順",
            [PageSortMode.TimeStampDescending] = "日付降順",
            [PageSortMode.Random] = "シャッフル",
        };

        //
        public static string ToDispString(this PageSortMode mode)
        {
            return PageSortModeList[mode];
        }

    }
}
