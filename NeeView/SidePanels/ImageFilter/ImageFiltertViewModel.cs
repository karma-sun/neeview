// Copyright (c) 2016 Mitsuhiro Ito (nee)
//
// This software is released under the MIT License.
// http://opensource.org/licenses/mit-license.php

using NeeView.ComponentModel;
using NeeView.Effects;
using NeeView.Windows.Input;
using NeeView.Windows.Property;
using System;
using System.Collections.Generic;
using System.ComponentModel;

namespace NeeView
{
    /// <summary>
    /// ImageEffect : ViewModel
    /// </summary>
    public class ImageFilterViewModel : BindableBase
    {
        /// <summary>
        /// Model property.
        /// </summary>
        private ImageFilter _model;
        public ImageFilter Model
        {
            get { return _model; }
            set { if (_model != value) { _model = value; RaisePropertyChanged(); } }
        }

        // PictureProfile
        public PictureProfile PictureProfile => PictureProfile.Current;

        //
        public PropertyDocument UnsharpMaskProfile { get; set; }

        //
        public ImageFilterViewModel(ImageFilter model)
        {
            _model = model;

            this.UnsharpMaskProfile = new PropertyDocument(_model.UnsharpMaskProfile);
        }


        // TODO: これモデルじゃね？
        public void ResetValue()
        {
            using (var lockerKey = ContentRebuild.Current.Locker.Lock())
            {
                _model.ResizeInterpolation = ResizeInterpolation.Lanczos;
                _model.Sharpen = true;
                this.UnsharpMaskProfile.Reset();
            }
        }
    }
}
