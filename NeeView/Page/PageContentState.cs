namespace NeeView
{
    /// <summary>
    /// 要求状態
    /// </summary>
    public enum PageContentState
    {
        None,
        Ahead,
        View,
    }

    public static class PageContentStateExtension
    {
        public static PageContentState Max(PageContentState x, PageContentState y)
        {
            return (x > y) ? x : y;
        }
    }
}
