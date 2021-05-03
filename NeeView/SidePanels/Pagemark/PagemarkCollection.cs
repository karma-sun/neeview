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
using NeeView.Properties;

namespace NeeView
{
    [Obsolete]
    public enum PagemarkOrder
    {
        FileName,
        Path,
    }

    [Obsolete]
    public class PagemarkCollection : BindableBase
    {
        public static TreeListNode<IPagemarkEntry> CreateRoot()
        {
            var items = new TreeListNode<IPagemarkEntry>();
            items.Value = new PagemarkFolder();

            return items;
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
                return JsonSerializer.Deserialize<Memento>(json, UserSettingTools.GetSerializerOptions()).Validate();
            }

            /// <summary>
            /// 互換補正処理 (ver38以降)
            /// </summary>
            private Memento Validate()
            {
                return this;
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
                // nop.
                ////config.Pagemark.PagemarkOrder = PagemarkOrder;
            }
        }

        #endregion
    }


    [Obsolete]
    public class PagemarkNode
    {
        public string Path { get; set; }

        public string EntryName { get; set; }

        public string DispName { get; set; }

        public bool IsExpanded { get; set; }

        public List<PagemarkNode> Children { get; set; }

        public bool IsFolder => Children != null;

        public IEnumerable<PagemarkNode> GetEnumerator()
        {
            yield return this;

            if (Children != null)
            {
                foreach (var child in Children)
                {
                    foreach (var node in child.GetEnumerator())
                    {
                        yield return node;
                    }
                }
            }
        }
    }

    [Obsolete]
    public static class PagemarkNodeConverter
    {
        public static PagemarkNode ConvertFrom(TreeListNode<IPagemarkEntry> source)
        {
            if (source == null) return null;

            var node = new PagemarkNode();

            if (source.Value is PagemarkFolder folder)
            {
                node.Path = folder.Path;
                node.IsExpanded = source.IsExpanded;
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
                node.IsExpanded = source.IsExpanded;
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


    // ページマークをプレイリストに変換する
    public static class PagemarkToPlaylistConverter
    {
#pragma warning disable CS0612 // 型またはメンバーが旧型式です
        public static PlaylistSource ConvertToPlaylist(PagemarkCollection.Memento memento)
        {
            var items = memento.Nodes.GetEnumerator()
                .Where(e => !e.IsFolder)
                .Select(e => new PlaylistSourceItem(LoosePath.Combine(e.Path, e.EntryName), e.DispName));

            return new PlaylistSource(items);
        }

        public static void PagemarkToPlaylist()
        {
            var path = Config.Current.Playlist.PagemarkPlaylist;
            if (File.Exists(path))
            {
                return;
            }

            // load pagemark
            var result = LoadPagemark(Config.Current.PagemarkLegacy?.PagemarkFilePath);
            if (result.pagemark is null)
            {
                return;
            }

            SavePagemarkPlaylist(result.pagemark);

            Config.Current.Playlist.CurrentPlaylist = Config.Current.Playlist.PagemarkPlaylist;

            // remove
            FileIO.RemoveFile(result.path);
            Config.Current.PagemarkLegacy.PagemarkFilePath = null;
        }

        public static void SavePagemarkPlaylist(PagemarkCollection.Memento pagemark)
        {
            // convert
            var playlistSource = ConvertToPlaylist(pagemark);

            // save
            playlistSource.Save(Config.Current.Playlist.PagemarkPlaylist, true);
        }


        // ページマーク読み込み
        private static (string path, PagemarkCollection.Memento pagemark) LoadPagemark(string filename)
        {
            if (filename is null) return default;

            App.Current.SemaphoreWait();
            try
            {
                var extension = Path.GetExtension(filename).ToLower();
                var filenameV1 = Path.ChangeExtension(filename, ".xml");

                var failedDialog = new LoadFailedDialog(Resources.Notice_LoadPagemarkFailed, Resources.Notice_LoadPagemarkFailedTitle);

                if (extension == ".json" && File.Exists(filename))
                {
                    PagemarkCollection.Memento memento = Load(PagemarkCollection.Memento.Load, filename, failedDialog);
                    return (filename, memento);
                }
                // before v.37
                else if (File.Exists(filenameV1))
                {
                    PagemarkCollection.Memento memento = Load(PagemarkCollection.Memento.LoadV1, filenameV1, failedDialog);
                    return (filenameV1, memento);
                }
                else
                {
                    return default;
                }
            }
            finally
            {
                App.Current.SemaphoreRelease();
            }

            PagemarkCollection.Memento Load(Func<string, PagemarkCollection.Memento> load, string path, LoadFailedDialog loadFailedDialog)
            {
                try
                {
                    return load(path);
                }
                catch (Exception ex)
                {
                    loadFailedDialog?.ShowDialog(ex);
                    return null;
                }
            }
        }


#pragma warning restore CS0612 // 型またはメンバーが旧型式です


    }

}
