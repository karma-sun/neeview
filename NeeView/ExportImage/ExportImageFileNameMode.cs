//using System.Drawing;

namespace NeeView
{
    public enum ExportImageFileNameMode
    {
        [AliasName("@EnumExportImageFileNameModeOriginal")]
        Original,

        [AliasName("@EnumExportImageFileNameModeBookPageNumber")]
        BookPageNumber,

        [AliasName("@EnumExportImageFileNameModeDeault", IsVisibled = false)]
        Default,
    }
}
