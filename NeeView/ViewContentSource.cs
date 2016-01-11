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

        // コンストラクタ
        // Pageから作成
        public ViewContentSource(Page page, int index)
        {
            Source = page.Content;
            Width = page.Width;
            Height = page.Height;
            Color = page.Color;
            FullPath = page.FullPath;
            Index = index;
        }

        // コントロール作成
        public FrameworkElement CreateControl(Binding foregroundBinding)
        {
            if (Source is BitmapSource)
            {
                var image = new Image();
                image.Source = (BitmapSource)Source;
                image.Stretch = Stretch.Fill;
                RenderOptions.SetBitmapScalingMode(image, BitmapScalingMode.HighQuality);
                return image;
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
