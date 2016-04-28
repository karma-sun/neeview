// Copyright (c) 2016 Mitsuhiro Ito (nee)
//
// This software is released under the MIT License.
// http://opensource.org/licenses/mit-license.php

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Data;
using System.Xml;

namespace NeeView
{

    /// <summary>
    /// 履歴
    /// </summary>
    public class BookHistory : INotifyPropertyChanged
    {
        #region NotifyPropertyChanged
        public event System.ComponentModel.PropertyChangedEventHandler PropertyChanged;

        protected void OnPropertyChanged([System.Runtime.CompilerServices.CallerMemberName] string name = "")
        {
            if (PropertyChanged != null)
            {
                PropertyChanged(this, new System.ComponentModel.PropertyChangedEventArgs(name));
            }
        }
        #endregion

        // 履歴変更イベント
        public event EventHandler<BookMementoCollectionChangedArgs> HistoryChanged;

        // 履歴コレクション
        // 膨大な数で変更が頻繁に行われるのでLinkedList
        public LinkedList<BookMementoUnit> Items { get; set; }


        /// <summary>
        /// 
        /// </summary>
        public BookHistory()
        {
            Items = new LinkedList<BookMementoUnit>();
        }

        // 要素数
        public int Count => Items.Count;

        // 先頭の要素
        public BookMementoUnit First => Items.First();

        // 履歴クリア
        public void Clear()
        {
            foreach (var node in Items)
            {
                node.HistoryNode = null;
            }
            Items.Clear();

            HistoryChanged?.Invoke(this, new BookMementoCollectionChangedArgs(BookMementoCollectionChangedType.Clear, null));
        }


        //
        public void Load(IEnumerable<Book.Memento> items)
        {
            // 履歴リストをクリア
            Clear();

            //
            foreach (var item in items)
            {
                var unit = ModelContext.BookMementoCollection.Find(item.Place);

                if (unit == null)
                {
                    unit = new BookMementoUnit();

                    unit.Memento = item;
                    unit.HistoryNode = new LinkedListNode<BookMementoUnit>(unit);
                    Items.AddLast(unit.HistoryNode);

                    ModelContext.BookMementoCollection.Add(unit);
                }
                else
                {
                    unit.Memento = item;
                    unit.HistoryNode = new LinkedListNode<BookMementoUnit>(unit);
                    Items.AddLast(unit.HistoryNode);
                }
            }

            HistoryChanged?.Invoke(this, new BookMementoCollectionChangedArgs(BookMementoCollectionChangedType.Load, null));
        }

        // 履歴更新
        public BookMementoUnit Add(BookMementoUnit unit, Book.Memento memento, bool isKeepOrder)
        {
            if (memento == null) return unit;
            Debug.Assert(unit == null || unit.Memento.Place == memento.Place);

            try
            {
                if (isKeepOrder)
                {
                    if (unit?.HistoryNode != null)
                    {
                        unit.Memento = memento;

                        HistoryChanged?.Invoke(this, new BookMementoCollectionChangedArgs(BookMementoCollectionChangedType.Update, memento.Place));
                    }
                }
                else if (unit == null)
                {
                    unit = new BookMementoUnit();

                    unit.Memento = memento;

                    unit.HistoryNode = new LinkedListNode<BookMementoUnit>(unit);
                    Items.AddFirst(unit.HistoryNode);

                    ModelContext.BookMementoCollection.Add(unit);
                    HistoryChanged?.Invoke(this, new BookMementoCollectionChangedArgs(BookMementoCollectionChangedType.Add, memento.Place));
                }
                else if (unit.HistoryNode != null)
                {
                    unit.Memento = memento;

                    Items.Remove(unit.HistoryNode);
                    Items.AddFirst(unit.HistoryNode);
                    HistoryChanged?.Invoke(this, new BookMementoCollectionChangedArgs(BookMementoCollectionChangedType.Add, memento.Place));
                }
                else
                {
                    unit.Memento = memento;

                    unit.HistoryNode = new LinkedListNode<BookMementoUnit>(unit);
                    Items.AddFirst(unit.HistoryNode);

                    HistoryChanged?.Invoke(this, new BookMementoCollectionChangedArgs(BookMementoCollectionChangedType.Add, memento.Place));
                }

                return unit;
            }
            catch (Exception e)
            {
                Debug.WriteLine(e.Message);
                return null;
            }
        }

        // 履歴追加
        public void Add(Book book, bool isKeepOrder)
        {
            if (book?.Place == null) return;
            if (book.Pages.Count <= 0) return;

            var memento = book.CreateMemento();
            var unit = ModelContext.BookMementoCollection.Find(memento.Place);

            Add(unit, memento, isKeepOrder);
        }


        // 履歴削除
        public void Remove(string place)
        {
            var unit = ModelContext.BookMementoCollection.Find(place);
            if (unit != null && unit.HistoryNode != null)
            {
                Items.Remove(unit.HistoryNode);
                unit.HistoryNode = null;
                HistoryChanged?.Invoke(this, new BookMementoCollectionChangedArgs(BookMementoCollectionChangedType.Remove, unit.Memento.Place));
            }
        }

        // 履歴検索
        public BookMementoUnit Find(string place)
        {
            if (place == null) return null;
            var unit = ModelContext.BookMementoCollection.Find(place);
            return unit?.HistoryNode != null ? unit : null;
        }


        // 最近使った履歴のリストアップ
        public List<Book.Memento> ListUp(int size)
        {
            return Items.Take(size).Select(e => e.Memento).ToList();
        }


        #region Memento

        /// <summary>
        /// 履歴Memento
        /// </summary>
        [DataContract]
        public class Memento
        {
            [DataMember(Name = "History")]
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
        public Memento CreateMemento(bool removeTemporary)
        {
            var memento = new Memento();
            memento.Items = this.Items.Select(e => e.Memento).ToList();

            if (removeTemporary)
            {
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
