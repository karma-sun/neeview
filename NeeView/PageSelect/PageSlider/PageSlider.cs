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
        [AliasName]
        LeftToRight,

        [AliasName]
        RightToLeft,

        [AliasName]
        SyncBookReadDirection,
    }

    // スライダー数値表示の配置
    public enum SliderIndexLayout
    {
        [AliasName]
        None,

        [AliasName]
        Left,

        [AliasName]
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

        // NOTE: スライダーとリンク。ページモード補正を行う
        public int SelectedIndex
        {
            get { return PageSelector.SelectedIndex; }
            set
            {
                var newValue = GetFixedIndex(value);
                if (newValue != PageSelector.SelectedIndex)
                {
                    SetSelectedIndex(newValue);
                    RaisePropertyChanged(nameof(SelectedIndexRaw));
                }
            }
        }

        // NOTE: スライダーテキストボックスとリンク
        public int SelectedIndexRaw
        {
            get { return PageSelector.SelectedIndex; }
            set
            {
                if (value != PageSelector.SelectedIndex)
                {
                    SetSelectedIndex(value);
                    RaisePropertyChanged(nameof(SelectedIndex));
                }
            }
        }

        private void SetSelectedIndex(int value)
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

        private int GetFixedIndex(int value)
        {
            if (!Config.Current.Slider.IsSyncPageMode)
            {
                return value;
            }

            if (PageSelector.PageMode == PageMode.SinglePage)
            {
                return value;
            }

            // 先頭ページは常に優先
            if (value == 0)
            {
                return value;
            }

            if (value == PageSelector.SelectedIndex)
            {
                return value;
            }

            // 「終端１ページを単独表示」対応
            if (value == PageSelector.MaxIndex && PageSelector.IsSupportedSingleLastPage)
            {
                return value;
            }

            var baseIndex = PageSelector.SelectedIndex;
            if (PageSelector.ViewPageCount < 2)
            {
                baseIndex = Math.Min(PageSelector.MaxIndex, baseIndex + (value > baseIndex ? 1 : 0));
            }
            else if (Math.Abs(value - baseIndex) < 2)
            {
                return baseIndex;
            }

            var delta = value - baseIndex;
            var newDelta = delta - (delta % 2);
            var newValue = baseIndex + newDelta;
            return newValue;
        }

        #endregion

        #region Methods

        private void PageSelector_SelectionChanged(object sender, EventArgs e)
        {
            if (sender == this) return;
            RaisePropertyChanged(nameof(SelectedIndex));
            RaisePropertyChanged(nameof(SelectedIndexRaw));
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

        #endregion
    }
}

