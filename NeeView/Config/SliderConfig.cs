using NeeLaboratory.ComponentModel;
using NeeView.Windows.Property;

namespace NeeView
{
    public class SliderConfig : BindableBase
    {
        private double _sliderOpacity = 1.0;
        private SliderIndexLayout _sliderIndexLayout = SliderIndexLayout.Right;
        private SliderDirection _sliderDirection = SliderDirection.SyncBookReadDirection;
        private bool _isSliderLinkedFilmStrip = true;
        private bool _isHidePageSliderInFullscreen = true;

        // スライダー透明度
        [PropertyPercent("@ParamSliderOpacity", Tips = "@ParamSliderOpacityTips")]
        public double Opacity
        {
            get { return _sliderOpacity; }
            set { SetProperty(ref _sliderOpacity, value); }
        }

        /// <summary>
        /// ページ数表示位置
        /// </summary>
        [PropertyMember("@ParamSliderIndexLayout")]
        public SliderIndexLayout SliderIndexLayout
        {
            get { return _sliderIndexLayout; }
            set { SetProperty(ref _sliderIndexLayout, value); }
        }

        /// <summary>
        /// スライダーの方向定義
        /// </summary>
        [PropertyMember("@ParamSliderDirection")]
        public SliderDirection SliderDirection
        {
            get { return _sliderDirection; }
            set { SetProperty(ref _sliderDirection, value); }
        }

        /// <summary>
        /// フィルムストリップとスライダーの連動
        /// フィルムストリップ表示時に限りフィルムストリップのみに連動し表示は変化しない(マウスを離したときに決定)
        /// </summary>
        [PropertyMember("@ParamSliderIsLinkedThumbnailList", Tips = "@ParamSliderIsLinkedThumbnailListTips")]
        public bool IsSliderLinkedFilmStrip
        {
            get { return _isSliderLinkedFilmStrip; }
            set { SetProperty(ref _isSliderLinkedFilmStrip, value); }
        }

        /// <summary>
        /// フルスクリーン時にスライダーを隠す
        /// </summary>
        [PropertyMember("@ParamIsHidePageSliderInFullscreen")]
        public bool IsHidePageSliderInFullscreen
        {
            get { return _isHidePageSliderInFullscreen; }
            set { SetProperty(ref _isHidePageSliderInFullscreen, value); }
        }
    }
}