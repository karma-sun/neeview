using NeeLaboratory.ComponentModel;
using NeeView.Collections.Generic;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Runtime.Serialization;
using System.Threading.Tasks;

namespace NeeView
{
    public class PageListBoxModel : BindableBase
    {
        private Page _selectedItem;
        private List<Page> _viewItems;


        public PageListBoxModel()
        {
            _viewItems = new List<Page>();
        }


        public event EventHandler<ViewItemsChangedEventArgs> ViewItemsChanged;


        // ページリスト(表示部用)
        public ObservableCollection<Page> PageCollection => BookOperation.Current.PageList;

        // 一度だけフォーカスするフラグ。
        // HACK: ここに動的表示制御用フラグがあるのが違和感
        public bool FocusAtOnce { get; set; }


        public Page SelectedItem
        {
            get { return _selectedItem; }
            set { SetProperty(ref _selectedItem, value); }
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
            BookHub.Current.ViewContentsChanged += BookHub_ViewContentsChanged;
            RefreshSelectedItem();
        }

        public void Unloaded()
        {
            BookHub.Current.ViewContentsChanged -= BookHub_ViewContentsChanged;
        }

        /// <summary>
        /// ブックのページが切り替わったときの処理
        /// </summary>
        private void BookHub_ViewContentsChanged(object sender, ViewPageCollectionChangedEventArgs e)
        {
            RefreshSelectedItem();
        }

        /// <summary>
        /// 表示マークと選択項目をブックにあわせる
        /// </summary>
        private void RefreshSelectedItem()
        {
            var pages = BookOperation.Current.Book?.GetViewPages();
            if (pages == null) return;

            var viewPages = pages.Where(i => i != null).OrderBy(i => i.Index).ToList();

            this.SelectedItem = viewPages.FirstOrDefault();
            this.ViewItems = viewPages;
        }


        public void Jump(Page page)
        {
            BookOperation.Current.JumpPage(page);
        }

        public bool CanRemove(Page page)
        {
            return FileIO.Current.CanRemovePage(page) && FileIOProfile.Current.IsEnabled;
                ////|| page.Entry.Instance is TreeListNode<IPagemarkEntry>;
        }

        public async Task RemoveAsync(Page page)
        {
#if false
            if (page.Entry.Instance is TreeListNode<IPagemarkEntry> pagemarkNode)
            {
                PagemarkCollection.Current.Remove(pagemarkNode);
            }
            else
            {
#endif
                // HACK: FileIOでPage型を処理するのはおかしい
                await FileIO.Current.RemovePageAsync(page);
            ////}
        }
    }
}
