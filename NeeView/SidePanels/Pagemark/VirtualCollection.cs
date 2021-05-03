using NeeView.Collections;
using NeeView.Collections.Generic;
using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Diagnostics;
using System.Linq;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Threading;

namespace NeeView
{
    /// <summary>
    /// 仮想パネル管理を行う項目
    /// </summary>
    [Obsolete]
    public interface IVirtualItem
    {
        /// <summary>
        /// 実体化解除までのカウント
        /// </summary>
        int DetachCount { get; set; }

        /// <summary>
        /// 実体化された時に呼ばれる
        /// </summary>
        void Attached();

        /// <summary>
        /// 実体化を解除された時に呼ばれる
        /// </summary>
        void Detached();
    }

    /// <summary>
    /// 仮想パネルの実項目管理用。
    /// 仮想化されたTreeListやListBoxで表示されなくなった項目のリソース廃棄タイミングを管理する
    /// </summary>
    /// <remarks>
    /// TContainerのDataContextは<see cref="IHasValue{TValue}"/>継承でなければならず、TValueは<see cref="IVirtualItem"/>継承である必要がある。監視はこのValueに対して行われる。
    /// TContainer.DataContext : <see cref="IHasValue{TValue}"/>
    ///     + Value: <see cref="IVirtualItem"/>
    /// </remarks>
    /// <typeparam name="TContainer">TreeViewItem,ListBoxItem等のコンテナ</typeparam>
    /// <typeparam name="TValue">Value型</typeparam>
    [Obsolete]
    public class VirtualCollection<TContainer, TValue>
        where TContainer : Control
    {
        private ItemsControl _itemsControl;
        private List<IVirtualItem> _items;
        public bool _darty;


        public VirtualCollection(ItemsControl itemsControl)
        {
            _itemsControl = itemsControl;

            _items = new List<IVirtualItem>();
        }


        public EventHandler<NotifyCollectionChangedEventArgs> CollectionChanged { get; set; }


        public List<IVirtualItem> Items => _items;

        public void Attach(IVirtualItem item)
        {
            if (!_items.Contains(item))
            {
                _items.Add(item);
                item.DetachCount = 0;
                item.Attached();
                CollectionChanged?.Invoke(this, new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Add, item));
                ////Debug.WriteLine($"Attached: {item}");
            }
        }

        public void Detach(IVirtualItem item)
        {
            if (_items.Contains(item))
            {
                _items.Remove(item);
                item.Detached();
                CollectionChanged?.Invoke(this, new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Remove, item));
                ////Debug.WriteLine($"Detached: {item}");
            }
        }

        public void Refresh()
        {
            _darty = true;
        }

        /// <summary>
        /// 実体化されていない項目をDetachする。
        /// </summary>
        /// <remarks>
        /// ScrollChanged等、表示が変化したときに呼び出す必要がある。
        /// 実体化が間に合っていない可能性を考慮して2度の除外判定が成立してから削除を行っている。
        /// </remarks>
        public bool CleanUp()
        {
            var nodes = CollectVisualChildren<TContainer>(_itemsControl).Select(e => e.DataContext);
            var values = nodes.OfType<IHasValue<TValue>>();
            var items = values.Select(e => e.Value).OfType<IVirtualItem>();

            var intersect = _items.Intersect(items);
            var removes = _items.Except(intersect);

            ////Debug.WriteLine($"CleanUp: {removes.Count()}/{items.Count()}/{values.Count()}/{nodes.Count()}");

            if (!removes.Any())
            {
                return false;
            }

            foreach (var item in removes)
            {
                item.DetachCount++;
            }

            var detaches = removes.Where(e => e.DetachCount > 1).ToList();
            foreach (var item in detaches)
            {
                ////Debug.WriteLine($"CleanUp.Detatched: {item}");
                item.Detached();
            }

            var rest = removes.Except(detaches);
            _items = intersect.Concat(rest).ToList();

            CollectionChanged?.Invoke(this, new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Remove, detaches));

            return true;
        }

        private IEnumerable<T> CollectVisualChildren<T>(Visual visual) where T : Visual
        {
            for (int i = 0; i < VisualTreeHelper.GetChildrenCount(visual); i++)
            {
                Visual child = (Visual)VisualTreeHelper.GetChild(visual, i);
                if (child != null)
                {
                    T correctlyTyped = child as T;
                    if (correctlyTyped != null)
                    {
                        yield return correctlyTyped;
                    }

                    foreach (var descendent in CollectVisualChildren<T>(child))
                    {
                        yield return descendent;
                    }
                }
            }
        }
    }

}
