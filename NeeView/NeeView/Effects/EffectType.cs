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
        [AliasName]
        None,

        [AliasName]
        Level,

        [AliasName]
        Hsv,

        [AliasName]
        ColorSelect,

        [AliasName]
        Blur,

        [AliasName]
        Bloom,

        [AliasName]
        Monochrome,

        [AliasName]
        ColorTone,

        [AliasName]
        Sharpen,

        [AliasName]
        Embossed,

        [AliasName]
        Pixelate,

        [AliasName]
        Magnify,

        [AliasName]
        Ripple,

        [AliasName]
        Swirl,
    }
}
