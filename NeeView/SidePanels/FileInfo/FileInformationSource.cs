using NeeLaboratory.ComponentModel;
using System.Windows.Media;

namespace NeeView
{
    public class FileInformationSource : BindableBase
    {
        public FileInformationSource(ViewContent viewContent)
        {
            this.ViewContent = viewContent;
        }


        public ViewContent ViewContent { get; private set; }

        public Page Page => ViewContent?.Page;

        public BitmapContent BitmapContent => ViewContent?.Content as BitmapContent;

        public PictureInfo PictureInfo => BitmapContent?.PictureInfo;

        public BitmapExif Exif => PictureInfo?.Exif;

        public double ThumbnailMaxSize => 96.0;

        public double ThumbnailWidth => BitmapContent != null ? BitmapContent.Size.Width * GetAspectoRatio() : 0.0;

        public double ThumbnailHeight => BitmapContent != null ? BitmapContent.Size.Height * GetAspectoRatio() : 0.0;

        public ImageSource ImageSource => BitmapContent?.ImageSource;

        public string FullPath => ViewContent?.Page?.Entry?.Link ?? ViewContent?.FullPath;

        public string ImageSize => (PictureInfo != null && PictureInfo.OriginalSize.Width > 0 && PictureInfo.OriginalSize.Height > 0)
                    ? $"{(int)PictureInfo.OriginalSize.Width} x {(int)PictureInfo.OriginalSize.Height}" + (PictureInfo.IsLimited ? "*" : "") + (Config.Current.Information.IsVisibleBitsPerPixel ? $" ({PictureInfo.BitsPerPixel}bit)" : "")
                    : null;

        public string FileSize => (PictureInfo != null && PictureInfo.Length > 0) ? string.Format("{0:#,0} KB", PictureInfo.Length > 0 ? (PictureInfo.Length + 1023) / 1024 : 0) : null;

        public string ShotInfo => Exif?.ShotInfo;

        public string ISOSpeedRatings => Exif != null && Exif.ISOSpeedRatings > 0 ? Exif.ISOSpeedRatings.ToString() : null;

        public string CameraModel => Exif?.Model;

        public string LastWriteTime => (PictureInfo != null && PictureInfo.LastWriteTime != default) ? PictureInfo.LastWriteTime.ToString(NeeView.Properties.Resources.Information_DateFormat) : null;

        public string DateTimeOriginal => (Exif != null && Exif.DateTimeOriginal != default) ? Exif.DateTimeOriginal.ToString(NeeView.Properties.Resources.Information_DateFormat) : null;

        public string Archiver => PictureInfo?.Archiver;

        public string Decoder => ((BitmapContent is AnimatedContent animatedContent && animatedContent.IsAnimated) || BitmapContent is MediaContent) ? "MediaPlayer" : PictureInfo?.Decoder;


        private double GetAspectoRatio()
        {
            if (BitmapContent is null) return 1.0;
            var length = BitmapContent.Size.Width > BitmapContent.Size.Height ? BitmapContent.Size.Width : BitmapContent.Size.Height;
            return ThumbnailMaxSize / length;
        }

        public void Update()
        {
            RaisePropertyChanged(null);
        }

    }
}
