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

namespace NeeView
{
    // 表示コンテンツ
    public class ViewContent
    {
        // コンテンツ
        public object Content { get; set; }

        // コンテンツの幅
        public double Width { get; set; }

        // コンテンツの高さ
        public double Height { get; set; }

        // コンテンツの色
        public Color Color { get; set; }


        // コンストラクタ
        // Pageから作成
        public ViewContent(Page page)
        {
            Content = page.Content;
            Width = page.Width;
            Height = page.Height;
            Color = page.Color;
        }

        // コントロール作成
        public FrameworkElement CreateControl(Binding foregroundBinding)
        {
            if (Content is BitmapSource)
            {
                var image = new Image();
                image.Source = (BitmapSource)Content;
                image.Stretch = Stretch.Fill;
                RenderOptions.SetBitmapScalingMode(image, BitmapScalingMode.HighQuality);
                return image;
            }
            else if (Content is Uri)
            {
                var media = new MediaElement();
                media.Source = (Uri)Content;
                media.MediaEnded += (s, e_) => media.Position = TimeSpan.FromMilliseconds(1);
                return media;
            }
            else if (Content is FilePageContext)
            {
                var control = new FilePageControl(Content as FilePageContext);
                control.SetBinding(FilePageControl.DefaultBrushProperty, foregroundBinding);
                return control;
            }
            else if (Content is string)
            {
                var context = new FilePageContext() { Icon = FilePageIcon.File, Message = (string)Content };
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
