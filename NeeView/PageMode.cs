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

        public static string ToDispString(this PageMode mode)
        {
            switch (mode)
            {
                case PageMode.SinglePage: return "単ページ表示";
                case PageMode.WidePage: return "見開き表示";
                default:
                    throw new NotSupportedException();
            }
        }

        public static int Size(this PageMode mode)
        {
            return mode == PageMode.WidePage ? 2 : 1;
        }
    }

}
