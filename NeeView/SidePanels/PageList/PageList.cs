using NeeLaboratory.ComponentModel;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace NeeView
{
    public class PageList : BindableBase
    {
        static PageList() => Current = new PageList();
        public static PageList Current { get; }


        private PageSortMode _pageSortMode;
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


        /// <summary>
        /// 配置
        /// </summary>
        public PageListPlacementService PageListPlacementService => PageListPlacementService.Current;

        /// <summary>
        /// サイドパネルでの場所表示用
        /// </summary>
        public string PlaceDispString
        {
            get { return LoosePath.GetFileName(BookOperation.Current.Address); }
        }

        /// <summary>
        /// 並び順
        /// </summary>
        public PageSortMode PageSortMode
        {
            get { return _pageSortMode; }
            set { _pageSortMode = value; BookSettingPresenter.Current.SetSortMode(value); }
        }


        // ページリスト(表示部用)
        public ObservableCollection<Page> PageCollection => BookOperation.Current.PageList;


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

            _pageSortMode = BookSettingPresenter.Current.LatestSetting.SortMode;
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
            BookOperation.Current.JumpPage(page);
        }

        public bool CanRemove(Page page)
        {
            return BookOperation.Current.CanDeleteFile(page);
        }

        public async Task RemoveAsync(Page page)
        {
            await BookOperation.Current.DeleteFileAsync(page);
        }

        public async Task RemoveAsync(List<Page> pages)
        {
            await BookOperation.Current.DeleteFileAsync(pages);
        }

        public void Copy(List<Page> pages)
        {
            ClipboardUtility.Copy(pages, new CopyFileCommandParameter() { MultiPagePolicy = MultiPagePolicy.All });
        }

        /// <summary>
        /// 履歴取得
        /// </summary>
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
            BookHub.Current.RequestLoadParent();
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

        public Memento CreateMemento()
        {
            var memento = new Memento();
            memento.PanelListItemStyle = Config.Current.PageList.PanelListItemStyle;
            memento.Format = Config.Current.PageList.Format;
            return memento;
        }

        #endregion Memento
    }
}
