// Copyright (c) 2016-2018 Mitsuhiro Ito (nee)
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
        [AliasName(PageStretchModeExtension.PageStretchMode_None)]
        None,

        [AliasName(PageStretchModeExtension.PageStretchMode_Inside)]
        Inside,

        [AliasName(PageStretchModeExtension.PageStretchMode_Outside)]
        Outside,

        [AliasName(PageStretchModeExtension.PageStretchMode_Uniform)]
        Uniform,

        [AliasName(PageStretchModeExtension.PageStretchMode_UniformToFill)]
        UniformToFill,

        [AliasName(PageStretchModeExtension.PageStretchMode_UniformToSize)]
        UniformToSize,

        [AliasName(PageStretchModeExtension.PageStretchMode_UniformToVertical)]
        UniformToVertical,
    }

    public static class PageStretchModeExtension
    {
        public const string PageStretchMode_None = "オリジナルサイズ";
        public const string PageStretchMode_Inside = "大きい場合ウィンドウサイズに合わせる";
        public const string PageStretchMode_Outside = "小さい場合ウィンドウサイズに合わせる";
        public const string PageStretchMode_Uniform = "ウィンドウサイズに合わせる";
        public const string PageStretchMode_UniformToFill = "ウィンドウいっぱいに広げる";
        public const string PageStretchMode_UniformToSize = "面積をウィンドウに合わせる";
        public const string PageStretchMode_UniformToVertical = "高さをウィンドウに合わせる";
    }
}
