using System;

namespace NeeView
{
    // 修飾マウスボタン
    [Flags]
    public enum ModifierMouseButtons
    {
        None = 0,
        LeftButton = (1 << 0),
        MiddleButton = (1 << 1),
        RightButton = (1 << 2),
        XButton1 = (1 << 3),
        XButton2 = (1 << 4),
    }
}
