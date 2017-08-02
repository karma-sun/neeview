// Copyright (c) 2016 Mitsuhiro Ito (nee)
//
// This software is released under the MIT License.
// http://opensource.org/licenses/mit-license.php

using System.Diagnostics;
using System.IO;
using System.Windows;
using System.Windows.Media.Imaging;

namespace NeeView
{
    /// <summary>
    /// Default Picture Factory
    /// </summary>
    public class DefaultPictureFactory : IPictureFactory
    {
        //
        private PictureStream _pictureStream = new PictureStream();

        //
        private BitmapFactory _bitmapFactory = new BitmapFactory();

        //
        public Picture Create(ArchiveEntry entry, PictureCreateOptions options)
        {
            var picture = new Picture(entry);

            using (var stream = _pictureStream.Create(entry))
            {
                // info
                var info = BitmapInfo.Create(stream.Stream);
                var originalSize = info.IsTranspose ? info.GetPixelSize().Transpose() : info.GetPixelSize();
                picture.PictureInfo.OriginalSize = originalSize;

                var maxSize = info.IsTranspose ? PictureProfile.Current.MaximumSize.Transpose() : PictureProfile.Current.MaximumSize;
                var size = (PictureProfile.Current.IsLimitSourceSize && !maxSize.IsContains(originalSize)) ? originalSize.Uniformed(maxSize) : Size.Empty;
                picture.PictureInfo.Size = size.IsEmpty ? originalSize : size;


                // bitmap
                if (options.HasFlag(PictureCreateOptions.CreateBitmap) || picture.PictureInfo.Size.IsEmpty)
                {
                    var bitmapSource = _bitmapFactory.Create(stream.Stream, info, size, new BitmapCreateSetting());

                    //
                    picture.PictureInfo.Exif = info.Metadata != null ? new BitmapExif(info.Metadata) : null;
                    picture.PictureInfo.Decoder = stream.Name ?? ".Net BitmapImage";
                    picture.PictureInfo.SetPixelInfo(bitmapSource, size.IsEmpty ? size : originalSize);

                    picture.BitmapSource = bitmapSource;
                }

                // thumbnail
                if (options.HasFlag(PictureCreateOptions.CreateThumbnail))
                {
                    using (var ms = new MemoryStream())
                    {
                        var thumbnailSize = ThumbnailProfile.Current.GetThumbnailSize(picture.PictureInfo.Size);
                        var setting = new BitmapCreateSetting();
                        _bitmapFactory.CreateImage(stream.Stream, info, ms, thumbnailSize, ThumbnailProfile.Current.Format, ThumbnailProfile.Current.Quality, ThumbnailProfile.Current.CreateBitmapCreateSetting());
                        picture.Thumbnail = ms.ToArray();

                        Debug.WriteLine($"Thumbnail: {picture.Thumbnail.Length / 1024}KB");
                    }
                }
            }

            return picture;
        }

        //
        public BitmapSource CreateBitmapSource(ArchiveEntry entry, Size size)
        {
            using (var stream = _pictureStream.Create(entry))
            {
                var setting = new BitmapCreateSetting();

                if (PictureProfile.Current.IsResizeFilterEnabled && !size.IsEmpty)
                {
                    setting.Mode = BitmapCreateMode.HighQuality;
                    setting.ProcessImageSettings = ImageFilter.Current.CreateProcessImageSetting();
                }

                return _bitmapFactory.Create(stream.Stream, null, size, setting);
            }
        }

        //
        public byte[] CreateImage(ArchiveEntry entry, Size size, BitmapImageFormat format, int quality, BitmapCreateSetting setting)
        {
            using (var stream = _pictureStream.Create(entry))
            using (var ms = new MemoryStream())
            {
                _bitmapFactory.CreateImage(stream.Stream, null, ms, size, format, quality, setting);
                return ms.ToArray();
            }
        }
    }
}
