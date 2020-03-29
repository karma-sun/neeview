using NeeLaboratory.ComponentModel;
using NeeView.Collections.Generic;
using NeeView.Windows.Property;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.Serialization;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading;
using System.Threading.Tasks;
using System.Xml;

namespace NeeView
{
    public class BookHistoryCollection : BindableBase
    {
        static BookHistoryCollection() => Current = new BookHistoryCollection();
        public static BookHistoryCollection Current { get; }

        #region Fields

        private Dictionary<string, FolderParameter.Memento> _folders = new Dictionary<string, FolderParameter.Memento>();

        #endregion

        #region Constructors

        private BookHistoryCollection()
        {
            HistoryChanged += (s, e) => RaisePropertyChanged(nameof(Count));
        }

        #endregion

        #region Events

        public event EventHandler<BookMementoCollectionChangedArgs> HistoryChanged;

        #endregion

        #region Prperties

        // 履歴コレクション
        public LinkedDicionary<string, BookHistory> Items { get; set; } = new LinkedDicionary<string, BookHistory>();

#if false
        // 履歴制限
        [PropertyMember("@ParamHistoryLimitSize")]
        public int LimitSize { get; set; }

        // 履歴制限(時間)
        [PropertyMember("@ParamHistoryLimitSpan")]
        public TimeSpan LimitSpan { get; set; }

        // 最後に開いたフォルダーの場所記憶
        [PropertyMember("@ParamHistoryIsKeepLastFolder", Tips = "@ParamHistoryIsKeepLastFolderTips")]
        public bool IsKeepLastFolder { get; set; }

        // フォルダーリストの情報記憶
        [PropertyMember("@ParamHistoryIsKeepFolderStatus")]
        public bool IsKeepFolderStatus { get; set; } = true;

        // 検索履歴の情報記憶
        [PropertyMember("@ParamHistoryIsKeepSearchHistory")]
        public bool IsKeepSearchHistory { get; set; } = true;
#endif

        // 要素数
        public int Count => Items.Count;

        // 先頭の要素
        public LinkedListNode<BookHistory> First => Items.First;

        #endregion

        #region Poperties for Folders

#if false
        // フォルダーリストで開いていた場所
        public string LastFolder { get; set; }

        // 最後に開いたフォルダー
        public string LastAddress { get; set; }
#endif

        // 検索履歴
        private ObservableCollection<string> _searchHistory = new ObservableCollection<string>();
        public ObservableCollection<string> SearchHistory
        {
            get { return _searchHistory; }
            set { SetProperty(ref _searchHistory, value); }
        }

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

