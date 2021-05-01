using NeeView.Media.Imaging.Metadata;
using System;
using System.Diagnostics;
using System.IO;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace NeeView
{
    /// <summary>
    /// Bitmap 情報
    /// </summary>
    public class BitmapInfo
    {
        public BitmapInfo()
        {
        }

        public BitmapInfo(BitmapFrame bitmapFrame)
        {
            this.PixelWidth = bitmapFrame.PixelWidth;
            this.PixelHeight = bitmapFrame.PixelHeight;
            this.BitsPerPixel = bitmapFrame.Format.BitsPerPixel;
            this.Metadata = new BitmapMetadataDatabase(GetBitmapMetadata(bitmapFrame));
            this.FrameCount = bitmapFrame.Decoder is GifBitmapDecoder gifBitmapDecoder ? gifBitmapDecoder.Frames.Count : 1;
            this.DpiX = bitmapFrame.DpiX;
            this.DpiY = bitmapFrame.DpiY;
            this.AspectWidth = bitmapFrame.Width;
            this.AspectHeight = bitmapFrame.Height;

            if (this.Metadata != null && this.Metadata[BitmapMetadataKey.Orientation] is ExifOrientation orientation)
            {
                switch (orientation)
                {
                    default:
                    case ExifOrientation.Normal:
                        break;
                    case ExifOrientation.MirrorHorizontal:
                        this.IsMirrorHorizontal = true;
                        break;
                    case ExifOrientation.Rotate180:
                        this.Rotation = Rotation.Rotate180;
                        break;
                    case ExifOrientation.MirrorVertical:
                        this.IsMirrorVertical = true;
                        break;
                    case ExifOrientation.MirrorHorizontal_Rotate270:
                        this.IsMirrorHorizontal = true;
                        this.Rotation = Rotation.Rotate270;
                        break;
                    case ExifOrientation.Rotate90:
                        this.Rotation = Rotation.Rotate90;
                        break;
                    case ExifOrientation.MirrorHorizontal_Rotate90:
                        this.IsMirrorHorizontal = true;
                        this.Rotation = Rotation.Rotate90;
                        break;
                    case ExifOrientation.Rotate270:
                        this.Rotation = Rotation.Rotate270;
                        break;
                }
            }
        }


        public int PixelWidth { get; private set; }
        public int PixelHeight { get; private set; }
        public int BitsPerPixel { get; private set; }
        public bool IsMirrorHorizontal { get; private set; }
        public bool IsMirrorVertical { get; private set; }
        public Rotation Rotation { get; private set; }
        public double DpiX { get; private set; }
        public double DpiY { get; private set; }
        public double AspectWidth { get; private set; }
        public double AspectHeight { get; private set; }
        public int FrameCount { get; private set; }
        public BitmapMetadataDatabase Metadata { get; private set; }

        // 転置？
        public bool IsTranspose => (this.Rotation == Rotation.Rotate90 || this.Rotation == Rotation.Rotate270);



        private BitmapMetadata GetBitmapMetadata(BitmapFrame bitmapFrame)
        {
            if (bitmapFrame.Decoder is GifBitmapDecoder decoder)
            {
                try
                {
                    return decoder.Metadata ?? (bitmapFrame.Metadata as BitmapMetadata);
                }
                catch
                {
                    // nop.
                }
            }

            return bitmapFrame.Metadata as BitmapMetadata;
        }


        public Size GetPixelSize()
        {
            return (this.PixelWidth == 0.0 || this.PixelHeight == 0.0) ? Size.Empty : new Size(this.PixelWidth, this.PixelHeight);
        }

        public Size GetAspectSize()
        {
            return (this.AspectWidth == 0.0 || this.AspectHeight == 0.0) ? Size.Empty : new Size(this.AspectWidth, this.AspectHeight);
        }


        public static BitmapInfo Create(Stream stream)
        {
            stream.Seek(0, SeekOrigin.Begin);

            try
            {
                var bitmapFrame = BitmapFrame.Create(stream, BitmapCreateOptions.DelayCreation | BitmapCreateOptions.IgnoreColorProfile, BitmapCacheOption.None);
                return new BitmapInfo(bitmapFrame);
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex.Message);
                return new BitmapInfo();
            }
        }


        #region 開発用

        [Conditional("DEBUG")]
        private void DumpMetaData(string prefix, BitmapMetadata metadata)
        {
            ImageMetadata im = metadata;

            foreach (var name in metadata)
            {
                string query;

                try
                {
                    query = prefix + "(" + metadata.Format + ")" + name;
                }
                catch
                {
                    query = prefix + name;
                }

                if (metadata.ContainsQuery(name))
                {
                    var element = metadata.GetQuery(name);
                    if (element is BitmapMetadata)
                    {
                        DumpMetaData(query, (BitmapMetadata)element);
                    }
                    else
                    {
                        Debug.WriteLine($"{query}: {element?.ToString()}");
                    }
                }
                else
                {
                    Debug.WriteLine($"{prefix}: {name}");
                }
            }
        }

        #endregion

    }
}
