// Copyright (c) 2016 Mitsuhiro Ito (nee)
//
// This software is released under the MIT License.
// http://opensource.org/licenses/mit-license.php

using NeeView.ComponentModel;
using System.Runtime.Serialization;

namespace NeeView
{
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
            private set { if (_isSliderDirectionReversed != value) { _isSliderDirectionReversed = value; RaisePropertyChanged(); } }
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
        /// ONであればサムネイルリストと連動、OFFであればページ切り替えと連動
        /// TODO: boolはわかりにくい。連動先をenumで。
        /// </summary>
        public bool IsSliderLinkedThumbnailList
        {
            get { return _IsSliderLinkedThumbnailList; }
            set { if (_IsSliderLinkedThumbnailList != value) { _IsSliderLinkedThumbnailList = value; RaisePropertyChanged(); } }
        }

        private bool _IsSliderLinkedThumbnailList = true;





        //
        public BookOperation BookOperation { get; private set; }

        //
        public BookHub BookHub { get; private set; }


        /// <summary>
        /// constructor
        /// </summary>
        public PageSlider(BookOperation bookOperation, BookHub bookHub)
        {
            Current = this;

            this.BookOperation = bookOperation;
            this.BookHub = bookHub;

            this.BookHub.SettingChanged +=
                (s, e) => UpdateIsSliderDirectionReversed();
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

