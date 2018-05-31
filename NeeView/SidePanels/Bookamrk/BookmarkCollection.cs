using NeeLaboratory.ComponentModel;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Data;
using System.Xml;

namespace NeeView
{
    public class BookmarkCollection : BindableBase
    {
        public static BookmarkCollection Current { get; private set; }


        #region Constructors

        public BookmarkCollection()
        {
            Current = this;
            Items = new ObservableCollection<Bookmark>();
        }

        #endregion

        #region Events

        public event EventHandler<BookMementoCollectionChangedArgs> BookmarkChanged;

        #endregion

        #region Properties

        private ObservableCollection<Bookmark> _items;
        public ObservableCollection<Bookmark> Items
        {
            get { return _items; }
            private set
            {
                _items = value;
                BindingOperations.EnableCollectionSynchronization(_items, new object());
                RaisePropertyChanged();
            }
        }

        private Bookmark _selectedItem;
        public Bookmark SelectedItem
        {
            get { return _selectedItem; }
            set { _selectedItem = value; RaisePropertyChanged(); }
        }

        #endregion


        #region Methods

        // クリア
        public void Clear()
        {
            Items.Clear();

            // TODO: GabageCollection

            BookmarkChanged?.Invoke(this, new BookMementoCollectionChangedArgs(BookMementoCollectionChangedType.Clear, null));
        }


        // 設定
        public void Load(IEnumerable<Bookmark> items, IEnumerable<Book.Memento> books)
        {
            Clear();

            foreach (var book in books)
            {
                BookMementoCollection.Current.Set(book);
            }

            foreach (var item in items)
            {
                Items.Add(new Bookmark() { Place = item.Place });
            }

            BookmarkChanged?.Invoke(this, new BookMementoCollectionChangedArgs(BookMementoCollectionChangedType.Load, null));
        }


        // 追加
        public BookMementoUnit Add(Book.Memento memento)
        {
            if (memento == null) return null;

            try
            {
                var unit = BookMementoCollection.Current.Set(memento);

                if (Contains(memento.Place))
                {
                    BookmarkChanged?.Invoke(this, new BookMementoCollectionChangedArgs(BookMementoCollectionChangedType.Update, memento.Place));
                }
                else
                {
                    Items.Insert(0, new Bookmark(unit));
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

        // ブックマーク？
        public bool Contains(string place)
        {
            if (place == null) return false;

            return Items.Any(e => e.Place == place);
        }

        // ブックマーク状態切り替え
        public bool Toggle(Book.Memento memento)
        {
            if (memento == null) return false;

            var node = Find(memento.Place);
            if (node == null)
            {
                var unit = Add(memento);
                return true;
            }
            else
            {
                Remove(node.Place);
                return false;
            }
        }


        // 削除
        public void Remove(string place)
        {
            if (Items.Remove(Find(place)))
            {
                BookmarkChanged?.Invoke(this, new BookMementoCollectionChangedArgs(BookMementoCollectionChangedType.Remove, place));
            }
        }

        // 無効なブックマークを削除
        public async Task RemoveUnlinkedAsync(CancellationToken token)
        {
            // 削除項目収集
            var unlinked = new List<Bookmark>();
            foreach (var item in this.Items)
            {
                if (!(await ArchiveFileSystem.ExistsAsync(item.Place, token)))
                {
                    unlinked.Add(item);
                }
            }

            // 削除実行
            foreach (var node in unlinked)
            {
                Debug.WriteLine($"BookmarkRemove: {node.Place}");
                Items.Remove(node);
            }
            BookmarkChanged?.Invoke(this, new BookMementoCollectionChangedArgs(BookMementoCollectionChangedType.Remove, null));
        }

        public Bookmark Find(string place)
        {
            if (place == null) return null;

            return Items.FirstOrDefault(e => e.Place == place);
        }


        public BookMementoUnit FindUnit(string place)
        {
            if (place == null) return null;

            return Find(place)?.Unit;
        }


        // となりを取得
        public Bookmark GetNeighbor(Bookmark item)
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
        public Bookmark MoveSelected(int direction)
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

        #endregion

        #region Memento

        /// <summary>
        /// 履歴Memento
        /// </summary>
        [DataContract]
        public class Memento
        {
            [DataMember]
            public int _Version { get; set; } = Config.Current.ProductVersionNumber;

            [DataMember]
            public List<Bookmark> Marks { get; set; }

            [DataMember(EmitDefaultValue = false)]
            public List<Book.Memento> Books { get; set; }

            [Obsolete]
            [DataMember(Name = "Items", EmitDefaultValue = false)]
            public List<Book.Memento> OldBooks { get; set; }

            private void Constructor()
            {
                Marks = new List<Bookmark>();
                Books = new List<Book.Memento>();
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
#pragma warning disable CS0612
                if (_Version < Config.GenerateProductVersionNumber(31, 0, 0))
                {
                    Marks = OldBooks != null
                        ? OldBooks.Select(e => new Bookmark() { Place = e.Place }).ToList()
                        : new List<Bookmark>();

                    Books = OldBooks ?? new List<Book.Memento>();
                    foreach (var book in Books)
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

            if (forSave)
            {
                // without temp folder
                memento.Marks = this.Items.Where(e => !e.Place.StartsWith(Temporary.TempDirectory)).ToList();
            }
            else
            {
                memento.Marks = this.Items.ToList();
            }

            memento.Books = memento.Marks.Select(e => e.Unit.Memento).Distinct().ToList();

            return memento;
        }

        // memento適用
        public void Restore(Memento memento)
        {
            this.Load(memento.Marks, memento.Books);
        }

        #endregion
    }
}
