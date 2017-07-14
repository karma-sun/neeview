// Copyright (c) 2016 Mitsuhiro Ito (nee)
//
// This software is released under the MIT License.
// http://opensource.org/licenses/mit-license.php

using System.Windows;
using System.Windows.Media;
using System.Windows.Shapes;

namespace NeeView
{
    /// <summary>
    /// Thumbnail ViewContent
    /// </summary>
    public class ThumbnailViewContent : ViewContent
    {
        #region Constructors

        public ThumbnailViewContent(ViewContentSource source) : base(source)
        {
        }

        #endregion

        #region Methods

        //
        public void Initialize(ViewContent oldViewContent)
        {
            //// Thumnbnailは変化する可能性があるのでチェックしない？
            //// TODO: 永遠と切り替わらない仮画像バグはこのあたりに原因の緒が？
            ////Debug.Assert(this.Source.GetContentType() == ViewContentType.Thumbnail);

            // binding parameter
            var parameter = CreateBindingParameter();

            // create view
            var view = new PageContentView(LoosePath.GetFileName(this.Source.Page.FullPath));
            view.Content = CreateView(this.Source, parameter);
            view.Text = LoosePath.GetFileName(this.Source.Page.FullPath);
            this.View = view;

            // content setting
            if (this.Size.Width == 0 && this.Size.Height == 0)
            {
                if (oldViewContent != null && oldViewContent.IsValid)
                {
                    this.Size = oldViewContent.Size;
                    this.Color = oldViewContent.Color;
                }
                else
                {
                    this.Size = new Size(480, 680);
                    this.Color = Colors.Black;
                }
            }
            else
            {
                var bitmapinfo = (this.Content as BitmapContent)?.BitmapInfo;
                this.Color = bitmapinfo != null ? bitmapinfo.Color : Colors.Black;
            }
        }

        /// <summary>
        /// サムネイルビュー生成
        /// </summary>
        /// <param name="source"></param>
        /// <param name="parameter"></param>
        /// <returns></returns>
        private FrameworkElement CreateView(ViewContentSource source, ViewContentParameters parameter)
        {
            var rectangle = new Rectangle();
            rectangle.Fill = source.CreateThumbnailBrush(parameter.Reserver);
            RenderOptions.SetBitmapScalingMode(rectangle, BitmapScalingMode.HighQuality);
            return rectangle;
        }

        //
        public override bool IsBitmapScalingModeSupported() => false;

        #endregion

        #region Static Methods

        public static ThumbnailViewContent Create(ViewContentSource source, ViewContent oldViewContent)
        {
            var viewContent = new ThumbnailViewContent(source);
            viewContent.Initialize(oldViewContent);
            return viewContent;
        }

        #endregion
    }
}
