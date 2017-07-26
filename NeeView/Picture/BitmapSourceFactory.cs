// Copyright (c) 2016 Mitsuhiro Ito (nee)
//
// This software is released under the MIT License.
// http://opensource.org/licenses/mit-license.php

using System;
using System.Diagnostics;
using System.IO;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace NeeView
{
    /// <summary>
    /// 標準の画像生成処理
    /// </summary>
    public class BitmapSourceFactory
    {
        // singleton
        //private static BitmapSourceFactory _current;
        //public static BitmapSourceFactory Current => _current = _current ?? new BitmapSourceFactory();

        //
        public BitmapInfo CreateInfo(Stream stream)
        {
            stream.Seek(0, SeekOrigin.Begin);
            var bitmapFrame = BitmapFrame.Create(stream, BitmapCreateOptions.DelayCreation | BitmapCreateOptions.IgnoreColorProfile, BitmapCacheOption.None);
            var info = new BitmapInfo(bitmapFrame.PixelWidth, bitmapFrame.PixelHeight, (BitmapMetadata)bitmapFrame.Metadata);
            return info;
        }

        //
        public BitmapImage Create(Stream stream, Size size)
        {
            return Create(stream, size, CreateInfo(stream));
        }

        //
        public BitmapImage Create(Stream stream, Size size, BitmapInfo info)
        {
            try
            {
                return Create(stream, BitmapCreateOptions.None, BitmapCacheOption.OnLoad, info, size);
            }
            catch (OutOfMemoryException)
            {
                throw;
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"DefaultBitmap: {ex.Message}");
                return Create(stream, BitmapCreateOptions.IgnoreColorProfile, BitmapCacheOption.OnLoad, info, size);
            }
        }


        //
        private BitmapImage Create(Stream stream, BitmapCreateOptions createOption, BitmapCacheOption cacheOption, BitmapInfo info, Size size)
        {
            // image 
            stream.Seek(0, SeekOrigin.Begin);
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

            if (info.IsMirrorHorizontal || info.IsMirrorVertical || info.Rotation != Rotation.Rotate0)
            {
                bmpImage.DecodePixelWidth = (bmpImage.DecodePixelWidth == 0 ? info.PixelWidth : bmpImage.DecodePixelWidth) * (info.IsMirrorHorizontal ? -1 : 1);
                bmpImage.DecodePixelHeight = (bmpImage.DecodePixelHeight == 0 ? info.PixelHeight : bmpImage.DecodePixelHeight) * (info.IsMirrorVertical ? -1 : 1);
                bmpImage.Rotation = info.Rotation;
            }

            bmpImage.EndInit();
            bmpImage.Freeze();

            // out of memory, maybe.
            if (stream.Length > 100 * 1024 && bmpImage.PixelHeight == 1 && bmpImage.PixelWidth == 1)
            {
                Debug.WriteLine("1x1!?");
                throw new OutOfMemoryException();
            }

            return bmpImage;
        }
    }

    public class BitmapInfo
    {
        public int PixelWidth { get; set; }
        public int PixelHeight { get; set; }
        public int Orinentation { get; set; }
        public bool IsMirrorHorizontal { get; set; }
        public bool IsMirrorVertical { get; set; }
        public Rotation Rotation { get; set; }
        public BitmapMetadata Metadata { get; set; }

        public BitmapInfo(int width, int height, BitmapMetadata metadata)
        {
            this.PixelWidth = width;
            this.PixelHeight = height;
            this.Metadata = metadata;

            if (metadata != null)
            {
                var exif = new ExifAccessor(metadata);
                this.Orinentation = exif.Orientation;

                switch (exif.Orientation)
                {
                    default:
                    case 1: // normal
                        break;
                    case 2: // Mirror horizontal
                        this.IsMirrorHorizontal = true;
                        break;
                    case 3: // Rotate 180
                        this.Rotation = Rotation.Rotate180;
                        break;
                    case 4: //Mirror vertical
                        this.IsMirrorVertical = true;
                        break;
                    case 5: // Mirror horizontal and rotate 270 CW
                        this.IsMirrorHorizontal = true;
                        this.Rotation = Rotation.Rotate270;
                        break;
                    case 6: //Rotate 90 CW
                        this.Rotation = Rotation.Rotate90;
                        break;
                    case 7: // Mirror horizontal and rotate 90 CW
                        this.IsMirrorHorizontal = true;
                        this.Rotation = Rotation.Rotate90;
                        break;
                    case 8: // Rotate 270 CW
                        this.Rotation = Rotation.Rotate270;
                        break;
                }
            }
        }
    }

}
