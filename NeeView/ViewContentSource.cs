// Copyright (c) 2016 Mitsuhiro Ito (nee)
//
// This software is released under the MIT License.
// http://opensource.org/licenses/mit-license.php

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Media;
using System.Windows.Media.Effects;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace NeeView
{
    // 表示コンテンツソース 1ページ分
    public class ViewContentSource
    {
        // ページ
        public Page Page { get; set; }

        // コンテンツソース
        public object Source { get; set; }

        // コンテンツソースサイズ
        public Size SourceSize { get; set; }

        // コンテンツの幅
        public double Width { get; set; }

        // コンテンツの高さ
        public double Height { get; set; }

        // コンテンツの色
        public Color Color { get; set; }

        // 表示名
        public string FullPath { get; set; }

        // ページの場所
        public PagePosition Position { get; set; }

        // ページサイズ
        public int PartSize { get; set; }

        // 方向
        public PageReadOrder ReadOrder { get; set; }


        // コンストラクタ
        // Pageから作成
        public ViewContentSource(Page page, PagePosition position, int size, PageReadOrder readOrder)
        {
            Page = page;
            Source = page.Content;
            SourceSize = new Size(page.Width, page.Height);
            Width = size == 2 ? page.Width : Math.Floor(page.Width * 0.5 + 0.4);
            Height = page.Height;
            Color = page.Color;
            FullPath = page.FullPath;
            Position = position;
            PartSize = size;
            ReadOrder = readOrder;
        }


        // ViewBox取得
        private Rect GetViewBox()
        {
            if (PartSize == 0) return new Rect(0, -0.00001, 0, 0.99999);
            if (PartSize == 2) return new Rect(-0.00001, -0.00001, 0.99999, 0.99999);

            bool isRightPart = Position.Part == 0;
            if (ReadOrder == PageReadOrder.LeftToRight) isRightPart = !isRightPart;

            double half = Width / SourceSize.Width;
            return isRightPart ? new Rect(0.99999 - half, -0.00001, half - 0.00001, 0.99999) : new Rect(-0.00001, -0.00001, half - 0.00001, 0.99999);
        }

        // エフェクトレンダリング用
        private static Image _RenderImage;

        //
        public static BitmapSource CreateEffectedBitmap(BitmapSource source, Size size, Effect effect)
        {
            if (effect == null) return source;

            if (_RenderImage == null) _RenderImage = new Image();

            _RenderImage.Source = source;
            _RenderImage.Width = size.Width;
            _RenderImage.Height = size.Height;
            _RenderImage.Effect = effect;
            _RenderImage.Stretch = Stretch.Fill;
            RenderOptions.SetBitmapScalingMode(_RenderImage, BitmapScalingMode.NearestNeighbor);

            var effectedBitmapSource = Utility.NVGraphics.CreateRenderBitmap(_RenderImage);

            _RenderImage.Source = null;
            _RenderImage.Effect = null;

            return effectedBitmapSource;
        }


        // コントロール作成
        public FrameworkElement CreateControl(Binding foregroundBinding, Binding bitmapScalingModeBinding, Effect effect)
        {
            if (Source is BitmapContent)
            {
                var brush = new ImageBrush();
                brush.ImageSource = CreateEffectedBitmap(((BitmapContent)Source).Source, SourceSize, effect);
                brush.AlignmentX = AlignmentX.Left;
                brush.AlignmentY = AlignmentY.Top;
                brush.Stretch = Stretch.Fill;
                brush.TileMode = TileMode.None;
                brush.Viewbox = GetViewBox();

                var rectangle = ModelContext.Recycle.RectanglePool.Allocate();
                rectangle.Fill = brush;
                rectangle.SetBinding(RenderOptions.BitmapScalingModeProperty, bitmapScalingModeBinding);
                rectangle.UseLayoutRounding = true;
                rectangle.SnapsToDevicePixels = true;

                return rectangle;
            }
            else if (Source is AnimatedGifContent)
            {
                var media = new MediaElement();
                media.Source = ((AnimatedGifContent)Source).Uri;
                media.MediaEnded += (s, e_) => media.Position = TimeSpan.FromMilliseconds(1);
                media.MediaFailed += (s, e_) => { throw new ApplicationException("MediaElementで致命的エラー", e_.ErrorException); };
                media.SetBinding(RenderOptions.BitmapScalingModeProperty, bitmapScalingModeBinding);

                var brush = new VisualBrush();
                brush.Visual = media;
                brush.Stretch = Stretch.Fill;
                brush.Viewbox = GetViewBox();

                var rectangle = new Rectangle();
                rectangle.Fill = brush;
                return rectangle;
            }
            else if (Source is FilePageContent)
            {
                var control = new FilePageControl(Source as FilePageContent);
                control.SetBinding(FilePageControl.DefaultBrushProperty, foregroundBinding);
                return control;
            }
            else if (Source is string)
            {
                var context = new FilePageContent() { Icon = FilePageIcon.File, Message = (string)Source };
                var control = new FilePageControl(context);
                control.SetBinding(FilePageControl.DefaultBrushProperty, foregroundBinding);
                return control;
            }
            else
            {
                return null;
            }
        }
    }


    // 表示コンテンツソース
    public class ViewSource
    {
        public List<ViewContentSource> Sources { get; set; }
        public int Direction { get; set; }
    }

}
