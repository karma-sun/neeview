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

        [WordNodeMember]
        public string Path => _page.SystemPath;

        [WordNodeMember]
        public long Size => _page.Length;

        [WordNodeMember]
        public string LastWriteTime => _page.LastWriteTime.ToString();
    }
}
