﻿// Copyright (c) 2016 Mitsuhiro Ito (nee)
//
// This software is released under the MIT License.
// http://opensource.org/licenses/mit-license.php

using PhotoSauce.MagicScaler;
using System;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Threading;
using System.Windows;
using System.Windows.Media.Imaging;

namespace NeeView
{
    /// <summary>
    /// MagicScaler BitmapFactory
    /// </summary>
    public class MagicScalerBitmapFactory : IBitmapFactory
    {
        static MagicScalerBitmapFactory()
        {
            // 処理が終わらなくなる場合があるため、SIMDを無効にしておく
            MagicImageProcessor.EnableSimd = false;
        }

        // 注意: sourceは上書きされます
        private ProcessImageSettings CreateSetting(Size size, FileFormat format, ProcessImageSettings source)
        {
            var setting = source ?? new ProcessImageSettings();
            ////setting.Width = size.IsEmpty ? 0 : (int)size.Width; // 縦横比維持のため、片側のみ設定
            setting.Height = size.IsEmpty ? 0 : (int)size.Height;
            setting.SaveFormat = format;

#if false
            // https://github.com/saucecontrol/PhotoSauce/issues/7
            // グローバル変数なので、同時に使用されると問題ある。
            if (setting.Interpolation.Equals(InterpolationSettings.NearestNeighbor))
            {
                MagicImageProcessor.EnablePlanarPipeline = false;
            }
            else
            {
                MagicImageProcessor.EnablePlanarPipeline = true;
            }
#endif

            return setting;
        }

        //
        public BitmapImage Create(Stream stream, BitmapInfo info, Size size)
        {
            return Create(stream, info, size, null);
        }

        //
        public BitmapImage Create(Stream stream, BitmapInfo info, Size size, ProcessImageSettings setting)
        {
            stream.Seek(0, SeekOrigin.Begin);

            using (var ms = new MemoryStream())
            {
                setting = CreateSetting(size, FileFormat.Bmp, setting);
                MagicImageProcessor.ProcessImage(stream, ms, setting);

                ms.Seek(0, SeekOrigin.Begin);
                var bitmap = new BitmapImage();
                bitmap.BeginInit();
                bitmap.CacheOption = BitmapCacheOption.OnLoad;
                bitmap.StreamSource = ms;
                bitmap.EndInit();
                bitmap.Freeze();

                return bitmap;
            }
        }

        //
        public void CreateImage(Stream stream, BitmapInfo info, Stream outStream, Size size, BitmapImageFormat format, int quality)
        {
            CreateImage(stream, info, outStream, size, format, quality, null);
        }

        //
        public void CreateImage(Stream stream, BitmapInfo info, Stream outStream, Size size, BitmapImageFormat format, int quality, ProcessImageSettings setting)
        {
            ////Debug.WriteLine($"MagicScalerImage: {size.Truncate()}");

            stream.Seek(0, SeekOrigin.Begin);

            setting = CreateSetting(size, CreateFormat(format), setting);
            setting.JpegQuality = quality;

            MagicImageProcessor.ProcessImage(stream, outStream, setting);
        }

        //
        private FileFormat CreateFormat(BitmapImageFormat format)
        {
            switch (format)
            {
                default:
                case BitmapImageFormat.Jpeg:
                    return FileFormat.Jpeg;
                case BitmapImageFormat.Png:
                    return FileFormat.Png;
            }
        }
    }




}