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
using NeeLaboratory.ComponentModel;
using System.IO;
using System.Threading;
using NeeView.Collections.Generic;
using System.Collections.Specialized;
using NeeView.Collections;

namespace NeeView
{
    /// <summary>
    /// HACK: ブックマークコレクションと処理を共通化させる
    /// </summary>
    public class PagemarkCollection : BindableBase
    {
        public static PagemarkCollection Current { get; private set; }


        // Constructors

        public PagemarkCollection()
        {
            Current = this;

            Items = CreateRoot();
        }



        // Events

        public event EventHandler<PagemarkCollectionChangedEventArgs> PagemarkChanged;


        // Properties

        private TreeListNode<IPagemarkEntry> _items;
        public TreeListNode<IPagemarkEntry> Items
        {
            get { return _items; }
            set { SetProperty(ref _items, value); }
        }

        public TreeListNode<IPagemarkEntry> DefaultFolder
        {
            get { return _items.Children.FirstOrDefault(e => e.Value is DefaultPagemarkFolder); }
        }


        // Methods

        public static TreeListNode<IPagemarkEntry> CreateRoot()
        {
            var items = new TreeListNode<IPagemarkEntry>();
            items.Value = new PagemarkFolder();
            items.Add(new TreeListNode<IPagemarkEntry>(new DefaultPagemarkFolder()));

            return items;
        }

        public void RaisePagemarkChangedEvent(PagemarkCollectionChangedEventArgs e)
        {
            PagemarkChanged?.Invoke(this, e);
        }

        public void Load(TreeListNode<IPagemarkEntry> nodes, IEnumerable<Book.Memento> books)
        {
            foreach (var book in books)
            {
                BookMementoCollection.Current.Set(book);
            }

            Items = nodes;

            PagemarkChanged?.Invoke(this, new PagemarkCollectionChangedEventArgs(EntryCollectionChangedAction.Replace));
        }


        public Pagemark Find(string place, string entryName)
        {
            if (place == null) return null;
            if (entryName == null) return null;

            return Items.Select(e => e.Value).OfType<Pagemark>().FirstOrDefault(e => e.Place == place && e.EntryName == entryName);
        }


        public BookMementoUnit FindUnit(string place)
        {
            if (place == null) return null;

            return Items.Select(e => e.Value).OfType<Pagemark>().FirstOrDefault(e => e.Place == place)?.Unit;
        }


        public TreeListNode<IPagemarkEntry> FindNode(string place, string entryName)
        {
            if (place == null) return null;
            if (entryName == null) return null;

            return Items.FirstOrDefault(e => e.Value is Pagemark pagemark && pagemark.Place == place && pagemark.EntryName == entryName);
        }


        public TreeListNode<IPagemarkEntry> FindNode(IPagemarkEntry entry)
        {
            if (entry == null) return null;

            return Items.FirstOrDefault(e => e.Value == entry);
        }

        public TreeListNode<IPagemarkEntry> FindNode(QueryPath path)
        {
            if (path == null)
            {
                return null;
            }

            if (path.Scheme == QueryScheme.Pagemark)
            {
                if (path.Path == null)
                {
                    return Items;
                }
                return FindNode(Items, path.Path.Split(LoosePath.Separator));
            }
            else if (path.Scheme == QueryScheme.File)
            {
                return Items.FirstOrDefault(e => e.Value is Pagemark pagemark && pagemark.Place == path.Path);
            }
            else
            {
                return null;
            }
        }


        private TreeListNode<IPagemarkEntry> FindNode(TreeListNode<IPagemarkEntry> node, IEnumerable<string> pathTokens)
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



        public bool Contains(string place, string entryName)
        {
            if (place == null) return false;

            return Find(place, entryName) != null;
        }


        public List<Pagemark> Collect(string place)
        {
            return Items.Select(e => e.Value).OfType<Pagemark>().Where(e => e.Place == place).ToList();
        }


