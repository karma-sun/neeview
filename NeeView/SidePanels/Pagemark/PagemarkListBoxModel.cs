using NeeLaboratory.ComponentModel;
using NeeView.Collections.Generic;
using NeeView.Windows;
using System;
using System.Collections.ObjectModel;
using System.Linq;

namespace NeeView
{
    public class PagemarkListBoxModel : BindableBase
    {
        // Fields

        private ObservableCollection<TreeListNode<IPagemarkEntry>> _items;
        private TreeListNode<IPagemarkEntry> _selectedItem;


        // Constructors

        public PagemarkListBoxModel()
        {
            PagemarkCollection.Current.PagemarkChanged += PagemarkCollection_PagemarkChanged;
        }


        // Events

        public event EventHandler Changing;
        public event EventHandler Changed;

        public ObservableCollection<TreeListNode<IPagemarkEntry>> Items
        {
            get { return _items; }
            set { SetProperty(ref _items, value); }
        }

        public TreeListNode<IPagemarkEntry> SelectedItem
        {
            get { return _selectedItem; }
            set { SetProperty(ref _selectedItem, value); }
        }

        // TODO: この参照方向はどうなの？
        public bool IsThumbnailVisibled => PagemarkList.Current.IsThumbnailVisibled;
        public PanelListItemStyle PanelListItemStyle => PagemarkList.Current.PanelListItemStyle;


        // Methods

        private void PagemarkCollection_PagemarkChanged(object sender, PagemarkCollectionChangedEventArgs e)
        {
            Refresh();
        }

        public void Decide(TreeListNode<IPagemarkEntry> item)
        {
            switch (item.Value)
            {
                case Pagemark pagemark:

                    bool isJumped = BookOperation.Current.JumpPagemarkInPlace(pagemark);
                    if (!isJumped)
                    {
                        var options = pagemark.EntryName != null ? BookLoadOption.IsPage : BookLoadOption.None;
                        BookHub.Current.RequestLoad(pagemark.Place, pagemark.EntryName, options, true);
                    }
                    break;
                case PagemarkFolder folder:
                    if (item.Children.Count > 0)
                    {
                        item.IsExpanded = !item.IsExpanded;
                        Refresh();
                    }
                    break;
            }
        }

        private void Refresh()
        {
            Changing?.Invoke(this, null);
            var collection = PagemarkCollection.Current.Items.GetExpandedCollection();
            Items = new ObservableCollection<TreeListNode<IPagemarkEntry>>(collection);
            Changed?.Invoke(this, null);
        }

        public void Add(string place, string entryName)
        {
            if (place == null) throw new ArgumentNullException(nameof(place));

            if (PagemarkCollection.Current.Contains(place, entryName))
            {
                return;
            }

            if (place.StartsWith(Temporary.TempDirectory))
            {
                // TODO: テンポラリは登録できない通知
                return;
            }

            // TODO: place指定でのunit取得をもっとスマートにできそう
            var unit = BookMementoCollection.Current.GetValid(place) ?? BookMementoCollection.Current.Set(CreateBookMemento(place));
            PagemarkCollection.Current.AddFirst(new Pagemark(unit, entryName));
        }

        // 指定したブックの設定作成
        // TODO: ここではない。 BookHubか？
        private Book.Memento CreateBookMemento(string place)
        {
            if (place == null) throw new ArgumentNullException();

            var memento = BookHub.Current.CreateBookMemento();
            if (memento == null || memento.Place != place)
            {
                memento = BookSetting.Current.BookMementoDefault.Clone();
                memento.Place = place;
            }
            return memento;
        }

        public bool Remove(TreeListNode<IPagemarkEntry> item)
        {
            int selectedIndex = Items.IndexOf(SelectedItem);

            if (item.Value is PagemarkFolder)
            {
                var count = item.Count(e => e.Value is Pagemark);
                if (count > 0)
                {
                    var dialog = new MessageDialog(string.Format(Properties.Resources.DialogPagemarkFolderDelete, count), string.Format(Properties.Resources.DialogPagemarkFolderDeleteTitle, item.Value.Name));
                    dialog.Commands.Add(UICommands.Delete);
                    dialog.Commands.Add(UICommands.Cancel);
                    var answer = dialog.ShowDialog();
                    if (answer != UICommands.Delete)
                    {
                        return false;
                    }
                }
            }

            bool isRemoved = PagemarkCollection.Current.Remove(item);
            if (isRemoved)
            {
                if (selectedIndex >= 0 && !Items.Contains(SelectedItem))
                {
                    selectedIndex = selectedIndex < Items.Count ? selectedIndex : Items.Count - 1;
                    if (selectedIndex >= 0)
                    {
                        SelectedItem = Items[selectedIndex];
                    }
                }
            }

            return isRemoved;
        }

        // 追加ボタンの動作。ここでいいのか？
        public void AddPagemark()
        {
            var place = BookHub.Current.Book?.Place;
            if (place == null)
            {
                return;
            }

            var entryName = BookHub.Current.Book.GetViewPage()?.FullPath; ;
            if (entryName == null)
            {
                return;
            }

            var node = PagemarkCollection.Current.FindNode(place, entryName);
            if (node == null)
            {
                Add(place, entryName);
                node = PagemarkCollection.Current.FindNode(place, entryName);
            }

            SelectedItem = node;
        }

