﻿using NeeView.Drawing;
using NeeView.Media.Imaging;
using NeeView.Threading;
using SharpVectors.Converters;
using SharpVectors.Renderers.Wpf;
using System;
using System.Diagnostics;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace NeeView
{
    public class SvgPictureSource : PictureSource
    {
        private static object _lock = new object();

        private PictureStreamSource _streamSource;
        private ImageSource _imageSource;


        public SvgPictureSource(ArchiveEntry entry, PictureInfo pictureInfo, PictureSourceCreateOptions createOptions) : base(entry, pictureInfo, createOptions)
        {
            _streamSource = new PictureStreamSource(entry);
        }


        public override long GetMemorySize()
        {
            return _streamSource.GetMemorySize();
        }

        private void Initialize(CancellationToken token)
        {
            if (_imageSource != null) return;

            token.ThrowIfCancellationRequested();

            using (var stream = _streamSource.CreateStream(token))
            {
                var settings = new WpfDrawingSettings();
                settings.IncludeRuntime = false;
                settings.TextAsGeometry = true;

                DrawingGroup drawing;
                lock (_lock)
                {
                    var reader = new FileSvgReader(settings);
                    drawing = reader.Read(stream);
                }
                drawing.Freeze();

                var image = new DrawingImage();
                image.Drawing = drawing;
                image.Freeze();

                _imageSource = image;
            }
        }

        public override PictureInfo CreatePictureInfo(CancellationToken token)
        {
            if (this.PictureInfo != null) return this.PictureInfo;

            token.ThrowIfCancellationRequested();

            Initialize(token);

            this.PictureInfo = new PictureInfo();

            var size = new Size(_imageSource.Width, _imageSource.Height);
            PictureInfo.OriginalSize = size;
            PictureInfo.Size = size;
            PictureInfo.BitsPerPixel = 32;
            PictureInfo.Decoder = "SharpVecotrs";

            return this.PictureInfo;
        }

        public override ImageSource CreateImageSource(Size size, BitmapCreateSetting setting, CancellationToken token)
        {
            Initialize(token);
            return _imageSource;
        }

        public override byte[] CreateImage(Size size, BitmapCreateSetting setting, BitmapImageFormat format, int quality, CancellationToken token)
        {
            if (size.IsEmptyOrZero()) throw new ArgumentOutOfRangeException(nameof(size));

            Initialize(token);

            BitmapSource bitmap = null;
            var task = new Task(() =>
            {
                bitmap = _imageSource.CreateThumbnail(size);
            });
            task.Start(SingleThreadedApartment.TaskScheduler); // STA
            task.Wait(token);

            using (var outStream = new MemoryStream())
            {
                var encoder = DefaultBitmapFactory.CreateEncoder(format, quality);
                encoder.Frames.Add(BitmapFrame.Create(bitmap));
                encoder.Save(outStream);
                return outStream.ToArray();
            }
        }

        public override byte[] CreateThumbnail(ThumbnailProfile profile, CancellationToken token)
        {
            token.ThrowIfCancellationRequested();

            Size size;
            if (PictureInfo != null)
            {
                size = PictureInfo.Size;
            }
            else
            {
                Initialize(token);
                size = new Size(_imageSource.Width, _imageSource.Height);
            }

            size = profile.GetThumbnailSize(size);
            var setting = profile.CreateBitmapCreateSetting(true);
            return CreateImage(size, setting, Config.Current.Thumbnail.Format, Config.Current.Thumbnail.Quality, token);
        }

        public override Size FixedSize(Size size)
        {
            // SVGはサイズ制限なし
            Debug.Assert(PictureInfo != null);
            return this.PictureInfo.Size;
        }
    }

}

