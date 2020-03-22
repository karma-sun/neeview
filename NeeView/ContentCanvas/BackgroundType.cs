using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace NeeView
{
    [Obsolete]
    public enum BackgroundStyleV1
    {
        Black,
        White,
        Auto,
        Check,
        Custom,
    }

    /// <summary>
    /// 背景の種類
    /// </summary>
    public enum BackgroundType
    {
        [AliasName("@EnumBackgroundStyleBlack")]
        Black,

        [AliasName("@EnumBackgroundStyleWhite")]
        White,

        [AliasName("@EnumBackgroundStyleAuto")]
        Auto,

        [AliasName("@EnumBackgroundStyleCheck")]
        Check,

        [AliasName("@EnumBackgroundStyleCheckDark")]
        CheckDark,

        [AliasName("@EnumBackgroundStyleCustom")]
        Custom
    };

    public static class BackgroundStyleExceptions
    {
        public static BackgroundType GetToggle(this BackgroundType mode)
        {
            return (BackgroundType)(((int)mode + 1) % Enum.GetNames(typeof(BackgroundType)).Length);
        }
    }
}
