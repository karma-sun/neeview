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
        [AliasName("@EnumPageModeSinglePage")]
        SinglePage,

        [AliasName("@EnumPageModeWidePage")]
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
