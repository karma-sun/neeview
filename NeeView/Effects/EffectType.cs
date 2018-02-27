using Microsoft.Expression.Media.Effects;
using NeeView.Windows.Property;
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
        [AliasName("なし")]
        None,

        [AliasName("レベル補正")]
        Level,

        [AliasName("色相、彩度、明度")]
        Hsv,

        [AliasName("色選択")]
        ColorSelect,

        [AliasName("ぼかし")]
        Blur,

        [AliasName("ブルーム")]
        Bloom,

        [AliasName("モノクローム")]
        Monochrome,

        [AliasName("カラートーン")]
        ColorTone,

        [AliasName("シャープ")]
        Sharpen,

        [AliasName("エンボス")]
        Embossed,

        [AliasName("ピクセレート")]
        Pixelate,

        [AliasName("拡大鏡")]
        Magnify,

        [AliasName("波紋")]
        Ripple,

        [AliasName("渦巻き")]
        Swirl,
    }
}
