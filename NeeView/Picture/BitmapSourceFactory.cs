// Copyright (c) 2016 Mitsuhiro Ito (nee)
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
    /// 標準の画像生成処理
    /// </summary>
    public class BitmapSourceFactory
    {
        // singleton
        private static BitmapSourceFactory _current;
        public static BitmapSourceFactory Current => _current = _current ?? new BitmapSourceFactory();


        public BitmapImage Create(byte[] raw)
        {
            if (raw == null) return null;

            using (var ms = new MemoryStream(raw))
            {
                return Create(ms, Size.Empty);
            }
        }

        //
        public BitmapImage Create(Stream stream, Size size)
        {
            try
            {
                return Create(stream, BitmapCreateOptions.None, BitmapCacheOption.OnLoad, size);
            }
            catch (Exception e)
            {
                Debug.WriteLine($"DefaultBitmap: {e.Message}");
                stream.Seek(0, SeekOrigin.Begin);
                return Create(stream, BitmapCreateOptions.IgnoreColorProfile, BitmapCacheOption.OnLoad, size);
            }
        }

        //
        private BitmapImage Create(Stream stream, BitmapCreateOptions createOption, BitmapCacheOption cacheOption, Size size)
        {
            var bmpImage = new BitmapImage();
            bmpImage.BeginInit();
            bmpImage.CreateOptions = createOption;
            bmpImage.CacheOption = cacheOption;
            bmpImage.StreamSource = stream;
            if (size != Size.Empty)
            {
                bmpImage.DecodePixelHeight = (int)size.Height;
                bmpImage.DecodePixelWidth = (int)size.Width;
            }
            bmpImage.EndInit();
            bmpImage.Freeze();

            return bmpImage;
        }

    }

}
