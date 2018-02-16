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
        [AliasName("1ページ表示")]
        SinglePage,

        [AliasName("2ページ表示")]
        WidePage,
    }

    public static class PageModeExtension
    {
        public static PageMode GetToggle(this PageMode mode)
        {
            return (PageMode)(((int)mode + 1) % Enum.GetNames(typeof(PageMode)).Length);
        }

        public static int Size(this PageMode mode)
        {
            return mode == PageMode.WidePage ? 2 : 1;
        }
    }
}
