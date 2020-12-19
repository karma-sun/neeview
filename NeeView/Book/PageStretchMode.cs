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
        [AliasName]
        None,

        [AliasName]
        Uniform,

        [AliasName]
        UniformToFill,

        [AliasName]
        UniformToSize,

        [AliasName]
        UniformToVertical,

        [AliasName]
        UniformToHorizontal,
    }
    
    #region Obsolete

    // 旧・画像のストレッチモード
    [Obsolete]
    public enum PageStretchModeV1
    {
        None,
        Inside,
        Outside,
        Uniform,
        UniformToFill,
        UniformToSize,
        UniformToVertical,
        UniformToHorizontal,
    }

    [Obsolete]
    public static class PageStretchModeV1Extension
    {
        public static PageStretchMode ToPageStretchMode(this PageStretchModeV1 self)
        {
            switch (self)
            {
                default:
                case PageStretchModeV1.None:
                case PageStretchModeV1.Inside:
                case PageStretchModeV1.Outside:
                    return PageStretchMode.None;
                case PageStretchModeV1.Uniform:
                    return PageStretchMode.Uniform;
                case PageStretchModeV1.UniformToFill:
                    return PageStretchMode.UniformToFill;
                case PageStretchModeV1.UniformToSize:
                    return PageStretchMode.UniformToSize;
                case PageStretchModeV1.UniformToVertical:
                    return PageStretchMode.UniformToVertical;
                case PageStretchModeV1.UniformToHorizontal:
                    return PageStretchMode.UniformToHorizontal;
            }
        }
    }

    #endregion Obsolete
}
