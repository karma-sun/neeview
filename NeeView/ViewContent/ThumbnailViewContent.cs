// Copyright (c) 2016-2018 Mitsuhiro Ito (nee)
//
// This software is released under the MIT License.
// http://opensource.org/licenses/mit-license.php

using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Shapes;

namespace NeeView
{
    /// <summary>
    /// Thumbnail ViewContent
    /// </summary>
    public class ThumbnailViewContent : ViewContent
    {
        #region Fields

        private Rectangle _thumbnailRectangle;

        #endregion

        #region Constructors

        public ThumbnailViewContent(ViewPage source, ViewContent old) : base(source, old)
        {
        }

        #endregion

        #region Methods

        //
        public void Initialize()
        {
            // binding parameter
            var parameter = CreateBindingParameter();

            // create view
            this.View = CreateView(this.Source, parameter);

            // content setting
            if (this.Size.Width == 0 && this.Size.Height == 0)
            {
                if (this.Reserver != null)
                {
                    this.Size = this.Reserver.Size;
                    this.Color = this.Reserver.Color;
                }
                else
                {
                    this.Size = new Size(480, 680);
                    this.Color = Colors.Black;
                }
            }
            else
            {
                this.Color = this.Content is BitmapContent bitmapContent ? bitmapContent.Color : Colors.Black;
            }
        }

        /// <summary>
        /// サムネイルビュー生成
        /// </summary>
        /// <param name="source"></param>
        /// <param name="parameter"></param>
        /// <returns></returns>
        private FrameworkElement CreateView(ViewPage source, ViewContentParameters parameter)
        {
            var grid = new Grid();

            var rectangle = new Rectangle();
            rectangle.Fill = source.CreateThumbnailBrush(this.Reserver);
            RenderOptions.SetBitmapScalingMode(rectangle, BitmapScalingMode.HighQuality);
            grid.Children.Add(rectangle);

            _thumbnailRectangle = rectangle;

            var textBlock = new TextBlock();
            textBlock.Text = LoosePath.GetFileName(source.Page.FullPath);
            textBlock.Foreground = new SolidColorBrush(Color.FromRgb(0xCC, 0xCC, 0xCC));
            textBlock.FontSize = 20;
            textBlock.Margin = new Thickness(10);
            textBlock.HorizontalAlignment = HorizontalAlignment.Center;
            textBlock.VerticalAlignment = VerticalAlignment.Center;
            
            grid.Children.Add(textBlock);

            return grid;
        }

        //
        public override bool IsBitmapScalingModeSupported() => false;

        //
        public override Brush GetViewBrush()
        {
            return _thumbnailRectangle?.Fill;
        }

        #endregion

        #region Static Methods

        public static ThumbnailViewContent Create(ViewPage source, ViewContent oldViewContent)
        {
            var viewContent = new ThumbnailViewContent(source, oldViewContent);
            viewContent.Initialize();
            return viewContent;
        }

        #endregion
    }
}
