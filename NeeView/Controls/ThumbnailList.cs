// Copyright (c) 2016-2018 Mitsuhiro Ito (nee)
//
// This software is released under the MIT License.
// http://opensource.org/licenses/mit-license.php

using NeeLaboratory;
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

        #region Fields

        private bool _isEnableThumbnailList;
        private bool _isHideThumbnailList;
        private double _thumbnailSize = 96.0;
        private bool _isVisibleThumbnailNumber;
        private bool _isVisibleThumbnailPlate = true;
        private bool _isSliderDirectionReversed;
        private int _PageNumber;
        private int _MaxPageNumber;
        private bool _isSelectedCenter;

        #endregion

        #region Properties

        /// <summary>
        /// サムネイルリスト表示
        /// </summary>
        public bool IsEnableThumbnailList
        {
            get { return _isEnableThumbnailList; }
            set { if (_isEnableThumbnailList != value) { _isEnableThumbnailList = value; RaisePropertyChanged(); } }
        }

        /// <summary>
        /// サムネイルを自動的に隠す
        /// </summary>
        public bool IsHideThumbnailList
        {
            get { return _isHideThumbnailList; }
            set { if (_isHideThumbnailList != value) { _isHideThumbnailList = value; RaisePropertyChanged(); } }
        }

        /// <summary>
        /// サムネイルを隠すことができる
        /// </summary>
        public bool CanHideThumbnailList => IsEnableThumbnailList && IsHideThumbnailList;

        /// <summary>
        /// サムネイルサイズ
        /// </summary>
        [PropertyRange("ページサムネイルサイズ", 16, 256, TickFrequency = 16, Format = "{0}×{0}")]
        public double ThumbnailSize
        {
            get { return _thumbnailSize; }
            set
            {
                value = MathUtility.Clamp(value, 16, 256);
                if (_thumbnailSize != value)
                {
                    _thumbnailSize = value;
                    RaisePropertyChanged();
                }
            }
        }

        /// <summary>
        /// ページ番号の表示
        /// </summary>
        [PropertyMember("ページ番号を表示する")]
        public bool IsVisibleThumbnailNumber
        {
            get { return _isVisibleThumbnailNumber; }
            set { if (_isVisibleThumbnailNumber != value) { _isVisibleThumbnailNumber = value; RaisePropertyChanged(); RaisePropertyChanged(nameof(ThumbnailNumberVisibility)); } }
        }

        /// <summary>
        /// ページ番号の表示状態
        /// TODO: Converterで
        /// </summary>
        public Visibility ThumbnailNumberVisibility => IsVisibleThumbnailNumber ? Visibility.Visible : Visibility.Collapsed;

        /// <summary>
        /// サムネイル台紙の表示
        /// </summary>
        [PropertyMember("背景を表示する", Tips = "自動的に隠される設定の場合にサムネイルリストの背景を表示します。")]
        public bool IsVisibleThumbnailPlate
        {
            get { return _isVisibleThumbnailPlate; }
            set { if (_isVisibleThumbnailPlate != value) { _isVisibleThumbnailPlate = value; RaisePropertyChanged(); } }
        }

        /// <summary>
        /// サムネイルリスト表示状態
        /// </summary>
        public Visibility ThumbnailListVisibility => this.BookOperation.GetPageCount() > 0 ? Visibility.Visible : Visibility.Collapsed;

        /// <summary>
        /// スライダー方向
        /// スライダーと連動
        /// </summary>
        public bool IsSliderDirectionReversed
        {
            get { return _isSliderDirectionReversed; }
            set { if (_isSliderDirectionReversed != value) { _isSliderDirectionReversed = value; RaisePropertyChanged(); Refleshed?.Invoke(this, null); } }
        }


        /// <summary>
        /// ページ番号
        /// </summary>
        public int PageNumber
        {
            get { return _PageNumber; }
            set { if (_PageNumber != value) { _PageNumber = value; RaisePropertyChanged(); PageNumberChanged?.Invoke(this, null); } }
        }

        /// <summary>
        /// 最大ページ番号
        /// </summary>
        public int MaxPageNumber
        {
            get { return _MaxPageNumber; }
            set { if (_MaxPageNumber != value) { _MaxPageNumber = value; RaisePropertyChanged(); PageNumberChanged?.Invoke(this, null); } }
        }

        /// <summary>
        /// スクロールビュータッチ操作の終端挙動
        /// </summary>
        [PropertyMember("サムネイルリストタッチスクロールの終端バウンド")]
        public bool IsManipulationBoundaryFeedbackEnabled { get; set; } = true;

        /// <summary>
        /// 選択した項目が中央に表示されるようにスクロールする
        /// </summary>
        [PropertyMember("選択した項目が中央に表示されるようにスクロールする")]
        public bool IsSelectedCenter
        {
            get { return _isSelectedCenter; }
            set { if (_isSelectedCenter != value) { _isSelectedCenter = value; RaisePropertyChanged(); } }
        }

        //
        public BookOperation BookOperation { get; private set; }
        public BookHub BookHub { get; private set; }

        #endregion

        #region Constructors 

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

        #endregion

        #region Events

        /// <summary>
        /// サムネイルリストの内容が更新された
        /// </summary>
        public event EventHandler Refleshed;

        /// <summary>
        /// ページ番号が更新された
        /// </summary>
        public event EventHandler PageNumberChanged;

        #endregion

        #region Methods

        //
        public bool ToggleVisibleThumbnailList()
        {
            return IsEnableThumbnailList = !IsEnableThumbnailList;
        }

        //
        public bool ToggleHideThumbnailList()
        {
            return IsHideThumbnailList = !IsHideThumbnailList;
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

        #endregion

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
