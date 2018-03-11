using NeeLaboratory.ComponentModel;
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

        //
        public void MouseWheel(object sender, MouseWheelEventArgs e)
        {
            int turn = MouseInputHelper.DeltaCount(e);

            for (int i = 0; i < turn; ++i)
            {
                if (e.Delta < 0)
                {
                    _model.BookOperation.NextPage();
                }
                else
                {
                    _model.BookOperation.PrevPage();
                }
            }
        }

        // ページ番号を決定し、コンテンツを切り替える
        public void Decide(bool force)
        {
            _model.Decide(force);
        }
    }
}

