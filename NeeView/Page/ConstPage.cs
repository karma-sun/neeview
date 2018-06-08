namespace NeeView
{
    public class ConstPage : Page
    {
        public ConstPage(ThumbnailType thumbnailType)
        {
            Content = new ConstContent(thumbnailType);
        }
    }
}
