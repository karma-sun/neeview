// Copyright (c) 2016 Mitsuhiro Ito (nee)
//
// This software is released under the MIT License.
// http://opensource.org/licenses/mit-license.php

using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media.Imaging;

namespace NeeView.Utility
{
    class NVDrawing
    {
        // サムネイル作成(System.Drawing)
        public static BitmapSource CreateThumbnail(BitmapSource source, System.Windows.Size maxSize)
        {
            Bitmap src = GetBitmap(source);

            //int w = src.Width * 10;
            //int h = src.Height * 10;

            double srcWidth = src.Width;
            double srcHeight = src.Height;
            var scaleX = srcWidth > maxSize.Width ? maxSize.Width / srcWidth : 1.0;
            var scaleY = srcHeight > maxSize.Height ? maxSize.Height / srcHeight : 1.0;
            var scale = scaleX > scaleY ? scaleY : scaleX;
            if (scale > 1.0) scale = 1.0;

            int destWidth = (int)(srcWidth * scale + 0.5) / 2 * 2;
            int destHeight = (int)(srcHeight * scale + 0.5) / 2 * 2;
            if (destWidth < 2) destWidth = 2;
            if (destHeight < 2) destHeight = 2;


            Bitmap dest = new Bitmap(destWidth, destHeight);

            Graphics g = Graphics.FromImage(dest);
            g.InterpolationMode = InterpolationMode.High;
            g.DrawImage(src, 0, 0, destWidth, destHeight);

            return GetBitmapSource(dest);

            /*
            foreach (InterpolationMode im in Enum.GetValues(typeof(InterpolationMode)))
            {
                if (im == InterpolationMode.Invalid)
                    continue;
                g.InterpolationMode = im;
                g.DrawImage(src, 0, 0, w, h);
                dest.Save(im.ToString() + ".png", ImageFormat.Png);
            }
            */
        }


        public static Bitmap GetBitmap(BitmapSource source)
        {
            Bitmap bmp = new Bitmap
            (
              source.PixelWidth,
              source.PixelHeight,
              System.Drawing.Imaging.PixelFormat.Format32bppPArgb
            );

            BitmapData data = bmp.LockBits
            (
                new System.Drawing.Rectangle(System.Drawing.Point.Empty, bmp.Size),
                ImageLockMode.WriteOnly,
                System.Drawing.Imaging.PixelFormat.Format32bppPArgb
            );

            source.CopyPixels
            (
              Int32Rect.Empty,
              data.Scan0,
              data.Height * data.Stride,
              data.Stride
            );

            bmp.UnlockBits(data);

            return bmp;
        }


        public static BitmapSource GetBitmapSource(Bitmap bitmap)
        {
            BitmapSource bitmapSource = System.Windows.Interop.Imaging.CreateBitmapSourceFromHBitmap
            (
                bitmap.GetHbitmap(),
                IntPtr.Zero,
                Int32Rect.Empty,
                BitmapSizeOptions.FromEmptyOptions()
            );

            bitmapSource.Freeze();

            return bitmapSource;
        }
    }
}
