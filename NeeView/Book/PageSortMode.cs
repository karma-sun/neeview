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
        [AliasName]
        FileName,

        [AliasName]
        FileNameDescending,

        [AliasName]
        TimeStamp,

        [AliasName]
        TimeStampDescending,

        [AliasName]
        Size,

        [AliasName]
        SizeDescending,

        [AliasName]
        Random,
    }

    public static class PageSortModeExtension
    {
        public static PageSortMode GetToggle(this PageSortMode mode)
        {
            return (PageSortMode)(((int)mode + 1) % Enum.GetNames(typeof(PageSortMode)).Length);
        }

        public static bool IsFileNameCategory(this PageSortMode mode)
        {
            return mode == PageSortMode.FileName || mode == PageSortMode.FileNameDescending;
        }
    }
}
