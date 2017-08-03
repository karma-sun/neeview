// Copyright (c) 2016 Mitsuhiro Ito (nee)
//
// This software is released under the MIT License.
// http://opensource.org/licenses/mit-license.php

using NeeView.Utility;
using System;
using System.Diagnostics;
using System.IO;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace NeeView
{
    /// <summary>
    /// PDF Picture Factory
    /// </summary>
    public class PdfPictureFactory : IPictureFactory
    {
        private MagicScalerBitmapFactory _magicScaler = new MagicScalerBitmapFactory();

        //
        public Picture Create(ArchiveEntry entry, PictureCreateOptions options)
        {
            var pdfArchiver = (PdfArchiver)entry.Archiver;
            var profile = PdfArchiverProfile.Current;

            var picture = new Picture(entry);

            var size = pdfArchiver.GetRenderSize(entry);
            picture.PictureInfo.Size = size;

            // bitmap
            if (options.HasFlag(PictureCreateOptions.CreateBitmap) || options.HasFlag(PictureCreateOptions.CreateThumbnail))
            {
                var bitmapSource = Utility.NVGraphics.ToBitmapSource(pdfArchiver.CraeteBitmapSource(entry, size));

                picture.PictureInfo.Decoder = "PDFium";
                picture.PictureInfo.SetPixelInfo(bitmapSource, Size.Empty);

                picture.BitmapSource = bitmapSource;
            }

            // thumbnail
            if (options.HasFlag(PictureCreateOptions.CreateThumbnail))
            {
                using (var ms = new MemoryStream())
                using (var intermediate = new MemoryStream())
                {
                    var encoder = new BmpBitmapEncoder();
                    encoder.Frames.Add(BitmapFrame.Create(picture.BitmapSource));
                    encoder.Save(intermediate);
                    intermediate.Seek(0, SeekOrigin.Begin);

                    var thumbnailSize = ThumbnailProfile.Current.GetThumbnailSize(picture.PictureInfo.Size);
                    _magicScaler.CreateImage(intermediate, null, ms, thumbnailSize, ThumbnailProfile.Current.Format, ThumbnailProfile.Current.Quality);
                    picture.Thumbnail = ms.ToArray();
                }
            }

            return picture;
        }

        //
        public BitmapSource CreateBitmapSource(ArchiveEntry entry, byte[] raw, Size size)
        {
            var pdfArchiver = (PdfArchiver)entry.Archiver;
            size = size.IsEmpty ? pdfArchiver.GetRenderSize(entry) : size;
            return Utility.NVGraphics.ToBitmapSource(pdfArchiver.CraeteBitmapSource(entry, size));
        }


        //
        public byte[] CreateImage(ArchiveEntry entry, byte[] raw, Size size, BitmapImageFormat format, int quality, BitmapCreateSetting setting)
        {
            using (var ms = new MemoryStream())
            {
                var pdfArchiver = (PdfArchiver)entry.Archiver;
                var defaultSize = pdfArchiver.GetRenderSize(entry);
                size = size.IsEmpty ? defaultSize : size;
                var renderSize = defaultSize.IsContains(size) ? defaultSize : size;

                if (renderSize != size || setting.Source != null)
                {
                    using (var intermediate = new MemoryStream())
                    {
                        if (setting.Source != null)
                        {
                            var encoder = new BmpBitmapEncoder();
                            encoder.Frames.Add(BitmapFrame.Create(setting.Source));
                            encoder.Save(intermediate);
                        }
                        else
                        {
                            pdfArchiver.CraeteBitmapSource(entry, renderSize).Save(intermediate, System.Drawing.Imaging.ImageFormat.Bmp);
                        }
                        intermediate.Seek(0, SeekOrigin.Begin);
                        _magicScaler.CreateImage(intermediate, null, ms, size, format, quality, setting.ProcessImageSettings);
                    }
                }
                else
                {
                    pdfArchiver.CraeteBitmapSource(entry, renderSize).SaveWithQuality(ms, CreateFormat(format), quality);
                }

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
