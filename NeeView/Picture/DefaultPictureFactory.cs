// Copyright (c) 2016 Mitsuhiro Ito (nee)
//
// This software is released under the MIT License.
// http://opensource.org/licenses/mit-license.php

using System.Diagnostics;
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
                var info = _bitmapFactory.CreateInfo(stream.Stream);
                var size = info.GetPixelSize();
                picture.PictureInfo.Size = info.IsTranspose ? info.GetPixelSize().Transpose() : info.GetPixelSize();

                // bitmap
                if (options.HasFlag(PictureCreateOptions.CreateBitmap))
                {
                    var maxSize = info.IsTranspose ? PictureProfile.Current.Maximum.Transpose() : PictureProfile.Current.Maximum;
                    size = (size.IsEmpty || maxSize.IsContains(size)) ? Size.Empty : size.Uniformed(maxSize);
                    var bitmapSource = _bitmapFactory.Create(stream.Stream, size, info);

                    //
                    picture.PictureInfo.Exif = info.Metadata != null ? new BitmapExif(info.Metadata) : null;
                    picture.PictureInfo.Decoder = stream.Name ?? ".Net BitmapImage";
                    picture.PictureInfo.SetPixelInfo(bitmapSource);

                    picture.BitmapSource = bitmapSource;
                }

                // thumbnail
                if (options.HasFlag(PictureCreateOptions.CreateThumbnail))
                {
                    var thumbnailSize = ThumbnailProfile.Current.GetThumbnailSize(picture.PictureInfo.Size);
                    picture.Thumbnail = _bitmapFactory.CreateImage(stream.Stream, thumbnailSize, ThumbnailProfile.Current.Quality);
                }
            }

            return picture;
        }

        //
        public BitmapSource CreateBitmapSource(ArchiveEntry entry, Size size)
        {
            using (var stream = _pictureStream.Create(entry))
            {
                return _bitmapFactory.Create(stream.Stream, size);
            }
        }

        //
        public byte[] CreateImage(ArchiveEntry entry, Size size, int quality)
        {
            using (var stream = _pictureStream.Create(entry))
            {
                return _bitmapFactory.CreateImage(stream.Stream, size, quality);
            }
        }

        //
        public Size CreateFixedSize(ArchiveEntry entry, Size size)
        {
            return PictureProfile.Current.CreateFixedSize(size);
        }
    }
}
