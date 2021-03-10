using NeeView.Text;
using System;
using System.Linq;

namespace NeeView
{
    public class InformationValueFactory
    {
        private InformationValueSource _source;

        public InformationValueFactory(InformationValueSource source)
        {
            _source = source;
        }

        public object Create(InformationKey key)
        {
            switch (key.ToInformationCategory())
            {
                case InformationCategory.File:
                    return CreateInformationFileValue(key);
                case InformationCategory.Image:
                    return CreateInformationImageValue(key);
                case InformationCategory.Metadata:
                    return _source.Metadata?.ElementAt(key.ToBitmapMetadataKey());
                default:
                    throw new NotSupportedException();
            }
        }

        private object CreateInformationFileValue(InformationKey key)
        {
            var page = _source.Page;

            switch (key)
            {
                case InformationKey.FileName:
                    return page?.EntryLastName;
                case InformationKey.FilePath:
                    return page?.Entry?.Link ?? page?.EntryFullName;
                case InformationKey.FileSize:
                    if (page is null || page.Length <= 0) return null;
                    return new FormatValue(page.Length > 0 ? (page.Length + 1023) / 1024 : 0, "{0:#,0} KB");
                case InformationKey.CreationTime:
                    return page?.CreationTime;
                case InformationKey.LastWriteTime:
                    return page?.LastWriteTime;
                case InformationKey.ArchivePath:
                    return page?.GetFolderPlace();
                case InformationKey.Archiver:
                    return page?.Entry?.Archiver;
                default:
                    throw new NotSupportedException();
            }
        }

        private object CreateInformationImageValue(InformationKey key)
        {
            var pictureInfo = _source.BitmapContent?.PictureInfo;

            switch (key)
            {
                case InformationKey.Dimensions:
                    if (pictureInfo is null || pictureInfo.OriginalSize.Width <= 0.0 || pictureInfo.OriginalSize.Height <= 0.0) return null;
                    return $"{(int)pictureInfo.OriginalSize.Width} x {(int)pictureInfo.OriginalSize.Height}" + (pictureInfo.IsLimited ? "*" : "");
                case InformationKey.BitDepth:
                    return new FormatValue(pictureInfo?.BitsPerPixel, "{0}", FormatValue.NotDefaultValueConverter<int>);
                case InformationKey.HorizontalResolution:
                    return new FormatValue(pictureInfo?.BitmapInfo?.DpiX, "{0:0.# dpi}", FormatValue.NotDefaultValueConverter<double>);
                case InformationKey.VerticalResolution:
                    return new FormatValue(pictureInfo?.BitmapInfo?.DpiY, "{0:0.# dpi}", FormatValue.NotDefaultValueConverter<double>);
                case InformationKey.Decoder:
                    return ((_source.BitmapContent is AnimatedContent animatedContent && animatedContent.IsAnimated) || _source.BitmapContent is MediaContent) ? "MediaPlayer" : pictureInfo?.Decoder;
                default:
                    throw new NotSupportedException();
            }
        }

    }
}
