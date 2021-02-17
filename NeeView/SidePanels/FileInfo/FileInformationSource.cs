using NeeLaboratory.ComponentModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Effects;

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

        public double IconMaxSize => 96.0;

        public FrameworkElement Icon => CreateIcon();

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



        public void Update()
        {
            RaisePropertyChanged(null);
        }


        public FrameworkElement CreateIcon()
        {
            if (BitmapContent?.ImageSource != null)
            {
                return CreateBitmapContentIcon(BitmapContent);
            }
            else if (Page?.Entry != null)
            {
                var entry = Page.Entry;

                if (entry.IsDirectory)
                {
                    if (entry.IsFileSystem)
                    {
                        return CreateSymbolFolderIcon();
                    }
                    else
                    {
                        return CreateSymbolIcon("/Archive");
                    }
                }
                else
                {
                    return CreateSymbolIcon(LoosePath.GetExtension(entry.EntryName).ToUpper());
                }
            }

            return null;
        }

        private FrameworkElement CreateBitmapContentIcon(BitmapContent bitmapContent)
        {
            if (bitmapContent?.ImageSource is null) return null;

            var length = bitmapContent.Size.Width > bitmapContent.Size.Height ? bitmapContent.Size.Width : bitmapContent.Size.Height;
            var retio = IconMaxSize / length;

            var image = new Image()
            {
                Source = bitmapContent.ImageSource,
                Width = bitmapContent.Size.Width * retio,
                Height = bitmapContent.Size.Height * retio,
                Stretch = Stretch.Fill,
                HorizontalAlignment = HorizontalAlignment.Center,
                VerticalAlignment = VerticalAlignment.Bottom,
                UseLayoutRounding = true,
                Effect = new DropShadowEffect()
                {
                    ShadowDepth = 2.0,
                    Opacity = 0.5
                },
            };
            RenderOptions.SetBitmapScalingMode(image, BitmapScalingMode.HighQuality);

            return image;
        }

        private FrameworkElement CreateSymbolFolderIcon()
        {
            var image = new Image()
            {
                Source = FileIconCollection.Current.CreateDefaultFolderIcon().GetBitmapSource(48.0),
                Width = 48.0,
                Height = 48.0,
                HorizontalAlignment = HorizontalAlignment.Center,
                VerticalAlignment = VerticalAlignment.Center,
                UseLayoutRounding = true,
            };
            return CreateSymbolIcon(image);
        }

        private FrameworkElement CreateSymbolIcon(string text)
        {
            var border = new Border()
            {
                HorizontalAlignment = HorizontalAlignment.Center,
                VerticalAlignment = VerticalAlignment.Center,
                MaxWidth = IconMaxSize,
                Child = new TextBlock()
                {
                    Text = text,
                    FontSize = 20.0,
                    FontWeight = FontWeights.Bold,
                    HorizontalAlignment = HorizontalAlignment.Left,
                    VerticalAlignment = VerticalAlignment.Center,
                    Margin = new Thickness(5),
                },
            };
            return CreateSymbolIcon(border);
        }

        private FrameworkElement CreateSymbolIcon(UIElement content)
        {
            var border = new Border()
            {
                HorizontalAlignment = HorizontalAlignment.Center,
                VerticalAlignment = VerticalAlignment.Center,
                Width = IconMaxSize,
                Height = IconMaxSize,
                Background = new SolidColorBrush(Color.FromArgb(0x20, 0x80, 0x80, 0x80)),
                Child = content,
            };

            return border;
        }

    }
}
