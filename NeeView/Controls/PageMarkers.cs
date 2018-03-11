using NeeLaboratory.ComponentModel;
using System.Collections.Generic;
using System.Linq;

namespace NeeView
{
    /// <summary>
    /// マーカー群表示用コレクション
    /// </summary>
    public class PageMarkerCollection
    {
        public List<int> Indexes { get; set; }
        public int Maximum { get; set; }
    }

    /// <summary>
    /// Pagemarkers : Model
    /// </summary>
    public class PageMarkers : BindableBase
    {
        //
        public PageMarkers(BookOperation bookOperation)
        {
            _bookOperation = bookOperation;

            _bookOperation.BookChanged +=
                (s, e) => Update();
            _bookOperation.PagesSorted +=
                (s, e) => Update();
            _bookOperation.PageRemoved +=
                (s, e) => Update();
            _bookOperation.PagemarkChanged +=
                (s, e) => Update();
        }

        //
        private BookOperation _bookOperation;


        /// <summary>
        /// MarkerCollection property.
        /// </summary>
        public PageMarkerCollection MarkerCollection
        {
            get { return _MarkerCollection; }
            set { if (_MarkerCollection != value) { _MarkerCollection = value; RaisePropertyChanged(); } }
        }

        private PageMarkerCollection _MarkerCollection;


        /// <summary>
        /// スライダー方向
        /// </summary>
        public bool IsSliderDirectionReversed
        {
            get { return _isSliderDirectionReversed; }
            set { if (_isSliderDirectionReversed != value) { _isSliderDirectionReversed = value; RaisePropertyChanged(); } }
        }

        private bool _isSliderDirectionReversed;


        /// <summary>
        /// マーカー更新
        /// </summary>
        private void Update()
        {
            var book = BookOperation.Current.Book;
            if (book != null && book.Markers.Any())
            {
                this.MarkerCollection = new PageMarkerCollection()
                {
                    Indexes = book.Markers.Select(e => e.Index).ToList(),
                    Maximum = book.Pages.Count - 1
                };
            }
            else
            { 
                this.MarkerCollection = null;
            }
        }

    }
}
