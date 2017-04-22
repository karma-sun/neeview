// Copyright (c) 2016 Mitsuhiro Ito (nee)
//
// This software is released under the MIT License.
// http://opensource.org/licenses/mit-license.php

using Microsoft.Expression.Media.Effects;
using NeeLaboratory.Windows.Property;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media;
using System.Windows.Media.Effects;

namespace NeeView.Effects
{
    //
    public enum EffectType
    {
        None,
        Level,
        Hsv,
        ColorSelect,
        Blur,
        Bloom,
        Monochrome,
        ColorTone,
        Sharpen,
        Embossed,
        Pixelate,
        Magnify,
        Ripple,
        Swirl,
    }

    public static class EffectTypeExtensions
    {
        public static Dictionary<EffectType, string> EffectTypeNames { get; private set; } = new Dictionary<EffectType, string>
        {
            [EffectType.None] = "なし (標準)",
            [EffectType.Level] = "レベル補正",
            [EffectType.Hsv] = "色相、彩度、明度",
            [EffectType.ColorSelect] = "色選択",
            [EffectType.Blur] = "ぼかし",
            [EffectType.Bloom] = "ブルーム",
            [EffectType.Monochrome] = "モノクローム",
            [EffectType.ColorTone] = "カラートーン",
            [EffectType.Sharpen] = "シャープ",
            [EffectType.Embossed] = "エンボス",
            [EffectType.Pixelate] = "ピクセレート",
            [EffectType.Magnify] = "拡大鏡",
            [EffectType.Ripple] = "波紋",
            [EffectType.Swirl] = "渦巻き",
        };

        public static string ToDispString(this EffectType my)
        {
            string name;
            EffectTypeNames.TryGetValue(my, out name);
            return name ?? my.ToString();
        }
    }
}
