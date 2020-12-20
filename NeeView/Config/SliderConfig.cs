using NeeLaboratory.ComponentModel;
using NeeView.Windows.Property;
using System;
using System.Text.Json.Serialization;

namespace NeeView
{
    public class SliderConfig : BindableBase
    {
        private bool _isVisible;
        private bool _isIsHidePageSlider;
        private double _sliderOpacity = 1.0;
        private SliderIndexLayout _sliderIndexLayout = SliderIndexLayout.Right;
        private SliderDirection _sliderDirection = SliderDirection.SyncBookReadDirection;
        private bool _isSliderLinkedFilmStrip = true;
        private bool _isHidePageSliderInAutoHideMode = true;
        private bool _isSyncPageMode;


        [JsonIgnore]
        [PropertyMapReadOnly]
        [PropertyMember]
        public bool IsVisible
        {
            get { return _isVisible; }
            set { SetProperty(ref _isVisible, value); }
        }

        // スライダーを自動的に隠す
        [PropertyMember]
        public bool IsHidePageSlider
        {
            get { return _isIsHidePageSlider; }
            set { SetProperty(ref _isIsHidePageSlider, value); }
        }

        /// <summary>
        /// スライダーを自動的に隠す(自動非表示モード)
        /// </summary>
        [PropertyMember]
        public bool IsHidePageSliderInAutoHideMode
        {
            get { return _isHidePageSliderInAutoHideMode; }
            set { SetProperty(ref _isHidePageSliderInAutoHideMode, value); }
        }

        // スライダー透明度
        [PropertyPercent]
        public double Opacity
        {
            get { return _sliderOpacity; }
            set { SetProperty(ref _sliderOpacity, value); }
        }

        /// <summary>
        /// ページ数表示位置
        /// </summary>
        [PropertyMember]
        public SliderIndexLayout SliderIndexLayout
        {
            get { return _sliderIndexLayout; }
            set { SetProperty(ref _sliderIndexLayout, value); }
        }

        /// <summary>
        /// スライダーの方向定義
        /// </summary>
        [PropertyMember]
        public SliderDirection SliderDirection
        {
            get { return _sliderDirection; }
            set { SetProperty(ref _sliderDirection, value); }
        }

        /// <summary>
        /// フィルムストリップとスライダーの連動
        /// フィルムストリップ表示時に限りフィルムストリップのみに連動し表示は変化しない(マウスを離したときに決定)
        /// </summary>
        [PropertyMember]
        public bool IsSliderLinkedFilmStrip
        {
            get { return _isSliderLinkedFilmStrip; }
            set { SetProperty(ref _isSliderLinkedFilmStrip, value); }
        }

        /// <summary>
        /// スライダーの移動量をページモードに従う
        /// </summary>
        [PropertyMember]
        public bool IsSyncPageMode
        {
            get { return _isSyncPageMode; }
            set { SetProperty(ref _isSyncPageMode, value); }
        }


        #region Obsolete

        [Obsolete] // ver.38
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingDefault)]
        public bool IsHidePageSliderInFullscreen
        {
            get { return default; }
            set { IsHidePageSliderInAutoHideMode = value; }
        }

        #endregion Obsoletet
    }
}