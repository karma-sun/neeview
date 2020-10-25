using NeeLaboratory.ComponentModel;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace NeeView.Runtime.LayoutPanel
{
    public class LayoutDockPanelContent : BindableBase 
    {
        private LayoutPanelCollection _selectedItem;


        public LayoutDockPanelContent(LayoutPanelManager manager)
        {
            LayoutPanelManager = manager;
        }


        public LayoutPanelManager LayoutPanelManager { get; set; }

        public List<LayoutPanelCollection> Items { get; } = new List<LayoutPanelCollection>();

        public LayoutPanelCollection SelectedItem
        {
            get { return _selectedItem; }
            set
            {
                if (_selectedItem != value)
                {
                    _selectedItem = Items.Contains(value) ? value : null;
                    RaisePropertyChanged();
                }
            }
        }


        public void Add(LayoutPanelCollection item)
        {
            if (Items.Contains(item)) return;
            Items.Add(item);
        }

        public void Remove(LayoutPanelCollection item)
        {
            if (item == SelectedItem)
            {
                SelectedItem = null;
            }
            Items.Remove(item);
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

        public void Insert(int index, LayoutPanelCollection item)
        {
            Items.Insert(index, item);
        }

        public void Move(int oldIndex, int newIndex)
        {
            var removedItem = Items[oldIndex];

            Items.RemoveAt(oldIndex);
            Items.Insert(newIndex, removedItem);
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

            (var collection, var item) = LayoutPanelManager.FindPanelContains(panel);

            if (item == target)
            {
                target.Move(target.IndexOf(panel), index);
            }
            else
            {
                item.Remove(panel);
                if (item.Count == 0)
                {
                    collection.Remove(item);
                }
                target.Insert(index, panel);
            }
        }

        // パネル単位でののドック所属を変更
        // リーダーは配下を伴う。リーダーでない場合は独立
        public void MovePanel(int index, LayoutPanel panel)
        {
            if (panel is null) throw new ArgumentNullException(nameof(panel));

            (var collection, var item) = LayoutPanelManager.FindPanelContains(panel);

            if (item == null) throw new InvalidOperationException();

            if (item.First() == panel)
            {
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
            else
            {
                item.Remove(panel);
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

            Items.Clear();
            Items.AddRange(memento.Panels.Select(e => new LayoutPanelCollection(e.Where(x => LayoutPanelManager.Panels.ContainsKey(x)).Select(x => LayoutPanelManager.Panels[x]))));
            SelectedItem = Items.FirstOrDefault(e => e.Any(x => x.Key == memento.SelectedItem));
        }

        #endregion
    }

}
