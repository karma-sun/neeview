﻿// Copyright (c) 2016 Mitsuhiro Ito (nee)
//
// This software is released under the MIT License.
// http://opensource.org/licenses/mit-license.php

using System.Diagnostics;
using System.IO;
using System.Threading.Tasks;
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

            using (var stream = new MemoryStream())
            {
                string decoder = null;

                // raw data
                using (var namedStream = _pictureStream.Create(entry))
                {
                    namedStream.Stream.CopyTo(stream);
                    picture.RawData = stream.ToArray();
                    decoder = namedStream.Name;
                }

                // info
                stream.Seek(0, SeekOrigin.Begin);
                var info = BitmapInfo.Create(stream);
                var originalSize = info.IsTranspose ? info.GetPixelSize().Transpose() : info.GetPixelSize();
                picture.PictureInfo.OriginalSize = originalSize;

                var maxSize = info.IsTranspose ? PictureProfile.Current.MaximumSize.Transpose() : PictureProfile.Current.MaximumSize;
                var size = (PictureProfile.Current.IsLimitSourceSize && !maxSize.IsContains(originalSize)) ? originalSize.Uniformed(maxSize) : Size.Empty;
                picture.PictureInfo.Size = size.IsEmpty ? originalSize : size;

                // bitmap
                if (options.HasFlag(PictureCreateOptions.CreateBitmap) || picture.PictureInfo.Size.IsEmpty)
                {
                    var bitmapSource = _bitmapFactory.Create(stream, info, size, new BitmapCreateSetting());

                    picture.PictureInfo.Exif = info.Metadata != null ? new BitmapExif(info.Metadata) : null;
                    picture.PictureInfo.Decoder = decoder ?? ".Net BitmapImage";
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
                        _bitmapFactory.CreateImage(stream, info, ms, thumbnailSize, ThumbnailProfile.Current.Format, ThumbnailProfile.Current.Quality, ThumbnailProfile.Current.CreateBitmapCreateSetting());
                        picture.Thumbnail = ms.ToArray();

                        ////Debug.WriteLine($"Thumbnail: {picture.Thumbnail.Length / 1024}KB");
                    }
                }

            }

            // RawData: メモリ圧縮のためにBMPはPNGに変換 (非同期)
            if (picture.RawData != null && picture.RawData[0] == 'B' && picture.RawData[1] == 'M')
            {
                Task.Run(() =>
                {
                    ////var sw = Stopwatch.StartNew();
                    ////var oldLength = picture.RawData.Length;

                    using (var inStream = new MemoryStream(picture.RawData))
                    using (var outStream = new MemoryStream())
                    {
                        var encoder = new PngBitmapEncoder();
                        encoder.Frames.Add(BitmapFrame.Create(inStream));
                        encoder.Save(outStream);
                        picture.RawData = outStream.ToArray();
                    }

                    ////sw.Stop();
                    ////Debug.WriteLine($"{entry.EntryLastName}: BMP to PNG: {sw.ElapsedMilliseconds}ms, {oldLength/1024}KB -> {picture.RawData.Length / 1024}KB");
                });
            }

            return picture;
        }

        //
        private Stream CreateStream(ArchiveEntry entry, byte[] raw)
        {
            if (raw != null)
            {
                return new MemoryStream(raw);
            }
            else
            {
                return _pictureStream.Create(entry).Stream;
            }
        }

        //
        public BitmapSource CreateBitmapSource(ArchiveEntry entry, byte[] raw, Size size)
        {
            using (var stream = CreateStream(entry, raw))
            {
                var setting = new BitmapCreateSetting();

                if (PictureProfile.Current.IsResizeFilterEnabled && !size.IsEmpty)
                {
                    setting.Mode = BitmapCreateMode.HighQuality;
                    setting.ProcessImageSettings = ImageFilter.Current.CreateProcessImageSetting();
                }

                return _bitmapFactory.Create(stream, null, size, setting);
            }
        }

        //
        public byte[] CreateImage(ArchiveEntry entry, byte[] raw, Size size, BitmapImageFormat format, int quality, BitmapCreateSetting setting)
        {
            using (var stream = CreateStream(entry, raw))
            using (var ms = new MemoryStream())
            {
                _bitmapFactory.CreateImage(stream, null, ms, size, format, quality, setting);
                return ms.ToArray();
            }
        }
    }
}