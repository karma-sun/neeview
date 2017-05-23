// Copyright (c) 2016 Mitsuhiro Ito (nee)
//
// This software is released under the MIT License.
// http://opensource.org/licenses/mit-license.php

using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using System.Collections.ObjectModel;
using System.Windows.Data;
using System.ComponentModel;
using System.Diagnostics;
using NeeView.ComponentModel;

namespace NeeView
{
    public class BookmarkCollection : BindableBase
    {
        public static BookmarkCollection Current { get; private set; }

        //
        public event EventHandler<BookMementoCollectionChangedArgs> BookmarkChanged;

        // ブックマーク
        private ObservableCollection<BookMementoUnitNode> _items;
        public ObservableCollection<BookMementoUnitNode> Items
        {
            get { return _items; }
            private set
            {
                _items = value;
                BindingOperations.EnableCollectionSynchronization(_items, new object());
                RaisePropertyChanged();
            }
        }

        /// <summary>
        /// SelectedItem Property
        /// </summary>
        private BookMementoUnitNode _selectedItem;
        public BookMementoUnitNode SelectedItem
        {
            get { return _selectedItem; }
            set { _selectedItem = value; RaisePropertyChanged(); }
        }

        //
        public BookmarkCollection()
        {
            Current = this;
            Items = new ObservableCollection<BookMementoUnitNode>();
        }

        // クリア
        public void Clear()
        {
            // new
            foreach (var node in Items)
            {
                node.Value.BookmarkNode = null;
            }
            Items.Clear();

            BookmarkChanged?.Invoke(this, new BookMementoCollectionChangedArgs(BookMementoCollectionChangedType.Clear, null));
        }


        // 設定
        public void Load(IEnumerable<Book.Memento> items)
        {
            Clear();

            //
            foreach (var item in items)
            {
                var unit = BookMementoCollection.Current.Find(item.Place);

                if (unit == null)
                {
                    unit = new BookMementoUnit();

                    unit.Memento = item;
                    unit.BookmarkNode = new BookMementoUnitNode(unit);
                    Items.Add(unit.BookmarkNode);

                    BookMementoCollection.Current.Add(unit);
                }
                else
                {
                    unit.Memento = item;
                    unit.BookmarkNode = new BookMementoUnitNode(unit);
                    Items.Add(unit.BookmarkNode);
                }
            }

            BookmarkChanged?.Invoke(this, new BookMementoCollectionChangedArgs(BookMementoCollectionChangedType.Load, null));
        }


        // 追加
        public BookMementoUnit Add(BookMementoUnit unit, Book.Memento memento)
        {
            if (memento == null) return unit;

            try
            {
                if (unit == null)
                {
                    unit = new BookMementoUnit();

                    unit.Memento = memento;

                    unit.BookmarkNode = new BookMementoUnitNode(unit);
                    Items.Add(unit.BookmarkNode);

                    BookMementoCollection.Current.Add(unit);
                    BookmarkChanged?.Invoke(this, new BookMementoCollectionChangedArgs(BookMementoCollectionChangedType.Add, memento.Place));
                }
                else if (unit.BookmarkNode != null)
                {
                    unit.Memento = memento;
                    BookmarkChanged?.Invoke(this, new BookMementoCollectionChangedArgs(BookMementoCollectionChangedType.Update, memento.Place));
                }
                else
                {
                    unit.Memento = memento;

                    unit.BookmarkNode = new BookMementoUnitNode(unit);
                    Items.Add(unit.BookmarkNode);

                    BookmarkChanged?.Invoke(this, new BookMementoCollectionChangedArgs(BookMementoCollectionChangedType.Add, memento.Place));
                }

                return unit;
            }
            catch (Exception e)
            {
                Debug.WriteLine(e.Message);
                return null;
            }
        }

        // ブックマーク状態切り替え
        public BookMementoUnit Toggle(BookMementoUnit unit, Book.Memento memento)
        {
            if (unit == null || unit.BookmarkNode == null)
            {
                return Add(unit, memento);
            }
            else
            {
                return Remove(unit.Memento.Place);
            }
        }

        // 削除
        public BookMementoUnit Remove(string place)
        {
            var unit = BookMementoCollection.Current.Find(place);
            if (unit != null && unit.BookmarkNode != null)
            {
                Items.Remove(unit.BookmarkNode);
                unit.BookmarkNode = null;
                BookmarkChanged?.Invoke(this, new BookMementoCollectionChangedArgs(BookMementoCollectionChangedType.Remove, place));
            }
            return unit;
        }