        public void Add(TreeListNode<IPagemarkEntry> node)
        {
            if (node == null) throw new ArgumentNullException(nameof(node));
            if (node.Root == null) throw new InvalidOperationException();

            ////ItemsRoot.Insert(0, node);
            DefaultFolder.Add(node);

            PagemarkChanged?.Invoke(this, new PagemarkCollectionChangedEventArgs(EntryCollectionChangedAction.Add, node.Parent, node));
        }


        public void Restore(TreeListNodeMemento<IPagemarkEntry> memento)
        {
            if (memento == null) throw new ArgumentNullException(nameof(memento));

            memento.Parent.Insert(memento.Index, memento.Node);
            PagemarkChanged?.Invoke(this, new PagemarkCollectionChangedEventArgs(EntryCollectionChangedAction.Add, memento.Parent, memento.Node));
        }


        public bool Remove(TreeListNode<IPagemarkEntry> node)
        {
            if (node == null) throw new ArgumentNullException(nameof(node));
            if (node.Root != Items.Root) throw new InvalidOperationException();

            var parent = node.Parent;
            if (node.RemoveSelf())
            {
                PagemarkChanged?.Invoke(this, new PagemarkCollectionChangedEventArgs(EntryCollectionChangedAction.Remove, parent, node));
                return true;
            }
            else
            {
                return false;
            }
        }

        /// <summary>
        /// 無効なページマークを削除.
        /// 現在の実装ではブックの有無のみ判定
        /// </summary>
        public async Task RemoveUnlinkedAsync(CancellationToken token)
        {
            await Task.Yield();

            // 削除項目収集
            // TODO: 重複するBOOKを再検索しないようにする
            var unlinked = new List<TreeListNode<IPagemarkEntry>>();
            foreach (var node in this.Items.Where(e => e.Value is Pagemark))
            {
                var pagemark = (Pagemark)node.Value;
                if (!(await ArchiveFileSystem.ExistsAsync(pagemark.Place, token)))
                {
                    unlinked.Add(node);
                }
            }

            // 削除実行
            foreach (var node in unlinked)
            {
                var pagemark = (Pagemark)node.Value;
                Debug.WriteLine($"PagemarkRemove: {pagemark.Place} - {pagemark.EntryName}");
                node.RemoveSelf();
            }

            PagemarkChanged?.Invoke(this, new PagemarkCollectionChangedEventArgs(EntryCollectionChangedAction.Replace));
        }



        public TreeListNode<IPagemarkEntry> AddNewFolder(TreeListNode<IPagemarkEntry> target)
        {
            if (target == Items || target.Value is PagemarkFolder)
            {
                var ignoreNames = target.Children.Where(e => e.Value is PagemarkFolder).Select(e => e.Value.Name);
                var name = GetValidateFolderName(ignoreNames, Properties.Resources.WordNewFolder, Properties.Resources.WordNewFolder);
                var node = new TreeListNode<IPagemarkEntry>(new PagemarkFolder() { Name = name });

                target.Add(node);
                target.IsExpanded = true;
                PagemarkChanged?.Invoke(this, new PagemarkCollectionChangedEventArgs(EntryCollectionChangedAction.Add, node.Parent, node));

                return node;
            }

            return null;
        }

        // NOTE: 未使用
        public void Move(TreeListNode<IPagemarkEntry> item, TreeListNode<IPagemarkEntry> target, int direction)
        {
            if (item.Value is PagemarkFolder && item.Parent != target.Parent)
            {
                var conflict = target.Parent.Children.FirstOrDefault(e => e.Value is PagemarkFolder && e.Value.Name == item.Value.Name);
                if (conflict != null)
                {
                    Merge(item, conflict);
                    return;
                }
            }

            MoveInner(item, target, direction);
        }