        // TODO: ここでToggleは漠然としすぎている。もっと上位で判定すべきか
        public bool Toggle(string place, string entryName)
        {
            if (place == null) return false;

            var node = PagemarkCollection.Current.FindNode(place, entryName);
            if (node == null)
            {
                Add(place, entryName);
                return true;
            }
            else
            {
                Remove(node);
                return false;
            }
        }


        /// <summary>
        /// 指定のマーカーに移動。存在しなければ移動しない
        /// </summary>
        public void Jump(string place, string entryName)
        {
            var node = PagemarkCollection.Current.FindNode(place, entryName);
            if (node != null)
            {
                SelectedItem = node;
            }
        }


        public void Move(DropInfo<TreeListNode<IPagemarkEntry>> dropInfo)
        {
            if (dropInfo == null) return;
            if (dropInfo.DragItem == dropInfo.DropItem) return;

            var item = dropInfo.DragItem;
            var target = dropInfo.DropItem;

            var indexFrom = Items.IndexOf(dropInfo.DragItem);
            var indexTo = Items.IndexOf(dropInfo.DropItem);


            const double margine = 0.25;

            if (target.Value is PagemarkFolder folder)
            {
                if (dropInfo.Position < margine)
                {
                    PagemarkCollection.Current.Move(item, target, -1);
                }
                else if (dropInfo.Position > (1.0 - margine) && !target.IsExpanded)
                {
                    PagemarkCollection.Current.Move(item, target, +1);
                }
                else
                {
                    PagemarkCollection.Current.MoveToChild(item, target);
                }
            }
            else
            {
                if (target.GetNext() == null && dropInfo.Position > (1.0 - margine))
                {
                    PagemarkCollection.Current.Move(item, target, +1);
                }
                else if (indexFrom < indexTo)
                {
                    PagemarkCollection.Current.Move(item, target, +1);
                }
                else
                {
                    PagemarkCollection.Current.Move(item, target, -1);
                }
            }
        }

        internal void NewFolder()
        {
            PagemarkCollection.Current.AddFirst(new PagemarkFolder() { Name = Properties.Resources.WordNewFolder });
        }


        public void PrevPagemark()
        {
            if (BookHub.Current.IsLoading) return;

            if (!CanMoveSelected(-1))
            {
                InfoMessage.Current.SetMessage(InfoMessageType.Notify, Properties.Resources.NotifyPagemarkPrevFailed);
                return;
            }

            if (MoveSelected(-1))
            {
                if (SelectedItem?.Value is Pagemark)
                {
                    Decide(SelectedItem);
                }
            }
        }

        public void NextPagemark()
        {
            if (BookHub.Current.IsLoading) return;

            if (!CanMoveSelected(+1))
            {
                InfoMessage.Current.SetMessage(InfoMessageType.Notify, Properties.Resources.NotifyPagemarkNextFailed);
                return;
            }

            if (MoveSelected(+1))
            {
                if (SelectedItem?.Value is Pagemark)
                {
                    Decide(SelectedItem);
                }
            }
        }

        public bool CanMoveSelected(int direction)
        {
            var pagemarks = Items.Where(e => e.Value is Pagemark);

            if (SelectedItem == null)
            {
                return pagemarks.Count() > 0;
            }
            else
            {
                var index = Items.IndexOf(SelectedItem);
                return direction > 0
                    ? index < Items.IndexOf(pagemarks.Last())
                    : index > Items.IndexOf(pagemarks.First());
            }
        }

        public bool MoveSelected(int direction)
        {
            if (direction == 0) throw new ArgumentOutOfRangeException(nameof(direction));

            if (SelectedItem == null)
            {
                var pagemarks = Items.Where(e => e.Value is Pagemark);
                var node = direction >= 0 ? pagemarks.FirstOrDefault() : pagemarks.LastOrDefault();
                if (node != null)
                {
                    SelectedItem = node;
                    return true;
                }
            }
            else
            {
                var node = GetNeighborPagemark(SelectedItem, direction);
                if (node != null)
                {
                    SelectedItem = node;
                    return true;
                }
            }

            return false;
        }

        private TreeListNode<IPagemarkEntry> GetNeighborPagemark(TreeListNode<IPagemarkEntry> item, int direction)
        {
            if (item == null) throw new ArgumentNullException(nameof(item));
            if (direction == 0) throw new ArgumentOutOfRangeException(nameof(direction));

            if (Items == null || Items.Count <= 0)
            {
                return null;
            }

            int index = Items.IndexOf(item);
            if (index < 0)
            {
                return null;
            }

            while (true)
            {
                index = index + direction;
                if (index < 0 || index >= Items.Count)
                {
                    return null;
                }
                var node = Items[index];
                if (node.Value is Pagemark)
                {
                    return node;
                }
            }
        }

    }
}
