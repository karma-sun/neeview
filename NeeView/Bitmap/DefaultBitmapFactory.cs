// Copyright (c) 2016-2017 Mitsuhiro Ito (nee)
//
// This software is released under the MIT License.
// http://opensource.org/licenses/mit-license.php

using System;
using System.Diagnostics;
using System.IO;
using System.Windows;
using System.Windows.Media.Imaging;

namespace NeeView
{
    /// <summary>
    /// BitmapFactory interface
    /// </summary>
    public interface IBitmapFactory
    {
        BitmapImage Create(Stream stream, BitmapInfo info, Size size);
        void CreateImage(Stream stream, BitmapInfo info, Stream outStream, Size size, BitmapImageFormat format, int quality);
    }

    /// <summary>
    /// Default BitmapFactory
    /// </summary>
    public class DefaultBitmapFactory : IBitmapFactory
    {
        //
        public BitmapImage Create(Stream stream, BitmapInfo info, Size size)
        {
            info = info ?? BitmapInfo.Create(stream);

            try
            {
                return Create(stream, BitmapCreateOptions.None, BitmapCacheOption.OnLoad, size, info);
            }
            catch (OutOfMemoryException)
            {
                throw;
            }
            catch (Exception ex)
            {
                // カラープロファイルを無効にして再生成
                Debug.WriteLine($"BitmapImage Failed: {ex.Message}\nTry IgnoreColorProfile ...");
                return Create(stream, BitmapCreateOptions.IgnoreColorProfile, BitmapCacheOption.OnLoad, size, info);
            }
        }

        //
        private BitmapImage Create(Stream stream, BitmapCreateOptions createOption, BitmapCacheOption cacheOption, Size size, BitmapInfo info)
        {
            stream.Seek(0, SeekOrigin.Begin);

            var bitmap = new BitmapImage();
            bitmap.BeginInit();
            bitmap.CreateOptions = createOption;
            bitmap.CacheOption = cacheOption;
            bitmap.StreamSource = stream;

            if (size != Size.Empty)
            {
                bitmap.DecodePixelHeight = info.IsTranspose ? (int)size.Width : (int)size.Height;
                bitmap.DecodePixelWidth = info.IsTranspose ? (int)size.Height : (int)size.Width;
            }

            if (info.IsMirrorHorizontal || info.IsMirrorVertical || info.Rotation != Rotation.Rotate0)
            {
                bitmap.DecodePixelWidth = (bitmap.DecodePixelWidth == 0 ? info.PixelWidth : bitmap.DecodePixelWidth) * (info.IsMirrorHorizontal ? -1 : 1);
                bitmap.DecodePixelHeight = (bitmap.DecodePixelHeight == 0 ? info.PixelHeight : bitmap.DecodePixelHeight) * (info.IsMirrorVertical ? -1 : 1);
                bitmap.Rotation = info.Rotation;
            }

            bitmap.EndInit();
            bitmap.Freeze();

            // out of memory, maybe.
            if (stream.Length > 100 * 1024 && bitmap.PixelHeight == 1 && bitmap.PixelWidth == 1)
            {
                Debug.WriteLine("1x1!?");
                throw new OutOfMemoryException();
            }

            return bitmap;
        }

        //
        public void CreateImage(Stream stream, BitmapInfo info, Stream outStream, Size size, BitmapImageFormat format, int quality)
        {
            Debug.WriteLine($"DefaultImage: {size.Truncate()}");

            BitmapSource bitmap = Create(stream, info, size);

            if (bitmap.DpiX != bitmap.DpiY)
            {
                Debug.WriteLine($"WrongDPI: {bitmap.DpiX},{bitmap.DpiY}");

                double dpi = 96;
                int width = bitmap.PixelWidth;
                int height = bitmap.PixelHeight;

                int stride = width * ((bitmap.Format.BitsPerPixel + 7) / 8);
                stride = (stride + 3) / 4 * 4; // 不要かも？

                byte[] pixelData = new byte[stride * height];
                bitmap.CopyPixels(pixelData, stride, 0);

                bitmap = BitmapSource.Create(width, height, dpi, dpi, bitmap.Format, bitmap.Palette, pixelData, stride);
            }

            var encoder = CreateEncoder(format, quality);
            encoder.Frames.Add(BitmapFrame.Create(bitmap));
            encoder.Save(outStream);
        }

        //
        private BitmapEncoder CreateEncoder(BitmapImageFormat format, int quality)
        {
            switch (format)
            {
                default:
                case BitmapImageFormat.Jpeg:
                    var jpegEncoder = new JpegBitmapEncoder();
                    jpegEncoder.QualityLevel = quality;
                    return jpegEncoder;
                case BitmapImageFormat.Png:
                    return new PngBitmapEncoder();
            }
        }
    }




}
