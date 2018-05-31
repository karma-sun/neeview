using NeeLaboratory.ComponentModel;
using NeeView.Windows.Property;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.Serialization;
using System.Threading;
using System.Threading.Tasks;
using System.Xml;

namespace NeeView
{
    public class BookHistoryCollection : BindableBase
    {
        public static BookHistoryCollection Current { get; private set; } = new BookHistoryCollection();

        #region Fields

        private Dictionary<string, FolderParameter.Memento> _folders = new Dictionary<string, FolderParameter.Memento>();

        #endregion

        #region Constructors

        public BookHistoryCollection()
        {
            Current = this;

            HistoryChanged += (s, e) => RaisePropertyChanged(nameof(Count));
        }

        #endregion

        #region Events

        public event EventHandler<BookMementoCollectionChangedArgs> HistoryChanged;

        #endregion

        #region Prperties

        // 履歴コレクション
        public LinkedDicionary<string, BookHistory> Items { get; set; } = new LinkedDicionary<string, BookHistory>();

        // 履歴制限
        [PropertyMember("@ParamHistoryLimitSize")]
        public int LimitSize { get; set; }

        // 履歴制限(時間)
        [PropertyMember("@ParamHistoryLimitSpan")]
        public TimeSpan LimitSpan { get; set; }

        // フォルダーリストの情報記憶
        [PropertyMember("@ParamHistoryIsKeepFolderStatus")]
        public bool IsKeepFolderStatus { get; set; } = true;

        // 要素数
        public int Count => Items.Count;

        // 先頭の要素
        public LinkedListNode<BookHistory> First => Items.First;

        #endregion

        #region Poperties for Folders

        // フォルダーリストで開いていた場所
        public string LastFolder { get; set; }

        // 最後に開いたフォルダー
        public string LastAddress { get; set; }

        #endregion

        #region Methods for Folders

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

        #endregion

        #region Methods

        //
        public IEnumerable<BookMementoUnit> CreateBookMementoUnitList()
        {
            return Items.Select(e => e.Unit);
        }

        // 履歴クリア
        public void Clear()
        {
            Items.Clear();
            BookMementoCollection.Current.CleanUp();
            HistoryChanged?.Invoke(this, new BookMementoCollectionChangedArgs(BookMementoCollectionChangedType.Clear, null));
        }

        //
        public void Load(IEnumerable<BookHistory> items, IEnumerable<Book.Memento> books)
        {
            Clear();

            foreach(var book in books)
            {
                BookMementoCollection.Current.Set(book);
            }

            foreach (var item in items)
            {
                var newItem = new BookHistory(item.Place, item.LastAccessTime);
                Items.AddLastRaw(newItem.Place, newItem);
            }

            HistoryChanged?.Invoke(this, new BookMementoCollectionChangedArgs(BookMementoCollectionChangedType.Load, null));
        }


        //
        public bool Contains(string place)
        {
            if (place == null) return false;

            return Items.ContainsKey(place);
        }


        // 履歴検索
        public LinkedListNode<BookHistory> FindNode(string place)
        {
            if (place == null) return null;

            return Items.Find(place);
        }

        public BookHistory Find(string place)
        {
            return FindNode(place)?.Value;
        }

        public BookMementoUnit FindUnit(string place)
        {
            return Find(place)?.Unit;
        }

        // 履歴追加
        public void Add(Book.Memento memento, bool isKeepOrder)
        {
            if (memento == null) return;

            try
            {
                var node = FindNode(memento.Place);
                if (node != null && isKeepOrder)
                {
                    node.Value.Unit.Memento = memento;
                    HistoryChanged?.Invoke(this, new BookMementoCollectionChangedArgs(BookMementoCollectionChangedType.Update, memento.Place));
                }
                else
                {
                    node = node ?? new LinkedListNode<BookHistory>(new BookHistory(BookMementoCollection.Current.Set(memento), DateTime.Now));
                    node.Value.Unit.Memento = memento;
                    node.Value.LastAccessTime = DateTime.Now;

                    if (node == Items.First)
                    {
                        HistoryChanged?.Invoke(this, new BookMementoCollectionChangedArgs(BookMementoCollectionChangedType.Update, memento.Place));
                    }
                    else
                    {
                        Items.AddFirst(node.Value.Place, node.Value);
                        HistoryChanged?.Invoke(this, new BookMementoCollectionChangedArgs(BookMementoCollectionChangedType.Add, memento.Place));
                    }
                }
            }
            catch (Exception e)
            {
                Debug.WriteLine(e.Message);
            }
        }

        // 履歴削除
        public void Remove(string place)
        {
            var node = FindNode(place);
            if (node != null)
            {
                Items.Remove(place);
                HistoryChanged?.Invoke(this, new BookMementoCollectionChangedArgs(BookMementoCollectionChangedType.Remove, place));
            }
        }

