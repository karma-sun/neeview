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
                var size = info.GetPixelSize();
                picture.PictureInfo.Size = info.IsTranspose ? info.GetPixelSize().Transpose() : info.GetPixelSize();

                // bitmap
                if (options.HasFlag(PictureCreateOptions.CreateBitmap) || picture.PictureInfo.Size.IsEmpty)
                {
                    var maxSize = info.IsTranspose ? PictureProfile.Current.Maximum.Transpose() : PictureProfile.Current.Maximum;
                    size = (size.IsEmpty || maxSize.IsContains(size)) ? Size.Empty : size.Uniformed(maxSize);
                    var bitmapSource = _bitmapFactory.Create(stream.Stream, info, size, BitmapCreateMode.Default);

                    //
                    picture.PictureInfo.Exif = info.Metadata != null ? new BitmapExif(info.Metadata) : null;
                    picture.PictureInfo.Decoder = stream.Name ?? ".Net BitmapImage";
                    picture.PictureInfo.SetPixelInfo(bitmapSource);

                    picture.BitmapSource = bitmapSource;
                }

                // thumbnail
                if (options.HasFlag(PictureCreateOptions.CreateThumbnail))
                {
                    using (var ms = new MemoryStream())
                    {
                        var thumbnailSize = ThumbnailProfile.Current.GetThumbnailSize(picture.PictureInfo.Size);
                        _bitmapFactory.CreateImage(stream.Stream, info, ms, thumbnailSize, ThumbnailProfile.Current.Format, ThumbnailProfile.Current.Quality, ThumbnailProfile.Current.CreateMode);
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
                var mode = PictureProfile.Current.IsResizeFilterEnabled ? BitmapCreateMode.HighQuality : BitmapCreateMode.Default;
                return _bitmapFactory.Create(stream.Stream, null, size, mode);
            }
        }

        //
        public byte[] CreateImage(ArchiveEntry entry, Size size, BitmapImageFormat format, int quality, BitmapCreateMode mode)
        {
            using (var stream = _pictureStream.Create(entry))
            using (var ms = new MemoryStream())
            {
                _bitmapFactory.CreateImage(stream.Stream, null, ms, size, format, quality, mode);
                return ms.ToArray();
            }
        }

        //
        public Size CreateFixedSize(ArchiveEntry entry, Size size)
        {
            return PictureProfile.Current.CreateFixedSize(size);
        }
    }
}
