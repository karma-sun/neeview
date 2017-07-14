// Copyright (c) 2016 Mitsuhiro Ito (nee)
//
// This software is released under the MIT License.
// http://opensource.org/licenses/mit-license.php

using System.Diagnostics;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace NeeView
{
    /// <summary>
    /// Bitmap ViewContent
    /// </summary>
    public class BitmapViewContent : ViewContent
    {
        #region Constructors

        public BitmapViewContent(ViewContentSource source) : base(source)
        {
        }

        #endregion

        #region Medhots

        public void Initialize(ViewContent oldViewContent)
        {
            Debug.Assert(this.Source.GetContentType() == ViewContentType.Bitmap);

            // binding parameter
            var parameter = CreateBindingParameter();

            // create view
            var view = new PageContentView(LoosePath.GetFileName(this.Source.Page.FullPath));
            view.Content = CreateView(this.Source, parameter);
            this.View = view;

            // content setting
            var bitmapContent = this.Content as BitmapContent;
            this.Color = bitmapContent.BitmapInfo.Color;
        }

        //
        protected FrameworkElement CreateView(ViewContentSource source, ViewContentParameters parameter)
        {
            return CreateView(source, parameter, ((BitmapContent)this.Content).BitmapSource);
        }

        //
        protected FrameworkElement CreateView(ViewContentSource source, ViewContentParameters parameter, BitmapSource bitmapSource)
        {
            var rectangle = new Rectangle();
            rectangle.Fill = source.CreatePageImageBrush(bitmapSource);
            rectangle.SetBinding(RenderOptions.BitmapScalingModeProperty, parameter.BitmapScalingMode);
            rectangle.UseLayoutRounding = true;
            rectangle.SnapsToDevicePixels = true;

            return rectangle;
        }

        //
        public override bool IsBitmapScalingModeSupported() => true;

        #endregion

        #region Static Methods

        public static BitmapViewContent Create(ViewContentSource source, ViewContent oldViewContent)
        {
            var viewContent = new BitmapViewContent(source);
            viewContent.Initialize(oldViewContent);
            return viewContent;
        }

        #endregion
    }
}
