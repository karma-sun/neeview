namespace NeeView
{
    public class ConstContent : PageContent
    {
        private ThumbnailType _thumbnailType;

        public ConstContent(ThumbnailType thumbnailType) : base(ArchiveEntry.Empty)
        {
            _thumbnailType = thumbnailType;
        }

        public override void InitializeThumbnail()
        {
            Thumbnail.Initialize(_thumbnailType);
        }
    }
}
