// Copyright (c) 2016 Mitsuhiro Ito (nee)
//
// This software is released under the MIT License.
// http://opensource.org/licenses/mit-license.php

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NeeView
{
    // 画像のストレッチモード
    public enum PageStretchMode
    {
        None, // もとの大きさ
        Inside,  // もとの大きさ、大きい場合はウィンドウサイズに合わせる
        Outside, // もとの大きさ、小さい場合はウィンドウサイズに合わせる
        Uniform, // ウィンドウサイズに合わせる
        UniformToFill, // ウィンドウいっぱいに広げる
    }

    public static class PageStretchModeExtension
    {
        // トグル
        public static PageStretchMode GetToggle(this PageStretchMode mode)
        {
            return (PageStretchMode)(((int)mode + 1) % Enum.GetNames(typeof(PageStretchMode)).Length);
        }

        // 表示名
        private static Dictionary<PageStretchMode, string> _DispStrings = new Dictionary<PageStretchMode, string>
        {
            [PageStretchMode.None] = "オリジナルサイズ",
            [PageStretchMode.Inside] = "大きい場合ウィンドウサイズに合わせる",
            [PageStretchMode.Outside] = "小さい場合ウィンドウサイズに合わせる",
            [PageStretchMode.Uniform] = "ウィンドウサイズに合わせる",
            [PageStretchMode.UniformToFill] = "ウィンドウいっぱいに広げる",
        };

        // 表示名取得
        public static string ToDispString(this PageStretchMode mode)
        {
            return _DispStrings[mode];
        }
    }

}
