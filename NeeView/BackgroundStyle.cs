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
    // 背景の種類
    public enum BackgroundStyle
    {
        Black,
        White,
        Auto,
        Check,
        Custom
    };

    public static class BackgroundStyleExceptions
    {
        public static BackgroundStyle GetToggle(this BackgroundStyle mode)
        {
            return (BackgroundStyle)(((int)mode + 1) % Enum.GetNames(typeof(BackgroundStyle)).Length);
        }

        public static string ToDispString(this BackgroundStyle mode)
        {
            switch (mode)
            {
                case BackgroundStyle.Black: return "背景を黒色にする";
                case BackgroundStyle.White: return "背景を白色にする";
                case BackgroundStyle.Auto: return "背景を画像に合わせた色にする";
                case BackgroundStyle.Check: return "背景をチェック模様にする";
                case BackgroundStyle.Custom: return "背景をカスタム背景にする";
                default:
                    throw new NotSupportedException();
            }
        }
    }
}
