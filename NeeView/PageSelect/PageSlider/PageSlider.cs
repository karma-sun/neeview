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

        private SliderIndexLayout _SliderIndexLayout = SliderIndexLayout.Right;
        private SliderDirection _sliderDirection = SliderDirection.SyncBookReadDirection;
        private bool _isSliderDirectionReversed;
        private bool _IsSliderLinkedThumbnailList = true;

        #endregion

        #region Constructors

        private PageSlider()
        {
            this.PageMarkers = new PageMarkers(BookOperation.Current);

            BookSettingPresenter.Current.SettingChanged += (s, e) => UpdateIsSliderDirectionReversed();

            ThumbnailList.Current.IsSliderDirectionReversed = this.IsSliderDirectionReversed;

            PageSelector.Current.SelectionChanged += PageSelector_SelectionChanged;
        }

        #endregion

        #region Properties

        /// <summary>
        /// ページマーカー表示のモデル
        /// </summary>
        public PageMarkers PageMarkers { get; private set; }

        /// <summary>
        /// ページ数表示位置
        /// </summary>
        [PropertyMember("@ParamSliderIndexLayout")]
        public SliderIndexLayout SliderIndexLayout
        {
            get { return _SliderIndexLayout; }
            set { if (_SliderIndexLayout != value) { _SliderIndexLayout = value; RaisePropertyChanged(); } }
        }

        /// <summary>
        /// スライダーの方向定義
        /// </summary>
        [PropertyMember("@ParamSliderDirection")]
        public SliderDirection SliderDirection
        {
            get { return _sliderDirection; }
            set { if (_sliderDirection != value) { _sliderDirection = value; RaisePropertyChanged(); UpdateIsSliderDirectionReversed(); } }
        }

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

        /// <summary>
        /// フィルムストリップとスライダーの連動
        /// フィルムストリップ表示時に限りフィルムストリップのみに連動し表示は変化しない(マウスを離したときに決定)
        /// </summary>
        [PropertyMember("@ParamSliderIsLinkedThumbnailList", Tips = "@ParamSliderIsLinkedThumbnailListTips")]
        public bool IsSliderLinkedThumbnailList
        {
            get { return _IsSliderLinkedThumbnailList; }
            set { if (_IsSliderLinkedThumbnailList != value) { _IsSliderLinkedThumbnailList = value; RaisePropertyChanged(); } }
        }

        //
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
            switch (this.SliderDirection)
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
        public bool IsThumbnailLinked() => ThumbnailList.Current.IsEnableThumbnailList && IsSliderLinkedThumbnailList;


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
        public class Memento
        {
            [DataMember, DefaultValue(SliderIndexLayout.Right)]
            public SliderIndexLayout SliderIndexLayout { get; set; }

            [DataMember, DefaultValue(SliderDirection.SyncBookReadDirection)]
            public SliderDirection SliderDirection { get; set; }

            [DataMember, DefaultValue(true)]
            public bool IsSliderLinkedThumbnailList { get; set; }


            [OnDeserializing]
            private void Deserializing(StreamingContext c)
            {
                this.InitializePropertyDefaultValues();
            }
        }

        public Memento CreateMemento()
        {
            var memento = new Memento();
            memento.SliderIndexLayout = this.SliderIndexLayout;
            memento.SliderDirection = this.SliderDirection;
            memento.IsSliderLinkedThumbnailList = this.IsSliderLinkedThumbnailList;
            return memento;
        }

        public void Restore(Memento memento)
        {
            if (memento == null) return;
            this.SliderIndexLayout = memento.SliderIndexLayout;
            this.SliderDirection = memento.SliderDirection;
            this.IsSliderLinkedThumbnailList = memento.IsSliderLinkedThumbnailList;
        }

        #endregion
    }
}

