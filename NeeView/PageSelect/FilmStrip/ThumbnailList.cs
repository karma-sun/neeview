﻿using NeeLaboratory;
using NeeLaboratory.ComponentModel;
using NeeView.Windows.Property;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Runtime.Serialization;
using System.Windows;
using System.Windows.Input;

namespace NeeView
{

    public class VisibleEventArgs : EventArgs
    {
        public VisibleEventArgs(bool isFocus)
        {
            IsFocus = isFocus;
        }

        public bool IsFocus { get; }
    }

    public interface IVisibleElement
    {
        bool IsVisible { get; }
    }


    /// <summary>
    /// ThumbnailList : Model
    /// </summary>
    public class ThumbnailList : BindableBase, IDisposable
    {
        static ThumbnailList() => Current = new ThumbnailList();
        public static ThumbnailList Current { get; }


        private bool _isSliderDirectionReversed;
        private ObservableCollection<Page> _items;
        private int _selectedIndex;
        private List<Page> _viewItems = new List<Page>();
        private PageThumbnailJobClient _jobClient;


        private ThumbnailList()
        {
            _jobClient = new PageThumbnailJobClient("FilmStrip", JobCategories.PageThumbnailCategory);

            PageSelector.Current.CollectionChanging += PageSelector_CollectionChanging;
            PageSelector.Current.CollectionChanged += PageSelector_CollectionChanged;
            PageSelector.Current.SelectionChanged += PageSelector_SelectionChanged;
            PageSelector.Current.ViewContentsChanged += PageSelector_ViewContentsChanged;

            Config.Current.FilmStrip.PropertyChanged += (s, e) =>
            {
                switch (e.PropertyName)
                {
                    case nameof(FilmStripConfig.IsEnabled):
                    case nameof(FilmStripConfig.IsHideFilmStrip):
                        RaisePropertyChanged(nameof(CanHideThumbnailList));
                        break;
                    case nameof(FilmStripConfig.IsVisibleNumber):
                        RaisePropertyChanged(nameof(ThumbnailNumberVisibility));
                        break;
                    case nameof(FilmStripConfig.ImageWidth):
                        Update();
                        break;
                }
            };

            UpdateItems();
        }


        public event EventHandler CollectionChanging;
        public event EventHandler CollectionChanged;
        public event EventHandler<ViewItemsChangedEventArgs> ViewItemsChanged;
        public event EventHandler<VisibleEventArgs> VisibleEvent;



        public IVisibleElement VisibleElement { get; set; }

        public bool IsVisible => VisibleElement?.IsVisible == true;

        public bool IsFocusAtOnce { get; set; }

        /// <summary>
        /// サムネイルを隠すことができる
        /// </summary>
        public bool CanHideThumbnailList => Config.Current.FilmStrip.IsEnabled && Config.Current.FilmStrip.IsHideFilmStrip;

        /// <summary>
        /// ページ番号の表示状態
        /// TODO: Converterで
        /// </summary>
        public Visibility ThumbnailNumberVisibility => Config.Current.FilmStrip.IsVisibleNumber ? Visibility.Visible : Visibility.Collapsed;


        /// <summary>
        /// フィルムストリップ表示状態
        /// </summary>
        public Visibility ThumbnailListVisibility => BookOperation.Current.GetPageCount() > 0 ? Visibility.Visible : Visibility.Collapsed;

        /// <summary>
        /// スライダー方向
        /// スライダーと連動
        /// </summary>
        public bool IsSliderDirectionReversed
        {
            get { return _isSliderDirectionReversed; }
            set { if (_isSliderDirectionReversed != value) { _isSliderDirectionReversed = value; RaisePropertyChanged(); UpdateItems(); } }
        }

        public PageSelector PageSelector => PageSelector.Current;

        public ObservableCollection<Page> Items
        {
            get { return _items; }
            set
            {
                if (_items != value)
                {
                    _items = value;
                    IsItemsDarty = true;
                    RaisePropertyChanged();
                    CollectionChanged?.Invoke(this, null);
                }
            }
        }

        // コレクション切り替え直後はListBoxに反映されない。
        // 反映されたらこのフラグをクリアする。
        public bool IsItemsDarty { get; set; }

        public int SelectedIndex
        {
            get
            {
                return _selectedIndex;
            }
            set
            {
                if (_selectedIndex != value)
                {
                    _selectedIndex = value;
                    if (_selectedIndex >= 0)
                    {
                        PageSelector.Current.SetSelectedIndex(this, GetIndexWithDirectionReverse(_selectedIndex), true);
                    }
                    RaisePropertyChanged();
                }
            }
        }

        public List<Page> ViewItems
        {
            get { return _viewItems; }
            set
            {
                if (_viewItems.SequenceEqual(value)) return;

                var removes = _viewItems.Where(e => !value.Contains(e));
                var direction = removes.Any() && value.Any() ? removes.First().Index < value.First().Index ? +1 : -1 : 0;

                _viewItems = value;

                ViewItemsChanged?.Invoke(this, new ViewItemsChangedEventArgs(_viewItems, direction));
            }
        }


        #region IDisposable Support
        private bool _disposedValue = false;

        protected virtual void Dispose(bool disposing)
        {
            if (!_disposedValue)
            {
                if (disposing)
                {
                    _jobClient.Dispose();
                }

                _disposedValue = true;
            }
        }

        public void Dispose()
        {
            Dispose(true);
        }
        #endregion


