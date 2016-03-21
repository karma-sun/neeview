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
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace NeeView
{
    // 表示コンテンツソース 1ページ分
    public class ViewContentSource
    {
        // コンテンツソース
        public object Source { get; set; }

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
            Source = page.Content;
            Width = size == 2 ? page.Width : page.Width * 0.5;
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
            if (PartSize == 0) new Rect(0, 0, 0, 1);
            if (PartSize == 2) return new Rect(0, 0, 1, 1);

            bool isRightPart = Position.Part == 0;
            if (ReadOrder == PageReadOrder.LeftToRight) isRightPart = !isRightPart;

            return isRightPart ? new Rect(0.5, 0, 0.5, 1) : new Rect(0, 0, 0.5, 1);
        }


        // コントロール作成
        public FrameworkElement CreateControl(Binding foregroundBinding, Binding bitmapScalingModeBinding)
        {
            if (Source is BitmapContent)
            {
                var brush = new ImageBrush();
                brush.ImageSource = ((BitmapContent)Source).Source;
                brush.Stretch = Stretch.Fill;
                brush.Viewbox = GetViewBox();

                var rectangle = new Rectangle();
                rectangle.Fill = brush;
                rectangle.SetBinding(RenderOptions.BitmapScalingModeProperty, bitmapScalingModeBinding);
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
