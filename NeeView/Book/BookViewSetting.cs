namespace NeeView
{
    /// <summary>
    /// BookViewer, BookViewGenerater setting
    /// </summary>
    public class BookViewSetting
    {
        public PageMode PageMode { get; set; }
        public PageReadOrder BookReadOrder { get; set; }
        public bool IsSupportedDividePage { get; set; }
        public bool IsSupportedSingleFirstPage { get; set; }
        public bool IsSupportedSingleLastPage { get; set; }
        public bool IsSupportedWidePage { get; set; }
    }


}
