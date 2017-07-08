// Copyright (c) 2016 Mitsuhiro Ito (nee)
//
// This software is released under the MIT License.
// http://opensource.org/licenses/mit-license.php

using NeeView.ComponentModel;
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
    public class BookHistory : BindableBase
    {
        public static BookHistory Current { get; private set; }

        // 履歴変更イベント
        public event EventHandler<BookMementoCollectionChangedArgs> HistoryChanged;

        // 履歴コレクション
        // 膨大な数で変更が頻繁に行われるのでLinkedList
        public LinkedList<BookMementoUnit> Items { get; set; }

        // フォルダーリストで開いていた場所
        public string LastFolder { get; set; }

        // 最後に開いたフォルダー
        public string LastAddress { get; set; }

        // 履歴制限
        private int _limitSize;

        // 履歴制限(時間)
        private TimeSpan _limitSpan;

        /// <summary>
        /// IsKeepFolderStatus property.
        /// </summary>
        public bool IsKeepFolderStatus { get; set; } = true;


        // フォルダー設定
        private Dictionary<string, FolderParameter.Memento> _folders = new Dictionary<string, FolderParameter.Memento>();

        // フォルダー設定
        public void SetFolderMemento(string path, FolderParameter.Memento memento)
        {
            path = path ?? "<<root>>";

            // 標準設定は記憶しない
            if (memento.IsDefault)
            {
                _folders.Remove(path);
            }
            else
            {
                _folders[path] = memento;
            }
        }

        // フォルダー設定取得
        public FolderParameter.Memento GetFolderMemento(string path)
        {
            path = path ?? "<<root>>";

            FolderParameter.Memento memento;
            _folders.TryGetValue(path, out memento);
            return memento ?? FolderParameter.Memento.Default;
        }

        /// <summary>
        /// 
        /// </summary>
        public BookHistory()
        {
            Current = this;
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
                var unit = BookMementoCollection.Current.Find(item.Place);

                if (unit == null)
                {
                    unit = new BookMementoUnit();

                    unit.Memento = item;
                    unit.HistoryNode = new LinkedListNode<BookMementoUnit>(unit);
                    Items.AddLast(unit.HistoryNode);

                    BookMementoCollection.Current.Add(unit);
                }
                else
                {
                    unit.Memento = item;
                    unit.HistoryNode = new LinkedListNode<BookMementoUnit>(unit);
                    Items.AddLast(unit.HistoryNode);
                }
            }

            LimitNow();
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
                        var lastAccessTime = unit.Memento?.LastAccessTime ?? DateTime.Now;
                        unit.Memento = memento;
                        unit.Memento.LastAccessTime = lastAccessTime;

                        HistoryChanged?.Invoke(this, new BookMementoCollectionChangedArgs(BookMementoCollectionChangedType.Update, memento.Place));
                    }
                }
                else if (unit == null)
                {
                    unit = new BookMementoUnit();

                    unit.Memento = memento;
                    unit.Memento.LastAccessTime = DateTime.Now;

                    unit.HistoryNode = new LinkedListNode<BookMementoUnit>(unit);
                    Items.AddFirst(unit.HistoryNode);
                    LimitNow();

                    BookMementoCollection.Current.Add(unit);
                    HistoryChanged?.Invoke(this, new BookMementoCollectionChangedArgs(BookMementoCollectionChangedType.Add, memento.Place));
                }
                else if (unit.HistoryNode != null)
                {
                    unit.Memento = memento;
                    unit.Memento.LastAccessTime = DateTime.Now;

                    if (Items.First == unit.HistoryNode)
                    {
                        HistoryChanged?.Invoke(this, new BookMementoCollectionChangedArgs(BookMementoCollectionChangedType.Update, memento.Place));
                    }
                    else
                    {
                        Items.Remove(unit.HistoryNode);
                        Items.AddFirst(unit.HistoryNode);
                        HistoryChanged?.Invoke(this, new BookMementoCollectionChangedArgs(BookMementoCollectionChangedType.Add, memento.Place));
                    }
                }
                else
                {
                    unit.Memento = memento;
                    unit.Memento.LastAccessTime = DateTime.Now;

                    unit.HistoryNode = new LinkedListNode<BookMementoUnit>(unit);
                    Items.AddFirst(unit.HistoryNode);
                    LimitNow();

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
            var unit = BookMementoCollection.Current.Find(memento.Place);

            Add(unit, memento, isKeepOrder);
        }


        // 履歴削除
        public void Remove(string place)
        {
            var unit = BookMementoCollection.Current.Find(place);
            if (unit != null && unit.HistoryNode != null)
            {
                Items.Remove(unit.HistoryNode);
                unit.HistoryNode = null;
                HistoryChanged?.Invoke(this, new BookMementoCollectionChangedArgs(BookMementoCollectionChangedType.Remove, unit.Memento.Place));
            }
        }

        // 全履歴削除
        public void RemoveAll()
        {
            foreach (var item in Items)
            {
                item.HistoryNode = null;
            }
            Items.Clear();
            HistoryChanged?.Invoke(this, new BookMementoCollectionChangedArgs(BookMementoCollectionChangedType.Remove, null));
        }

        // 無効な履歴削除
        public void RemoveUnlinked()
        {
            var node = Items.First;
            while (node != null)
            {
                var next = node.Next;
                var place = node.Value.Memento.Place;
                if (!System.IO.File.Exists(place) && !System.IO.Directory.Exists(place))
                {
                    Debug.WriteLine($"HistoryRemove: {place}");
                    Items.Remove(node);
                    node.Value.HistoryNode = null;
                }
                node = next;
            }

            HistoryChanged?.Invoke(this, new BookMementoCollectionChangedArgs(BookMementoCollectionChangedType.Remove, null));
        }


        // 履歴数制限 現在のリスト
        private bool LimitNow()
        {
            return false;

            // フォルダーリストに不具合が出るので処理無効
#if false
            int oldCount = Items.Count;

            // limit size
            if (LimitSize != 0)
            {
                while (Items.Count > LimitSize)
                {
                    var last = Items.Last();
                    Items.Remove(last);
                    last.HistoryNode = null;
                }
            }

            // limit time
            if (LimitSpan != default(TimeSpan))
            {
                var limitTime = DateTime.Now - LimitSpan;
                while (Items.Last.Value.Memento.LastAccessTime < limitTime)
                {
                    var last = Items.Last();
                    Items.Remove(last);
                    last.HistoryNode = null;
                }
            }

            return oldCount != Items.Count;
#endif
        }



        // 履歴検索
        public BookMementoUnit Find(string place)
        {
            if (place == null) return null;
            var unit = BookMementoCollection.Current.Find(place);
            return unit?.HistoryNode != null ? unit : null;
        }

        // 最近使った履歴のリストアップ
        public List<Book.Memento> ListUp(int size)
        {
            return Items.Take(size).Select(e => e.Memento).ToList();
        }

        /// <summary>
        /// 範囲指定して履歴をリストアップ
        /// </summary>
        /// <param name="current">基準位置</param>
        /// <param name="direction">方向</param>
        /// <param name="size">取得サイズ</param>
        /// <returns></returns>
        internal List<string> ListUp(string current, int direction, int size)
        {
            var list = new List<string>();
            var now = Find(current);
            var unit = now ?? Items.FirstOrDefault();
            if (now == null && unit != null && direction < 0)
            {
                list.Add(unit.Memento.Place);
            }
            for (int i = 0; i < size; i++)
            {
                unit = direction < 0 ? unit?.HistoryNode?.Next?.Value : unit?.HistoryNode?.Previous?.Value; // リストと履歴の方向は逆
                if (unit == null) break;
                list.Add(unit.Memento.Place);
            }
            return list;
        }



        #region Memento

        /// <summary>
        /// 履歴Memento
        /// </summary>
        [DataContract]
        public class Memento : BindableBase
        {
            [DataMember]
            public int _Version { get; set; }

            [DataMember(Name = "History")]
            public List<Book.Memento> Items { get; set; }

            [DataMember(Order = 8)]
            public string LastFolder { get; set; }

            [DataMember(Order = 12)]
            public int LimitSize { get; set; }

            [DataMember(Order = 12)]
            public TimeSpan LimitSpan { get; set; }

            [DataMember(Order = 19)]
            public bool IsKeepFolderStatus { get; set; }

            [DataMember]
            public Dictionary<string, FolderParameter.Memento> Folders { get; set; }

            [DataMember]
            public string LastAddress { get; set; }


            // no used
            [Obsolete, DataMember(Order = 8, EmitDefaultValue = false)]
            public Dictionary<string, FolderOrder> FolderOrders { get; set; } // no used (ver.22)


            //
            private void Constructor()
            {
                Items = new List<Book.Memento>();
                LimitSize = -1;
                IsKeepFolderStatus = true;
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

            [OnDeserialized]
            private void Deserialized(StreamingContext c)
            {
                if (_Version < Config.GenerateProductVersionNumber(1, 19, 0))
                {
                    if (LimitSize == 0) LimitSize = -1;
                }
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

            memento._Version = Config.Current.ProductVersionNumber;

            memento.Items = this.Items.Select(e => e.Memento).ToList();
            memento.Folders = _folders;
            memento.LastFolder = this.LastFolder;
            memento.LimitSize = _limitSize;
            memento.LimitSpan = _limitSpan;
            memento.IsKeepFolderStatus = IsKeepFolderStatus;
            memento.LastAddress = App.Current.IsOpenLastBook ? this.LastAddress : null;

            if (forSave)
            {
                // テンポラリフォルダーを除外
                memento.Items.RemoveAll((e) => e.Place.StartsWith(Temporary.TempDirectory));
                // 履歴保持数制限適用
                memento.Items = Limit(memento.Items); // 履歴保持数制限
                // フォルダー保存制限
                if (!memento.IsKeepFolderStatus)
                {
                    memento.Folders = null;
                    memento.LastFolder = null;
                }
            }

            return memento;
        }

        // memento適用
        public void Restore(Memento memento, bool fromLoad)
        {
            this.LastFolder = memento.LastFolder;
            this.LastAddress = memento.LastAddress;
            _folders = memento.Folders ?? _folders;
            _limitSize = memento.LimitSize;
            _limitSpan = memento.LimitSpan;
            IsKeepFolderStatus = memento.IsKeepFolderStatus;

            this.Load(fromLoad ? Limit(memento.Items) : memento.Items);

#pragma warning disable CS0612
            // ver.22
            if (memento.FolderOrders != null)
            {
                Debug.WriteLine("[[Compatible]]: FolderOrders");
                _folders = memento.FolderOrders.ToDictionary(e => e.Key, e => new FolderParameter.Memento() { FolderOrder = e.Value });
            }
#pragma warning restore CS0612

        }


        // 履歴数制限
        private List<Book.Memento> Limit(List<Book.Memento> source)
        {
            // limit size
            var collection = _limitSize == -1 ? source : source.Take(_limitSize);

            // limit time
            var limitTime = DateTime.Now - _limitSpan;
            collection = _limitSpan == default(TimeSpan) ? collection : collection.TakeWhile(e => e.LastAccessTime > limitTime);

            return collection.ToList();
        }


        #endregion
    }
}
