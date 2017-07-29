// Copyright (c) 2016 Mitsuhiro Ito (nee)
//
// This software is released under the MIT License.
// http://opensource.org/licenses/mit-license.php

using PhotoSauce.MagicScaler;
using System.Diagnostics;
using System.IO;
using System.Windows;
using System.Windows.Media.Imaging;

namespace NeeView
{
    /// <summary>
    /// MagicScaler BitmapFactory
    /// </summary>
    public class MagicScalerBitmapFactory : IBitmapFactory
    {
        // MagicScaler設定.
        // 内部で変更されます。
        public ProcessImageSettings Setting { get; set; }

        //
        private ProcessImageSettings CreateSetting(Size size, FileFormat format)
        {
            var setting = this.Setting ?? new ProcessImageSettings();
            setting.Width = size.IsEmpty ? 0 : (int)size.Width;
            setting.Height = size.IsEmpty ? 0 : (int)size.Height;
            setting.SaveFormat = format;

            return setting;
        }

        //
        public BitmapImage Create(Stream stream, BitmapInfo info, Size size)
        {
            stream.Seek(0, SeekOrigin.Begin);

            using (var ms = new MemoryStream())
            {
                var setting = CreateSetting(size, FileFormat.Bmp);
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
            Debug.WriteLine($"MagicScalerImage: {size.Truncate()}");

            stream.Seek(0, SeekOrigin.Begin);

            var setting = CreateSetting(size, CreateFormat(format));
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
