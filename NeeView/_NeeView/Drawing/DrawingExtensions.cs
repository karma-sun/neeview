using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media.Imaging;

namespace NeeView.Drawing
{
    public static class DrawingExtensions
    {
        #region Win32API
        [System.Runtime.InteropServices.DllImport("gdi32.dll")]
        public static extern bool DeleteObject(IntPtr hObject);
        #endregion

        /// <summary>
        /// サムネイル作成 (System.Drawing版)
        /// </summary>
        /// <param name="source"></param>
        /// <param name="maxSize"></param>
        /// <returns></returns>
        public static BitmapSource CreateThumbnail(this BitmapSource source, System.Windows.Size maxSize)
        {
            Bitmap src = source.ToBitmap();

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

            return dest.ToBitmapSource();
        }

        /// <summary>
        /// BitmapSource to Drawing.Bitmap
        /// /// </summary>
        /// <param name="source"></param>
        /// <returns></returns>
        public static Bitmap ToBitmap(this BitmapSource source)
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



        /// <summary>
        /// Drawing.Image を BitmapSource に変換
        /// </summary>
        /// <param name="image"></param>
        /// <returns></returns>
        public static BitmapSource ToBitmapSource(this System.Drawing.Image image)
        {
            return ToBitmapSource(image as System.Drawing.Bitmap);
        }

        /// <summary>
        /// Drawing.Bitmap を BitmapSource に変換
        /// </summary>
        /// <param name="bitmap"></param>
        /// <returns></returns>
        public static BitmapSource ToBitmapSource(this System.Drawing.Bitmap bitmap)
        {
            if (bitmap == null) return null;

            var hBitmap = bitmap.GetHbitmap();
            try
            {
                var bitmapSource = System.Windows.Interop.Imaging.CreateBitmapSourceFromHBitmap(hBitmap, IntPtr.Zero, Int32Rect.Empty, BitmapSizeOptions.FromEmptyOptions());
                bitmapSource.Freeze();
                return bitmapSource;
            }
            finally
            {
                DeleteObject(hBitmap);
            }
        }


        /// <summary>
        /// ストリームにJPEG保存 (System.Drawing版)
        /// </summary>
        /// <param name="image">Drawing.Image</param>
        /// <param name="stream"></param>
        /// <param name="format"></param>
        /// <param name="quality"></param>
        public static void SaveWithQuality(this Image image, Stream stream, ImageFormat format, int quality)
        {
            var encoder = ImageCodecInfo.GetImageEncoders().FirstOrDefault(e => e.FormatID == format.Guid);
            if (encoder == null) return;

            using (EncoderParameters encoderParams = new EncoderParameters(1))
            using (EncoderParameter encoderParam = new EncoderParameter(System.Drawing.Imaging.Encoder.Quality, quality))
            {
                encoderParams.Param[0] = encoderParam;
                image.Save(stream, encoder, encoderParams);
            }
        }

    }
}
