using NeeLaboratory.ComponentModel;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace NeeView
{
    public class PageList : BindableBase
    {
        static PageList() => Current = new PageList();
        public static PageList Current { get; }


        private PageSortMode _pageSortMode;
        private PageSortModeClass _pageSortModeClass = PageSortModeClass.Full;
        private Page _selectedItem;
        private List<Page> _selectedItems;
        private List<Page> _viewItems = new List<Page>();


        private PageList()
        {
            BookOperation.Current.AddPropertyChanged(nameof(BookOperation.PageList), BookOperation_PageListChanged);

            PageHistory.Current.Changed += (s, e) => PageHistoryChanged?.Invoke(s, e);
        }


        /// <summary>
        /// ページコレクションの変更通知
        /// </summary>
        public event EventHandler CollectionChanging;
        public event EventHandler CollectionChanged;

        public event EventHandler PageHistoryChanged;

        /// <summary>
        ///  表示ページの変更通知
        /// </summary>
        public event EventHandler<ViewItemsChangedEventArgs> ViewItemsChanged;


        // サムネイル画像が表示される？？
        public bool IsThumbnailVisibled
        {
            get
            {
                switch (Config.Current.PageList.PanelListItemStyle)
                {
                    default:
                        return false;
                    case PanelListItemStyle.Thumbnail:
                        return true;
                    case PanelListItemStyle.Content:
                        return Config.Current.Panels.ContentItemProfile.ImageWidth > 0.0;
                    case PanelListItemStyle.Banner:
                        return Config.Current.Panels.BannerItemProfile.ImageWidth > 0.0;
                }
            }
        }

        public PanelListItemStyle PanelListItemStyle
        {
            get => Config.Current.PageList.PanelListItemStyle;
            set => Config.Current.PageList.PanelListItemStyle = value;
        }

        /// <summary>
        /// サイドパネルでの場所表示用
        /// </summary>
        public string PlaceDispString
        {
            get { return LoosePath.GetFileName(BookOperation.Current.Address); }
        }

        /// <summary>
        /// ブックのパス
        /// </summary>
        public string Path
        {
            get { return BookOperation.Current.Address; }
        }

        /// <summary>
        /// 並び順
        /// </summary>
        public PageSortMode PageSortMode
        {
            get { return _pageSortMode; }
            set { _pageSortMode = value; BookSettingPresenter.Current.SetSortMode(value); }
        }

        public Dictionary<PageSortMode, string> PageSortModeList
        {
            get { return _pageSortModeClass.GetPageSortModeMap(); }
        }

        public PageSortModeClass PageSortModeClass
        {
            get { return _pageSortModeClass; }
            set
            {
                if (SetProperty(ref _pageSortModeClass, value))
                {
                    RaisePropertyChanged(nameof(PageSortModeList));
                }
            }
        }



        // ページリスト(表示部用)
        public ObservableCollection<Page> PageCollection => BookOperation.Current.PageList;

        public List<Page> Items
        {
            get { return PageCollection.ToList(); }
        }

        public Page SelectedItem
        {
            get { return _selectedItem; }
            set { SetProperty(ref _selectedItem, value); }
        }

        public List<Page> SelectedItems
        {
            get { return _selectedItems; }
            set { SetProperty(ref _selectedItems, value); }
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


        public void Loaded()
        {
            BookOperation.Current.ViewContentsChanged += BookOperation_ViewContentsChanged;
            RefreshSelectedItem();
        }

        public void Unloaded()
        {
            BookOperation.Current.ViewContentsChanged -= BookOperation_ViewContentsChanged;
        }

        /// <summary>
        /// ブックが変更された時の処理
        /// </summary>
        private void BookOperation_PageListChanged(object sender, PropertyChangedEventArgs e)
        {
            RefreshCollection();
        }

        /// <summary>
        /// ブックのページが切り替わったときの処理
        /// </summary>
        private void BookOperation_ViewContentsChanged(object sender, ViewContentSourceCollectionChangedEventArgs e)
        {
            RefreshSelectedItem();
        }


        /// <summary>
        /// ページコレクション更新
        /// </summary>
        private void RefreshCollection()
        {
            CollectionChanging?.Invoke(this, null);

            RaisePropertyChanged(nameof(PageCollection));
            RaisePropertyChanged(nameof(PlaceDispString));

            PageSortModeClass = BookOperation.Current.Book != null ? BookOperation.Current.Book.PageSortModeClass : PageSortModeClass.Full;
            PageSortMode = PageSortModeClass.ValidatePageSortMode(BookSettingPresenter.Current.LatestSetting.SortMode);
            RaisePropertyChanged(nameof(PageSortMode));

            RefreshSelectedItem();

            CollectionChanged?.Invoke(this, null);
        }

        /// <summary>
        /// 表示マークと選択項目をブックにあわせる
        /// </summary>
        private void RefreshSelectedItem()
        {
            var pages = BookOperation.Current.Book?.Viewer.GetViewPages();
            if (pages == null) return;

            var viewPages = pages.Where(i => i != null).OrderBy(i => i.Index).ToList();

            //this.SelectedItem = viewPages.FirstOrDefault();
            var page = viewPages.FirstOrDefault();
            if (SelectedItems == null || SelectedItems.Count <= 1 || !SelectedItems.Contains(page))
            {
                this.SelectedItem = page;
            }

            this.ViewItems = viewPages;
        }


        public void Jump(Page page)
        {
            BookOperation.Current.JumpPage(this, page);
        }

        public List<KeyValuePair<int, PageHistoryUnit>> GetHistory(int direction, int size)
        {
            return PageHistory.Current.GetHistory(direction, size);
        }

        public bool CanMoveToPrevious()
        {
            return PageHistory.Current.CanMoveToPrevious();
        }

        public void MoveToPrevious()
        {
            PageHistory.Current.MoveToPrevious();
        }

        public bool CanMoveToNext()
        {
            return PageHistory.Current.CanMoveToNext();
        }

        public void MoveToNext()
        {
            PageHistory.Current.MoveToNext();
        }

        public void MoveToHistory(KeyValuePair<int, PageHistoryUnit> item)
        {
            PageHistory.Current.MoveToHistory(item);
        }

        public bool CanMoveToParent()
        {
            return BookHub.Current.CanLoadParent();
        }

        public void MoveToParent()
        {
            BookHub.Current.RequestLoadParent(this);
        }

        #region Memento
        [DataContract]
        public class Memento : IMemento
        {
            [DataMember]
            public PanelListItemStyle PanelListItemStyle { get; set; }

            [DataMember]
            public PageNameFormat Format { get; set; }

            public void RestoreConfig(Config config)
            {
                config.PageList.PanelListItemStyle = PanelListItemStyle;
                config.PageList.Format = Format;
            }
        }

        #endregion Memento
    }
}
