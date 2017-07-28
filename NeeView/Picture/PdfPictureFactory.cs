// Copyright (c) 2016 Mitsuhiro Ito (nee)
//
// This software is released under the MIT License.
// http://opensource.org/licenses/mit-license.php

using System.Windows;
using System.Windows.Media.Imaging;

namespace NeeView
{
    /// <summary>
    /// PDF Picture Factory
    /// </summary>
    public class PdfPictureFactory : IPictureFactory
    {
        public Picture Create(ArchiveEntry entry)
        {
            var pdfArchiver = (PdfArchiver)entry.Archiver;
            var profile = PdfArchiverProfile.Current;

            var size = pdfArchiver.GetRenderSize(entry);
            var bitmapSource = pdfArchiver.CraeteBitmapSource(entry, size);

            var picture = new Picture(entry);

            picture.PictureInfo.Decoder = "PDFium";
            picture.PictureInfo.SetPixelInfo(bitmapSource);

            picture.BitmapSource = bitmapSource;

            return picture;
        }

        public BitmapSource CreateBitmapSource(ArchiveEntry entry, Size size)
        {
            var pdfArchiver = (PdfArchiver)entry.Archiver;
            size = size.IsEmpty ? pdfArchiver.GetRenderSize(entry) : size;
            return pdfArchiver.CraeteBitmapSource(entry, size);
        }

        public Size CreateFixedSize(ArchiveEntry entry, Size size)
        {
            return PdfArchiverProfile.Current.CreateFixedSize(size);
        }
    }
}
