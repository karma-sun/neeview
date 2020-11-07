using NeeLaboratory.ComponentModel;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Linq;

namespace NeeView.Runtime.LayoutPanel
{
    public class DragDropDescriptor : IDragDropDescriptor
    {
        private readonly LayoutPanelManager _manager;

        public DragDropDescriptor(LayoutPanelManager manager)
        {
            if (manager == null) throw new ArgumentNullException();
            _manager = manager;
        }

        public void DragBegin()
        {
            _manager.RaiseDragBegin();
        }

        public void DragEnd()
        {
            _manager.RaiseDragEnd();
        }
    }

    public class LayoutDockPanelContent : BindableBase
    {
        private LayoutPanelCollection _selectedItem;
        private LayoutPanelCollection _lastSelectedItem;

        public LayoutDockPanelContent(LayoutPanelManager manager)
        {
            LayoutPanelManager = manager;
            Items.CollectionChanged += Items_CollectionChanged;
        }


        private void Items_CollectionChanged(object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
        {
            UpdateLeaderPanels();
        }

        private void UpdateLeaderPanels()
        { 
            LeaderPanels = Items.Select(x => x.First()).ToList();
        }

        public LayoutPanelManager LayoutPanelManager { get; set; }


        public ObservableCollection<LayoutPanelCollection> Items { get; } = new ObservableCollection<LayoutPanelCollection>();

        public LayoutPanelCollection SelectedItem
        {
            get { return _selectedItem; }
            set
            {
                if (_selectedItem != value)
                {
                    _selectedItem = Items.Contains(value) ? value : null;
                    if (_selectedItem != null)
                    {
                        _lastSelectedItem = _selectedItem;
                    }
                    RaisePropertyChanged();
                }
            }
        }

        public LayoutPanelCollection LastSelectedItem
        {
            get { return Items.Contains(_lastSelectedItem) ? _lastSelectedItem : null; }
        }



        private List<LayoutPanel> _leaderPanels;
        public List<LayoutPanel> LeaderPanels
        {
            get { return _leaderPanels; }
            set { SetProperty(ref _leaderPanels, value); }
        }


        private void AttachItemsChangeCallback(LayoutPanelCollection item)
        {
            item.CollectionChanged += LayoutPanelCollection_CollectionChanged;
        }

        private void DetachItemsChangeCallback(LayoutPanelCollection item)
        {
            item.CollectionChanged -= LayoutPanelCollection_CollectionChanged;
        }

        private void LayoutPanelCollection_CollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            UpdateLeaderPanels();
        }


        public void ToggleSelectedItem()
        {
            if (SelectedItem is null)
            {
                SelectedItem = LastSelectedItem ?? Items.FirstOrDefault();
            }
            else
            {
                SelectedItem = null;
            }
        }


        public void Add(LayoutPanelCollection item)
        {
            if (Items.Contains(item)) return;

            Items.Add(item);
            AttachItemsChangeCallback(item);
        }

        public void Insert(int index, LayoutPanelCollection item)
        {
            Items.Insert(index, item);
            AttachItemsChangeCallback(item);
        }

        public void Clear()
        {
            SelectedItem = null;
            foreach(var item in Items)
            {
                DetachItemsChangeCallback(item);
            }
            Items.Clear();
        }


        public void Remove(LayoutPanelCollection item)
        {
            if (!Items.Contains(item)) return;

            if (item == SelectedItem)
            {
                SelectedItem = null;
            }
            Items.Remove(item);
            DetachItemsChangeCallback(item);
        }

        public void RemoveAt(int index)
        {
            Remove(Items.ElementAtOrDefault(index));
        }

        public bool Contains(LayoutPanelCollection item)
        {
            return Items.Contains(item);
        }

        public LayoutPanelCollection FirstOrDefault(Func<LayoutPanelCollection, bool> predicate)
        {
            return Items.FirstOrDefault(predicate);
        }

        public LayoutPanelCollection FirstOrDefaultPanelContains(LayoutPanel panel)
        {
            return Items.FirstOrDefault(e => e.Contains(panel));
        }

