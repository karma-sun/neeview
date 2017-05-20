// Copyright (c) 2016 Mitsuhiro Ito (nee)
//
// This software is released under the MIT License.
// http://opensource.org/licenses/mit-license.php

using NeeView.ComponentModel;

namespace NeeView
{
    /// <summary>
    /// ThumbnailList : ViewModel
    /// </summary>
    public class ThumbnailListViewModel : BindableBase
    {

        /// <summary>
        /// Model property.
        /// </summary>
        public ThumbnailList Model
        {
            get { return _model; }
            set { if (_model != value) { _model = value; RaisePropertyChanged(); } }
        }

        private ThumbnailList _model;

        /// <summary>
        /// constructor
        /// </summary>
        /// <param name="model"></param>
        public ThumbnailListViewModel(ThumbnailList model)
        {
            if (model == null) return;

            _model = model;
        }
    }

}
