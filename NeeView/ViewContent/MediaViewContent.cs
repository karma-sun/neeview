// Copyright (c) 2016-2018 Mitsuhiro Ito (nee)
//
// This software is released under the MIT License.
// http://opensource.org/licenses/mit-license.php

using System;
using System.Diagnostics;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Shapes;

namespace NeeView
{
    public class MediaViewContent : BitmapViewContent
    {
        #region Constructors

        public MediaViewContent(ViewPage source, ViewContent old) : base(source, old)
        {
        }

        #endregion

        #region Methods

        //
        public new void Initialize()
        {
            // binding parameter
            var parameter = CreateBindingParameter();

            // create view
            this.View = CreateView(this.Source, parameter);

            // content setting
            var animatedContent = this.Content as MediaContent;
            this.Color = animatedContent.Color;
            this.FileProxy = animatedContent.FileProxy;
        }


        /// <summary>
        /// アニメーションビュー生成
        /// </summary>
        /// <param name="source"></param>
        /// <param name="parameter"></param>
        /// <returns></returns>
        private new FrameworkElement CreateView(ViewPage source, ViewContentParameters parameter)
        {
            //
            var image = base.CreateView(source, parameter);
            image?.SetBinding(Rectangle.VisibilityProperty, parameter.AnimationImageVisibility);

            //
            var media = new MediaElement();
            media.Source = new Uri(((MediaContent)Content).FileProxy.Path);
            media.MediaEnded += (s, e_) => media.Position = TimeSpan.FromMilliseconds(1);
            media.MediaFailed += (s, e_) => { throw new ApplicationException("MediaElementで致命的エラー", e_.ErrorException); };
            media.SetBinding(RenderOptions.BitmapScalingModeProperty, parameter.BitmapScalingMode);

            var brush = new VisualBrush();
            brush.Visual = media;
            brush.Stretch = Stretch.Fill;
            brush.Viewbox = source.GetViewBox();

            var canvas = new Rectangle();
            canvas.Fill = brush;
            canvas.SetBinding(Rectangle.VisibilityProperty, parameter.AnimationPlayerVisibility);

            //
            var grid = new Grid();
            if (image != null) grid.Children.Add(image);
            grid.Children.Add(canvas);

            return grid;
        }

        //
        public override bool Rebuild(double scale)
        {
            return true;
        }

        #endregion

        #region Static Methods

        public new static MediaViewContent Create(ViewPage source, ViewContent oldViewContent)
        {
            var viewContent = new MediaViewContent(source, oldViewContent);
            viewContent.Initialize();
            return viewContent;
        }

        #endregion
    }
}
