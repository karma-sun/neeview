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

            App.Current.Dispatcher.Invoke(() =>
            {
                var image = new Image();
                image.Source = source;
                image.Width = width;
                image.Height = height;
                image.Stretch = Stretch.Fill;
                RenderOptions.SetBitmapScalingMode(image, BitmapScalingMode.HighQuality);
                image.UseLayoutRounding = true;

                // ビューツリー外でも正常にレンダリングするようにする処理
                image.Measure(new Size(image.Width, image.Height));
                image.Arrange(new Rect(new Size(image.Width, image.Height)));
                image.UpdateLayout();

                double dpi = 96.0;
                bmp = new RenderTargetBitmap((int)width, (int)height, dpi, dpi, PixelFormats.Pbgra32);
                bmp.Render(image);
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


        // ViewBox取得
        /*
        private static Rect GetViewBox()
        {
            return new Rect(0, -0.00001, 0, 0.99999);

            if (PartSize == 0) return new Rect(0, -0.00001, 0, 0.99999);
            if (PartSize == 2) return new Rect(-0.00001, -0.00001, 0.99999, 0.99999);

            bool isRightPart = Position.Part == 0;
            if (ReadOrder == PageReadOrder.LeftToRight) isRightPart = !isRightPart;

            double half = Width / SourceSize.Width;
            return isRightPart ? new Rect(0.99999 - half, -0.00001, half - 0.00001, 0.99999) : new Rect(-0.00001, -0.00001, half - 0.00001, 0.99999);
        }
        */
    }
}
