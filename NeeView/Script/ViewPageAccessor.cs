namespace NeeView
{
    public class ViewPageAccessor : PageAccessor
    {
        public ViewPageAccessor(Page page) : base(page)
        {
        }

        [WordNodeMember]
        public double Width => this.Source.Size.Width;

        [WordNodeMember]
        public double Height => this.Source.Size.Height;
    }
}
