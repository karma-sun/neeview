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
            return new List<FileInformationRecord>(Enum.GetValues(typeof(InformationKey)).Cast<InformationKey>().Select(e => new FileInformationRecord(e, null)));
        }

        public void Update()
        {
            this.GpsLocation = CreateGpsLocate();
            this.Properties = CreateProperties();
        }

        public List<FileInformationRecord> CreateProperties()
        {
            var factory = new InformationValueFactory(new InformationValueSource(ViewContent?.Page, BitmapContent, Metadata));
            return new List<FileInformationRecord>(Enum.GetValues(typeof(InformationKey)).Cast<InformationKey>().Select(e => new FileInformationRecord(e, factory.Create(e))));
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