        public int IndexOf(LayoutPanelCollection item)
        {
            return Items.IndexOf(item);
        }



        public void Move(int oldIndex, int newIndex)
        {
            var newIndexFixed = NeeLaboratory.MathUtility.Clamp(newIndex, 0, Items.Count - 1);

            var removedItem = Items[oldIndex];

            Remove(removedItem);
            Insert(newIndexFixed, removedItem);
        }

        public bool ContainsPanel(LayoutPanel panel)
        {
            return Items.SelectMany(e => e).Contains(panel);
        }

        public void AddPanel(LayoutPanel panel)
        {
            if (ContainsPanel(panel)) throw new InvalidOperationException();
            Add(new LayoutPanelCollection() { panel });
        }

        public void AddPanelRange(IEnumerable<LayoutPanel> panels)
        {
            foreach (var panel in panels)
            {
                AddPanel(panel);
            }
        }

        public void RemovePanel(LayoutPanel panel)
        {
            var list = FirstOrDefault(e => e.Contains(panel));
            if (list == null) return;

            if (list.IsStandAlone(panel))
            {
                Remove(list);
            }
            else
            {
                list.Remove(panel);
            }
        }

        // パネルの配置を変更
        // 表示中のパネルに追加配置する動作
        public void MovePanelA(LayoutPanelCollection target, int index, LayoutPanel panel)
        {
            if (target is null) throw new ArgumentNullException(nameof(target));
            if (panel is null) throw new ArgumentNullException(nameof(panel));

            (var dock, var collection) = LayoutPanelManager.FindPanelContains(panel);

            if (collection == target)
            {
                target.Move(target.IndexOf(panel), index);
            }
            else
            {
                collection.Remove(panel);
                if (collection.Count == 0)
                {
                    dock.Remove(collection);
                }
                target.Insert(index, panel);
            }
        }



        // パネル単位でののドック所属を変更
        // リーダーは配下を伴う。リーダーでない場合は独立
        public void MovePanel(int index, LayoutPanel panel)
        {
            if (panel is null) throw new ArgumentNullException(nameof(panel));

            (var dock, var collection) = LayoutPanelManager.FindPanelContains(panel);

            if (collection == null) throw new InvalidOperationException();

            if (collection.First() == panel)
            {
                if (dock == this)
                {
                    Move(Items.IndexOf(collection), index);
                }
                else
                {
                    dock.Remove(collection);
                    Insert(index, collection);
                }
            }
            else
            {
                collection.Remove(panel);
                Insert(index, new LayoutPanelCollection() { panel });
            }
        }


        // リスト単位でのドック移動
        // ドラッグの単位はパネルなのでこちらは使われない？
        public void MovePanel(int index, LayoutPanelCollection item)
        {
            if (item == null) throw new ArgumentNullException(nameof(item));

            var collection = LayoutPanelManager.FindPanelListCollection(item);
            if (collection == null) throw new InvalidOperationException();

            if (collection == this)
            {
                Move(Items.IndexOf(item), index);
            }
            else
            {
                collection.Remove(item);
                Insert(index, item);
            }
        }

        #region Memento

        public class Memento
        {
            public List<List<string>> Panels { get; set; }
            public string SelectedItem { get; set; }
        }

        public Memento CreateMemento()
        {
            var memento = new Memento();
            memento.Panels = Items.Select(e => e.Select(x => x.Key).ToList()).ToList();
            memento.SelectedItem = SelectedItem?.First().Key;
            return memento;
        }

        public void Restore(Memento memento)
        {
            if (memento == null) return;

            Clear();
            foreach (var item in memento.Panels.Select(e => new LayoutPanelCollection(e.Where(x => LayoutPanelManager.Panels.ContainsKey(x)).Select(x => LayoutPanelManager.Panels[x]))))
            {
                Add(item);
            }
            SelectedItem = Items.FirstOrDefault(e => e.Any(x => x.Key == memento.SelectedItem));
        }

        #endregion
    }


}
