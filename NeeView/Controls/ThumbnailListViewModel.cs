using NeeLaboratory.ComponentModel;
using System;
using System.Collections.Generic;
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
        private List<Page> _viewItems = new List<Page>();

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

            BookHub.Current.ViewContentsChanged += BookHub_ViewContentsChanged;

            UpdateItems();
        }

        #endregion

        #region Events

        public event EventHandler<ViewItemsChangedEventArgs> ViewItemsChanged; 

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


        private int _selectedIndex;
        public int SelectedIndex
        {
            get { return _selectedIndex; }
            set { if (_selectedIndex != value) { _selectedIndex = value; RaisePropertyChanged(); } }
        }

        private Page _selectedItem;
        public Page SelectedItem
        {
            get { return _selectedItem; }
            set { if (_selectedItem != value) { _selectedItem = value; RaisePropertyChanged(); } }
        }



        //
        public List<Page> ViewItems
        {
            get { return _viewItems; }
            set
            {
                if (_viewItems.SequenceEqual(value)) return;

                var removes = _viewItems.Where(e => !value.Contains(e));
                var direction = removes.Any() ? removes.First().Index < value.First().Index ? +1 : -1 : 0;

                if (!_model.IsSliderDirectionReversed)
                {
                    direction = direction * -1;
                }

                _viewItems = value;

                ViewItemsChanged?.Invoke(this, new ViewItemsChangedEventArgs(_viewItems, direction));
            }
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

        //
        private void BookHub_ViewContentsChanged(object sender, ViewPageCollectionChangedEventArgs e)
        {
            var contents = e?.ViewPageCollection?.Collection;
            if (contents == null) return;

            var mainContent = contents.Count > 0 ? (contents.First().PagePart.Position < contents.Last().PagePart.Position ? contents.First() : contents.Last()) : null;
            if (mainContent != null)
            {
                SelectedItem = mainContent.Page;
            }

            this.ViewItems = contents.Where(i => i != null).Select(i => i.Page).ToList();
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
