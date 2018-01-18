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
    // ページ表示モード
    public enum PageMode
    {
        SinglePage,
        WidePage,
    }

    public static class PageModeExtension
    {
        public static PageMode GetToggle(this PageMode mode)
        {
            return (PageMode)(((int)mode + 1) % Enum.GetNames(typeof(PageMode)).Length);
        }

        //
        public static Dictionary<PageMode, string> PageModeList { get; } = new Dictionary<PageMode, string>
        {
            [PageMode.SinglePage] = "1ページ表示",
            [PageMode.WidePage] = "2ページ表示",
        };

        public static string ToDispString(this PageMode mode)
        {
            return PageModeList[mode];
        }

        public static int Size(this PageMode mode)
        {
            return mode == PageMode.WidePage ? 2 : 1;
        }
    }
}
