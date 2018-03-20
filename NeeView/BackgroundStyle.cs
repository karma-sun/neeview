using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NeeView
{
    // 背景の種類
    public enum BackgroundStyle
    {
        [AliasName("@EnumBackgroundStyleBlack")]
        Black,

        [AliasName("@EnumBackgroundStyleWhite")]
        White,

        [AliasName("@EnumBackgroundStyleAuto")]
        Auto,

        [AliasName("@EnumBackgroundStyleCheck")]
        Check,

        [AliasName("@EnumBackgroundStyleCustom")]
        Custom
    };

    public static class BackgroundStyleExceptions
    {
        public static BackgroundStyle GetToggle(this BackgroundStyle mode)
        {
            return (BackgroundStyle)(((int)mode + 1) % Enum.GetNames(typeof(BackgroundStyle)).Length);
        }
    }
}
