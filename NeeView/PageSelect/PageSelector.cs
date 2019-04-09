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
            BookOperation.Current.PageListChanged += BookOperation_PageListChanged;
            BookOperation.Current.ViewContentsChanged += BookOperation_ViewContentsChanged;
        }

        #endregion

        #region Events

        public event EventHandler SelectionChanged;

        public event EventHandler<ViewContentsChangedEventArgs> ViewContentsChanged;

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
                ////Debug.WriteLine($"> PageSelector.SelectedIndex={_selectedIndex}");

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

        private void BookOperation_PageListChanged(object sender, EventArgs e)
        {
            RaisePropertyChanged(nameof(MaxIndex));
            RaiseViewContentsChanged(sender, BookOperation.Current.Book?.Viewer.ViewPageCollection, true);
        }

        private void BookOperation_ViewContentsChanged(object sender, ViewContentSourceCollectionChangedEventArgs e)
        {
            RaiseViewContentsChanged(sender, e?.ViewPageCollection, false);
        }

        private void RaiseViewContentsChanged(object sender, ViewContentSourceCollection viewPageCollection, bool isBookOpen)
        { 
            var contents = viewPageCollection?.Collection;
            if (contents == null) return;

            ViewContentsChanged?.Invoke(sender, new ViewContentsChangedEventArgs(viewPageCollection, isBookOpen));

            var mainContent = contents.Count > 0 ? (contents.First().PagePart.Position < contents.Last().PagePart.Position ? contents.First() : contents.Last()) : null;
            if (mainContent != null)
            {
                SetSelectedIndex(sender, mainContent.Page.Index, false);
                SelectionChanged?.Invoke(sender, null);
            }
        }

        #endregion
    }


    // 表示コンテンツ変更イベント
    public class ViewContentsChangedEventArgs : EventArgs
    {
        public ViewContentsChangedEventArgs(ViewContentSourceCollection viewPageCollection, bool isBookOpen)
        {
            ViewPageCollection = viewPageCollection;
            IsBookOpen = isBookOpen;
        }

        /// <summary>
        /// 表示コンテンツ
        /// </summary>
        public ViewContentSourceCollection ViewPageCollection { get; private set; }

        /// <summary>
        /// 本を新しく開いたとき
        /// </summary>
        public bool IsBookOpen { get; private set; }
    }
}

