// Copyright (c) 2016 Mitsuhiro Ito (nee)
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
using System.Windows.Data;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace NeeView
{
    // ページパーツ定義
    [Flags]
    public enum PagePart
    {
        None = 0,
        Right = (1 << 0),
        Left = (1 << 1),
        All = Right | Left,
    }

    // 表示コンテンツソース
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

        // ページ番号
        public int Index { get; set; }

        // ページパーツ
        public PagePart Part { get; set; }

        // ページパーツのViewBoxテーブル
        private static Dictionary<PagePart, Rect> _ViewBox = new Dictionary<PagePart, Rect>
        {
            [PagePart.None] = new Rect(0, 0, 0, 1),
            [PagePart.Right] = new Rect(0.5, 0, 0.5, 1),
            [PagePart.Left] = new Rect(0, 0, 0.5, 1),
            [PagePart.All] = new Rect(0, 0, 1, 1),
        };

        // コンストラクタ
        // Pageから作成
        public ViewContentSource(Page page, PageValue index, int size)
        {
            //double half = Math.Floor(page.Width * 0.5 + 0.5);

            Source = page.Content;
            Width = size == 2 ? page.Width : page.Width * 0.5;
            Height = page.Height;
            Color = page.Color;
            FullPath = page.FullPath;
            Index = index.Index;
            Part = size == 2 ? PagePart.All : index.Part == 0 ? PagePart.Right : PagePart.Left;
        }

        // コントロール作成
        public FrameworkElement CreateControl(Binding foregroundBinding)
        {
            if (Source is BitmapSource)
            {
                var brush = new ImageBrush();
                brush.ImageSource = (BitmapSource)Source;
                brush.Stretch = Stretch.Fill;
                brush.Viewbox = _ViewBox[Part];
                var rectangle = new Rectangle();
                rectangle.Fill = brush;
                RenderOptions.SetBitmapScalingMode(rectangle, BitmapScalingMode.HighQuality);
                return rectangle;
#if false
                var image = new Image();
                image.Source = (BitmapSource)Source;
                image.Stretch = Stretch.Fill;
                RenderOptions.SetBitmapScalingMode(image, BitmapScalingMode.HighQuality);
                return image;
#endif
            }
            else if (Source is Uri)
            {
                var media = new MediaElement();
                media.Source = (Uri)Source;
                media.MediaEnded += (s, e_) => media.Position = TimeSpan.FromMilliseconds(1);
                return media;
            }
            else if (Source is FilePageContext)
            {
                var control = new FilePageControl(Source as FilePageContext);
                control.SetBinding(FilePageControl.DefaultBrushProperty, foregroundBinding);
                return control;
            }
            else if (Source is string)
            {
                var context = new FilePageContext() { Icon = FilePageIcon.File, Message = (string)Source };
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

}
