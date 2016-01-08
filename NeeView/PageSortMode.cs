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
    // ページ整列
    public enum PageSortMode
    {
        FileName,
        TimeStamp,
        Random,
    }

    public static class PageSortModeExtension
    {
        public static PageSortMode GetToggle(this PageSortMode mode)
        {
            return (PageSortMode)(((int)mode + 1) % Enum.GetNames(typeof(PageSortMode)).Length);
        }

        public static string ToDispString(this PageSortMode mode)
        {
            switch (mode)
            {
                case PageSortMode.FileName: return "ファイル名順";
                case PageSortMode.TimeStamp: return "日付順";
                case PageSortMode.Random: return "ランダムに並べる";
                default:
                    throw new NotSupportedException();
            }
        }
    }


}
