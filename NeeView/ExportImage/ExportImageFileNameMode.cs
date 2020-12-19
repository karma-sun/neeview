//using System.Drawing;

namespace NeeView
{
    public enum ExportImageFileNameMode
    {
        [AliasName]
        Original,

        [AliasName]
        BookPageNumber,

        [AliasName(IsVisibled = false)]
        Default,
    }
}
