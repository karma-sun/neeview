// Copyright (c) 2016 Mitsuhiro Ito (nee)
//
// This software is released under the MIT License.
// http://opensource.org/licenses/mit-license.php

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
        public Picture Create(ArchiveEntry entry)
        {
            var picture = new Picture(entry);

            using (var stream = _pictureStream.Create(entry))
            {
                // info
                var info = _bitmapFactory.CreateInfo(stream.Stream);
                var size = info.GetPixelSize();

                // bitmap
                size = (size.IsEmpty || PictureProfile.Current.Maximum.IsContains(size)) ? Size.Empty : size.Uniformed(PictureProfile.Current.Maximum);
                var bitmapSource = _bitmapFactory.Create(stream.Stream, size, info);

                //
                picture.PictureInfo.Exif = info.Metadata != null ? new BitmapExif(info.Metadata) : null;
                picture.PictureInfo.Decoder = stream.Name ?? ".Net BitmapImage";
                picture.PictureInfo.SetPixelInfo(bitmapSource);

                picture.BitmapSource = bitmapSource;
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

        public Size CreateFixedSize(ArchiveEntry entry, Size size)
        {
            return PictureProfile.Current.CreateFixedSize(size);
        }
    }
}
