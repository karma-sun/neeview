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

namespace NeeView
{
    public class PagemarkCollection : BindableBase
    {
        public static PagemarkCollection Current { get; private set; }


        // Constructors

        public PagemarkCollection()
        {
            Current = this;

            Items = new TreeListNode<IPagemarkEntry>();
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


        // Methods

        public void Clear()
        {
            Items.Clear();
            BookMementoCollection.Current.CleanUp();

            PagemarkChanged?.Invoke(this, new PagemarkCollectionChangedEventArgs(NotifyCollectionChangedAction.Reset));
        }


        public void Load(TreeListNode<IPagemarkEntry> nodes, IEnumerable<Book.Memento> books)
        {
            foreach (var book in books)
            {
                BookMementoCollection.Current.Set(book);
            }

            Items = nodes;

            PagemarkChanged?.Invoke(this, new PagemarkCollectionChangedEventArgs(NotifyCollectionChangedAction.Replace));
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


        public bool Contains(string place, string entryName)
        {
            if (place == null) return false;

            return Find(place, entryName) != null;
        }


        public List<Pagemark> Collect(string place)
        {
            return Items.Select(e => e.Value).OfType<Pagemark>().Where(e => e.Place == place).ToList();
        }


        public void AddFirst(IPagemarkEntry item)
        {
            if (item == null) throw new ArgumentNullException(nameof(item));

            var node = new TreeListNode<IPagemarkEntry>(item);
            Items.Root.Insert(0, node);
            PagemarkChanged?.Invoke(this, new PagemarkCollectionChangedEventArgs(NotifyCollectionChangedAction.Add, node));
        }


        public bool Remove(TreeListNode<IPagemarkEntry> node)
        {
            if (node == null) throw new ArgumentNullException(nameof(node));
            if (node.Root != Items.Root) throw new InvalidOperationException();

            if (node.RemoveSelf())
            {
                PagemarkChanged?.Invoke(this, new PagemarkCollectionChangedEventArgs(NotifyCollectionChangedAction.Remove, node));
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

            PagemarkChanged?.Invoke(this, new PagemarkCollectionChangedEventArgs(NotifyCollectionChangedAction.Replace));
        }


        public void Move(TreeListNode<IPagemarkEntry> item, TreeListNode<IPagemarkEntry> target, int direction)
        {
            if (item == target) return;
            if (target.ParentContains(item)) return; // TODO: 例外にすべき？

            item.RemoveSelf();
            target.Parent.Insert(target, direction, item);

            PagemarkChanged?.Invoke(this, new PagemarkCollectionChangedEventArgs(NotifyCollectionChangedAction.Move, item));
        }


        public void MoveToChild(TreeListNode<IPagemarkEntry> item, TreeListNode<IPagemarkEntry> target)
        {
            if (item == target) return;
            if (target.ParentContains(item)) return; // TODO: 例外にすべき？

            item.RemoveSelf();
            target.Insert(0, item);
            target.IsExpanded = true;

            PagemarkChanged?.Invoke(this, new PagemarkCollectionChangedEventArgs(NotifyCollectionChangedAction.Move, item));
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



        #region Memento

        [DataContract]
        [KnownType(typeof(Pagemark))]
        [KnownType(typeof(PagemarkFolder))]
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
                Nodes = new TreeListNode<IPagemarkEntry>();
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
            this.Load(memento.Nodes, memento.Books);
        }

        #endregion
    }

}
