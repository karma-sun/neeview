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
using NeeView.Collections;
using System.Text.Json.Serialization;
using System.Text.Json;

namespace NeeView
{
    public enum PagemarkOrder
    {
        FileName,
        Path,
    }

    public class PagemarkCollection : BindableBase
    {
        static PagemarkCollection() => Current = new PagemarkCollection();
        public static PagemarkCollection Current { get; }


        // Constructors

        private PagemarkCollection()
        {
            Items = CreateRoot();

            Config.Current.Pagemark.AddPropertyChanged(nameof(PagemarkConfig.PagemarkOrder), (s, e) =>
            {
                if (Sort())
                {
                    PagemarkChanged?.Invoke(this, new PagemarkCollectionChangedEventArgs(EntryCollectionChangedAction.Replace));
                }
            });
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

        public static TreeListNode<IPagemarkEntry> CreateRoot()
        {
            var items = new TreeListNode<IPagemarkEntry>();
            items.Value = new PagemarkFolder();

            return items;
        }

        public void RaisePagemarkChangedEvent(PagemarkCollectionChangedEventArgs e)
        {
            PagemarkChanged?.Invoke(this, e);
        }

        public void Load(TreeListNode<IPagemarkEntry> nodes)
        {
            Items = nodes;
            Sort();

            PagemarkChanged?.Invoke(this, new PagemarkCollectionChangedEventArgs(EntryCollectionChangedAction.Reset));
        }


        public Pagemark Find(string place, string entryName)
        {
            if (place == null) return null;
            if (entryName == null) return null;

            return Items.Select(e => e.Value).OfType<Pagemark>().FirstOrDefault(e => e.Path == place && e.EntryName == entryName);
        }

        public TreeListNode<IPagemarkEntry> FindNode(string place, string entryName)
        {
            if (place == null) return null;
            if (entryName == null) return null;

            return Items.FirstOrDefault(e => e.Value is Pagemark pagemark && pagemark.Path == place && pagemark.EntryName == entryName);
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
                return Items.FirstOrDefault(e => e.Value is Pagemark pagemark && pagemark.Path == path.Path);
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
            return Items.Select(e => e.Value).OfType<Pagemark>().Where(e => e.Path == place).ToList();
        }


        public void Add(TreeListNode<IPagemarkEntry> node)
        {
            if (node == null) throw new ArgumentNullException(nameof(node));
            if (node.Root == null) throw new InvalidOperationException();

            if (node.Value is Pagemark pagemark)
            {
                var parent = Items.Children.FirstOrDefault(e => e.Value is PagemarkFolder folder && folder.Path == pagemark.Path);
                if (parent == null)
                {
                    parent = new TreeListNode<IPagemarkEntry>(new PagemarkFolder() { Path = pagemark.Path }) { IsExpanded = true };
                    Items.Insert(GetInsertIndex(Items, parent), parent);
                    PagemarkChanged?.Invoke(this, new PagemarkCollectionChangedEventArgs(EntryCollectionChangedAction.Add, parent.Parent, parent));
                }

                parent.Insert(GetInsertIndex(parent, node), node);
                PagemarkChanged?.Invoke(this, new PagemarkCollectionChangedEventArgs(EntryCollectionChangedAction.Add, node.Parent, node));
            }
            else
            {
                Items.Insert(GetInsertIndex(Items, node), node);
                PagemarkChanged?.Invoke(this, new PagemarkCollectionChangedEventArgs(EntryCollectionChangedAction.Add, node.Parent, node));
            }
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

                if (parent != Items && !parent.Children.Any())
                {
                    var grandParent = parent.Parent;
                    parent.RemoveSelf();
                    PagemarkChanged?.Invoke(this, new PagemarkCollectionChangedEventArgs(EntryCollectionChangedAction.Remove, grandParent, parent));
                }

                return true;
            }

            return false;
        }


        private bool Sort()
        {
            if (_items == null) return false;

            _items.Sort(CreateComparer(Config.Current.Pagemark.PagemarkOrder));
            return true;
        }

        private IComparer<TreeListNode<IPagemarkEntry>> CreateComparer(PagemarkOrder order, TreeListNode<IPagemarkEntry> parent)
        {
            return CreateComparer(parent == _items ? order : PagemarkOrder.FileName);
        }

        private IComparer<TreeListNode<IPagemarkEntry>> CreateComparer(PagemarkOrder order)
        {
            switch (order)
            {
                default:
                case PagemarkOrder.FileName:
                    return new ComparerDispName();
                case PagemarkOrder.Path:
                    return new ComparerName();
            }
        }

        /// <summary>
        /// ソート用：表示名で比較(昇順)
        /// </summary>
        public class ComparerDispName : IComparer<TreeListNode<IPagemarkEntry>>
        {
            public int Compare(TreeListNode<IPagemarkEntry> x, TreeListNode<IPagemarkEntry> y)
            {
                return NativeMethods.StrCmpLogicalW(x.Value.DispName, y.Value.DispName);
            }
        }

