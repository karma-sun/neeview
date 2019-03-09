namespace NeeView
{
    public class ConstPage : Page
    {
        public ConstPage(ThumbnailType thumbnailType) : base("", null)
        {
            Content = new ConstContent(thumbnailType);
        }
    }
}
