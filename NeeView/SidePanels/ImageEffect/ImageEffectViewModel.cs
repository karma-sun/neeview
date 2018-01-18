// Copyright (c) 2016-2018 Mitsuhiro Ito (nee)
//
// This software is released under the MIT License.
// http://opensource.org/licenses/mit-license.php

using NeeView.ComponentModel;
using NeeView.Effects;
using NeeView.Windows.Property;
using System.ComponentModel;

namespace NeeView
{
    /// <summary>
    /// ImageEffect : ViewModel
    /// </summary>
    public class ImageEffectViewModel : BindableBase
    {
        //
        public ImageEffectViewModel(ImageEffect model, ImageFilter imageFilter)
        {
            _model = model;
            _imageFilter = imageFilter;

            this.UnsharpMaskProfile = new PropertyDocument(_imageFilter.UnsharpMaskProfile);
            this.CustomSizeProfile = new PropertyDocument(PictureProfile.Current.CustomSize);
        }


        /// <summary>
        /// Model property.
        /// </summary>
        private ImageEffect _model;
        public ImageEffect Model
        {
            get { return _model; }
            set { if (_model != value) { _model = value; RaisePropertyChanged(); } }
        }

        /// <summary>
        /// ImageFilter property.
        /// </summary>
        private ImageFilter _imageFilter;
        public ImageFilter ImageFilter
        {
            get { return _imageFilter; }
            set { if (_imageFilter != value) { _imageFilter = value; RaisePropertyChanged(); } }
        }

        // PictureProfile
        public PictureProfile PictureProfile => PictureProfile.Current;

        // ContentCanvs
        public ContentCanvas ContentCanvas => ContentCanvas.Current;

        //
        public PropertyDocument UnsharpMaskProfile { get; set; }

        //
        public PropertyDocument CustomSizeProfile { get; set; }


        // TODO: これモデルじゃね？
        public void ResetValue()
        {
            using (var lockerKey = ContentRebuild.Current.Locker.Lock())
            {
                _imageFilter.ResizeInterpolation = ResizeInterpolation.Lanczos;
                _imageFilter.Sharpen = true;
                this.UnsharpMaskProfile.Reset();
            }
        }

    }

}