        /// <summary>
        /// 検索履歴登録
        /// </summary>
        public void AddSearchHistory(string keyword)
        {
            if (string.IsNullOrWhiteSpace(keyword)) return;

            if (SearchHistory.Count <= 0)
            {
                SearchHistory.Add(keyword);
            }
            else if (SearchHistory.First() != keyword)
            {
                int index = SearchHistory.IndexOf(keyword);
                if (index > 0)
                {
                    SearchHistory.Move(index, 0);
                }
                else
                {
                    SearchHistory.Insert(0, keyword);
                }
            }

            while (SearchHistory.Count > 6)
            {
                SearchHistory.RemoveAt(this.SearchHistory.Count - 1);
            }
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
            Items.Clear();
            BookMementoCollection.Current.CleanUp();

            foreach (var book in books)
            {
                BookMementoCollection.Current.Set(book);
            }

            foreach (var item in items)
            {
                try
                {
                    var newItem = new BookHistory() { Path = item.Path, LastAccessTime = item.LastAccessTime };
                    Items.AddLastRaw(newItem.Path, newItem);
                }
                catch (Exception ex)
                {
                    Debug.WriteLine(ex.Message);
                }
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
                var node = FindNode(memento.Path);
                if (node != null && isKeepOrder)
                {
                    node.Value.Unit.Memento = memento;
                    HistoryChanged?.Invoke(this, new BookMementoCollectionChangedArgs(BookMementoCollectionChangedType.Update, memento.Path));
                }
                else
                {
                    node = node ?? new LinkedListNode<BookHistory>(new BookHistory(BookMementoCollection.Current.Set(memento), DateTime.Now));
                    node.Value.Unit.Memento = memento;
                    node.Value.LastAccessTime = DateTime.Now;

                    if (node == Items.First)
                    {
                        HistoryChanged?.Invoke(this, new BookMementoCollectionChangedArgs(BookMementoCollectionChangedType.Update, memento.Path));
                    }
                    else
                    {
                        Items.AddFirst(node.Value.Path, node.Value);
                        HistoryChanged?.Invoke(this, new BookMementoCollectionChangedArgs(BookMementoCollectionChangedType.Add, memento.Path));
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

        // まとめて履歴削除
        public void Remove(IEnumerable<string> places)
        {
            if (places == null) return;

            var unlinked = places.Where(e => FindNode(e) != null);

            if (unlinked.Any())
            {
                foreach (var place in unlinked)
                {
                    Debug.WriteLine($"HistoryRemove: {place}");
                    Items.Remove(place);
                }

                HistoryChanged?.Invoke(this, new BookMementoCollectionChangedArgs(BookMementoCollectionChangedType.Remove, null));
            }
        }

        // 無効な履歴削除
        public async Task RemoveUnlinkedAsync(CancellationToken token)
        {
            // 削除項目収集
            var unlinked = new List<LinkedListNode<BookHistory>>();
            for (var node = this.Items.First; node != null; node = node.Next)
            {
                if (!await ArchiveEntryUtility.ExistsAsync(node.Value.Path, token))
                {
                    unlinked.Add(node);
                }
            }

            if (unlinked.Any())
            {
                foreach (var node in unlinked)
                {
                    Debug.WriteLine($"HistoryRemove: {node.Value.Path}");
                    Items.Remove(node.Value.Path);
                }

                HistoryChanged?.Invoke(this, new BookMementoCollectionChangedArgs(BookMementoCollectionChangedType.Remove, null));
            }
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


        public void Rename(string src, string dst)
        {
            var item = Items.Find(src);
            if (item != null)
            {
                Items.Remove(dst);
                Items.Remap(src, dst);
                item.Value.Path = dst;
                HistoryChanged?.Invoke(this, new BookMementoCollectionChangedArgs(BookMementoCollectionChangedType.Add, dst));
            }
        }

        #endregion


        #region Memento

        /// <summary>
        /// 履歴Memento
        /// </summary>
        [DataContract(Name = "BookHistory.Memento")]
        public class Memento : BindableBase, IMemento
        {
            [JsonPropertyName("Format")]
            public FormatVersion Format { get; set; }

            [JsonIgnore]
            [DataMember]
            public int _Version { get; set; }

            [DataMember(Name = "Histories")]
            public List<BookHistory> Items { get; set; }

            [DataMember]
            public List<Book.Memento> Books { get; set; }

            [JsonIgnore]
            [DataMember(Order = 8)]
            public string LastFolder { get; set; }

            [JsonIgnore]
            [DataMember(Order = 12)]
            public int LimitSize { get; set; }

            [JsonIgnore]
            [DataMember(Order = 12)]
            public TimeSpan LimitSpan { get; set; }

            [JsonIgnore]
            [DataMember(Order = 19)]
            public bool IsKeepFolderStatus { get; set; }

            [JsonIgnore]
            [DataMember]
            public bool IsKeepLastFolder { get; set; }

            [DataMember]
            public Dictionary<string, FolderParameter.Memento> Folders { get; set; }

            [JsonIgnore]
            [DataMember]
            public string LastAddress { get; set; }

            [JsonIgnore]
            [DataMember]
            public bool IsKeepSearchHistory { get; set; }

            [DataMember(EmitDefaultValue = false)]
            public List<string> SearchHistory { get; set; }

            // no used
            [JsonIgnore]
            [Obsolete, DataMember(Order = 8, EmitDefaultValue = false)]
            public Dictionary<string, FolderOrder> FolderOrders { get; set; } // no used (ver.22)

            [JsonIgnore]
            [Obsolete, DataMember(Name = "History", EmitDefaultValue = false)]
            public List<Book.Memento> OldBooks { get; set; } // no used (ver.31)

            //
            private void Constructor()
            {
                Items = new List<BookHistory>();
                Books = new List<Book.Memento>();
                LimitSize = -1;
                IsKeepFolderStatus = true;
                IsKeepLastFolder = false;
                IsKeepSearchHistory = true;
            }

            public Memento()
            {
                Constructor();
            }

            [OnDeserializing]
            private void OnDeserializing(StreamingContext c)
            {
                Constructor();
            }

            [OnDeserialized]
            private void OnDeserialized(StreamingContext c)
            {
#pragma warning disable CS0612
                // before v31
                if (_Version < Environment.GenerateProductVersionNumber(31, 0, 0))
                {
                    Items = OldBooks != null
                        ? OldBooks.Select(e => new BookHistory() { Path = e.Path, LastAccessTime = e.LastAccessTime }).ToList()
                        : new List<BookHistory>();

                    Books = OldBooks ?? new List<Book.Memento>();
                    foreach (var book in Books)
                    {
                        book.LastAccessTime = default(DateTime);
                    }

                    OldBooks = null;
                }

                // before 36
                if (_Version < Environment.GenerateProductVersionNumber(36, 0, 0))
                {
                    IsKeepLastFolder = IsKeepFolderStatus;
                }
#pragma warning restore CS0612
            }


            public void Save(string path)
            {
                Format = new FormatVersion(Environment.SolutionName + ".History");

                var json = JsonSerializer.SerializeToUtf8Bytes(this, UserSettingTools.GetSerializerOptions());
                File.WriteAllBytes(path, json);
            }

            public static Memento Load(string path)
            {
                var json = File.ReadAllBytes(path);
                return Load(new ReadOnlySpan<byte>(json));
            }

            public static Memento Load(Stream stream)
            {
                using (var ms = new MemoryStream())
                {
                    stream.CopyTo(ms);
                    return Load(new ReadOnlySpan<byte>(ms.ToArray()));
                }
            }

            public static Memento Load(ReadOnlySpan<byte> json)
            {
                return JsonSerializer.Deserialize<Memento>(json, UserSettingTools.GetSerializerOptions());

                // TODO: v.38以後の互換性処理をここで？
            }

            #region Legacy

            // ファイルに保存
            [Obsolete]
            public void SaveV1(string path)
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
            public static Memento LoadV1(string path)
            {
                using (var stream = new FileStream(path, FileMode.Open, FileAccess.Read))
                {
                    return LoadV1(stream);
                }
            }

            // ストリームから読み込み
            public static Memento LoadV1(Stream stream)
            {
                using (XmlReader xr = XmlReader.Create(stream))
                {
                    DataContractSerializer serializer = new DataContractSerializer(typeof(Memento));
                    Memento memento = (Memento)serializer.ReadObject(xr);
                    return memento;
                }
            }

            #endregion Legacy

            // 合成
            public void Merge(Memento memento)
            {
                Debug.WriteLine("HistoryMerge...");

                if (_Version != memento._Version)
                {
                    Debug.WriteLine("HistoryMerge failed: Illigal version");
                    return;
                }

                bool isDarty = false;
                var itemMap = Items.ToDictionary(e => e.Path, e => e);
                var bookMap = Books.ToDictionary(e => e.Path, e => e);
                var importBookMap = memento.Books.ToDictionary(e => e.Path, e => e);

                foreach (var item in memento.Items)
                {
                    if (itemMap.ContainsKey(item.Path))
                    {
                        if (itemMap[item.Path].LastAccessTime < item.LastAccessTime)
                        {
                            Debug.WriteLine($"HistoryMerge: Update: {item.Path}");
                            itemMap[item.Path] = item;
                            bookMap[item.Path] = importBookMap[item.Path];
                            isDarty = true;
                        }
                    }
                    else
                    {
                        Debug.WriteLine($"HistoryMerge: Add: {item.Path}");
                        itemMap.Add(item.Path, item);
                        bookMap.Add(item.Path, importBookMap[item.Path]);
                        isDarty = true;
                    }
                }

                if (isDarty)
                {
                    Items = Limit(itemMap.Values.OrderByDescending(e => e.LastAccessTime), LimitSize, LimitSpan).ToList();
                    Books = bookMap.Values.ToList();
                }
            }

            public void RestoreConfig(Config config)
            {
                config.StartUp.IsOpenLastFolder = IsKeepLastFolder;
                config.StartUp.LastFolderPath = LastFolder;
                config.StartUp.LastBookPath = LastAddress;
                config.History.IsKeepFolderStatus = IsKeepFolderStatus;
                config.History.IsKeepSearchHistory = IsKeepSearchHistory;
                config.History.LimitSize = LimitSize;
                config.History.LimitSpan = LimitSpan;
            }
        }

        // memento作成
        public Memento CreateMemento()
        {
            var memento = new Memento();

#if false
            memento._Version = Environment.ProductVersionNumber;
            memento.LastFolder = Config.Current.StartUp.LastFolderPath;
            memento.LimitSize = Config.Current.History.LimitSize;
            memento.LimitSpan = Config.Current.History.LimitSpan;
            memento.IsKeepFolderStatus = Config.Current.History.IsKeepFolderStatus;
            memento.IsKeepLastFolder = Config.Current.StartUp.IsOpenLastFolder;
            memento.LastAddress = Config.Current.StartUp.LastBookPath;
            memento.IsKeepSearchHistory = Config.Current.History.IsKeepSearchHistory;  
#endif

            memento.Items = Limit(this.Items.Where(e => !e.Path.StartsWith(Temporary.Current.TempDirectory)), Config.Current.History.LimitSize, Config.Current.History.LimitSpan).ToList();
            memento.Books = memento.Items.Select(e => e.Unit.Memento).ToList();

            if (Config.Current.History.IsKeepFolderStatus)
            {
                memento.Folders = _folders;
            }

            if (Config.Current.History.IsKeepSearchHistory)
            {
                memento.SearchHistory = this.SearchHistory.Any() ? this.SearchHistory.ToList() : null;
            }

            return memento;
        }

        // memento適用
        public void Restore(Memento memento, bool fromLoad)
        {
            if (memento == null) return;

            _folders = memento.Folders ?? _folders;

            if (memento.IsKeepSearchHistory)
            {
                this.SearchHistory = memento.SearchHistory != null ? new ObservableCollection<string>(memento.SearchHistory) : new ObservableCollection<string>();
            }

            this.Load(fromLoad ? Limit(memento.Items, memento.LimitSize, memento.LimitSpan) : memento.Items, memento.Books);
        }


        // 履歴数制限
        public static IEnumerable<BookHistory> Limit(IEnumerable<BookHistory> source, int limitSize, TimeSpan limitSpan)
        {
            // limit size
            var collection = limitSize == -1 ? source : source.Take(limitSize);

            // limit time
            var limitTime = DateTime.Now - limitSpan;
            collection = limitSpan == default ? collection : collection.TakeWhile(e => e.LastAccessTime > limitTime);

            return collection;
        }


#endregion
    }
}
