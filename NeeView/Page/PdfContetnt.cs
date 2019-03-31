using System.Windows;

namespace NeeView
{
    /// <summary>
    /// PDFページコンテンツ
    /// 今のところ画像コンテンツと同じ
    /// </summary>
    public class PdfContetnt : BitmapContent
    {
        public PdfContetnt(ArchiveEntry entry) : base(entry)
        {
        }

        public override bool CanResize => true;

        public override Size GetRenderSize(Size size)
        {
            return size;
        }
    }
}
