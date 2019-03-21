using System.Threading;
using System.Windows;
using System.Windows.Media.Imaging;

namespace NeeView
{
    /// <summary>
    /// Pictureの元データ管理
    /// </summary>
    public abstract class PictureSource
    {
        protected bool _ignoreImageCache;

        public PictureSource(ArchiveEntry entry, bool ignoreImageCache)
        {
            ArchiveEntry = entry;
            _ignoreImageCache = ignoreImageCache;
        }

        public ArchiveEntry ArchiveEntry { get; }
        public PictureInfo PictureInfo { get; protected set; }

        public abstract void Initialize(CancellationToken token);
        public abstract BitmapSource CreateBitmapSource(Size size, BitmapCreateSetting setting, CancellationToken token);
        public abstract byte[] CreateImage(Size size, BitmapCreateSetting setting, BitmapImageFormat format, int quality, CancellationToken token);
    }


    public static class PictureSourceFactory
    {
        public static PictureSource Create(ArchiveEntry entry, bool ignoreImageCache, CancellationToken token)
        {
            PictureSource pictureSource;
            if (entry.Archiver is PdfArchiver)
            {
                pictureSource = new PdfPictureSource(entry, ignoreImageCache);
            }
            else
            {
                pictureSource = new DefaultPictureSource(entry, ignoreImageCache);
            }

            pictureSource.Initialize(token);
            return pictureSource;
        }
    }


    public static class PictureSourceExtensions
    {
        public static byte[] CreateThumbnail(this PictureSource self, CancellationToken token)
        {
            if (self == null) return null;

            var size = ThumbnailProfile.Current.GetThumbnailSize(self.PictureInfo.Size);
            var setting = ThumbnailProfile.Current.CreateBitmapCreateSetting();
            return self.CreateImage(size, setting, ThumbnailProfile.Current.Format, ThumbnailProfile.Current.Quality, token);
        }
    }
}
