// Copyright (c) 2016 Mitsuhiro Ito (nee)
//
// This software is released under the MIT License.
// http://opensource.org/licenses/mit-license.php

using Microsoft.Expression.Media.Effects;
using NeeLaboratory.Property;
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
        Blur,
        Bloom,
        Monochrome,
        ColorTone,
        Embossed,
        Pixelate,
        Sharpen,
        Magnify,
        Ripple,
        Swirl,

       // MyGrayscale,
    }

    public static class EffectTypeExtensions
    {
        public static Dictionary<EffectType, string> EffectTypeNames { get; private set; } = new Dictionary<EffectType, string>
        {
            [EffectType.None] = "なし (標準)",
            [EffectType.Blur] = "ぼかし",
            [EffectType.Bloom] = "ブルーム照明",
            [EffectType.Monochrome] = "モノクロ",
            [EffectType.ColorTone] = "カラートーン",
            [EffectType.Embossed] = "エンボス",
            [EffectType.Pixelate] = "モザイク",
            [EffectType.Sharpen] = "シャープ",
            [EffectType.Magnify] = "拡大鏡",
            [EffectType.Ripple] = "波紋",
            [EffectType.Swirl] = "渦巻き",

            //[EffectType.MyGrayscale] = "グレイスケール",
        };

        public static string ToDispString(this EffectType my)
        {
            string name;
            EffectTypeNames.TryGetValue(my, out name);
            return name ?? my.ToString();
        }
    }
    
}
