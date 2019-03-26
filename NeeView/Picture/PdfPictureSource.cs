using NeeView.Drawing;
using System.IO;
using System.Threading;
using System.Windows;
using System.Windows.Media.Imaging;

namespace NeeView
{
    public class PdfPictureSource : PictureSource
    {
        private MagicScalerBitmapFactory _magicScaler = new MagicScalerBitmapFactory();

        private PdfArchiver _pdfArchive;

        public PdfPictureSource(ArchiveEntry entry, PictureSourceCreateOptions createOptions) : base(entry, createOptions)
        {
            _pdfArchive = (PdfArchiver)entry.Archiver;
        }

        public override void InitializePictureInfo(CancellationToken token)
        {
            this.PictureInfo = new PictureInfo(ArchiveEntry);

            var size = _pdfArchive.GetRenderSize(ArchiveEntry);
            PictureInfo.OriginalSize = size;
            PictureInfo.Size = size;
            PictureInfo.Decoder = "PDFium";
        }

        public override BitmapSource CreateBitmapSource(Size size, BitmapCreateSetting setting, CancellationToken token)
        {
            size = size.IsEmpty ? _pdfArchive.GetRenderSize(ArchiveEntry) : size;
            var bitmapSource = _pdfArchive.CraeteBitmapSource(ArchiveEntry, size).ToBitmapSource();

            // 色情報とBPP設定
            PictureInfo.SetPixelInfo(bitmapSource);

            return bitmapSource;
        }

        public override byte[] CreateImage(Size size, BitmapCreateSetting setting, BitmapImageFormat format, int quality, CancellationToken token)
        {
            using (var outStream = new MemoryStream())
            {
                var defaultSize = _pdfArchive.GetRenderSize(ArchiveEntry);
                size = size.IsEmpty ? defaultSize : size;
                var renderSize = defaultSize.IsContains(size) ? defaultSize : size;

                if (renderSize != size || setting.Source != null)
                {
                    using (var intermediate = new MemoryStream())
                    {
                        if (setting.Source != null)
                        {
                            var encoder = new BmpBitmapEncoder();
                            encoder.Frames.Add(BitmapFrame.Create(setting.Source));
                            encoder.Save(intermediate);
                        }
                        else
                        {
                            _pdfArchive.CraeteBitmapSource(ArchiveEntry, renderSize).Save(intermediate, System.Drawing.Imaging.ImageFormat.Bmp);
                        }
                        intermediate.Seek(0, SeekOrigin.Begin);
                        _magicScaler.CreateImage(intermediate, null, outStream, size, format, quality, setting.ProcessImageSettings);
                    }
                }
                else
                {
                    _pdfArchive.CraeteBitmapSource(ArchiveEntry, renderSize).SaveWithQuality(outStream, CreateFormat(format), quality);
                }

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
            Size size;
            if (PictureInfo != null)
            {
                size = PictureInfo.Size;
            }
            else
            {
                size = _pdfArchive.GetRenderSize(ArchiveEntry);
            }

            size = profile.GetThumbnailSize(size);
            var setting = profile.CreateBitmapCreateSetting();
            return CreateImage(size, setting, profile.Format, profile.Quality, token);
        }
    }
}
