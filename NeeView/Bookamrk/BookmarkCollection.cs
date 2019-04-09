using NeeLaboratory.ComponentModel;
using NeeView.Collections;
using NeeView.Collections.Generic;
using System;
using System.Collections.Generic;
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
        static BookmarkCollection() => Current = new BookmarkCollection();
        public static BookmarkCollection Current { get; }


        // Constructors

        private BookmarkCollection()
        {
            Items = new TreeListNode<IBookmarkEntry>();
            Items.Value = new BookmarkFolder();
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

        public void RaiseBookmarkChangedEvent(BookmarkCollectionChangedEventArgs e)
        {
            BookmarkChanged?.Invoke(this, e);
        }


        public void Load(TreeListNode<IBookmarkEntry> nodes, IEnumerable<Book.Memento> books)
        {
            foreach (var book in books)
            {
                BookMementoCollection.Current.Set(book);
            }

            Items = nodes;
            Items.Value = new BookmarkFolder();

            BookmarkChanged?.Invoke(this, new BookmarkCollectionChangedEventArgs(EntryCollectionChangedAction.Reset));
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


        public TreeListNode<IBookmarkEntry> FindNode(string path)
        {
            if (path == null) return null;

            return FindNode(new QueryPath(path));
        }

        public TreeListNode<IBookmarkEntry> FindNode(QueryPath path)
        {
            if (path == null)
            {
                return null;
            }

            if (path.Scheme == QueryScheme.Bookmark)
            {
                if (path.Path == null)
                {
                    return Items;
                }
                return FindNode(Items, path.Path.Split(LoosePath.Separator));
            }
            else if (path.Scheme == QueryScheme.Pagemark)
            {
                return Items.FirstOrDefault(e => e.Value is Bookmark bookmark && bookmark.Place == path.SimplePath);
            }
            else if (path.Scheme == QueryScheme.File)
            {
                return Items.FirstOrDefault(e => e.Value is Bookmark bookmark && bookmark.Place == path.SimplePath);
            }
            else
            {
                return null;
            }
        }

        private TreeListNode<IBookmarkEntry> FindNode(TreeListNode<IBookmarkEntry> node, IEnumerable<string> pathTokens)
        {
            if (pathTokens == null)
            {
                return null;
            }

            if (!pathTokens.Any())
            {
                return node;
            }

            var name = pathTokens.First();
            var child = node.Children.FirstOrDefault(e => e.Value.Name == name);
            if (child != null)
            {
                return FindNode(child, pathTokens.Skip(1));
            }

            return null;
        }


        public bool Contains(string place)
        {
            if (place == null) return false;

            return Find(place) != null;
        }

        public bool Contains(TreeListNode<IBookmarkEntry> node)
        {
            return Items == node.Root;
        }

        public void AddFirst(TreeListNode<IBookmarkEntry> node)
        {
            if (node == null) throw new ArgumentNullException(nameof(node));

            node.UpdateEntryTime();
            Items.Root.Insert(0, node);
            BookmarkChanged?.Invoke(this, new BookmarkCollectionChangedEventArgs(EntryCollectionChangedAction.Add, node.Parent, node));
        }

        // TODO: 重複チェックをここで行う
        public void AddToChild(TreeListNode<IBookmarkEntry> node, TreeListNode<IBookmarkEntry> parent)
        {
            if (node == null) throw new ArgumentNullException(nameof(node));

            parent = parent ?? Items.Root;

            node.UpdateEntryTime();
            parent.Add(node);
            BookmarkChanged?.Invoke(this, new BookmarkCollectionChangedEventArgs(EntryCollectionChangedAction.Add, node.Parent, node));
        }


        public void Restore(TreeListNodeMemento<IBookmarkEntry> memento)
        {
            if (memento == null) throw new ArgumentNullException(nameof(memento));

            if (!Contains(memento.Parent))
            {
                return;
            }

            var index = memento.Index > memento.Parent.Children.Count ? memento.Parent.Children.Count : memento.Index;

            memento.Parent.Insert(index, memento.Node);
            BookmarkChanged?.Invoke(this, new BookmarkCollectionChangedEventArgs(EntryCollectionChangedAction.Add, memento.Node.Parent, memento.Node));
        }


        public bool Remove(TreeListNode<IBookmarkEntry> node)
        {
            if (node == null) return false;
            if (node.Root != Items.Root) throw new InvalidOperationException();

            var parent = node.Parent;
            if (node.RemoveSelf())
            {
                BookmarkChanged?.Invoke(this, new BookmarkCollectionChangedEventArgs(EntryCollectionChangedAction.Remove, parent, node));
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
            // 削除項目収集
            var unlinked = new List<TreeListNode<IBookmarkEntry>>();
            foreach (var node in Items.Where(e => e.Value is Bookmark))
            {
                var bookmark = (Bookmark)node.Value;
                if (!await ArchiveEntryUtility.ExistsAsync(bookmark.Place, token))
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

                BookmarkChanged?.Invoke(this, new BookmarkCollectionChangedEventArgs(EntryCollectionChangedAction.Replace));
            }
        }


        public TreeListNode<IBookmarkEntry> AddNewFolder(TreeListNode<IBookmarkEntry> target)
        {
            if (target == Items || target.Value is BookmarkFolder)
            {
                var ignoreNames = target.Children.Where(e => e.Value is BookmarkFolder).Select(e => e.Value.Name);
                var name = GetValidateFolderName(ignoreNames, Properties.Resources.WordNewFolder, Properties.Resources.WordNewFolder);
                var node = new TreeListNode<IBookmarkEntry>(new BookmarkFolder() { Name = name });

                target.Add(node);
                target.IsExpanded = true;
                BookmarkChanged?.Invoke(this, new BookmarkCollectionChangedEventArgs(EntryCollectionChangedAction.Add, node.Parent, node));

                return node;
            }

            return null;
        }


        public void Move(TreeListNode<IBookmarkEntry> item, TreeListNode<IBookmarkEntry> target, int direction)
        {
            if (item.Value is BookmarkFolder && item.Parent != target.Parent)
            {
                var conflict = target.Parent.Children.FirstOrDefault(e => e.Value is BookmarkFolder && e.Value.Name == item.Value.Name);
                if (conflict != null)
                {
                    Merge(item, conflict);
                    return;
                }
            }

            MoveInner(item, target, direction);
        }

        private void MoveInner(TreeListNode<IBookmarkEntry> item, TreeListNode<IBookmarkEntry> target, int direction)
        {
            if (item == target) return;
            if (target.ParentContains(item)) return; // TODO: 例外にすべき？

            bool isChangeDirectory = item.Parent != target.Parent;

            var parent = item.Parent;
            var oldIndex = parent.Children.IndexOf(item);
            item.RemoveSelf();
            if (isChangeDirectory)
            {
                BookmarkChanged?.Invoke(this, new BookmarkCollectionChangedEventArgs(EntryCollectionChangedAction.Remove, parent, item));
            }

            item.UpdateEntryTime();
            target.Parent.Insert(target, direction, item);
            if (isChangeDirectory)
            {
                BookmarkChanged?.Invoke(this, new BookmarkCollectionChangedEventArgs(EntryCollectionChangedAction.Add, item.Parent, item));
            }
            else
            {
                BookmarkChanged?.Invoke(this, new BookmarkCollectionChangedEventArgs(EntryCollectionChangedAction.Move, item.Parent, item) { OldIndex = oldIndex });
            }
        }

        public void MoveToChild(TreeListNode<IBookmarkEntry> item, TreeListNode<IBookmarkEntry> target)
        {
            if (target != Items && !(target.Value is BookmarkFolder))
            {
                return;
            }
            if (item.Parent == target)
            {
                return;
            }

            if (item.Value is BookmarkFolder folder)
            {
                if (target.ParentContains(item))
                {
                    return;
                }

                var conflict = target.Children.FirstOrDefault(e => folder.IsEqual(e.Value));
                if (conflict != null)
                {
                    Merge(item, conflict);
                }
                else
                {
                    MoveToChildInner(item, target);
                }
            }

            else if (item.Value is Bookmark bookmark)
            {
                var conflict = target.Children.FirstOrDefault(e => bookmark.IsEqual(e.Value));
                if (conflict != null)
                {
                    Remove(item);
                }
                else
                {
                    MoveToChildInner(item, target);
                }
            }
        }

        private void MoveToChildInner(TreeListNode<IBookmarkEntry> item, TreeListNode<IBookmarkEntry> target)
        {
            if (item == target) return;
            if (target.ParentContains(item)) return; // TODO: 例外にすべき？

            var parent = item.Parent;
            item.RemoveSelf();
            BookmarkChanged?.Invoke(this, new BookmarkCollectionChangedEventArgs(EntryCollectionChangedAction.Remove, parent, item));

            item.UpdateEntryTime();
            target.Insert(0, item);
            target.IsExpanded = true;
            BookmarkChanged?.Invoke(this, new BookmarkCollectionChangedEventArgs(EntryCollectionChangedAction.Add, item.Parent, item));
        }


        public void Merge(TreeListNode<IBookmarkEntry> item, TreeListNode<IBookmarkEntry> target)
        {
            if (!(item.Value is BookmarkFolder && target.Value is BookmarkFolder)) throw new ArgumentException();

            var parent = item.Parent;
            if (item.RemoveSelf())
            {
                BookmarkChanged?.Invoke(this, new BookmarkCollectionChangedEventArgs(EntryCollectionChangedAction.Remove, parent, item));
            }

            foreach (var child in item.Children.ToList())
            {
                child.RemoveSelf();
                if (child.Value is BookmarkFolder folder)
                {
                    var conflict = target.Children.FirstOrDefault(e => folder.IsEqual(e.Value));
                    if (conflict != null)
                    {
                        Merge(child, conflict);
                        continue;
                    }
                }
                else if (child.Value is Bookmark bookmark)
                {
                    var conflict = target.Children.FirstOrDefault(e => bookmark.IsEqual(e.Value));
                    if (conflict != null)
                    {
                        continue;
                    }
                }

                target.Add(child);
                BookmarkChanged?.Invoke(this, new BookmarkCollectionChangedEventArgs(EntryCollectionChangedAction.Add, target, child));
            }
        }

        public void Rename(string src, string dst)
        {
            foreach (var item in Items)
            {
                if (item.Value is Bookmark bookmark && bookmark.Place == src)
                {
                    bookmark.Place = dst;
                    BookmarkChanged?.Invoke(this, new BookmarkCollectionChangedEventArgs(EntryCollectionChangedAction.Rename, item.Parent, item));
                }
            }
        }


        public string GetValidateFolderName(IEnumerable<string> names, string name, string defaultName)
        {
            name = BookmarkFolder.GetValidateName(name);
            if (string.IsNullOrWhiteSpace(name))
            {
                name = defaultName;
            }
            if (names.Contains(name))
            {
                int count = 1;
                string newName = name;
                do
                {
                    newName = $"{name} ({++count})";
                }
                while (names.Contains(newName));
                name = newName;
            }

            return name;
        }


        private void ValidateFolderName(TreeListNode<IBookmarkEntry> node)
        {
            var names = new List<string>();

            foreach (var child in node.Children.Where(e => e.Value is BookmarkFolder))
            {
                ValidateFolderName(child);

                var folder = ((BookmarkFolder)child.Value);

                var name = BookmarkFolder.GetValidateName(folder.Name);
                if (string.IsNullOrWhiteSpace(name))
                {
                    name = "_";
                }
                if (names.Contains(name))
                {
                    int count = 1;
                    string newName = name;
                    do
                    {
                        newName = $"{name} ({++count})";
                    }
                    while (names.Contains(newName));
                    name = newName;
                }
                names.Add(name);

                folder.Name = name;
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

                if (_Version < Config.GenerateProductVersionNumber(33, 0, 0))
                {
                    foreach (var book in Books)
                    {
                        try
                        {
                            if (book.Place != null)
                            {
                                book.IsDirectorty = Directory.Exists(book.Place);
                            }
                        }
                        catch (Exception ex)
                        {
                            Debug.WriteLine(ex.Message);
                        }
                    }
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
        public Memento CreateMemento()
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

            if (memento._Version < Config.GenerateProductVersionNumber(32, 0, 0))
            {
                ValidateFolderName(memento.Nodes);
            }

            this.Load(memento.Nodes, memento.Books);
        }

        #endregion
    }

    public static class TreeListNodeExtensions
    {
        public static QueryPath CreateQuery<T>(this TreeListNode<T> node, QueryScheme scheme)
            where T : IHasName
        {
            var path = string.Join("\\", node.Hierarchy.Select(e => e.Value).Skip(1).OfType<T>().Select(e => e.Name));
            return new QueryPath(scheme, path, null);
        }

        /// <summary>
        /// Bookmark用パス等価判定
        /// </summary>
        public static bool IsEqual(this TreeListNode<IBookmarkEntry> node, QueryPath path)
        {
            if (node is null || path is null)
            {
                return false;
            }

            if (path.Scheme == QueryScheme.Bookmark)
            {
                return node.CreateQuery(QueryScheme.Bookmark) == path;
            }
            else if (path.Scheme == QueryScheme.File)
            {
                if (node.Value is Bookmark bookmark)
                {
                    return bookmark.Place == path.SimplePath;
                }
            }

            return false;
        }
    }

    /// <summary>
    /// TreeListNode&lt;IBookmarkEntry&rt; 拡張関数
    /// </summary>
    public static class BookmarkTreeListNodeExtensions
    {
        /// <summary>
        /// 登録日を現在日時で更新
        /// </summary>
        public static void UpdateEntryTime(this TreeListNode<IBookmarkEntry> node)
        {
            if (node.Value is Bookmark bookmark)
            {
                bookmark.EntryTime = DateTime.Now;
            }
        }

        /// <summary>
        /// Query生成
        /// </summary>
        public static QueryPath CreateQuery(this TreeListNode<IBookmarkEntry> node)
        {
            return node.CreateQuery(QueryScheme.Bookmark);
        }
    }
}
