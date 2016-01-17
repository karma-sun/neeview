using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NeeView
{
    public enum PageMode
    {
        DividePage = 0,
        SinglePage = 1,
        WidePage = 2,
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
                case PageMode.DividePage: return "分割ページ表示";
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