        // NOTE: 未使用
        private void MoveInner(TreeListNode<IPagemarkEntry> item, TreeListNode<IPagemarkEntry> target, int direction)
        {
            if (item == target) return;
            if (target.ParentContains(item)) return; // TODO: 例外にすべき？

            bool isChangeDirectory = item.Parent != target.Parent;

            var parent = item.Parent;
            var oldIndex = parent.Children.IndexOf(item);
            item.RemoveSelf();
            if (isChangeDirectory)
            {
                PagemarkChanged?.Invoke(this, new PagemarkCollectionChangedEventArgs(EntryCollectionChangedAction.Remove, parent, item));
            }

            target.Parent.Insert(target, direction, item);
            if (isChangeDirectory)
            {
                PagemarkChanged?.Invoke(this, new PagemarkCollectionChangedEventArgs(EntryCollectionChangedAction.Add, item.Parent, item));
            }
            else
            {
                PagemarkChanged?.Invoke(this, new PagemarkCollectionChangedEventArgs(EntryCollectionChangedAction.Move, item.Parent, item) { OldIndex = oldIndex });
            }
        }


        public void MoveToChild(TreeListNode<IPagemarkEntry> item, TreeListNode<IPagemarkEntry> target)
        {
            if (!(target.Value is PagemarkFolder))
            {
                return;
            }
            if (item.Parent == target)
            {
                return;
            }

            if (item.Value is PagemarkFolder folder)
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

            else if (item.Value is Pagemark pagemark)
            {
                var conflict = target.Children.FirstOrDefault(e => pagemark.IsEqual(e.Value));
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

        private void MoveToChildInner(TreeListNode<IPagemarkEntry> item, TreeListNode<IPagemarkEntry> target)
        {
            if (item == target) return;
            if (target.ParentContains(item)) return; // TODO: 例外にすべき？

            var parent = item.Parent;
            item.RemoveSelf();
            PagemarkChanged?.Invoke(this, new PagemarkCollectionChangedEventArgs(EntryCollectionChangedAction.Remove, parent, item));

            target.Insert(0, item);
            target.IsExpanded = true;
            PagemarkChanged?.Invoke(this, new PagemarkCollectionChangedEventArgs(EntryCollectionChangedAction.Add, item.Parent, item));
        }

        public void Merge(TreeListNode<IPagemarkEntry> item, TreeListNode<IPagemarkEntry> target)
        {
            if (!(item.Value is PagemarkFolder && target.Value is PagemarkFolder)) throw new ArgumentException();

            var parent = item.Parent;
            if (item.RemoveSelf())
            {
                PagemarkChanged?.Invoke(this, new PagemarkCollectionChangedEventArgs(EntryCollectionChangedAction.Remove, parent, item));
            }

            foreach (var child in item.Children.ToList())
            {
                child.RemoveSelf();
                if (child.Value is PagemarkFolder folder)
                {
                    var conflict = target.Children.FirstOrDefault(e => folder.IsEqual(e.Value));
                    if (conflict != null)
                    {
                        Merge(child, conflict);
                        continue;
                    }
                }
                else if (child.Value is Pagemark pagemark)
                {
                    var conflict = target.Children.FirstOrDefault(e => pagemark.IsEqual(e.Value));
                    if (conflict != null)
                    {
                        continue;
                    }
                }

                target.Add(child);
                PagemarkChanged?.Invoke(this, new PagemarkCollectionChangedEventArgs(EntryCollectionChangedAction.Add, target, child));
            }
        }



        public void Rename(string src, string dst)
        {
            foreach (var item in Items)
            {
                if (item.Value is Pagemark pagemark && pagemark.Place == src)
                {
                    pagemark.Place = dst;
                }
            }
        }



        public string GetValidateFolderName(IEnumerable<string> names, string name, string defaultName)
        {
            name = PagemarkFolder.GetValidateName(name);
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

        private static void ValidateFolderName(TreeListNode<IPagemarkEntry> node)
        {
            var names = new List<string>();

            foreach (var child in node.Children.Where(e => e.Value is PagemarkFolder))
            {
                ValidateFolderName(child);

                var folder = ((PagemarkFolder)child.Value);

                var name = PagemarkFolder.GetValidateName(folder.Name);
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

        private static void ValidateDefaultFolder(TreeListNode<IPagemarkEntry> items)
        {
            // 既定のページマークフォルダーにルートのページマークを集める
            var defaultFolder = items.Children.FirstOrDefault(e => e.Value is DefaultPagemarkFolder);
            if (defaultFolder == null)
            {
                defaultFolder = new TreeListNode<IPagemarkEntry>(new DefaultPagemarkFolder());
                items.Insert(0, defaultFolder);
            }
            foreach (var item in items.Children.Where(e => e != defaultFolder).ToList())
            {
                item.RemoveSelf();
                defaultFolder.Insert(0, item);
            }
        }

        private static async Task ValidateAsync(TreeListNode<IPagemarkEntry> items)
        {
            try
            {
                // 個別のページマーク情報更新
                foreach (var pagemark in items.Select(e => e.Value).OfType<Pagemark>())
                {
                    await pagemark.ValidateAsync();
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex.Message);
            }
        }



        #region Memento

        [DataContract]
        [KnownType(typeof(Pagemark))]
        [KnownType(typeof(PagemarkFolder))]
        [KnownType(typeof(DefaultPagemarkFolder))]
        public class Memento
        {
            [DataMember]
            public int _Version { get; set; } = Config.Current.ProductVersionNumber;

            [DataMember]
            public TreeListNode<IPagemarkEntry> Nodes { get; set; }

            [DataMember]
            public List<Book.Memento> Books { get; set; }

            [Obsolete, DataMember(EmitDefaultValue = false)]
            public List<Pagemark> Marks { get; set; }

            [Obsolete, DataMember(Name = "Items", EmitDefaultValue = false)]
            public List<Book.Memento> OldBooks { get; set; }


            private void Constructor()
            {
                Nodes = CreateRoot();
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
                    Nodes = new TreeListNode<IPagemarkEntry>();
                    foreach (var mark in Marks ?? new List<Pagemark>())
                    {
                        Nodes.Add(mark);
                    }

                    Books = OldBooks ?? new List<Book.Memento>();
                    foreach (var book in Books)
                    {
                        book.LastAccessTime = default(DateTime);
                    }

                    Marks = null;
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
        public Memento CreateMemento(bool removeTemporary)
        {
            var memento = new Memento();
            memento.Nodes = Items;
            memento.Books = Items.Select(e => e.Value).OfType<Pagemark>().Select(e => e.Unit.Memento).Distinct().ToList();

            // TODO: removeTemporary は登録時に
            if (removeTemporary)
            {
                Debug.WriteLine("Warning: not support removeTemporary parameter at PagemarkCollection.CreateMemento()");
            }

            /*
            memento.Books = removeTemporary
                ? this.Items.Select(e => e.Unit.Memento).Distinct().Where(e => !e.Place.StartsWith(Temporary.TempDirectory)).ToList()
                : this.Items.Select(e => e.Unit.Memento).Distinct().ToList();

            memento.Marks = removeTemporary
                ? this.Items.Where(e => !e.Place.StartsWith(Temporary.TempDirectory)).ToList()
                : this.Items.ToList();
            */

            return memento;
        }

        // memento適用
        public void Restore(Memento memento)
        {
            if (memento._Version < Config.GenerateProductVersionNumber(32, 0, 0))
            {
                memento.Nodes.Value = new PagemarkFolder();
                ValidateFolderName(memento.Nodes);
                ValidateDefaultFolder(memento.Nodes);
                Task.Run(() => ValidateAsync(memento.Nodes).Wait()).Wait(); // NOTE: デッドロック回避のためあえてタスク化
            }

            this.Load(memento.Nodes, memento.Books);
        }

        #endregion
    }

}
