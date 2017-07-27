// Copyright (c) 2016 Mitsuhiro Ito (nee)
//
// This software is released under the MIT License.
// http://opensource.org/licenses/mit-license.php

using System;
using System.ComponentModel;
using System.Diagnostics;
using System.Threading.Tasks;
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
        protected void Resize(Size size)
        {
            var picture = ((BitmapContent)this.Content)?.Picture;
            picture?.Resize(size);
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



        // スケールされたリソースを作成中
        private volatile bool _rebuilding;

        //
        public override bool Rebuild(double scale)
        {
            if (_rebuilding) return false;

            var size = PictureProfile.Current.IsResizeFilterEnabled ? new Size(this.Width * scale, this.Height * scale) : Size.Empty;

            _rebuilding = true;

            Task.Run(() =>
            {
                try
                {
                    Resize(size);
                    App.Current.Dispatcher.Invoke((Action)(() => this.View = CreateView(this.Source, CreateBindingParameter())));
                }
                catch (Exception ex)
                {
                    Debug.WriteLine(ex.Message);
                }
                finally
                {
                    _rebuilding = false;
                }
            });

            return true;
        }

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
