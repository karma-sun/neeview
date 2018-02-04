// Copyright (c) 2016-2018 Mitsuhiro Ito (nee)
//
// This software is released under the MIT License.
// http://opensource.org/licenses/mit-license.php

using NeeLaboratory.ComponentModel;
using NeeView.Windows.Property;
using System;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Runtime.Serialization;
using System.Windows;
using System.Windows.Input;

namespace NeeView
{
    /// <summary>
    /// ThumbnailList : Model
    /// </summary>
    public class ThumbnailList : BindableBase
    {
        public static ThumbnailList Current { get; private set; }

        // サムネイル有効
        private bool _isEnableThumbnailList;
        public bool IsEnableThumbnailList
        {
            get { return _isEnableThumbnailList; }
            set { if (_isEnableThumbnailList != value) { _isEnableThumbnailList = value; RaisePropertyChanged(); } }
        }

        //
        public bool ToggleVisibleThumbnailList()
        {
            return IsEnableThumbnailList = !IsEnableThumbnailList;
        }


        // サムネイルを自動的に隠す
        private bool _isHideThumbnailList;
        public bool IsHideThumbnailList
        {
            get { return _isHideThumbnailList; }
            set { if (_isHideThumbnailList != value) { _isHideThumbnailList = value; RaisePropertyChanged(); } }
        }

        //
        public bool ToggleHideThumbnailList()
        {
            return IsHideThumbnailList = !IsHideThumbnailList;
        }


        public bool CanHideThumbnailList => IsEnableThumbnailList && IsHideThumbnailList;


        // サムネイルサイズ
        private double _thumbnailSize = 96.0;
        public double ThumbnailSize
        {
            get { return _thumbnailSize; }
            set { if (_thumbnailSize != value) { _thumbnailSize = value; RaisePropertyChanged(); } }
        }

        // ページ番号の表示
        private bool _isVisibleThumbnailNumber;
        public bool IsVisibleThumbnailNumber
        {
            get { return _isVisibleThumbnailNumber; }
            set { if (_isVisibleThumbnailNumber != value) { _isVisibleThumbnailNumber = value; RaisePropertyChanged(); RaisePropertyChanged(nameof(ThumbnailNumberVisibility)); } }
        }

        // ページ番号の表示
        // TODO: Converterで
        public Visibility ThumbnailNumberVisibility => IsVisibleThumbnailNumber ? Visibility.Visible : Visibility.Collapsed;

        // サムネイル台紙の表示
        private bool _isVisibleThumbnailPlate = true;
        public bool IsVisibleThumbnailPlate
        {
            get { return _isVisibleThumbnailPlate; }
            set { if (_isVisibleThumbnailPlate != value) { _isVisibleThumbnailPlate = value; RaisePropertyChanged(); } }
        }


        // サムネイルリスト表示状態
        public Visibility ThumbnailListVisibility => this.BookOperation.GetPageCount() > 0 ? Visibility.Visible : Visibility.Collapsed;


        /// <summary>
        /// IsSliderDirectionReversed property.
        /// </summary>
        public bool IsSliderDirectionReversed
        {
            get { return _isSliderDirectionReversed; }
            set { if (_isSliderDirectionReversed != value) { _isSliderDirectionReversed = value; RaisePropertyChanged(); Refleshed?.Invoke(this, null); } }
        }

        private bool _isSliderDirectionReversed;


        //
        public event EventHandler PageNumberChanged;

        /// <summary>
        /// PageNumber property.
        /// </summary>
        public int PageNumber
        {
            get { return _PageNumber; }
            set { if (_PageNumber != value) { _PageNumber = value; RaisePropertyChanged(); PageNumberChanged?.Invoke(this, null); } }
        }

        private int _PageNumber;


        /// <summary>
        /// MaxPageNumber property.
        /// </summary>
        public int MaxPageNumber
        {
            get { return _MaxPageNumber; }
            set { if (_MaxPageNumber != value) { _MaxPageNumber = value; RaisePropertyChanged(); PageNumberChanged?.Invoke(this, null); } }
        }

        private int _MaxPageNumber;

        // スクロールビュータッチ操作の終端挙動
        public bool IsManipulationBoundaryFeedbackEnabled { get; set; } = true;

        /// <summary>
        /// IsSelectedCenter property.
        /// </summary>
        private bool _isSelectedCenter;
        public bool IsSelectedCenter
        {
            get { return _isSelectedCenter; }
            set { if (_isSelectedCenter != value) { _isSelectedCenter = value; RaisePropertyChanged(); } }
        }


        public BookOperation BookOperation { get; private set; }
        public BookHub BookHub { get; private set; }

        public event EventHandler Refleshed;

