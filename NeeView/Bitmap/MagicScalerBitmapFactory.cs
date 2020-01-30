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
        // 注意: sourceは上書きされます
        private ProcessImageSettings CreateSetting(Size size, FileFormat format, ProcessImageSettings source)
        {
            var setting = source ?? new ProcessImageSettings();

            // widthを0にすると、heightを基準にwidthを決定する
            setting.Width = size.IsEmpty ? 0 : (int)size.Width;
            setting.Height = size.IsEmpty ? 0 : (int)size.Height;
            setting.ResizeMode = setting.Width == 0 ? CropScaleMode.Crop : CropScaleMode.Stretch;
            setting.SaveFormat = format;

            return setting;
        }

        //
        public BitmapImage Create(Stream stream, BitmapInfo info, Size size, CancellationToken token)
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
        public void CreateImage(Stream stream, BitmapInfo info, Stream outStream, Size size, BitmapImageFormat format, int quality, CancellationToken token)
        {
            token.ThrowIfCancellationRequested();
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
