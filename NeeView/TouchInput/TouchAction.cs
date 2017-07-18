// Copyright (c) 2016 Mitsuhiro Ito (nee)
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
        Gesture,
    }

    //
    public static class TouchActionExtensions
    {
        public static Dictionary<TouchAction, string> TouchActionList = new Dictionary<TouchAction, string>
        {
            [TouchAction.None] = "なし",
            [TouchAction.Drag] = "ビュー操作",
            [TouchAction.Gesture] = "ジェスチャー"
        };

        public static string ToDispString(this TouchAction element)
        {
            return TouchActionList[element];
        }
    }
}
