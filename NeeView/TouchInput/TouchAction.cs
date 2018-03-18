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

    //
    public static class TouchActionExtensions
    {
        public static Dictionary<TouchAction, string> TouchActionTips = new Dictionary<TouchAction, string>
        {
            [TouchAction.None] = null,
            [TouchAction.Drag] = "タッチによるビュー操作です",
            [TouchAction.MouseDrag] = "マウスの左ボタンドラッグと同じビュー操作です",
            [TouchAction.Gesture] = "ジェスチャー入力です"
        };
    }
}
