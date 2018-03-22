using System.Collections.Generic;

namespace NeeView
{
    // タッチアクション
    public enum TouchAction
    {
        [AliasName("@EnumTouchActionNone")]
        None,

        [AliasName("@EnumTouchActionDrag")]
        Drag,

        [AliasName("@EnumTouchActionMouseDrag")]
        MouseDrag,

        [AliasName("@EnumTouchActionGesture")]
        Gesture,
    }
}