        private void PageSelector_CollectionChanging(object sender, EventArgs e)
        {
            // 未処理のサムネイル要求を解除
            _jobClient.CancelOrder();

            IsItemsDarty = true;
            CollectionChanging?.Invoke(sender, e);
        }

        private void PageSelector_CollectionChanged(object sender, EventArgs e)
        {
            Update();
        }

        private void Update()
        {
            UpdateItems();
            RaisePropertyChanged(nameof(ThumbnailListVisibility));
        }

        private void PageSelector_SelectionChanged(object sender, EventArgs e)
        {
            if (sender == this) return;
            UpdateSelectedIndex();
        }

        private void PageSelector_ViewContentsChanged(object sender, ViewContentsChangedEventArgs e)
        {
            var contents = e?.ViewPageCollection?.Collection;
            if (contents == null) return;

            this.ViewItems = contents.Where(i => i != null).Select(i => i.Page).OrderBy(i => i.Index).ToList();
        }

        private int GetIndexWithDirectionReverse(int value)
        {
            return Math.Max(-1, IsSliderDirectionReversed ? PageSelector.Current.MaxIndex - value : value);
        }

        private void UpdateItems()
        {
            if (IsSliderDirectionReversed)
            {
                // 右から左
                this.Items = BookOperation.Current.PageList != null ? new ObservableCollection<Page>(BookOperation.Current.PageList.Reverse()) : null;
            }
            else
            {
                // 左から右
                this.Items = BookOperation.Current.PageList;
            }
        }

        // PageSelecterの値でSelectedIndexを更新
        private void UpdateSelectedIndex()
        {
            SelectedIndex = GetIndexWithDirectionReverse(PageSelector.Current.SelectedIndex);
        }

        public void MoveSelectedIndex(int delta)
        {
            int index = SelectedIndex + delta;
            if (index < 0)
                index = 0;
            if (index >= this.Items.Count)
                index = this.Items.Count - 1;

            SelectedIndex = index;
        }

        public void FlushSelectedIndex()
        {
            PageSelector.Current.FlushSelectedIndex(this);
            UpdateSelectedIndex();
        }

        public bool SetVisibleThumbnailList(bool isVisible)
        {
            Config.Current.FilmStrip.IsEnabled = isVisible;

            if (Config.Current.FilmStrip.IsEnabled && !IsVisible)
            {
                VisibleEvent?.Invoke(this, new VisibleEventArgs(true));
            }

            return Config.Current.FilmStrip.IsEnabled;
        }

        public bool ToggleVisibleThumbnailList(bool byMenu)
        {
            bool isVisible = byMenu ? !Config.Current.FilmStrip.IsEnabled : !IsVisible;
            return SetVisibleThumbnailList(isVisible);
        }

        public bool ToggleHideThumbnailList()
        {
            return Config.Current.FilmStrip.IsHideFilmStrip = !Config.Current.FilmStrip.IsHideFilmStrip;
        }

        // サムネイル要求
        public void RequestThumbnail(int start, int count, int margin, int direction)
        {
            if (IsSliderDirectionReversed)
            {
                start = PageSelector.Current.MaxIndex - (start + count - 1);
                direction = -direction;
            }

            var pageList = BookOperation.Current.PageList;

            if (pageList == null || Config.Current.FilmStrip.ImageWidth < 8.0) return;

            // フィルムストリップが無効の場合、処理しない
            if (!Config.Current.FilmStrip.IsEnabled) return;

            // 本の切り替え中は処理しない
            if (!BookOperation.Current.IsEnabled) return;

            /////Debug.WriteLine($"> RequestThumbnail: {start} - {start + count - 1}");

            // 要求. 中央値優先
            int center = start + count / 2;
            var pages = Enumerable.Range(start - margin, count + margin * 2 - 1)
                .Where(i => i >= 0 && i < pageList.Count)
                .Select(e => pageList[e])
                .OrderBy(e => Math.Abs(e.Index - center));

            _jobClient.Order(pages.ToList());
        }

        // サムネイル要求解除
        public void CancelThumbnailRequest()
        {
            _jobClient.CancelOrder();
        }

        /// <summary>
        ///  タッチスクロール終端挙動汎用
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        public void ScrollViewer_ManipulationBoundaryFeedback(object sender, ManipulationBoundaryFeedbackEventArgs e)
        {
            if (!Config.Current.FilmStrip.IsManipulationBoundaryFeedbackEnabled)
            {
                e.Handled = true;
            }
        }

        public void FocusAtOnce()
        {
            IsFocusAtOnce = true;
        }


        #region Memento
        [DataContract]
        public class Memento : IMemento
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
            private void OnDeserializing(StreamingContext c)
            {
                this.IsManipulationBoundaryFeedbackEnabled = true;
            }

            public void RestoreConfig(Config config)
            {
                config.FilmStrip.IsEnabled = IsEnableThumbnailList;
                config.FilmStrip.IsHideFilmStrip = IsHideThumbnailList;
                config.FilmStrip.ImageWidth = ThumbnailSize;
                config.FilmStrip.IsVisibleNumber = IsVisibleThumbnailNumber;
                config.FilmStrip.IsManipulationBoundaryFeedbackEnabled = IsManipulationBoundaryFeedbackEnabled;
                config.FilmStrip.IsSelectedCenter = IsSelectedCenter;
            }
        }

        #endregion

    }
}
