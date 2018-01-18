// Copyright (c) 2016-2018 Mitsuhiro Ito (nee)
//
// This software is released under the MIT License.
// http://opensource.org/licenses/mit-license.php

using NeeView.ComponentModel;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Windows.Data;

namespace NeeView
{
    /// <summary>
    /// ThumbnailList : ViewModel
    /// </summary>
    public class ThumbnailListViewModel : BindableBase
    {
        #region Fields

        private ThumbnailList _model;
        private ObservableCollection<Page> _items;

        #endregion

        #region Constructors

        /// <summary>
        /// constructor
        /// </summary>
        /// <param name="model"></param>
        public ThumbnailListViewModel(ThumbnailList model)
        {
            if (model == null) return;

            _model = model;
            _model.Refleshed += (s, e) => UpdateItems();

            _model.AddPropertyChanged(nameof(model.PageNumber), (s, e) => RaisePropertyChanged(nameof(PageNumber)));

            UpdateItems();
        }

        #endregion

        #region Properties

        /// <summary>
        /// Model property.
        /// </summary>
        public ThumbnailList Model
        {
            get { return _model; }
            set { if (_model != value) { _model = value; RaisePropertyChanged(); } }
        }

        /// <summary>
        /// Items property.
        /// </summary>
        public ObservableCollection<Page> Items
        {
            get { return _items; }
            set { if (_items != value) { _items = value; RaisePropertyChanged(); } }
        }

        /// <summary>
        /// PageNumber property.
        /// </summary>
        public int PageNumber
        {
            get { return _model.IsSliderDirectionReversed ? _model.MaxPageNumber - _model.PageNumber : _model.PageNumber; }
            set
            {
                value = _model.IsSliderDirectionReversed ? _model.MaxPageNumber - value : value;
                if (_model.MaxPageNumber != value)
                {
                    _model.MaxPageNumber = value;
                    RaisePropertyChanged();
                }
            }
        }

        //
        public int MaxPageNumber
        {
            get { return _model.MaxPageNumber; }
        }

        #endregion

        #region Methods

        //
        public void UpdateItems()
        {
            if (_model.IsSliderDirectionReversed)
            {
                // 右から左
                this.Items = _model.BookOperation.PageList != null ? new ObservableCollection<Page>(_model.BookOperation.PageList.Reverse()) : null;
            }
            else
            {
                // 左から右
                this.Items = _model.BookOperation.PageList;
            }
        }

        // サムネイル要求
        public void RequestThumbnail(int start, int count, int margin, int direction)
        {
            ////Debug.WriteLine($"> RequestThumbnail VM: {start} - {start + count - 1}");

            if (_model.IsSliderDirectionReversed)
            {
                start = _model.MaxPageNumber - (start + count - 1);
                direction = -direction;
            }
            _model.RequestThumbnail(start, count, margin, direction);
        }

        #endregion
    }
}
