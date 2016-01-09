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

namespace NeeView
{
    /// <summary>
    /// 履歴
    /// </summary>
    public class BookHistory
    {
        // 履歴
        public LinkedList<Book.Memento> History { get; private set; } = new LinkedList<Book.Memento>();

        // 履歴保持最大数
        private int _MaxHistoryCount = 100;
        public int MaxHistoryCount
        {
            get { return _MaxHistoryCount; }
            set { _MaxHistoryCount = value; Resize(); }
        }

        // 履歴クリア
        public void Clear()
        {
            History.Clear();
        }

        // 履歴サイズ調整
        private void Resize()
        {
            while (History.Count > MaxHistoryCount)
            {
                History.RemoveLast();
            }
        }

        // 履歴追加
        public void Add(Book book)
        {
            if (book?.Place == null) return;
            if (book.Pages.Count <= 0) return;

            var item = History.FirstOrDefault(e => e.Place == book.Place);
            if (item != null) History.Remove(item);

            var setting = new Book.Memento();
            setting = book.CreateMemento(); //.Store(book);
            History.AddFirst(setting);

            Resize();
        }

        // 履歴削除
        public void Remove(string place)
        {
            var item = History.FirstOrDefault(e => e.Place == place);
            if (item != null) History.Remove(item);
        }

        // 履歴検索
        public Book.Memento Find(string place)
        {
            return History.FirstOrDefault(e => e.Place == place);
        }

        // 最近使った履歴のリストアップ
        public List<Book.Memento> ListUp(int size)
        {
            var list = new List<Book.Memento>();
            foreach (var item in History)
            {
                if (list.Count >= size) break;
                list.Add(item);
            }
            return list;
        }


        #region Memento

        /// <summary>
        /// 履歴Memento
        /// </summary>
        [DataContract]
        public class Memento
        {
            [DataMember]
            public List<Book.Memento> History { get; set; }

            [DataMember]
            public int MaxHistoryCount { get; set; }

            private void Constructor()
            {
                History = new List<Book.Memento>();
                MaxHistoryCount = 100;
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
        }

        // memento作成
        public Memento CreateMemento()
        {
            var memento = new Memento();
            memento.History = this.History.ToList();
            memento.MaxHistoryCount = this.MaxHistoryCount;
            return memento;
        }

        // memento適用
        public void Restore(Memento memento)
        {
            this.History = new LinkedList<Book.Memento>(memento.History);
            this.MaxHistoryCount = memento.MaxHistoryCount;
        }

        #endregion
    }
}
