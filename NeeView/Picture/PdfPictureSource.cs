using NeeView.Drawing;
using System;
using System.Diagnostics;
using System.IO;
using System.Threading;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace NeeView
{
    public class PdfPictureSource : PictureSource
    {
        private PdfArchiver _pdfArchive;

        public PdfPictureSource(ArchiveEntry entry, PictureInfo pictureInfo, PictureSourceCreateOptions createOptions) : base(entry, pictureInfo, createOptions)
        {
            _pdfArchive = (PdfArchiver)entry.Archiver;
        }

        public override PictureInfo CreatePictureInfo(CancellationToken token)
        {
            if (PictureInfo != null) return PictureInfo;

            var pictureInfo = new PictureInfo(ArchiveEntry);
            var originalSize = _pdfArchive.GetSourceSize(ArchiveEntry);
            pictureInfo.OriginalSize = originalSize;
            var maxSize = Config.Current.Performance.MaximumSize;
            var size = (Config.Current.Performance.IsLimitSourceSize && !maxSize.IsContains(originalSize)) ? originalSize.Uniformed(maxSize) : originalSize;
            pictureInfo.Size = size;
            pictureInfo.BitsPerPixel = 32;
            pictureInfo.Decoder = "PDFium";
            this.PictureInfo = pictureInfo;
            
            return PictureInfo;
        }

        private Size GetImageSize()
        {
            if (this.PictureInfo is null)
            {
                CreatePictureInfo(CancellationToken.None);
            }

            return PictureInfo.Size;
        }


        public override ImageSource CreateImageSource(Size size, BitmapCreateSetting setting, CancellationToken token)
        {
            size = size.IsEmpty ? GetImageSize() : size;
            var bitmapSource = _pdfArchive.CraeteBitmapSource(ArchiveEntry, size).ToBitmapSource();

            // 色情報設定
            PictureInfo.SetPixelInfo(bitmapSource);

            return bitmapSource;
        }

        public override byte[] CreateImage(Size size, BitmapCreateSetting setting, BitmapImageFormat format, int quality, CancellationToken token)
        {
            using (var outStream = new MemoryStream())
            {
                size = size.IsEmpty ? GetImageSize() : size;
                _pdfArchive.CraeteBitmapSource(ArchiveEntry, size).SaveWithQuality(outStream, CreateFormat(format), quality);
                return outStream.ToArray();
            }
        }

        private System.Drawing.Imaging.ImageFormat CreateFormat(BitmapImageFormat format)
        {
            switch (format)
            {
                default:
                case BitmapImageFormat.Jpeg:
                    return System.Drawing.Imaging.ImageFormat.Jpeg;
                case BitmapImageFormat.Png:
                    return System.Drawing.Imaging.ImageFormat.Png;
            }
        }


        public override byte[] CreateThumbnail(ThumbnailProfile profile, CancellationToken token)
        {
            var size = profile.GetThumbnailSize(GetImageSize());
            var setting = profile.CreateBitmapCreateSetting();
            return CreateImage(size, setting, Config.Current.Thumbnail.Format, Config.Current.Thumbnail.Quality, token);
        }

        public override Size FixedSize(Size size)
        {
            var imageSize = GetImageSize();

            size = size.IsEmpty ? imageSize : size;

            // 最小サイズ
            if (Config.Current.Archive.Pdf.RenderSize.IsContains(size))
            {
                size = size.Uniformed(Config.Current.Archive.Pdf.RenderSize);
            }

            // 最大サイズ
            var maxWixth = Math.Max(imageSize.Width, Config.Current.Performance.MaximumSize.Width);
            var maxHeight = Math.Max(imageSize.Height, Config.Current.Performance.MaximumSize.Height);
            var maxSize = new Size(maxWixth, maxHeight);
            size = size.Limit(maxSize);

            return size;
        }
    }
}
