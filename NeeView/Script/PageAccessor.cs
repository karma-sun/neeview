namespace NeeView
{
    public class PageAccessor
    {
        private Page _page;

        public PageAccessor(Page page)
        {
            _page = page;
        }

        internal Page Source => _page;

        public string Path => _page.SystemPath;

        public long Size => _page.Length;

        public string LastWriteTime => _page.LastWriteTime.ToString();
    }


    public class ViewPageAccessor : PageAccessor
    {
        public ViewPageAccessor(Page page) : base(page)
        {
        }
      
        public double Width => this.Source.Size.Width;
        
        public double Height => this.Source.Size.Height;
    }
}
