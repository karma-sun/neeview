// Copyright (c) 2016 Mitsuhiro Ito (nee)
//
// This software is released under the MIT License.
// http://opensource.org/licenses/mit-license.php

using NeeView.Utility;
using System;
using System.Diagnostics;
using System.IO;
using System.Windows;
using System.Windows.Media.Imaging;

namespace NeeView
{
    /// <summary>
    /// PDF Picture Factory
    /// </summary>
    public class PdfPictureFactory : IPictureFactory
    {
        public Picture Create(ArchiveEntry entry, PictureCreateOptions options)
        {
            var pdfArchiver = (PdfArchiver)entry.Archiver;
            var profile = PdfArchiverProfile.Current;

            var picture = new Picture(entry);

            var size = pdfArchiver.GetRenderSize(entry);
            picture.PictureInfo.Size = size;

            // bitmap
            if (options.HasFlag(PictureCreateOptions.CreateBitmap))
            {
                var bitmapSource = Utility.NVGraphics.ToBitmapSource(pdfArchiver.CraeteBitmapSource(entry, size));

                picture.PictureInfo.Decoder = "PDFium";
                picture.PictureInfo.SetPixelInfo(bitmapSource);

                picture.BitmapSource = bitmapSource;
            }

            // thumbnail
            if (options.HasFlag(PictureCreateOptions.CreateThumbnail))
            {
                using (var ms = new MemoryStream())
                {
                    var thumbnailSize = ThumbnailProfile.Current.GetThumbnailSize(picture.PictureInfo.Size);
                    pdfArchiver.CraeteBitmapSource(entry, thumbnailSize).SaveWithQuality(ms, CreateFormat(ThumbnailProfile.Current.Format), ThumbnailProfile.Current.Quality);
                    picture.Thumbnail = ms.ToArray();
                }
            }

            return picture;
        }

        public BitmapSource CreateBitmapSource(ArchiveEntry entry, Size size)
        {
            var pdfArchiver = (PdfArchiver)entry.Archiver;
            size = size.IsEmpty ? pdfArchiver.GetRenderSize(entry) : size;
            return Utility.NVGraphics.ToBitmapSource(pdfArchiver.CraeteBitmapSource(entry, size));
        }

        public Size CreateFixedSize(ArchiveEntry entry, Size size)
        {
            return PdfArchiverProfile.Current.CreateFixedSize(size);
        }

        //
        public byte[] CreateImage(ArchiveEntry entry, Size size, BitmapImageFormat format, int quality, BitmapCreateSetting setting)
        {
            using (var ms = new MemoryStream())
            {
                var pdfArchiver = (PdfArchiver)entry.Archiver;
                size = size.IsEmpty ? pdfArchiver.GetRenderSize(entry) : size;
                pdfArchiver.CraeteBitmapSource(entry, size).SaveWithQuality(ms, CreateFormat(format), quality);
                return ms.ToArray();
            }
        }


        //
        private System.Drawing.Imaging.ImageFormat CreateFormat(BitmapImageFormat format)
        {
            switch (format)
            {
                default:
                case BitmapImageFormat.Jpeg:
                    return System.Drawing.Imaging.ImageFormat.Jpeg;
                case BitmapImageFormat.Png:
                    return System.Drawing.Imaging.ImageFormat.Png;
            }
        }
    }
}
