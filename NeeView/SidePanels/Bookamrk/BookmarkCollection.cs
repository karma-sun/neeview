using NeeLaboratory.ComponentModel;
using NeeView.Collections.Generic;
using System;
using System.Collections.Generic;
using System.Collections.Specialized;
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


        // Constructors

        public BookmarkCollection()
        {
            Current = this;
            Items = new TreeListNode<IBookmarkEntry>();
        }


        // Events

        public event EventHandler<BookmarkCollectionChangedEventArgs> BookmarkChanged;


        // Properties

        private TreeListNode<IBookmarkEntry> _items;
        public TreeListNode<IBookmarkEntry> Items
        {
            get { return _items; }
            set { SetProperty(ref _items, value); }
        }


        // Methods

        public void Clear()
        {
            Items.Clear();
            BookMementoCollection.Current.CleanUp();

            BookmarkChanged?.Invoke(this, new BookmarkCollectionChangedEventArgs(NotifyCollectionChangedAction.Reset));
        }


        public void Load(TreeListNode<IBookmarkEntry> nodes, IEnumerable<Book.Memento> books)
        {
            foreach (var book in books)
            {
                BookMementoCollection.Current.Set(book);
            }

            Items = nodes;

            BookmarkChanged?.Invoke(this, new BookmarkCollectionChangedEventArgs(NotifyCollectionChangedAction.Replace));
        }


        public Bookmark Find(string place)
        {
            if (place == null) return null;

            return Items.Select(e => e.Value).OfType<Bookmark>().FirstOrDefault(e => e.Place == place);
        }


        public BookMementoUnit FindUnit(string place)
        {
            if (place == null) return null;

            return Find(place)?.Unit;
        }


        public TreeListNode<IBookmarkEntry> FindNode(IBookmarkEntry entry)
        {
            if (entry == null) return null;

            return Items.FirstOrDefault(e => e.Value == entry);
        }


        public bool Contains(string place)
        {
            if (place == null) return false;

            return Find(place) != null;
        }


        public void AddFirst(TreeListNode<IBookmarkEntry> node)
        {
            if (node == null) throw new ArgumentNullException(nameof(node));

            Items.Root.Insert(0, node);
            BookmarkChanged?.Invoke(this, new BookmarkCollectionChangedEventArgs(NotifyCollectionChangedAction.Add, node));
        }


        public void Restore(TreeListNodeMemento<IBookmarkEntry> memento)
        {
            if (memento == null) throw new ArgumentNullException(nameof(memento));

            memento.Parent.Insert(memento.Index, memento.Node);
            BookmarkChanged?.Invoke(this, new BookmarkCollectionChangedEventArgs(NotifyCollectionChangedAction.Add, memento.Node));
        }


        public bool Remove(TreeListNode<IBookmarkEntry> node)
        {
            if (node == null) throw new ArgumentNullException(nameof(node));
            if (node.Root != Items.Root) throw new InvalidOperationException();

            if (node.RemoveSelf())
            {
                BookmarkChanged?.Invoke(this, new BookmarkCollectionChangedEventArgs(NotifyCollectionChangedAction.Remove, node));
                return true;
            }
            else
            {
                return false;
            }
        }


        // 無効な履歴削除
        public async Task RemoveUnlinkedAsync(CancellationToken token)
        {
            await Task.Yield();

            // 削除項目収集
            var unlinked = new List<TreeListNode<IBookmarkEntry>>();
            foreach (var node in Items.Where(e => e.Value is Bookmark))
            {
                var bookmark = (Bookmark)node.Value;
                if (!(await ArchiveFileSystem.ExistsAsync(bookmark.Place, token)))
                {
                    unlinked.Add(node);
                }
            }

            // 削除実行
            if (unlinked.Count > 0)
            {
                foreach (var node in unlinked)
                {
                    var bookmark = (Bookmark)node.Value;
                    Debug.WriteLine($"BookmarkRemove: {bookmark.Place}");
                    node.RemoveSelf();
                }

                BookmarkChanged?.Invoke(this, new BookmarkCollectionChangedEventArgs(NotifyCollectionChangedAction.Replace));
            }
        }


        public void Move(TreeListNode<IBookmarkEntry> item, TreeListNode<IBookmarkEntry> target, int direction)
        {
            if (item == target) return;
            if (target.ParentContains(item)) return; // TODO: 例外にすべき？

            item.RemoveSelf();
            target.Parent.Insert(target, direction, item);

            BookmarkChanged?.Invoke(this, new BookmarkCollectionChangedEventArgs(NotifyCollectionChangedAction.Move, item));
        }


        public void MoveToChild(TreeListNode<IBookmarkEntry> item, TreeListNode<IBookmarkEntry> target)
        {
            if (item == target) return;
            if (target.ParentContains(item)) return; // TODO: 例外にすべき？

            item.RemoveSelf();
            target.Insert(0, item);
            target.IsExpanded = true;

            BookmarkChanged?.Invoke(this, new BookmarkCollectionChangedEventArgs(NotifyCollectionChangedAction.Move, item));
        }


        public void Rename(string src, string dst)
        {
            foreach (var item in Items)
            {
                if (item.Value is Bookmark bookmark && bookmark.Place == src)
                {
                    bookmark.Place = dst;
                }
            }
        }


        #region Memento

        [DataContract]
        [KnownType(typeof(Bookmark))]
        [KnownType(typeof(BookmarkFolder))]
        public class Memento
        {
            [DataMember]
            public int _Version { get; set; } = Config.Current.ProductVersionNumber;

            [DataMember]
            public TreeListNode<IBookmarkEntry> Nodes { get; set; }

            [DataMember(EmitDefaultValue = false)]
            public List<Book.Memento> Books { get; set; }

            [DataMember]
            public QuickAccessCollection.Memento QuickAccess { get; set; }

            [Obsolete]
            [DataMember(Name = "Items", EmitDefaultValue = false)]
            public List<Book.Memento> OldBooks { get; set; } // no used (ver.31)

            private void Constructor()
            {
                Nodes = new TreeListNode<IBookmarkEntry>();
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
                    Nodes = new TreeListNode<IBookmarkEntry>();
                    foreach (var book in OldBooks)
                    {
                        Nodes.Add(new Bookmark() { Place = book.Place });
                    }

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
        // TODO: forSave parameter
        public Memento CreateMemento(bool forSave)
        {
            var memento = new Memento();
            memento.Nodes = Items;
            memento.Books = Items.Select(e => e.Value).OfType<Bookmark>().Select(e => e.Unit.Memento).Distinct().ToList();

            // QuickAccess情報もここに保存する
            memento.QuickAccess = QuickAccessCollection.Current.CreateMemento();

            return memento;
        }

        // memento適用
        public void Restore(Memento memento)
        {
            QuickAccessCollection.Current.Restore(memento.QuickAccess);

            this.Load(memento.Nodes, memento.Books);
        }

        #endregion
    }
}
