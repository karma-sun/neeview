// Copyright (c) 2016 Mitsuhiro Ito (nee)
//
// This software is released under the MIT License.
// http://opensource.org/licenses/mit-license.php

using System;
using System.ComponentModel;
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

        public BitmapViewContent(ViewContentSource source, ViewContent old) : base(source, old)
        {
        }

        #endregion

        #region Medhots

        public void Initialize()
        {
            // binding parameter
            var parameter = CreateBindingParameter();

            // create view
            this.View = CreateView(this.Source, parameter);

            // content setting
            var bitmapContent = this.Content as BitmapContent;
            this.Color = bitmapContent.Color;

            //
            bitmapContent.Picture?.AddPropertyChanged(nameof(Picture.BitmapSource), PictureBitmapSourceChanged);
        }


        private void PictureBitmapSourceChanged(object semder, PropertyChangedEventArgs arg)
        {
            var parameter = CreateBindingParameter();
            App.Current.Dispatcher.BeginInvoke((Action)(() => this.View = CreateView(this.Source, parameter)));
        }

        //
        protected FrameworkElement CreateView(ViewContentSource source, ViewContentParameters parameter)
        {
            return CreateView(source, parameter, ((BitmapContent)this.Content).BitmapSource);
        }

        //
        protected FrameworkElement CreateView(ViewContentSource source, ViewContentParameters parameter, BitmapSource bitmap)
        {
            var rectangle = new Rectangle();
            rectangle.Fill = source.CreatePageImageBrush(bitmap);
            rectangle.SetBinding(RenderOptions.BitmapScalingModeProperty, parameter.BitmapScalingMode);
            rectangle.UseLayoutRounding = true;
            rectangle.SnapsToDevicePixels = true;

            return rectangle;
        }

        //
        public override bool IsBitmapScalingModeSupported() => true;

        //
        public override Brush GetViewBrush()
        {
            return (this.View as Rectangle)?.Fill;
        }


#if false
        //
        public override bool Rebuild(double scale)
        {
            var picture = ((BitmapContent)this.Content).Picture;
            if (picture == null) return true;
            //
            var size = new Size(this.Width * scale, this.Height * scale);
            picture.RequestCreateBitmap(size);

            return true;
        }
#endif

#endregion

#region Static Methods

        public static BitmapViewContent Create(ViewContentSource source, ViewContent oldViewContent)
        {
            var viewContent = new BitmapViewContent(source, oldViewContent);
            viewContent.Initialize();
            return viewContent;
        }

#endregion
    }
}
