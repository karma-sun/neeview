// Copyright (c) 2016 Mitsuhiro Ito (nee)
//
// This software is released under the MIT License.
// http://opensource.org/licenses/mit-license.php

using NeeView.ComponentModel;
using NeeView.Effects;
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
        public ImageFilterViewModel(ImageFilter model)
        {
            _model = model;
        }
    }

}
