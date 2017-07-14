// Copyright (c) 2016 Mitsuhiro Ito (nee)
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
    /// <summary>
    /// Animated ViewContent
    /// </summary>
    public class AnimatedViewContent : BitmapViewContent
    {
        #region Constructors

        public AnimatedViewContent(ViewContentSource source) : base(source)
        {
        }

        #endregion
        
        #region Methods

        //
        public new void Initialize(ViewContent oldViewContent)
        {
            Debug.Assert(this.Source.GetContentType() == ViewContentType.Anime);

            // binding parameter
            var parameter = CreateBindingParameter();

            // create view
            var view = new PageContentView(LoosePath.GetFileName(this.Source.Page.FullPath));
            view.Content = CreateView(this.Source, parameter);
            this.View = view;

            // content setting
            var animatedContent = this.Content as AnimatedContent;
            this.Color = animatedContent.BitmapInfo.Color;
            this.FileProxy = animatedContent.FileProxy;
        }


        /// <summary>
        /// アニメーションビュー生成
        /// </summary>
        /// <param name="source"></param>
        /// <param name="parameter"></param>
        /// <returns></returns>
        private new FrameworkElement CreateView(ViewContentSource source, ViewContentParameters parameter)
        {
            //
            var image = base.CreateView(source, parameter);
            image.SetBinding(Rectangle.VisibilityProperty, parameter.AnimationImageVisibility);

            //
            var media = new MediaElement();
            media.Source = new Uri(((AnimatedContent)Content).FileProxy.Path);
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
            grid.Children.Add(image);
            grid.Children.Add(canvas);

            return grid;
        }

        #endregion

        #region Static Methods

        public new static AnimatedViewContent Create(ViewContentSource source, ViewContent oldViewContent)
        {
            var viewContent = new AnimatedViewContent(source);
            viewContent.Initialize(oldViewContent);
            return viewContent;
        }

        #endregion
    }
}
