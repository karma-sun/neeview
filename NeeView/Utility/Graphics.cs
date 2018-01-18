// Copyright (c) 2016-2018 Mitsuhiro Ito (nee)
//
// This software is released under the MIT License.
// http://opensource.org/licenses/mit-license.php

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace NeeView.Utility
{
    public static class NVGraphics
    {
        /// <summary>
        /// ビューコントロールをレンダリングしてBitmapにする
        /// </summary>
        /// <param name="visual">Width,Heightが設定されたコントロール</param>
        /// <returns>レンダリングされた画像</returns>
        public static BitmapSource CreateRenderBitmap(FrameworkElement visual)
        {
            visual.Measure(new Size(visual.Width, visual.Height));
            visual.Arrange(new Rect(new Size(visual.Width, visual.Height)));
            visual.UpdateLayout();

            double dpi = 96.0;
            RenderTargetBitmap bmp = new RenderTargetBitmap((int)visual.Width, (int)visual.Height, dpi, dpi, PixelFormats.Pbgra32);
            bmp.Render(visual);
            bmp.Freeze();

            return bmp;
        }


        // サムネイル作成
        public static BitmapSource CreateThumbnail(BitmapSource source, Size maxSize)
        {
            if (source == null) return null;

            double width = source.PixelWidth;
            double height = source.PixelHeight;

            // maxSize.Height が nan のときはバナー
            bool isBanner = double.IsNaN(maxSize.Height);

            var scaleX = width > maxSize.Width ? maxSize.Width / width : 1.0;
            var scaleY = height > maxSize.Height ? maxSize.Height / height : 1.0;
            var scale = scaleX > scaleY ? scaleY : scaleX;
            if (scale > 1.0) scale = 1.0;

            if (scale < 0.99)
            {
                width = (int)(width * scale + 0.5) / 2 * 2;
                height = (int)(height * scale + 0.5) / 2 * 2;
                if (width < 2.0) width = 2.0;
                if (height < 2.0) height = 2.0;
            }

            RenderTargetBitmap bmp = null;

            if (App.Current == null) return null;

            App.Current?.Dispatcher.Invoke(() =>
            {
                var canvas = new Canvas();
                canvas.Width = width;
                canvas.Height = height;

                var image = new Image();
                image.Source = source;
                image.Width = width;
                image.Height = height;
                image.Stretch = Stretch.Fill;

                double bannerHeight = (int)(width * 0.25);
                if (isBanner && bannerHeight < height) // && maxSize.Width * 0.25 < height)
                {
                    canvas.Height = bannerHeight;

                    double top = -(int)(height * 0.3 - bannerHeight * 0.5);
                    if (top < -height) top = -height;
                    if (top > 0) top = 0;
                    Canvas.SetTop(image, top);
                }

                RenderOptions.SetBitmapScalingMode(image, BitmapScalingMode.HighQuality);
                image.UseLayoutRounding = true;

                canvas.Children.Add(image);

                // ビューツリー外でも正常にレンダリングするようにする処理
                canvas.Measure(new Size(canvas.Width, canvas.Height));
                canvas.Arrange(new Rect(new Size(canvas.Width, canvas.Height)));
                canvas.UpdateLayout();

                double dpi = 96.0;
                bmp = new RenderTargetBitmap((int)canvas.Width, (int)canvas.Height, dpi, dpi, PixelFormats.Pbgra32);
                bmp.Render(canvas);
                bmp.Freeze();
            });

            return bmp;
        }



        // サムネイル作成(DrawingVisual版)
        // 完全非同期にできるが、品質が悪い
        public static BitmapSource CreateThumbnailByDrawingVisual(BitmapSource source, Size maxSize)
        {
            if (source == null) return null;

            double width = source.PixelWidth;
            double height = source.PixelHeight;

            var scaleX = width > maxSize.Width ? maxSize.Width / width : 1.0;
            var scaleY = height > maxSize.Height ? maxSize.Height / height : 1.0;
            var scale = scaleX > scaleY ? scaleY : scaleX;
            if (scale > 1.0) scale = 1.0;

            if (scale < 0.99)
            {
                width = (int)(width * scale + 0.5) / 2 * 2;
                height = (int)(height * scale + 0.5) / 2 * 2;
                if (width < 2.0) width = 2.0;
                if (height < 2.0) height = 2.0;
            }

            var visual = new DrawingVisual();
            RenderOptions.SetBitmapScalingMode(visual, BitmapScalingMode.HighQuality);

            using (var context = visual.RenderOpen())
            {
                context.DrawImage(source, new Rect(0, 0, width, height));
            }

            double dpi = 96.0;
            var bmp = new RenderTargetBitmap((int)width, (int)height, dpi, dpi, PixelFormats.Pbgra32);
            bmp.Render(visual);
            bmp.Freeze();

            return bmp;
        }



        [System.Runtime.InteropServices.DllImport("gdi32.dll")]
        public static extern bool DeleteObject(IntPtr hObject);

        // Drawing.Image を BitmapSource に変換
        public static BitmapSource ToBitmapSource(System.Drawing.Image image)
        {
            return ToBitmapSource(image as System.Drawing.Bitmap);
        }

        // Drawing.Bitmap を BitmapSource に変換
        public static BitmapSource ToBitmapSource(System.Drawing.Bitmap bitmap)
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
    }
}
