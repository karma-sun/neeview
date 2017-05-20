// Copyright (c) 2016 Mitsuhiro Ito (nee)
//
// This software is released under the MIT License.
// http://opensource.org/licenses/mit-license.php

using NeeView.ComponentModel;
using System.Windows;
using System.Windows.Controls;

namespace NeeView
{
    /// <summary>
    /// PageSlider : ViewModel
    /// </summary>
    public class PageSliderViewModel : BindableBase
    {
        /// <summary>
        /// Model property.
        /// </summary>
        public PageSlider Model
        {
            get { return _model; }
            set { if (_model != value) { _model = value; RaisePropertyChanged(); } }
        }

        private PageSlider _model;


        /// <summary>
        /// IsSliderWithIndex property.
        /// </summary>
        public bool IsSliderWithIndex => _model != null && _model.SliderIndexLayout != SliderIndexLayout.None;

        /// <summary>
        /// SliderIndexDock property.
        /// </summary>
        public Dock SliderIndexDock => _model != null && _model.SliderIndexLayout == SliderIndexLayout.Left ? Dock.Left : Dock.Right;


        //
        public bool CanSliderLinkedThumbnailList => /*IsEnableThumbnailList &&*/ _model.IsSliderLinkedThumbnailList;


        // ページスライダー表示フラグ
        public Visibility PageSliderVisibility => _model != null && _model.BookOperation.GetPageCount() > 0 ? Visibility.Visible : Visibility.Hidden;


        /// <summary>
        /// constructor
        /// </summary>
        /// <param name="model"></param>
        public PageSliderViewModel(PageSlider model)
        {
            if (model == null) return;

            _model = model;

            _model.AddPropertyChanged(nameof(_model.SliderIndexLayout),
                (s, e) =>
                {
                    RaisePropertyChanged(nameof(IsSliderWithIndex));
                    RaisePropertyChanged(nameof(SliderIndexDock));
                });

            _model.BookHub.BookChanged +=
                (s, e) =>
                {
                    RaisePropertyChanged(nameof(PageSliderVisibility));
                };
        }
    }
}