        // 無効な履歴削除
        public async Task RemoveUnlinkedAsync(CancellationToken token)
        {
            // 削除項目収集
            var unlinked = new List<LinkedListNode<BookHistory>>();
            for (var node = this.Items.First; node != null; node = node.Next)
            {
                if (!(await ArchiveFileSystem.ExistsAsync(node.Value.Place, token)))
                {
                    unlinked.Add(node);
                }
            }

            // 削除実行
            foreach (var node in unlinked)
            {
                Debug.WriteLine($"HistoryRemove: {node.Value.Place}");
                Items.Remove(node.Value.Place);
            }

            HistoryChanged?.Invoke(this, new BookMementoCollectionChangedArgs(BookMementoCollectionChangedType.Remove, null));
        }


        // 最近使った履歴のリストアップ
        public List<BookHistory> ListUp(int size)
        {
            return Items.Take(size).ToList();
        }

        /// <summary>
        /// 範囲指定して履歴をリストアップ
        /// </summary>
        /// <param name="current">基準位置</param>
        /// <param name="direction">方向</param>
        /// <param name="size">取得サイズ</param>
        /// <returns></returns>
        internal List<BookHistory> ListUp(string current, int direction, int size)
        {
            var list = new List<BookHistory>();

            var now = FindNode(current);
            var node = now ?? Items.First;

            if (now == null && node != null && direction < 0)
            {
                list.Add(node.Value);
            }

            for (int i = 0; i < size; i++)
            {
                node = direction < 0 ? node?.Next : node?.Previous; // リストと履歴の方向は逆

                if (node == null) break;
                list.Add(node.Value);
            }

            return list;
        }

        #endregion


        #region Memento

        /// <summary>
        /// 履歴Memento
        /// </summary>
        [DataContract(Name = "BookHistory.Memento")]
        public class Memento : BindableBase
        {
            [DataMember]
            public int _Version { get; set; }

            [DataMember(Name = "Histories")]
            public List<BookHistory> Items { get; set; }

            [Obsolete]
            [DataMember(Name = "History", EmitDefaultValue = false)]
            public List<Book.Memento> OldBooks { get; set; }

            [DataMember]
            public List<Book.Memento> Books { get; set; }

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
                Items = new List<BookHistory>();
                Books = new List<Book.Memento>();
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

#pragma warning disable CS0612
                if (_Version < Config.GenerateProductVersionNumber(31, 0, 0))
                {
                    Items = OldBooks != null
                        ? OldBooks.Select(e => new BookHistory() { Place = e.Place, LastAccessTime = e.LastAccessTime }).ToList()
                        : new List<BookHistory>();

                    Books = OldBooks ?? new List<Book.Memento>();
                    foreach(var book in Books)
                    {
                        book.LastAccessTime = default(DateTime);
                    }

                    OldBooks = null;
                }
#pragma warning restore CS0612
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
                using (var stream = new FileStream(path, FileMode.Open, FileAccess.Read))
                {
                    return Load(stream);
                }
            }

            // ストリームから読み込み
            public static Memento Load(Stream stream)
            {
                using (XmlReader xr = XmlReader.Create(stream))
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

            memento.Folders = _folders;
            memento.LastFolder = this.LastFolder;
            memento.LimitSize = this.LimitSize;
            memento.LimitSpan = this.LimitSpan;
            memento.IsKeepFolderStatus = IsKeepFolderStatus;
            memento.LastAddress = App.Current.IsOpenLastBook ? this.LastAddress : null;

            if (forSave)
            {
                memento.Items = Limit(this.Items.Where(e => !e.Place.StartsWith(Temporary.TempDirectory))).ToList();
                memento.Books = memento.Items.Select(e => e.Unit.Memento).ToList();

                // フォルダー保存制限
                if (!memento.IsKeepFolderStatus)
                {
                    memento.Folders = null;
                    memento.LastFolder = null;
                }
            }
            else
            {
                memento.Items = this.Items.ToList();
                memento.Books = memento.Items.Select(e => e.Unit.Memento).ToList();
            }

            return memento;
        }

        // memento適用
        public void Restore(Memento memento, bool fromLoad)
        {
            this.LastFolder = memento.LastFolder;
            this.LastAddress = memento.LastAddress;
            _folders = memento.Folders ?? _folders;
            this.LimitSize = memento.LimitSize;
            this.LimitSpan = memento.LimitSpan;
            IsKeepFolderStatus = memento.IsKeepFolderStatus;

            this.Load(fromLoad ? Limit(memento.Items) : memento.Items, memento.Books);

#pragma warning disable CS0612
            // ver.22
            if (memento.FolderOrders != null)
            {
                _folders = memento.FolderOrders.ToDictionary(e => e.Key, e => new FolderParameter.Memento() { FolderOrder = e.Value });
            }
#pragma warning restore CS0612

        }

        // 履歴数制限
        private IEnumerable<BookHistory> Limit(IEnumerable<BookHistory> source)
        {
            // limit size
            var collection = LimitSize == -1 ? source : source.Take(LimitSize);

            // limit time
            var limitTime = DateTime.Now - LimitSpan;
            collection = LimitSpan == default(TimeSpan) ? collection : collection.TakeWhile(e => e.LastAccessTime > limitTime);

            return collection;
        }


        #endregion
    }
}
