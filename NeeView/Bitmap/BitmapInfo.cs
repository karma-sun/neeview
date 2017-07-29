// Copyright (c) 2016 Mitsuhiro Ito (nee)
//
// This software is released under the MIT License.
// http://opensource.org/licenses/mit-license.php

using System.Diagnostics;
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

        public int PixelWidth { get; set; }
        public int PixelHeight { get; set; }
        public bool IsMirrorHorizontal { get; set; }
        public bool IsMirrorVertical { get; set; }
        public Rotation Rotation { get; set; }
        public BitmapMetadata Metadata { get; set; }

        // 転置？
        public bool IsTranspose => (this.Rotation == Rotation.Rotate90 || this.Rotation == Rotation.Rotate270);

        #endregion

        #region Constructors

        //
        public BitmapInfo()
        {
        }

        //
        public BitmapInfo(int width, int height, BitmapMetadata metadata)
        {
            this.PixelWidth = width;
            this.PixelHeight = height;
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

        //
        public Size GetPixelSize()
        {
            return (this.PixelWidth == 0.0 || this.PixelHeight == 0.0) ? Size.Empty : new Size(this.PixelWidth, this.PixelHeight);
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
