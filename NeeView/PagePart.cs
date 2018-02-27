namespace NeeView
{
    // 表示ページ情報
    public class PagePart
    {
        public PagePart(PagePosition position, int partSize, PageReadOrder partOrder)
        {
            Position = position;
            PartSize = partSize;
            PartOrder = partOrder;
        }

        public PagePosition Position { get; }
        public int PartSize { get; }
        public PageReadOrder PartOrder { get; }
    }
}
