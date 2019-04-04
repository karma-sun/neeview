namespace NeeView
{
    /// <summary>
    /// BookPageViewer, BookPageViewGenerater setting
    /// </summary>
    public class BookPageViewSetting
    {
        public PageMode PageMode { get; set; }
        public PageReadOrder BookReadOrder { get; set; }
        public bool IsSupportedDividePage { get; set; }
        public bool IsSupportedSingleFirstPage { get; set; }
        public bool IsSupportedSingleLastPage { get; set; }
        public bool IsSupportedWidePage { get; set; }
    }


}
