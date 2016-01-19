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
    // 見開き時のページ並び
    public enum PageReadOrder
    {
        RightToLeft,
        LeftToRight,
    }

    public static class PageReadOrderExtension
    {
        public static PageReadOrder GetToggle(this PageReadOrder mode)
        {
            return (PageReadOrder)(((int)mode + 1) % Enum.GetNames(typeof(PageReadOrder)).Length);
        }

        public static string ToDispString(this PageReadOrder mode)
        {
            switch (mode)
            {
                case PageReadOrder.RightToLeft: return "右開き";
                case PageReadOrder.LeftToRight: return "左開き";
                default:
                    throw new NotSupportedException();
            }
        }
    }
}
