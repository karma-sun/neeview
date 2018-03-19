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
        [AliasName("@EnumPageReadOrderRightToLeft")]
        RightToLeft,

        [AliasName("@EnumPageReadOrderLeftToRight")]
        LeftToRight,
    }

    public static class PageReadOrderExtension
    {
        public static PageReadOrder GetToggle(this PageReadOrder mode)
        {
            return (PageReadOrder)(((int)mode + 1) % Enum.GetNames(typeof(PageReadOrder)).Length);
        }
    }
}
