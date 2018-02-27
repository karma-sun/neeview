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
        [AliasName("背景を黒色にする")]
        Black,

        [AliasName("背景を白色にする")]
        White,

        [AliasName("背景を画像に合わせた色にする")]
        Auto,

        [AliasName("背景をチェック模様にする")]
        Check,

        [AliasName("背景をカスタム背景にする")]
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
