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
    }
}