        /// <summary>
        /// ソート用：名前(パス)で比較(昇順)
        /// </summary>
        public class ComparerName : IComparer<TreeListNode<IPagemarkEntry>>
        {
            public int Compare(TreeListNode<IPagemarkEntry> x, TreeListNode<IPagemarkEntry> y)
            {
                return NativeMethods.StrCmpLogicalW(x.Value.Name, y.Value.Name);
            }
        }

        /// <summary>
        /// 挿入位置を求める
        /// </summary>
        /// <param name="parent"></param>
        /// <param name="node"></param>
        /// <returns></returns>
        private int GetInsertIndex(TreeListNode<IPagemarkEntry> parent, TreeListNode<IPagemarkEntry> node)
        {
            var comparer = CreateComparer(Config.Current.Pagemark.PagemarkOrder, parent);
            for (int index = 0; index < parent.Children.Count; ++index)
            {
                var child = parent.Children[index];
                if (child == node) continue;

                if (comparer.Compare(node, child) < 0)
                {
                    return index;
                }
            }

            return parent.Children.Count;
        }

        /// <summary>
        /// 指定項目を兄弟の仲の適切な順位に移動
        /// </summary>
        public void SortOne(TreeListNode<IPagemarkEntry> node)
        {
            if (node == null) throw new ArgumentNullException(nameof(node));
            if (node.Parent == null) return;

            var indexX = node.GetIndex();
            var indexY = GetInsertIndex(node.Parent, node);

            indexY = indexY - (indexX < indexY ? 1 : 0);
            if (indexX != indexY)
            {
                node.Parent.Children.Move(indexX, indexY);
                PagemarkChanged?.Invoke(this, new PagemarkCollectionChangedEventArgs(EntryCollectionChangedAction.Move, node.Parent, node));
            }
        }

        /// <summary>
        /// 無効なページマークを削除.
        /// 現在の実装ではブックの有無のみ判定
        /// </summary>
        public async Task<int> RemoveUnlinkedAsync(CancellationToken token)
        {
            // 削除項目収集
            var unlinked = new List<TreeListNode<IPagemarkEntry>>();
            foreach (var node in this.Items.Children)
            {
                if (node.Value is PagemarkFolder folder)
                {
                    if (!(await ArchiveEntryUtility.ExistsAsync(folder.Path, token)))
                    {
                        unlinked.Add(node);
                    }
                }
            }

            // 削除実行
            int count = 0;
            foreach (var node in unlinked)
            {
                Debug.WriteLine($"PagemarkRemove: {node.Value.DispName}");
                count += node.Children.Count;
                node.RemoveSelf();
            }

            PagemarkChanged?.Invoke(this, new PagemarkCollectionChangedEventArgs(EntryCollectionChangedAction.Replace));

            return count;
        }

        /// <summary>
        /// 表示名変更
        /// </summary>
        public void RenameDispName(TreeListNode<IPagemarkEntry> node, string newName)
        {
            if (node.Value is Pagemark pagemark)
            {
                pagemark.DispName = string.IsNullOrWhiteSpace(newName) ? null : newName;
                PagemarkChanged?.Invoke(this, new PagemarkCollectionChangedEventArgs(EntryCollectionChangedAction.Rename, node.Parent, node));
            }
        }


        /// <summary>
        /// ファイル名の変更に追従
        /// </summary>
        public void Rename(string src, string dst)
        {
            foreach (var item in Items)
            {
                if (item.Value is PagemarkFolder folder && folder.Path == src)
                {
                    folder.Path = dst;
                    SortOne(item);

                    foreach (var child in item)
                    {
                        if (child.Value is Pagemark pagemark && pagemark.Path == src)
                        {
                            pagemark.Path = dst;
                        }
                    }

                    PagemarkChanged?.Invoke(this, new PagemarkCollectionChangedEventArgs(EntryCollectionChangedAction.Rename, item.Parent, item));

                    return;
                }
            }
        }


        private static TreeListNode<IPagemarkEntry> ConvertToBookUnitFormat(TreeListNode<IPagemarkEntry> source)
        {

            var map = new Dictionary<string, List<Pagemark>>();

            foreach (var pagemark in source.Select(e => e.Value).OfType<Pagemark>())
            {
                var place = pagemark.Path;

                if (!map.ContainsKey(place))
                {
                    map.Add(place, new List<Pagemark>());
                }

                map[place].Add(pagemark);
            }

            var items = CreateRoot();

            foreach (var key in map.Keys.OrderBy(e => LoosePath.GetFileName(e), new NameComparer()))
            {
                var node = new TreeListNode<IPagemarkEntry>(new PagemarkFolder() { Path = key }) { IsExpanded = true };
                items.Add(node);

                foreach (var pagemark in map[key].OrderBy(e => e.DispName, new NameComparer()))
                {
                    node.Add(new TreeListNode<IPagemarkEntry>(pagemark));
                }
            }

            return items;
        }


#region Memento

