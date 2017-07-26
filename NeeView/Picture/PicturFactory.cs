// Copyright (c) 2016 Mitsuhiro Ito (nee)
//
// This software is released under the MIT License.
// http://opensource.org/licenses/mit-license.php

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media.Imaging;

namespace NeeView
{
    /// <summary>
    /// Picture Factory interface.
    /// </summary>
    public interface IPictureFactory
    {
        Picture Create(ArchiveEntry entry);
        BitmapSource CreateBitmapSource(ArchiveEntry entry, Size size);
    }

    /// <summary>
    /// Picture Factory
    /// </summary>
    public class PicturFactory : IPictureFactory
    {
        private DefaultPictureFactory _defaultFactory = new DefaultPictureFactory();

        private PdfPictureFactory _pdfFactory = new PdfPictureFactory();

        //
        public Picture Create(ArchiveEntry entry)
        {
            if (entry.Archiver is PdfArchiver)
            {
                return _pdfFactory.Create(entry);
            }
            else
            {
                return _defaultFactory.Create(entry);
            }
        }

        //
        public BitmapSource CreateBitmapSource(ArchiveEntry entry, Size size)
        {
            if (entry.Archiver is PdfArchiver)
            {
                return _pdfFactory.CreateBitmapSource(entry, size);
            }
            else
            {
                return _defaultFactory.CreateBitmapSource(entry, size);
            }
        }
    }

    /// <summary>
    /// Default Picture Factory
    /// </summary>
    public class DefaultPictureFactory : IPictureFactory
    {
        //
        private PictureStream _pictureStream = new PictureStream();

        //
        private BitmapSourceFactory _bitmapFactory = new BitmapSourceFactory();

        //
        public Picture Create(ArchiveEntry entry)
        {
            var picture = new Picture(entry);

            using (var stream = _pictureStream.Create(entry))
            {
                // info
                var bitmapFrame = BitmapFrame.Create(stream.Stream, BitmapCreateOptions.DelayCreation | BitmapCreateOptions.IgnoreColorProfile, BitmapCacheOption.None);
                picture.PictureInfo2 = new PictureInfo(entry, new Size(bitmapFrame.PixelWidth, bitmapFrame.PixelHeight), (BitmapMetadata)bitmapFrame.Metadata);
                picture.PictureInfo2.Decoder = stream.Name ?? ".Net BitmapImage";

                // bitmap
                var bitmapSource = _bitmapFactory.Create(stream.Stream, Size.Empty);
                picture.PictureInfo2.SetPixelInfo(bitmapSource);
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
    }

    /// <summary>
    /// PDF Picture Factory
    /// </summary>
    public class PdfPictureFactory : IPictureFactory
    {
        public Picture Create(ArchiveEntry entry)
        {
            var pdfArchiver = (PdfArchiver)entry.Archiver;

            var bitmapSource = pdfArchiver.CraeteBitmapSource(entry, PdfArchiverProfile.Current.RenderSize);

            var picture = new Picture(entry);

            picture.PictureInfo2 = new PictureInfo();
            picture.PictureInfo2.Size = new Size(bitmapSource.PixelWidth, bitmapSource.PixelHeight);
            picture.PictureInfo2.Decoder = "PDFium";

            picture.BitmapSource = bitmapSource;

            return picture;
        }

        public BitmapSource CreateBitmapSource(ArchiveEntry entry, Size size)
        {
            var pdfArchiver = (PdfArchiver)entry.Archiver;

            return pdfArchiver.CraeteBitmapSource(entry, size);
        }
    }
}
