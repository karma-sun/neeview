// Copyright (c) 2016-2017 Mitsuhiro Ito (nee)
//
// This software is released under the MIT License.
// http://opensource.org/licenses/mit-license.php

namespace NeeView
{
    // 長押しモード
    public enum LongButtonDownMode
    {
        None,
        Loupe
    }

    //
    public static class LongButtonDownModeExtensions
    {
        public static string ToTips(this LongButtonDownMode element)
        {
            switch (element)
            {
                default:
                    return null;
                case LongButtonDownMode.Loupe:
                    return "一時的に画像を拡大表示します\nルーペ表示中にホイール操作で拡大率を変更できます";
            }
        }
    }

}
