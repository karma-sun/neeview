using NeeLaboratory.ComponentModel;
using System.Runtime.Serialization;
using System.ComponentModel;
using NeeView.Windows.Property;
using System;

namespace NeeView
{
    // スライダーの方向
    public enum SliderDirection
    {
        [AliasName("@EnumSliderDirectionLeftToRight")]
        LeftToRight,

        [AliasName("@EnumSliderDirectionRightToLeft")]
        RightToLeft,

        [AliasName("@EnumSliderDirectionSyncBookReadDirection")]
        SyncBookReadDirection,
    }

    // スライダー数値表示の配置
    public enum SliderIndexLayout
    {
        [AliasName("@EnumSliderIndexLayoutNone")]
        None,

        [AliasName("@EnumSliderIndexLayoutLeft")]
        Left,

        [AliasName("@EnumSliderIndexLayoutRight")]
        Right,
    }


    /// <summary>
    /// PageSlider : Model
    /// </summary>
    public class PageSlider : BindableBase
    {
        static PageSlider() => Current = new PageSlider();
        public static PageSlider Current { get; }

        #region Fields

        private bool _isSliderDirectionReversed;

        #endregion

        #region Constructors

        private PageSlider()
        {
            this.PageMarkers = new PageMarkers(BookOperation.Current);

            BookSettingPresenter.Current.SettingChanged += (s, e) => UpdateIsSliderDirectionReversed();

            ThumbnailList.Current.IsSliderDirectionReversed = this.IsSliderDirectionReversed;

            PageSelector.Current.SelectionChanged += PageSelector_SelectionChanged;

            Config.Current.Slider.AddPropertyChanged(nameof(SliderConfig.SliderDirection), (s, e) =>
            {
                UpdateIsSliderDirectionReversed();
            });

            UpdateIsSliderDirectionReversed();
        }

        #endregion

        #region Properties

        /// <summary>
        /// ページマーカー表示のモデル
        /// </summary>
        public PageMarkers PageMarkers { get; private set; }

        /// <summary>
        /// 実際のスライダー方向
        /// </summary>
        public bool IsSliderDirectionReversed
        {
            get { return _isSliderDirectionReversed; }
            private set
            {
                if (_isSliderDirectionReversed != value)
                {
                    _isSliderDirectionReversed = value;
                    RaisePropertyChanged();
                    ThumbnailList.Current.IsSliderDirectionReversed = _isSliderDirectionReversed;
                    this.PageMarkers.IsSliderDirectionReversed = _isSliderDirectionReversed;
                }
            }
        }

        public PageSelector PageSelector => PageSelector.Current;

        public int SelectedIndex
        {
            get { return PageSelector.SelectedIndex; }
            set
            {
                if (!IsThumbnailLinked())
                {
                    PageSelector.SetSelectedIndex(this, value, false);
                    PageSelector.Jump(this);
                }
                else
                {
                    PageSelector.SetSelectedIndex(this, value, true);
                }
            }
        }

        #endregion

        #region Methods

        private void PageSelector_SelectionChanged(object sender, EventArgs e)
        {
            if (sender == this) return;
            RaisePropertyChanged(nameof(SelectedIndex));
        }

        // スライダー方向更新
        private void UpdateIsSliderDirectionReversed()
        {
            switch (Config.Current.Slider.SliderDirection)
            {
                default:
                case SliderDirection.LeftToRight:
                    IsSliderDirectionReversed = false;
                    break;
                case SliderDirection.RightToLeft:
                    IsSliderDirectionReversed = true;
                    break;
                case SliderDirection.SyncBookReadDirection:
                    IsSliderDirectionReversed = BookSettingPresenter.Current.LatestSetting.BookReadOrder == PageReadOrder.RightToLeft;
                    break;
            }
        }

        /// <summary>
        /// スライドとフィルムストリップを連動させるかを判定
        /// </summary>
        public bool IsThumbnailLinked() => Config.Current.FilmStrip.IsEnabled && Config.Current.Slider.IsSliderLinkedFilmStrip;


        // ページ番号を決定し、コンテンツを切り替える
        public void Jump(bool force)
        {
            if (force || IsThumbnailLinked())
            {
                PageSelector.Jump(this);
            }
        }

        #endregion

        #region Memento
        [DataContract]
        public class Memento : IMemento
        {
            [DataMember, DefaultValue(SliderIndexLayout.Right)]
            public SliderIndexLayout SliderIndexLayout { get; set; }

            [DataMember, DefaultValue(SliderDirection.SyncBookReadDirection)]
            public SliderDirection SliderDirection { get; set; }

            [DataMember, DefaultValue(true)]
            public bool IsSliderLinkedThumbnailList { get; set; }


            [OnDeserializing]
            private void OnDeserializing(StreamingContext c)
            {
                this.InitializePropertyDefaultValues();
            }

            public void RestoreConfig(Config config)
            {
                config.Slider.SliderIndexLayout = SliderIndexLayout;
                config.Slider.SliderDirection = SliderDirection;
                config.Slider.IsSliderLinkedFilmStrip = IsSliderLinkedThumbnailList;
            }
        }

        public Memento CreateMemento()
        {
            var memento = new Memento();
            memento.SliderIndexLayout = Config.Current.Slider.SliderIndexLayout;
            memento.SliderDirection = Config.Current.Slider.SliderDirection;
            memento.IsSliderLinkedThumbnailList = Config.Current.Slider.IsSliderLinkedFilmStrip;
            return memento;
        }

        #endregion
    }
}

