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

        public void RaiseBookmarkChangedEvent(BookmarkCollectionChangedEventArgs e)
        {
            BookmarkChanged?.Invoke(this, e);
        }

        public void Clear()
        {
            Items.Clear();
            BookMementoCollection.Current.CleanUp();

            BookmarkChanged?.Invoke(this, new BookmarkCollectionChangedEventArgs(EntryCollectionChangedAction.Reset));
        }


        public void Load(TreeListNode<IBookmarkEntry> nodes, IEnumerable<Book.Memento> books)
        {
            foreach (var book in books)
            {
                BookMementoCollection.Current.Set(book);
            }

            Items = nodes;

            BookmarkChanged?.Invoke(this, new BookmarkCollectionChangedEventArgs(EntryCollectionChangedAction.Replace));
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

            var scheme = QueryScheme.Bookmark.ToSchemeString();

            if (path.StartsWith(scheme))
            {
                path = path.Substring(scheme.Length).Trim(LoosePath.Separator);

                if (path == "")
                {
                    return Items;
                }

                return FindNode(Items, path.Split(LoosePath.Separator));
            }
            else
            {
                return Items.FirstOrDefault(e => e.Value is Bookmark bookmark && bookmark.Place == path);
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


        public static string CreatePath(TreeListNode<IBookmarkEntry> node)
        {
            return QueryScheme.Bookmark.ToSchemeString() + "\\" + string.Join("\\", node.Hierarchy.Select(e => e.Value).Cast<IBookmarkEntry>());
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
            BookmarkChanged?.Invoke(this, new BookmarkCollectionChangedEventArgs(EntryCollectionChangedAction.Add, node.Parent, node));
        }

        // TODO: 重複チェックをここで行う
        public void AddToChild(TreeListNode<IBookmarkEntry> node, TreeListNode<IBookmarkEntry> parent)
        {
            if (node == null) throw new ArgumentNullException(nameof(node));

            parent = parent ?? Items.Root;

            parent.Add(node);
            BookmarkChanged?.Invoke(this, new BookmarkCollectionChangedEventArgs(EntryCollectionChangedAction.Add, node.Parent, node));
        }


        public void Restore(TreeListNodeMemento<IBookmarkEntry> memento)
        {
            if (memento == null) throw new ArgumentNullException(nameof(memento));

            memento.Parent.Insert(memento.Index, memento.Node);
            BookmarkChanged?.Invoke(this, new BookmarkCollectionChangedEventArgs(EntryCollectionChangedAction.Add, memento.Node.Parent, memento.Node));
        }


        public bool Remove(TreeListNode<IBookmarkEntry> node)
        {
            if (node == null) throw new ArgumentNullException(nameof(node));
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
                    var conflict = target.Children.FirstOrDefault(e =>bookmark.IsEqual(e.Value));
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
        public static string CreatePath<T>(this TreeListNode<T> node, string scheme)
            where T : IHasName
        {
            var path = string.Join("\\", node.Hierarchy.Select(e => e.Value).OfType<T>().Select(e => e.Name));
            if (scheme != null)
            {
                if (string.IsNullOrEmpty(path))
                {
                    path = scheme + "\\";
                }
                else
                {
                    path = LoosePath.Combine(scheme, path);
                }
            }
            return path;
        }
    }
}