        [DataContract]
        [KnownType(typeof(Pagemark))]
        [KnownType(typeof(PagemarkFolder))]
        public class Memento
        {
            [JsonPropertyName("Format")]
            public FormatVersion Format { get; set; }

            [JsonIgnore]
            [DataMember]
            public int _Version { get; set; } = Environment.ProductVersionNumber;

            [JsonIgnore]
            [Obsolete, DataMember(Name = "Nodes", EmitDefaultValue = false)]
            public TreeListNode<IPagemarkEntry> NodesLegacy { get; set; }

            [DataMember(Name = "NodesV2")]
            public PagemarkNode Nodes { get; set; }

            [JsonIgnore]
            [DataMember]
            public PagemarkOrder PagemarkOrder { get; set; }


            [JsonIgnore]
            [Obsolete, DataMember(EmitDefaultValue = false)]
            public List<Book.Memento> Books { get; set; }

            [JsonIgnore]
            [Obsolete, DataMember(EmitDefaultValue = false)]
            public List<Pagemark> Marks { get; set; }

            [JsonIgnore]
            [Obsolete, DataMember(Name = "Items", EmitDefaultValue = false)]
            public List<Book.Memento> OldBooks { get; set; }


            private void Constructor()
            {
                Nodes = new PagemarkNode();
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
                if (_Version < Environment.GenerateProductVersionNumber(31, 0, 0))
                {
                    NodesLegacy = new TreeListNode<IPagemarkEntry>();
                    foreach (var mark in Marks ?? new List<Pagemark>())
                    {
                        NodesLegacy.Add(mark);
                    }

                    Books = OldBooks ?? new List<Book.Memento>();
                    foreach (var book in Books)
                    {
                        book.LastAccessTime = default(DateTime);
                    }

                    Marks = null;
                    OldBooks = null;
                }

                // 新しいフォーマットに変換
                if (_Version < Environment.GenerateProductVersionNumber(32, 0, 0))
                {
                    NodesLegacy = ConvertToBookUnitFormat(NodesLegacy);
                }

                if (_Version < Environment.GenerateProductVersionNumber(37, 0, 0))
                {
                    Nodes = PagemarkNodeConverter.ConvertFrom(NodesLegacy) ?? new PagemarkNode();
                    NodesLegacy = null;
                }
#pragma warning restore CS0612
            }

            public void Save(string path)
            {
                Format = new FormatVersion(Environment.SolutionName + ".Pagemark", Environment.AssemblyVersion.Major, Environment.AssemblyVersion.Minor, 0);

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

#endregion

            public void RestoreConfig(Config config)
            {
                config.Pagemark.PagemarkOrder = PagemarkOrder;
            }
        }

        // memento作成
        public Memento CreateMemento()
        {
            var memento = new Memento();
            memento.Nodes = PagemarkNodeConverter.ConvertFrom(Items);
            memento.PagemarkOrder = Config.Current.Pagemark.PagemarkOrder;
            return memento;
        }

        // memento適用
        public void Restore(Memento memento)
        {
            if (memento == null) return;

            this.Load(PagemarkNodeConverter.ConvertToTreeListNode(memento.Nodes));
        }

#endregion
    }



    public class PagemarkNode
    {
        public string Path { get; set; }

        public string EntryName { get; set; }

        public string DispName { get; set; }

        public List<PagemarkNode> Children { get; set; }

        public bool IsFolder => Children != null;
    }

    public static class PagemarkNodeConverter
    {
        public static PagemarkNode ConvertFrom(TreeListNode<IPagemarkEntry> source)
        {
            if (source == null) return null;

            var node = new PagemarkNode();

            if (source.Value is PagemarkFolder folder)
            {
                node.Path = folder.Path;
                node.Children = new List<PagemarkNode>();
                foreach (var child in source.Children)
                {
                    node.Children.Add(ConvertFrom(child));
                }
            }
            else if (source.Value is Pagemark pagemark)
            {
                node.Path = pagemark.Path;
                node.EntryName = pagemark.EntryName;
                node.DispName = pagemark.DispNameRaw;
            }
            else
            {
                throw new NotSupportedException();
            }

            return node;
        }

        public static TreeListNode<IPagemarkEntry> ConvertToTreeListNode(PagemarkNode source)
        {
            var node = new TreeListNode<IPagemarkEntry>();

            if (source.IsFolder)
            {
                node.Value = new PagemarkFolder()
                {
                    Path = source.Path,
                };
                foreach (var child in source.Children)
                {
                    node.Add(ConvertToTreeListNode(child));
                }
            }
            else
            {
                node.Value = new Pagemark(source.Path, source.EntryName)
                {
                    DispName = source.DispName,
                };
            }

            return node;
        }
    }
}
