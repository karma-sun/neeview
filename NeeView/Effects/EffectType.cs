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
        [AliasName("@EnumEffectTypeNone")]
        None,

        [AliasName("@EnumEffectTypeLevel")]
        Level,

        [AliasName("@EnumEffectTypeHsv")]
        Hsv,

        [AliasName("@EnumEffectTypeColorSelect")]
        ColorSelect,

        [AliasName("@EnumEffectTypeBlur")]
        Blur,

        [AliasName("@EnumEffectTypeBloom")]
        Bloom,

        [AliasName("@EnumEffectTypeMonochrome")]
        Monochrome,

        [AliasName("@EnumEffectTypeColorTone")]
        ColorTone,

        [AliasName("@EnumEffectTypeSharpen")]
        Sharpen,

        [AliasName("@EnumEffectTypeEmbossed")]
        Embossed,

        [AliasName("@EnumEffectTypePixelate")]
        Pixelate,

        [AliasName("@EnumEffectTypeMagnify")]
        Magnify,

        [AliasName("@EnumEffectTypeRipple")]
        Ripple,

        [AliasName("@EnumEffectTypeSwirl")]
        Swirl,
    }
}
