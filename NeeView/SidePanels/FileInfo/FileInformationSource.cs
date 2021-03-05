using NeeLaboratory.ComponentModel;
using NeeLaboratory.Windows.Input;
using NeeView.Media.Imaging.Metadata;
using NeeView.Text;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
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

            OpenPlaceCommand = new RelayCommand(OpenPlace, () => CanOpenPlace);
            OpenMapCommand = new RelayCommand(OpenMap, () => CanOpenMap);

            Update();
        }


        public RelayCommand OpenPlaceCommand { get; private set; }
        public RelayCommand OpenMapCommand { get; private set; }


        public ViewContent ViewContent { get; private set; }

        public Page Page => ViewContent?.Page;

        public BitmapContent BitmapContent => ViewContent?.Content as BitmapContent;

        public PictureInfo PictureInfo => BitmapContent?.PictureInfo;

        public BitmapMetadataDatabase Metadata => PictureInfo?.Metadata;

        public double IconMaxSize => 96.0;

        public FrameworkElement Icon => CreateIcon();




        private Dictionary<FilePropertyKey, object> _FileProperties;
        public Dictionary<FilePropertyKey, object> FileProperties
        {
            get { return _FileProperties; }
            set { SetProperty(ref _FileProperties, value); }
        }


        private Dictionary<ImagePropertyKey, object> _ImageProperties;
        public Dictionary<ImagePropertyKey, object> ImageProperties
        {
            get { return _ImageProperties; }
            set { SetProperty(ref _ImageProperties, value); }
        }


        private Dictionary<BitmapMetadataKey, object> _Description;
        public Dictionary<BitmapMetadataKey, object> Description
        {
            get { return _Description; }
            set { SetProperty(ref _Description, value); }
        }


        private Dictionary<BitmapMetadataKey, object> _Origin;
        public Dictionary<BitmapMetadataKey, object> Origin
        {
            get { return _Origin; }
            set { SetProperty(ref _Origin, value); }
        }


        private Dictionary<BitmapMetadataKey, object> _Camera;
        public Dictionary<BitmapMetadataKey, object> Camera
        {
            get { return _Camera; }
            set { SetProperty(ref _Camera, value); }
        }


        private Dictionary<BitmapMetadataKey, object> _AdvancedPhoto;
        public Dictionary<BitmapMetadataKey, object> AdvancedPhoto
        {
            get { return _AdvancedPhoto; }
            set { SetProperty(ref _AdvancedPhoto, value); }
        }


        private Dictionary<BitmapMetadataKey, object> _Gps;
        public Dictionary<BitmapMetadataKey, object> Gps
        {
            get { return _Gps; }
            set { SetProperty(ref _Gps, value); }
        }


        public GpsLocation GpsLocation { get; private set; }


        public bool CanOpenPlace => FileProperties[FilePropertyKey.FolderPlace] != null;

        public bool CanOpenMap => GpsLocation != null;


        public InformationConfig InformationConfig => Config.Current.Information;


        public void Update()
        {
            UpdateFileProperties();
            UpdateImageProperties();
            UpdateMetadata();

            RaisePropertyChanged(null);
        }

        private void UpdateMetadata()
        {
            UpdateDescription();
            UpdateOrigin();
            UpdateCamera();
            UpdateAdvancedPhoto();
            UpdateGps();
        }


        private void UpdateFileProperties()
        {
            var page = ViewContent?.Page;

            FileProperties = new Dictionary<FilePropertyKey, object>()
            {
                [FilePropertyKey.FileName] = ViewContent?.FileName,
                [FilePropertyKey.ArchivePath] = ViewContent?.Page?.Entry?.Link ?? ViewContent?.FullPath,
                [FilePropertyKey.FileSize] = (page != null && page.Length > 0) ? string.Format("{0:#,0} KB", page.Length > 0 ? (page.Length + 1023) / 1024 : 0) : null,
                [FilePropertyKey.CreationTime] = (page != null && page.CreationTime != default) ? (object)page.CreationTime : null,
                [FilePropertyKey.LastWriteTime] = (page != null && page.LastWriteTime != default) ? (object)page.LastWriteTime : null,
                [FilePropertyKey.Archiver] = page?.Entry?.Archiver?.ToString(),
                [FilePropertyKey.FolderPlace] = ViewContent?.FolderPlace,
            };

            RaisePropertyChanged(nameof(CanOpenPlace));
        }

        private void UpdateImageProperties()
        {
            ImageProperties = new Dictionary<ImagePropertyKey, object>()
            {
                [ImagePropertyKey.Dimensions] = (PictureInfo != null && PictureInfo.OriginalSize.Width > 0 && PictureInfo.OriginalSize.Height > 0) ? $"{(int)PictureInfo.OriginalSize.Width} x {(int)PictureInfo.OriginalSize.Height}" + (PictureInfo.IsLimited ? "*" : "") : null,
                [ImagePropertyKey.BitDepth] = new FormatValue(PictureInfo?.BitsPerPixel, "{0}", FormatValue.NotDefaultValueConverter<int>),
                [ImagePropertyKey.HorizontalResolution] = new FormatValue(PictureInfo?.BitmapInfo?.DpiX, "{0:0.# dpi}", FormatValue.NotDefaultValueConverter<double>),
                [ImagePropertyKey.VerticalResolution] = new FormatValue(PictureInfo?.BitmapInfo?.DpiY, "{0:0.# dpi}", FormatValue.NotDefaultValueConverter<double>),
                [ImagePropertyKey.Decoder] = ((BitmapContent is AnimatedContent animatedContent && animatedContent.IsAnimated) || BitmapContent is MediaContent) ? "MediaPlayer" : PictureInfo?.Decoder,
            };
        }


        private void UpdateDescription()
        {
            Description = Metadata?.Where(e => e.Key.GetGroup() == BitmapMetadataGroup.Description).ToDictionary(e => e.Key, e => e.Value);
        }

        private void UpdateOrigin()
        {
            Origin = Metadata?.Where(e => e.Key.GetGroup() == BitmapMetadataGroup.Origin).ToDictionary(e => e.Key, e => e.Value);
        }

        private void UpdateCamera()
        {
            Camera = Metadata?.Where(e => e.Key.GetGroup() == BitmapMetadataGroup.Camera).ToDictionary(e => e.Key, e => e.Value);
        }

        private void UpdateAdvancedPhoto()
        {
            AdvancedPhoto = Metadata?.Where(e => e.Key.GetGroup() == BitmapMetadataGroup.AdvancedPhoto).ToDictionary(e => e.Key, e => e.Value);
        }

        private void UpdateGps()
        {
            Gps = Metadata?.Where(e => e.Key.GetGroup() == BitmapMetadataGroup.GPS).ToDictionary(e => e.Key, e => e.Value);
            UpdateGpsLocate();
        }


        private void UpdateGpsLocate()
        {
            if (Metadata != null && Metadata[BitmapMetadataKey.GPSLatitude] is ExifGpsDegree lat && Metadata[BitmapMetadataKey.GPSLongitude] is ExifGpsDegree lon)
            {
                GpsLocation = new GpsLocation(lat, lon);
            }
            else
            {
                GpsLocation = null;
            }

            RaisePropertyChanged(nameof(CanOpenMap));
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


        public void OpenPlace()
        {
            var place = ViewContent?.Page?.GetFolderOpenPlace();
            if (!string.IsNullOrWhiteSpace(place))
            {
                System.Diagnostics.Process.Start("explorer.exe", "/select,\"" + place + "\"");
            }
        }

        public void OpenMap()
        {
            GpsLocation?.OpenMap(Config.Current.Information.MapProgramFormat);
        }

    }
}