        /// <summary>
        /// constructor
        /// </summary>
        /// <param name="pageSlider"></param>
        /// <param name="bookHub"></param>
        public ThumbnailList(BookOperation bookOperation, BookHub bookHub)
        {
            Current = this;

            this.BookOperation = bookOperation;
            this.BookHub = bookHub;

            this.BookHub.BookChanging +=
                OnBookChanging;

            this.BookHub.BookChanged +=
                OnBookChanged;

            this.BookOperation.PagesSorted +=
                OnPageListChanged;
        }

        // 本が変更される
        private void OnBookChanging(object sender, EventArgs e)
        {
            // 未処理のサムネイル要求を解除
            JobEngine.Current.Clear(QueueElementPriority.PageThumbnail);
        }

        // 本が変更された
        private void OnBookChanged(object sender, BookMementoType bookmarkType)
        {
            RaisePropertyChanged(nameof(ThumbnailListVisibility));
            Refleshed?.Invoke(this, null);
        }

        // ページの並び順が変更された
        private void OnPageListChanged(object sender, EventArgs e)
        {
            JobEngine.Current.Clear(QueueElementPriority.PageThumbnail);

            App.Current.Dispatcher.BeginInvoke((Action)(() =>
                {
                    RaisePropertyChanged(nameof(ThumbnailListVisibility));
                    Refleshed?.Invoke(this, null);
                }));
        }


        // サムネイル要求
        public void RequestThumbnail(int start, int count, int margin, int direction)
        {
            var pageList = BookOperation.Current.PageList;

            if (pageList == null || ThumbnailSize < 8.0) return;

            // サムネイルリストが無効の場合、処理しない
            if (!IsEnableThumbnailList) return;

            // 本の切り替え中は処理しない
            if (!this.BookOperation.IsEnabled) return;

            //Debug.WriteLine($"> RequestThumbnail: {start} - {start + count - 1}");

            // 未処理の要求を解除
            JobEngine.Current.Clear(QueueElementPriority.PageThumbnail);

            // 要求. 中央値優先
            int center = start + count / 2;
            var pages = Enumerable.Range(start - margin, count + margin * 2 - 1)
                .Where(i => i >= 0 && i < pageList.Count)
                .Select(e => pageList[e])
                .OrderBy(e => Math.Abs(e.Index - center));

            foreach (var page in pages)
            {
                page.LoadThumbnail(QueueElementPriority.PageThumbnail);
            }
        }

        /// <summary>
        ///  タッチスクロール終端挙動汎用
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        public void ScrollViewer_ManipulationBoundaryFeedback(object sender, ManipulationBoundaryFeedbackEventArgs e)
        {
            if (!this.IsManipulationBoundaryFeedbackEnabled)
            {
                e.Handled = true;
            }
        }


        #region Memento
        [DataContract]
        public class Memento
        {
            [DataMember]
            public bool IsEnableThumbnailList { get; set; }
            [DataMember]
            public bool IsHideThumbnailList { get; set; }
            [DataMember]
            public double ThumbnailSize { get; set; }
            [DataMember]
            public bool IsVisibleThumbnailNumber { get; set; }
            [DataMember]
            public bool IsVisibleThumbnailPlate { get; set; }
            [DataMember, DefaultValue(true)]
            [PropertyMember("サムネイルリストタッチスクロールの終端バウンド", Tips = "サムネイルリストのタッチスクロール操作での終端跳ね返り挙動の有効/無効を設定します")]
            public bool IsManipulationBoundaryFeedbackEnabled { get; set; }
            [DataMember]
            public bool IsSelectedCenter { get; set; }

            [OnDeserializing]
            private void Deserializing(StreamingContext c)
            {
                this.IsManipulationBoundaryFeedbackEnabled = true;
            }
        }

        //
        public Memento CreateMemento()
        {
            var memento = new Memento();
            memento.IsEnableThumbnailList = this.IsEnableThumbnailList;
            memento.IsHideThumbnailList = this.IsHideThumbnailList;
            memento.ThumbnailSize = this.ThumbnailSize;
            memento.IsVisibleThumbnailNumber = this.IsVisibleThumbnailNumber;
            memento.IsVisibleThumbnailPlate = this.IsVisibleThumbnailPlate;
            memento.IsManipulationBoundaryFeedbackEnabled = this.IsManipulationBoundaryFeedbackEnabled;
            memento.IsSelectedCenter = this.IsSelectedCenter;
            return memento;
        }

        //
        public void Restore(Memento memento)
        {
            if (memento == null) return;
            this.IsEnableThumbnailList = memento.IsEnableThumbnailList;
            this.IsHideThumbnailList = memento.IsHideThumbnailList;
            this.ThumbnailSize = memento.ThumbnailSize;
            this.IsVisibleThumbnailNumber = memento.IsVisibleThumbnailNumber;
            this.IsVisibleThumbnailPlate = memento.IsVisibleThumbnailPlate;
            this.IsManipulationBoundaryFeedbackEnabled = memento.IsManipulationBoundaryFeedbackEnabled;
            this.IsSelectedCenter = memento.IsSelectedCenter;
        }
        #endregion

    }

}
