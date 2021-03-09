using NeeLaboratory.ComponentModel;
using NeeLaboratory.Windows.Input;
using NeeView.Media.Imaging.Metadata;
using NeeView.Text;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Media;
using System.Windows.Media.Effects;

namespace NeeView
{

    public class FileInformationSource : BindableBase
    {
        private List<FileInformationRecord> _properties;


        public FileInformationSource(ViewContent viewContent)
        {
            this.ViewContent = viewContent;

            Update();
        }


        public ViewContent ViewContent { get; private set; }

        public Page Page => ViewContent?.Page;

        public BitmapContent BitmapContent => ViewContent?.Content as BitmapContent;

        public PictureInfo PictureInfo => BitmapContent?.PictureInfo;

        public BitmapMetadataDatabase Metadata => PictureInfo?.Metadata;

        public double IconMaxSize => 96.0;

        public FrameworkElement Icon => CreateIcon();

        public List<FileInformationRecord> Properties
        {
            get { return _properties; }
            set { SetProperty(ref _properties, value); }
        }

        public GpsLocation GpsLocation { get; private set; }



        public static List<FileInformationRecord> CreatePropertiesTemplate()
        {
            var items = new List<FileInformationRecord>();

            items.AddRange(Enum.GetValues(typeof(FilePropertyKey)).Cast<FilePropertyKey>().Select(e => new FileInformationRecord(InformationGroup.File, e.ToAliasName(), null)));
            items.AddRange(Enum.GetValues(typeof(ImagePropertyKey)).Cast<ImagePropertyKey>().Select(e => new FileInformationRecord(InformationGroup.Image, e.ToAliasName(), null)));
            items.AddRange(Enum.GetValues(typeof(BitmapMetadataKey)).Cast<BitmapMetadataKey>().Select(e => new FileInformationRecord(e.ToInformationGroup(), e.ToAliasName(), null)));

            return items;
        }

        public void Update()
        {
            var items = new List<FileInformationRecord>();

            var page = ViewContent?.Page;
            items.Add(new FileInformationRecord(InformationGroup.File, FilePropertyKey.FileName.ToAliasName(), ViewContent?.FileName));
            items.Add(new FileInformationRecord(InformationGroup.File, FilePropertyKey.ArchivePath.ToAliasName(), ViewContent?.Page?.Entry?.Link ?? ViewContent?.FullPath));
            items.Add(new FileInformationRecord(InformationGroup.File, FilePropertyKey.FileSize.ToAliasName(), (page != null && page.Length > 0) ? string.Format("{0:#,0} KB", page.Length > 0 ? (page.Length + 1023) / 1024 : 0) : null));
            items.Add(new FileInformationRecord(InformationGroup.File, FilePropertyKey.CreationTime.ToAliasName(), (page != null && page.CreationTime != default) ? (object)page.CreationTime : null));
            items.Add(new FileInformationRecord(InformationGroup.File, FilePropertyKey.LastWriteTime.ToAliasName(), (page != null && page.LastWriteTime != default) ? (object)page.LastWriteTime : null));
            items.Add(new FileInformationRecord(InformationGroup.File, FilePropertyKey.Archiver.ToAliasName(), page?.Entry?.Archiver?.ToString()));
            items.Add(new FileInformationRecord(InformationGroup.File, FilePropertyKey.FolderPlace.ToAliasName(), ViewContent?.FolderPlace));

            items.Add(new FileInformationRecord(InformationGroup.Image, ImagePropertyKey.Dimensions.ToAliasName(), (PictureInfo != null && PictureInfo.OriginalSize.Width > 0 && PictureInfo.OriginalSize.Height > 0) ? $"{(int)PictureInfo.OriginalSize.Width} x {(int)PictureInfo.OriginalSize.Height}" + (PictureInfo.IsLimited ? "*" : "") : null));
            items.Add(new FileInformationRecord(InformationGroup.Image, ImagePropertyKey.BitDepth.ToAliasName(), new FormatValue(PictureInfo?.BitsPerPixel, "{0}", FormatValue.NotDefaultValueConverter<int>)));
            items.Add(new FileInformationRecord(InformationGroup.Image, ImagePropertyKey.HorizontalResolution.ToAliasName(), new FormatValue(PictureInfo?.BitmapInfo?.DpiX, "{0:0.# dpi}", FormatValue.NotDefaultValueConverter<double>)));
            items.Add(new FileInformationRecord(InformationGroup.Image, ImagePropertyKey.VerticalResolution.ToAliasName(), new FormatValue(PictureInfo?.BitmapInfo?.DpiY, "{0:0.# dpi}", FormatValue.NotDefaultValueConverter<double>)));
            items.Add(new FileInformationRecord(InformationGroup.Image, ImagePropertyKey.Decoder.ToAliasName(), ((BitmapContent is AnimatedContent animatedContent && animatedContent.IsAnimated) || BitmapContent is MediaContent) ? "MediaPlayer" : PictureInfo?.Decoder));

            if (Metadata != null)
            {
                items.AddRange(Metadata.Select(e => new FileInformationRecord(e.Key.ToInformationGroup(), e.Key.ToAliasName(), e.Value)));
            }
            else
            {
                items.AddRange(Enum.GetValues(typeof(BitmapMetadataKey)).Cast<BitmapMetadataKey>().Select(e => new FileInformationRecord(e.ToInformationGroup(), e.ToAliasName(), null)));
            }

            GpsLocation = CreateGpsLocate();

            this.Properties = items;
        }


        private GpsLocation CreateGpsLocate()
        {
            if (Metadata != null && Metadata[BitmapMetadataKey.GPSLatitude] is ExifGpsDegree lat && Metadata[BitmapMetadataKey.GPSLongitude] is ExifGpsDegree lon)
            {
                return new GpsLocation(lat, lon);
            }

            return null;
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
            var imageSource = FileIconCollection.Current.CreateDefaultFolderIcon().GetBitmapSource(256.0);

            var image = new Image()
            {
                Source = imageSource,
                Width = 64.0,
                Height = 64.0,
                HorizontalAlignment = HorizontalAlignment.Center,
                VerticalAlignment = VerticalAlignment.Center,
                UseLayoutRounding = true,
            };
            RenderOptions.SetBitmapScalingMode(image, BitmapScalingMode.HighQuality);

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
                Background = new SolidColorBrush(Color.FromArgb(0x10, 0x80, 0x80, 0x80)),
                Child = content,
            };

            return border;
        }


        public bool CanOpenPlace()
        {
            return ViewContent?.FolderPlace != null;
        }

        public void OpenPlace()
        {
            var place = ViewContent?.Page?.GetFolderOpenPlace();
            if (!string.IsNullOrWhiteSpace(place))
            {
                ExternalProcess.Start("explorer.exe", "/select,\"" + place + "\"");
            }
        }

        public bool CanOpenMap()
        {
            return GpsLocation != null;
        }

        public void OpenMap()
        {
            GpsLocation?.OpenMap(Config.Current.Information.MapProgramFormat);
        }

    }

}
