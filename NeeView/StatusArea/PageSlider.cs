// Copyright (c) 2016 Mitsuhiro Ito (nee)
//
// This software is released under the MIT License.
// http://opensource.org/licenses/mit-license.php

using NeeView.ComponentModel;
using System;
using System.Runtime.Serialization;
using System.ComponentModel;
using System.Diagnostics;

namespace NeeView
{
    // スライダーの方向
    public enum SliderDirection
    {
        LeftToRight, // 左から右
        RightToLeft, // 右から左
        SyncBookReadDirection, // 本を開く方向にあわせる
    }

    // スライダー数値表示の配置
    public enum SliderIndexLayout
    {
        None, // 表示なし
        Left, // 左
        Right, // 右
    }


    /// <summary>
    /// PageSlider : Model
    /// </summary>
    public class PageSlider : BindableBase
    {
        public static PageSlider Current { get; private set; }

        /// <summary>
        /// SliderIndexType property.
        /// </summary>
        public SliderIndexLayout SliderIndexLayout
        {
            get { return _SliderIndexLayout; }
            set { if (_SliderIndexLayout != value) { _SliderIndexLayout = value; RaisePropertyChanged(); } }
        }

        private SliderIndexLayout _SliderIndexLayout = SliderIndexLayout.Right;



        /// <summary>
        /// スライダーの方向
        /// </summary>
        public SliderDirection SliderDirection
        {
            get { return _sliderDirection; }
            set { if (_sliderDirection != value) { _sliderDirection = value; RaisePropertyChanged(); UpdateIsSliderDirectionReversed(); } }
        }

        private SliderDirection _sliderDirection = SliderDirection.RightToLeft;


        // スライダー方向
        private bool _isSliderDirectionReversed;
        public bool IsSliderDirectionReversed
        {
            get { return _isSliderDirectionReversed; }
            private set { if (_isSliderDirectionReversed != value) { _isSliderDirectionReversed = value; RaisePropertyChanged(); _thumbnailList.IsSliderDirectionReversed = _isSliderDirectionReversed; } }
        }

        //
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
                    IsSliderDirectionReversed = this.BookHub.BookMemento.BookReadOrder == PageReadOrder.RightToLeft;
                    break;
            }
        }


        /// <summary>
        /// サムネイルリストとスライダーの連動
        /// サムネイルリスト表示時に限りサムネイルリストのみに連動し表示は変化しない(マウスを離したときに決定)
        /// </summary>
        public bool IsSliderLinkedThumbnailList
        {
            get { return _IsSliderLinkedThumbnailList; }
            set { if (_IsSliderLinkedThumbnailList != value) { _IsSliderLinkedThumbnailList = value; RaisePropertyChanged(); } }
        }

        private bool _IsSliderLinkedThumbnailList = true;


        /// <summary>
        /// スライドとサムネイルリストを連動させるかを判定
        /// </summary>
        /// <returns></returns>
        private bool IsThumbnailLinked() => _thumbnailList.IsEnableThumbnailList && IsSliderLinkedThumbnailList;



        /// <summary>
        /// PageNumber property.
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

        private int _pageNumber;

        // ページ番号設定
        // プロパティはスライダーからの操作でページ切り替え命令を実行するため、純粋にスライダーの値を変化させる場合はこのメソッドを使用する
        private void SetPageNumber(int num)
        {
            _pageNumber = num;
            RaisePropertyChanged(nameof(PageNumber));
            _thumbnailList.PageNumber = num;
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

        private int _maxPageNumber;



        //
        public BookOperation BookOperation { get; private set; }

        //
        public BookHub BookHub { get; private set; }

        //
        private ThumbnailList _thumbnailList;

        /// <summary>
        /// constructor
        /// </summary>
        public PageSlider(BookOperation bookOperation, BookHub bookHub, ThumbnailList thumbnailList)
        {
            Current = this;

            this.BookOperation = bookOperation;
            this.BookHub = bookHub;

            this.BookHub.SettingChanged +=
                (s, e) => UpdateIsSliderDirectionReversed();

            this.BookOperation.BookChanged += BookOperation_BookChanged;
            this.BookOperation.PageChanged += BookOperation_PageChanged;

            _thumbnailList = thumbnailList;
            _thumbnailList.IsSliderDirectionReversed = this.IsSliderDirectionReversed;
        }

        private void BookOperation_PageChanged(object sender, PageChangedEventArgs e)
        {
            // スライダーによる変化の場合は更新しないようにする
            if (e.Sender == this) return;

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
                ////BookOperation.Current.SetIndex(BookOperation.Current.Index);
                this.BookOperation.RequestPageIndex(this, this.PageNumber);
            }
        }



        #region Memento
        [DataContract]
        public class Memento
        {
            [DataMember]
            public SliderIndexLayout SliderIndexLayout { get; set; }
            [DataMember]
            public SliderDirection SliderDirection { get; set; }
            [DataMember]
            public bool IsSliderLinkedThumbnailList { get; set; }
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

