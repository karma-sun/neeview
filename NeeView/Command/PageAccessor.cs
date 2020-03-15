namespace NeeView
{
    public class PageAccessor
    {
        private Page _page;

        public PageAccessor(Page page)
        {
            _page = page;
        }

        public string Path => _page.SystemPath;

        public double Width => _page.Size.Width;
        public double Height => _page.Size.Height;
    }



}
