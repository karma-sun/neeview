using System.Threading;

namespace NeeView
{
    public static class PictureSourceFactory
    {
        public static PictureSource Create(ArchiveEntry entry, PictureInfo pictureInfo, PictureSourceCreateOptions createOptions, CancellationToken token)
        {
            if (entry.Archiver is PdfArchiver)
            {
                return new PdfPictureSource(entry, pictureInfo, createOptions);
            }
            else if (PictureProfile.Current.IsSvgSupported(entry.EntryName))
            {
                return new SvgPictureSource(entry, pictureInfo, createOptions);
            }
            else
            {
                return new DefaultPictureSource(entry, pictureInfo, createOptions);
            }
        }
    }

}
