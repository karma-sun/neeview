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
        [AliasName("@EnumPageStretchModeNone")]
        None,

        [AliasName("@EnumPageStretchModeInside")]
        Inside,

        [AliasName("@EnumPageStretchModeOutside")]
        Outside,

        [AliasName("@EnumPageStretchModeUniform")]
        Uniform,

        [AliasName("@EnumPageStretchModeUniformToFill")]
        UniformToFill,

        [AliasName("@EnumPageStretchModeUniformToSize")]
        UniformToSize,

        [AliasName("@EnumPageStretchModeUniformToVertical")]
        UniformToVertical,
    }
}
