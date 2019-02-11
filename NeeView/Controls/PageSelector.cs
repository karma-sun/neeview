using NeeLaboratory.ComponentModel;
using System;
using System.Diagnostics;
using System.Linq;

namespace NeeView
{
    /// <summary>
    /// スライダーやフィルムストリップと連動したページ選択の提供
    /// </summary>
    public class PageSelector : BindableBase
    {
        static PageSelector() => Current = new PageSelector();
        public static PageSelector Current { get; }

        #region Fields

        private int _selectedIndex;

        #endregion

        #region Constructors

        private PageSelector()
        {
            // TODO: BookOperator経由のイベントにする
            BookHub.Current.ViewContentsChanged += BookHub_ViewContentsChanged;
        }

        #endregion

        #region Events

        public event EventHandler SelectionChanged;

        public event EventHandler<ViewPageCollectionChangedEventArgs> ViewContentsChanged;

        #endregion

        #region Properties

        public int MaxIndex => BookOperation.Current.GetMaxPageIndex();

        public int SelectedIndex
        {
            get { return _selectedIndex; }
        }

        public Page SelectedItem
        {
            get
            {
                if (!BookOperation.Current.IsValid || _selectedIndex < 0 || BookOperation.Current.Book.Pages.Count <= _selectedIndex) return null;
                return BookOperation.Current.Book.Pages[_selectedIndex];
            }
        }

        #endregion

        #region Methods

        internal void FlushSelectedIndex(object sender)
        {
            SetSelectedIndex(sender, BookOperation.Current.GetPageIndex(), true);
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

