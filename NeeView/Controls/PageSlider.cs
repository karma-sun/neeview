using NeeLaboratory.ComponentModel;
using System;
using System.Runtime.Serialization;
using System.ComponentModel;
using System.Diagnostics;
using NeeView.Windows.Property;

namespace NeeView
{
    // スライダーの方向
    public enum SliderDirection
    {
        [AliasName("▶ 左から右")]
        LeftToRight,

        [AliasName("◀ 右から左")]
        RightToLeft,

        [AliasName("本を開く方向に依存")]
        SyncBookReadDirection,
    }

    // スライダー数値表示の配置
    public enum SliderIndexLayout
    {
        [AliasName("表示しない")]
        None,

        [AliasName("左")]
        Left,

        [AliasName("右")]
        Right,
    }


    /// <summary>
    /// PageSlider : Model
    /// </summary>
    public class PageSlider : BindableBase
    {
        public static PageSlider Current { get; private set; }

        #region Fields

        private SliderIndexLayout _SliderIndexLayout = SliderIndexLayout.Right;
        private SliderDirection _sliderDirection = SliderDirection.SyncBookReadDirection;
        private bool _isSliderDirectionReversed;
        private bool _IsSliderLinkedThumbnailList = true;
        private int _pageNumber;
        private int _maxPageNumber;
        private BookSetting _bookSetting;
        private ThumbnailList _thumbnailList;

        #endregion

        #region Constructors

        public PageSlider(BookOperation bookOperation, BookSetting bookSetting, BookHub bookHub, ThumbnailList thumbnailList)
        {
            Current = this;

            this.PageMarkers = new PageMarkers(bookOperation);

            this.BookOperation = bookOperation;
            this.BookHub = bookHub;
            _bookSetting = bookSetting;

            _bookSetting.SettingChanged +=
                (s, e) => UpdateIsSliderDirectionReversed();

            this.BookOperation.BookChanged += BookOperation_BookChanged;
            this.BookOperation.PageChanged += BookOperation_PageChanged;

            _thumbnailList = thumbnailList;
            _thumbnailList.IsSliderDirectionReversed = this.IsSliderDirectionReversed;
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
        [PropertyMember("ページ数表示位置")]
        public SliderIndexLayout SliderIndexLayout
        {
            get { return _SliderIndexLayout; }
            set { if (_SliderIndexLayout != value) { _SliderIndexLayout = value; RaisePropertyChanged(); } }
        }

        /// <summary>
        /// スライダーの方向定義
        /// </summary>
        [PropertyMember("スライダーの方向")]
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
                    _thumbnailList.IsSliderDirectionReversed = _isSliderDirectionReversed;
                    this.PageMarkers.IsSliderDirectionReversed = _isSliderDirectionReversed;
                }
            }
        }

        /// <summary>
        /// サムネイルリストとスライダーの連動
        /// サムネイルリスト表示時に限りサムネイルリストのみに連動し表示は変化しない(マウスを離したときに決定)
        /// </summary>
        [PropertyMember("スライダーでのリアルタイム変化はサムネイルリストにのみ適用", Tips = "決定した時にページを切り替えます。")]
        public bool IsSliderLinkedThumbnailList
        {
            get { return _IsSliderLinkedThumbnailList; }
            set { if (_IsSliderLinkedThumbnailList != value) { _IsSliderLinkedThumbnailList = value; RaisePropertyChanged(); } }
        }

        /// <summary>
        /// スライダーが示すページ番号
        /// </summary>
        public int PageNumber
        {
            get { return _pageNumber; }
            set
            {
                if (_pageNumber != value)
                {
                    SetPageNumber(value);

                    // ページ切り替え命令発行
                    if (!IsThumbnailLinked())
                    {
                        this.BookOperation.RequestPageIndex(this, _pageNumber);
                    }
                }
            }
        }

        /// <summary>
        /// MaxPageNumber property.
        /// </summary>
        public int MaxPageNumber
        {
            get { return _maxPageNumber; }
            set
            {
                if (_maxPageNumber != value)
                {
                    _maxPageNumber = value;
                    RaisePropertyChanged();
                    _thumbnailList.MaxPageNumber = _maxPageNumber;
                }
            }
        }

        //
        public BookOperation BookOperation { get; private set; }

        //
        public BookHub BookHub { get; private set; }

        #endregion

        #region Methods

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
                    IsSliderDirectionReversed = _bookSetting.BookMemento.BookReadOrder == PageReadOrder.RightToLeft;
                    break;
            }
        }

        /// <summary>
        /// スライドとサムネイルリストを連動させるかを判定
        /// </summary>
        /// <returns></returns>
        private bool IsThumbnailLinked() => _thumbnailList.IsEnableThumbnailList && IsSliderLinkedThumbnailList;


        // ページ番号設定
        // プロパティはスライダーからの操作でページ切り替え命令を実行するため、純粋にスライダーの値を変化させる場合はこのメソッドを使用する
        private void SetPageNumber(int num)
        {
            _pageNumber = num;
            RaisePropertyChanged(nameof(PageNumber));
            _thumbnailList.PageNumber = num;
        }


        private void BookOperation_PageChanged(object sender, PageChangedEventArgs e)
        {
            // スライダーによる変化の場合は更新しないようにする
            if (sender == this) return;

            this.SetPageNumber(this.BookOperation.GetPageIndex());
        }

        private void BookOperation_BookChanged(object sender, EventArgs e)
        {
            if (!this.BookOperation.IsValid) return;
            this.MaxPageNumber = this.BookOperation.GetMaxPageIndex();
            this.SetPageNumber(this.BookOperation.GetPageIndex());
        }

        // ページ番号を決定し、コンテンツを切り替える
        public void Decide(bool force)
        {
            if (force || IsThumbnailLinked())
            {
                this.BookOperation.RequestPageIndex(this, this.PageNumber);
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

        //
        public Memento CreateMemento()
        {
            var memento = new Memento();
            memento.SliderIndexLayout = this.SliderIndexLayout;
            memento.SliderDirection = this.SliderDirection;
            memento.IsSliderLinkedThumbnailList = this.IsSliderLinkedThumbnailList;
            return memento;
        }

        //
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

