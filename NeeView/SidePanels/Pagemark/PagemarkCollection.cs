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

namespace NeeView
{
    public class PagemarkCollection : BindableBase
    {
        public static PagemarkCollection Current { get; private set; }

        #region Constructors

        public PagemarkCollection()
        {
            Current = this;

            Items = new ObservableCollection<Pagemark>();
        }

        #endregion

        #region Properties

        private ObservableCollection<Pagemark> _items;
        public ObservableCollection<Pagemark> Items
        {
            get { return _items; }
            private set
            {
                _items = value;
                BindingOperations.EnableCollectionSynchronization(_items, new object());
                RaisePropertyChanged();
            }
        }

        private Pagemark _selectedItem;
        public Pagemark SelectedItem
        {
            get { return _selectedItem; }
            set
            {
                if (_selectedItem != value)
                {
                    _selectedItem = value;
                    RaisePropertyChanged();
                }
            }
        }

        #endregion

        #region Methods

        // クリア
        public void Clear()
        {
            Items.Clear();
        }


        public void Load(IEnumerable<Book.Memento> items, IEnumerable<Pagemark> marks)
        {
            Clear();

            var units = new List<BookMementoUnit>();
            foreach (var item in items)
            {
                var unit = BookMementoCollection.Current.Set(item);
                units.Add(unit);
            }

            foreach (var mark in marks)
            {
                var unit = units.FirstOrDefault(e => e.Place == mark.Place);
                if (unit != null)
                {
                    Items.Add(new Pagemark(unit, mark.EntryName));
                }
            }
        }

        /// <summary>
        /// 無効なページマークを削除.
        /// 現在の実装ではブックの有無のみ判定
        /// </summary>
        /// <param name="token"></param>
        /// <returns></returns>
        public async Task RemoveUnlinkedAsync(CancellationToken token)
        {
            // 削除項目収集
            var unlinked = new List<Pagemark>();
            foreach (var mark in this.Items)
            {
                if (!(await ArchiveFileSystem.ExistsAsync(mark.Place, token)))
                {
                    unlinked.Add(mark);
                }
            }

            // 削除実行
            foreach (var pagemark in unlinked)
            {
                var place = pagemark.Unit.Memento.Place;

                Debug.WriteLine($"PagemarkRemove: {pagemark.Place} - {pagemark.EntryName}");
                Items.Remove(pagemark);
            }
        }

        //
        public bool Contains(string place, string page)
        {
            return Items.Any(e => e.Place == place && e.EntryName == page);
        }

        // 検索
        public Pagemark Find(string place, string page)
        {
            return Items.FirstOrDefault(e => e.Place == place && e.EntryName == page);
        }

        // 検索
        public BookMementoUnit FindUnit(string place)
        {
            if (place == null) return null;

            var item = Items.FirstOrDefault(e => e.Place == place);
            return item?.Unit;
        }

        // 名前変更
        public void Rename(string src, string dst)
        {
            if (src == null || dst == null) return;

            foreach (var mark in Items.Where(e => e.Place == src))
            {
                mark.Place = dst;
            }
        }



        // となりを取得
        public Pagemark GetNeighbor(Pagemark mark)
        {
            if (Items == null || Items.Count <= 0) return null;

            int index = Items.IndexOf(mark);
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
                return mark;
            }
        }


        /// <summary>
        /// 
        /// </summary>
        /// <param name="direction"></param>
        /// <returns></returns>
        public bool CanMoveSelected(int direction)
        {
            if (SelectedItem == null)
            {
                return Items.Count > 0;
            }
            else
            {
                int index = Items.IndexOf(SelectedItem) + direction;
                return (index >= 0 && index < Items.Count);
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="direction"></param>
        /// <returns></returns>
        public Pagemark MoveSelected(int direction)
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

        /// <summary>
        /// 指定のマーカーに移動。存在しなければ移動しない
        /// </summary>
        /// <param name="place"></param>
        /// <param name="entryName"></param>
        /// <returns></returns>
        public Pagemark Move(string place, string entryName)
        {
            var mark = Search(place, entryName);
            if (mark != null)
            {
                SelectedItem = mark;
            }

            return SelectedItem;
        }


        /// <summary>
        /// マーカー追加
        /// </summary>
        /// <param name="mark"></param>
        public void Add(Pagemark mark)
        {
            if (!Items.Contains(mark))
            {
                ////Marks.Add(mark);
                Items.Insert(0, mark);
            }
        }

        /// <summary>
        /// マーカー削除
        /// </summary>
        /// <param name="mark"></param>
        public void Remove(Pagemark mark)
        {
            Items.Remove(mark);
        }


        /// <summary>
        /// マーカー追加/削除
        /// </summary>
        /// <param name="mark"></param>
        /// <returns></returns>
        public bool Toggle(Pagemark mark)
        {
            var index = Items.IndexOf(mark);
            if (index < 0)
            {
                Add(mark);
                return true;
            }
            else
            {
                Remove(mark);
                return false;
            }
        }

        /// <summary>
        /// 指定フォルダーのマーカー収集
        /// </summary>
        /// <param name="place"></param>
        /// <returns></returns>
        public List<Pagemark> Collect(string place)
        {
            return Items.Where(e => e.Place == place).ToList();
        }

        /// <summary>
        /// マーカーの検索
        /// </summary>
        /// <param name="place"></param>
        /// <param name="entryName"></param>
        /// <returns></returns>
        public Pagemark Search(string place, string entryName)
        {
            return Items.FirstOrDefault(e => e.Place == place && e.EntryName == entryName);
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
            public List<Pagemark> Marks { get; set; }

            [Obsolete]
            [DataMember(Name = "Items", EmitDefaultValue = false)]
            public List<Book.Memento> OldBooks { get; set; }

            [DataMember]
            public List<Book.Memento> Books { get; set; }

            private void Constructor()
            {
                Marks = new List<Pagemark>();
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
        public Memento CreateMemento(bool removeTemporary)
        {
            var memento = new Memento();

            memento.Books = removeTemporary
                ? this.Items.Select(e => e.Unit.Memento).Distinct().Where(e => !e.Place.StartsWith(Temporary.TempDirectory)).ToList()
                : this.Items.Select(e => e.Unit.Memento).Distinct().ToList();

            memento.Marks = removeTemporary
                ? this.Items.Where(e => !e.Place.StartsWith(Temporary.TempDirectory)).ToList()
                : this.Items.ToList();

            return memento;
        }

        // memento適用
        public void Restore(Memento memento)
        {
            this.Load(memento.Books, memento.Marks);
        }

        #endregion
    }

}
