﻿using NeeLaboratory.ComponentModel;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace NeeView
{
    public class QuickAccessCollection : BindableBase
    {
        static QuickAccessCollection() => Current = new QuickAccessCollection();
        public static QuickAccessCollection Current { get; }


        public event CollectionChangeEventHandler CollectionChanged;

        private ObservableCollection<QuickAccess> _items = new ObservableCollection<QuickAccess>();
        public ObservableCollection<QuickAccess> Items
        {
            get { return _items; }
            set { SetProperty(ref _items, value); }
        }


        public void Insert(int index, QuickAccess item)
        {
            if (item is null) throw new ArgumentNullException(nameof(item));

            Items.Insert(index, item);
            CollectionChanged?.Invoke(this, new CollectionChangeEventArgs(CollectionChangeAction.Add, item));
        }

        public bool Remove(QuickAccess item)
        {
            if (item is null) throw new ArgumentNullException(nameof(item));

            var isRemoved = Items.Remove(item);
            CollectionChanged?.Invoke(this, new CollectionChangeEventArgs(CollectionChangeAction.Remove, item));
            return isRemoved;
        }

        public void Move(int srcIndex, int dstIndex)
        {
            if (srcIndex == dstIndex) return;

            var item = Items[srcIndex];

            Items.RemoveAt(srcIndex);
            CollectionChanged?.Invoke(this, new CollectionChangeEventArgs(CollectionChangeAction.Remove, item));

            Items.Insert(dstIndex, item);
            CollectionChanged?.Invoke(this, new CollectionChangeEventArgs(CollectionChangeAction.Add, item));
        }


        #region Memento

        [DataContract]
        public class Memento : IMemento
        {
            [DataMember(EmitDefaultValue = false)]
            public List<QuickAccess> Items { get; set; }

            [OnDeserializing]
            private void OnDeserializing(StreamingContext c)
            {
                this.InitializePropertyDefaultValues();
            }
        }

        public Memento CreateMemento()
        {
            var memento = new Memento();

            memento.Items = new List<QuickAccess>(this.Items);

            return memento;
        }

        public void Restore(Memento memento)
        {
            if (memento == null) return;

            this.Items = new ObservableCollection<QuickAccess>(memento.Items);
            CollectionChanged?.Invoke(this, new CollectionChangeEventArgs(CollectionChangeAction.Refresh, null));
        }

        #endregion

    }


}
