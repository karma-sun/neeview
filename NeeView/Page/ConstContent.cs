namespace NeeView
{
    public class ConstContent : PageContent
    {
        private ThumbnailType _thumbnailType;

        public ConstContent(ThumbnailType thumbnailType) : base(ArchiveEntry.Empty)
        {
            _thumbnailType = thumbnailType;
        }
        public ThumbnailType ThumbnailType => _thumbnailType;

        public override IContentLoader CreateContentLoader()
        {
            return new ConstContentLoader(this);
        }
    }
}
