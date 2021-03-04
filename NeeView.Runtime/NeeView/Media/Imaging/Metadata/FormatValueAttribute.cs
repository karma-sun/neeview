using System;

namespace NeeView.Media.Imaging.Metadata
{
    [Flags]
    public enum FormatValueAttribute
    {
        None = 0,

        // 可能なら実数として扱う
        Numetrical = (1 << 0),

        // 分数を約分する
        Reduction = (1 << 1), 
    }

}
