// Copyright (c) 2016-2017 Mitsuhiro Ito (nee)
//
// This software is released under the MIT License.
// http://opensource.org/licenses/mit-license.php


using System.Collections.Generic;

namespace NeeView
{
    // タッチアクション
    public enum TouchAction
    {
        None,
        Drag,
        MouseDrag,
        Gesture,
    }

    //
    public static class TouchActionExtensions
    {
        public static Dictionary<TouchAction, string> TouchActionList = new Dictionary<TouchAction, string>
        {
            [TouchAction.None] = "なし",
            [TouchAction.Drag] = "タッチビュー操作",
            [TouchAction.MouseDrag] = "マウスドラッグ操作",
            [TouchAction.Gesture] = "ジェスチャー"
        };

        public static Dictionary<TouchAction, string> TouchActionLimitedList = new Dictionary<TouchAction, string>
        {
            [TouchAction.Drag] = TouchActionList[TouchAction.Drag],
            [TouchAction.MouseDrag] = TouchActionList[TouchAction.MouseDrag],
            [TouchAction.Gesture] = TouchActionList[TouchAction.Gesture],
        };

        public static Dictionary<TouchAction, string> TouchActionTips = new Dictionary<TouchAction, string>
        {
            [TouchAction.None] = null,
            [TouchAction.Drag] = "タッチによるビュー操作です",
            [TouchAction.MouseDrag] = "マウスの左ボタンドラッグと同じビュー操作です",
            [TouchAction.Gesture] = "ジェスチャー入力です"
        };

        public static string ToDispString(this TouchAction element)
        {
            return TouchActionList[element];
        }
    }
}
