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
        #region Properties

        public int PixelWidth { get; private set; }
        public int PixelHeight { get; private set; }
        public int BitsPerPixel { get; private set; }
        public bool IsMirrorHorizontal { get; private set; }
        public bool IsMirrorVertical { get; private set; }
        public Rotation Rotation { get; private set; }
        public BitmapMetadata Metadata { get; private set; }

        // 転置？
        public bool IsTranspose => (this.Rotation == Rotation.Rotate90 || this.Rotation == Rotation.Rotate270);

        #endregion

        #region Constructors

        public BitmapInfo()
        {
        }

        public BitmapInfo(int width, int height, int bitsPerPixel, BitmapMetadata metadata)
        {
            this.PixelWidth = width;
            this.PixelHeight = height;
            this.BitsPerPixel = bitsPerPixel;
            this.Metadata = metadata;

            if (metadata != null)
            {
                var exif = new ExifAccessor(metadata);

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

        #endregion

        #region Methods

        public Size GetPixelSize()
        {
            return (this.PixelWidth == 0.0 || this.PixelHeight == 0.0) ? Size.Empty : new Size(this.PixelWidth, this.PixelHeight);
        }

        #endregion

        #region Static Methods

        public static BitmapInfo Create(Stream stream)
        {
            stream.Seek(0, SeekOrigin.Begin);

            try
            {
                var bitmapFrame = BitmapFrame.Create(stream, BitmapCreateOptions.DelayCreation | BitmapCreateOptions.IgnoreColorProfile, BitmapCacheOption.None);
                return new BitmapInfo(bitmapFrame.PixelWidth, bitmapFrame.PixelHeight, bitmapFrame.Format.BitsPerPixel, (BitmapMetadata)bitmapFrame.Metadata);
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex.Message);
                return new BitmapInfo();
            }
        }

        #endregion

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
                        System.Diagnostics.Debug.WriteLine($"{query}: {element?.ToString()}");
                    }
                }
                else
                {
                    System.Diagnostics.Debug.WriteLine($"{prefix}: {name}");
                }
            }
        }

        #endregion

    }
}
