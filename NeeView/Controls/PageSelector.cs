using NeeLaboratory.ComponentModel;
using System;
using System.Diagnostics;
using System.Linq;

namespace NeeView
{
    public class PageSelector : BindableBase
    {
        #region Fields

        private BookOperation _bookOperation;
        private int _maxIndex;
        private int _selectedIndex;

        #endregion

        #region Constructors

        public PageSelector()
        {
            _bookOperation = BookOperation.Current;

            _bookOperation.PageListChanged += BookOperation_PageListChanged;

            // TODO: BookOperator経由のイベントにする
            BookHub.Current.ViewContentsChanged += BookHub_ViewContentsChanged;
        }

        #endregion

        #region Events

        public event EventHandler SelectionChanged;

        public event EventHandler<ViewPageCollectionChangedEventArgs> ViewContentsChanged;

        #endregion

        #region Properties

        // TODO: 更新タイミング
        public int MaxIndex
        {
            get { return _maxIndex; }
            set { SetProperty(ref _maxIndex, value); }
        }

        public int SelectedIndex
        {
            get { return _selectedIndex; }
        }

        public Page SelectedItem
        {
            get
            {
                if (!_bookOperation.IsValid || _selectedIndex < 0 || _bookOperation.Book.Pages.Count <= _selectedIndex) return null;
                return _bookOperation.Book.Pages[_selectedIndex];
            }
        }

        #endregion

        #region Methods

        private void BookOperation_PageListChanged(object sender, EventArgs e)
        {
            if (!_bookOperation.IsValid) return;
            this.MaxIndex = _bookOperation.GetMaxPageIndex();
        }

        internal void FlushSelectedIndex(object sender)
        {
            SetSelectedIndex(sender, _bookOperation.GetPageIndex(), true);
        }

        public bool SetSelectedIndex(object sender, int value, bool raiseChangedEvent)
        {
            if (SetProperty(ref _selectedIndex, value, nameof(SelectedIndex)))
            {
                ////Debug.WriteLine($"Set: {_selectedIndex}");

                if (raiseChangedEvent)
                {
                    SelectionChanged?.Invoke(sender, null);
                }

                return true;
            }
            else
            {
                return false;
            }
        }

        public void Jump(object sender)
        {
            ////Debug.WriteLine($"Jump: {_selectedIndex}");
            BookOperation.Current.RequestPageIndex(sender, _selectedIndex);
        }

        private void BookHub_ViewContentsChanged(object sender, ViewPageCollectionChangedEventArgs e)
        {
            var contents = e?.ViewPageCollection?.Collection;
            if (contents == null) return;

            var mainContent = contents.Count > 0 ? (contents.First().PagePart.Position < contents.Last().PagePart.Position ? contents.First() : contents.Last()) : null;
            if (mainContent != null)
            {
                SetSelectedIndex(sender, mainContent.Page.Index, false);
                SelectionChanged?.Invoke(sender, null);
            }

            ViewContentsChanged?.Invoke(sender, e);
        }
    }

    #endregion
}

