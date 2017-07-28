// Copyright (c) 2016 Mitsuhiro Ito (nee)
//
// This software is released under the MIT License.
// http://opensource.org/licenses/mit-license.php

using PhotoSauce.MagicScaler;
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
    public class BitmapFactory
    {
        /// <summary>
        /// 画像のサイズ、メタ情報のみ取得
        /// </summary>
        /// <param name="stream"></param>
        /// <returns></returns>
        public BitmapInfo CreateInfo(Stream stream)
        {
            stream.Seek(0, SeekOrigin.Begin);

            try
            {
                var bitmapFrame = BitmapFrame.Create(stream, BitmapCreateOptions.DelayCreation | BitmapCreateOptions.IgnoreColorProfile, BitmapCacheOption.None);
                return new BitmapInfo(bitmapFrame.PixelWidth, bitmapFrame.PixelHeight, (BitmapMetadata)bitmapFrame.Metadata);
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex.Message);
                return new BitmapInfo();
            }
        }

        /// <summary>
        /// Bitmap生成
        /// </summary>
        /// <param name="stream"></param>
        /// <param name="size"></param>
        /// <returns></returns>
        public BitmapImage Create(Stream stream, Size size)
        {
            return Create(stream, size, CreateInfo(stream));
        }

        /// <summary>
        /// Bitmap生成
        /// </summary>
        /// <param name="stream"></param>
        /// <param name="size"></param>
        /// <param name="info"></param>
        /// <returns></returns>
        public BitmapImage Create(Stream stream, Size size, BitmapInfo info)
        {
            // by MagicScaler
            if (!size.IsEmpty && PictureProfile.Current.IsResizeFilterEnabled)
            {
                try
                {
                    return CreateByMagicScaler(stream, size);
                }
                catch (OutOfMemoryException)
                {
                    throw;
                }
                catch (Exception ex)
                {
                    Debug.WriteLine("MagicScaler Failed:" + ex.Message);
                }
            }

            // by Default
            return CreateByBitmapImage(stream, size, info);
        }


        /// <summary>
        /// Bitmap生成
        /// </summary>
        /// <param name="stream"></param>
        /// <param name="size"></param>
        /// <param name="info"></param>
        /// <returns></returns>
        public BitmapImage CreateByBitmapImage(Stream stream, Size size, BitmapInfo info)
        {
            try
            {
                return CreateByBitmapImage(stream, BitmapCreateOptions.None, BitmapCacheOption.OnLoad, size, info);
            }
            catch (OutOfMemoryException)
            {
                throw;
            }
            catch (Exception ex)
            {
                // カラープロファイルを無効にして再生成
                Debug.WriteLine($"BitmapImage Failed: {ex.Message}\nTry IgnoreColorProfile ...");
                return CreateByBitmapImage(stream, BitmapCreateOptions.IgnoreColorProfile, BitmapCacheOption.OnLoad, size, info);
            }
        }
        
        /// <summary>
        /// Bitmap生成
        /// </summary>
        /// <param name="stream"></param>
        /// <param name="createOption"></param>
        /// <param name="cacheOption"></param>
        /// <param name="size"></param>
        /// <param name="info"></param>
        /// <returns></returns>
        private BitmapImage CreateByBitmapImage(Stream stream, BitmapCreateOptions createOption, BitmapCacheOption cacheOption, Size size, BitmapInfo info)
        {
            stream.Seek(0, SeekOrigin.Begin);

            var bitmap = new BitmapImage();
            bitmap.BeginInit();
            bitmap.CreateOptions = createOption;
            bitmap.CacheOption = cacheOption;
            bitmap.StreamSource = stream;

            if (size != Size.Empty)
            {
                bitmap.DecodePixelHeight = (int)size.Height;
                bitmap.DecodePixelWidth = (int)size.Width;
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


        /// <summary>
        /// MagicScalerで指定サイズの画像を生成
        /// </summary>
        /// <param name="stream"></param>
        /// <param name="size"></param>
        /// <returns></returns>
        private BitmapImage CreateByMagicScaler(Stream stream, Size size)
        {
            Debug.WriteLine($"MagicScaler: {size.Truncate()}");

            stream.Seek(0, SeekOrigin.Begin);

            using (var ms = new MemoryStream())
            {
                var setting = new ProcessImageSettings();
                setting.Width = (int)size.Width;
                setting.Height = (int)size.Height;
                setting.SaveFormat = FileFormat.Bmp; // 速度優先のため出力はBMP

                // シャープネスON/OFF
                //setting.Sharpen = false;

                // ハイブリッドモード (速度 or 品質)
                //setting.HybridMode = HybridScaleMode.Off;

                // 補完アルゴリズム
                //var interporatoin = new InterpolationSettings(new PhotoSauce.MagicScaler.Interpolators.LanczosInterpolator());
                //setting.Interpolation = interporatoin;

                //MagicImageProcessor.EnableSimd = true;
                //MagicImageProcessor.EnablePlanarPipeline = true;

                // GO!
                MagicImageProcessor.ProcessImage(stream, ms, setting);

                // ビットマップ化
                ms.Seek(0, SeekOrigin.Begin);

                var bi = new BitmapImage();
                bi.BeginInit();
                bi.CacheOption = BitmapCacheOption.OnLoad; // ;
                bi.StreamSource = ms;
                bi.EndInit();
                bi.Freeze();

                return bi;
            }
        }
    }

}