        // 無効なブックマークを削除
        public void RemoveUnlinked()
        {
            // 削除項目収集
            var unlinked = Items.Where(e =>
            {
                var place = e.Value.Memento.Place;
                return (!System.IO.File.Exists(place) && !System.IO.Directory.Exists(place));
            })
            .ToList();

            // 削除実行
            foreach(var node in unlinked)
            {
                Debug.WriteLine($"BookmarkRemove: {node.Value.Memento.Place}");
                Items.Remove(node);
                node.Value.BookmarkNode = null;
            }
            BookmarkChanged?.Invoke(this, new BookMementoCollectionChangedArgs(BookMementoCollectionChangedType.Remove, null));
        }

        // 更新
        public void Update(Book book)
        {
            if (book?.Place == null) return;
            if (book.Pages.Count <= 0) return;

            Update(BookMementoCollection.Current.Find(book.Place), book.CreateMemento());
        }

        // 更新
        public void Update(BookMementoUnit unit, Book.Memento memento)
        {
            if (memento == null) return;
            Debug.Assert(unit == null || unit.Memento.Place == memento.Place);

            if (unit != null && unit.BookmarkNode != null)
            {
                unit.Memento = memento;
                BookmarkChanged?.Invoke(this, new BookMementoCollectionChangedArgs(BookMementoCollectionChangedType.Update, memento.Place));
            }
        }

        // 検索
        public BookMementoUnit Find(string place)
        {
            if (place == null) return null;
            var unit = BookMementoCollection.Current.Find(place);
            return unit?.BookmarkNode != null ? unit : null;
        }


        // となりを取得
        public BookMementoUnitNode GetNeighbor(BookMementoUnitNode item)
        {
            if (Items == null || Items.Count <= 0) return null;

            int index = Items.IndexOf(item);
            if (index < 0) return Items[0];

            if (index + 1 < Items.Count)
            {
                return Items[index + 1];
            }
            else if (index > 0)
            {
                return Items[index - 1];
            }
            else
            {
                return item;
            }
        }


        /// <summary>
        /// 
        /// </summary>
        /// <param name="direction"></param>
        /// <returns></returns>
        public bool CanMoveSelected(int direction)
        {
            if (SelectedItem == null)
            {
                return Items.Count > 0;
            }
            else
            {
                return direction > 0
                    ? SelectedItem != Items.Last()
                    : SelectedItem != Items.First();
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="direction"></param>
        /// <returns></returns>
        public BookMementoUnitNode MoveSelected(int direction)
        {
            if (SelectedItem == null)
            {
                SelectedItem = direction >= 0 ? Items.FirstOrDefault() : Items.LastOrDefault();
            }
            else
            {
                int index = Items.IndexOf(SelectedItem) + direction;
                if (index >= 0 && index < Items.Count)
                {
                    SelectedItem = Items[index];
                }
            }

            return SelectedItem;
        }


        #region Memento

        /// <summary>
        /// 履歴Memento
        /// </summary>
        [DataContract]
        public class Memento
        {
            [DataMember]
            public List<Book.Memento> Items { get; set; }

            private void Constructor()
            {
                Items = new List<Book.Memento>();
            }

            public Memento()
            {
                Constructor();
            }

            [OnDeserializing]
            private void Deserializing(StreamingContext c)
            {
                Constructor();
            }

            // 結合
            public void Merge(Memento memento)
            {
                Items = Items.Concat(memento?.Items).Distinct(new Book.MementoPlaceCompare()).ToList();
            }

            // ファイルに保存
            public void Save(string path)
            {
                XmlWriterSettings settings = new XmlWriterSettings();
                settings.Encoding = new System.Text.UTF8Encoding(false);
                settings.Indent = true;
                using (XmlWriter xw = XmlWriter.Create(path, settings))
                {
                    DataContractSerializer serializer = new DataContractSerializer(typeof(Memento));
                    serializer.WriteObject(xw, this);
                }
            }

            // ファイルから読み込み
            public static Memento Load(string path)
            {
                using (XmlReader xr = XmlReader.Create(path))
                {
                    DataContractSerializer serializer = new DataContractSerializer(typeof(Memento));
                    Memento memento = (Memento)serializer.ReadObject(xr);
                    return memento;
                }
            }
        }

        // memento作成
        public Memento CreateMemento(bool forSave)
        {
            var memento = new Memento();
            memento.Items = this.Items.Select(e => e.Value.Memento).ToList();
            if (forSave)
            {
                // テンポラリフォルダーを除外
                memento.Items.RemoveAll((e) => e.Place.StartsWith(Temporary.TempDirectory));
            }

            return memento;
        }

        // memento適用
        public void Restore(Memento memento)
        {
            this.Load(memento.Items);
        }

        #endregion
    }
}
