﻿using NeeLaboratory.ComponentModel;
using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace NeeView
{
    /// <summary>
    /// PageSlider : ViewModel
    /// </summary>
    public class PageSliderViewModel : BindableBase
    {
        #region Fields

        private PageSlider _model;
        private MouseWheelDelta _mouseWheelDelta = new MouseWheelDelta();

        #endregion

        #region Constructors

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

            BookOperation.Current.BookChanged +=
                (s, e) =>
                {
                    RaisePropertyChanged(nameof(PageSliderVisibility));
                };
        }

        #endregion

        #region Properties

        public PageSlider Model
        {
            get { return _model; }
            set { if (_model != value) { _model = value; RaisePropertyChanged(); } }
        }

        public bool IsSliderWithIndex => _model != null && _model.SliderIndexLayout != SliderIndexLayout.None;

        public Dock SliderIndexDock => _model != null && _model.SliderIndexLayout == SliderIndexLayout.Left ? Dock.Left : Dock.Right;

        public Visibility PageSliderVisibility => _model != null && BookOperation.Current.GetPageCount() > 0 ? Visibility.Visible : Visibility.Hidden;

        #endregion

        #region Methods

        public void MouseWheel(object sender, MouseWheelEventArgs e)
        {
            int turn = _mouseWheelDelta.NotchCount(e);
            if (turn == 0) return;

            for (int i = 0; i < Math.Abs(turn); ++i)
            {
                if (turn < 0)
                {
                    BookOperation.Current.NextPage();
                }
                else
                {
                    BookOperation.Current.PrevPage();
                }
            }
        }

        // ページ番号を決定し、コンテンツを切り替える
        public void Jump(bool force)
        {
            _model.Jump(force);
        }

        #endregion
    }
}
