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

namespace NeeView
{
    public class BookmarkCollection : INotifyPropertyChanged
    {
        #region NotifyPropertyChanged
        public event System.ComponentModel.PropertyChangedEventHandler PropertyChanged;

        protected void OnPropertyChanged([System.Runtime.CompilerServices.CallerMemberName] string name = "")
        {
            if (PropertyChanged != null)
            {
                PropertyChanged(this, new System.ComponentModel.PropertyChangedEventArgs(name));
            }
        }
        #endregion

        //
        public event EventHandler<BookMementoCollectionChangedArgs> BookmarkChanged;

        // ブックマーク
        private ObservableCollection<Book.Memento> _Items;
        public ObservableCollection<Book.Memento> Items
        {
            get { return _Items; }
            private set
            {
                _Items = value;
                BindingOperations.EnableCollectionSynchronization(_Items, new object());
                OnPropertyChanged();
            }
        }

        public BookmarkCollection()
        {
            Items = new ObservableCollection<Book.Memento>();
        }

        // クリア
        public void Clear()
        {
            Items.Clear();
            BookmarkChanged?.Invoke(this, new BookMementoCollectionChangedArgs(HistoryChangedType.Clear, null));
        }

        // 追加
        public void Add(Book book)
        {
            if (book?.Place == null) return;
            if (book.Pages.Count <= 0) return;

            // 既に存在する場合は上書き、そうでない場合は新規追加
            var item = Items.FirstOrDefault(e => e.Place == book.Place);
            var setting = book.CreateMemento();
            if (item != null)
            {
                int index = Items.IndexOf(item);
                setting.CopyTo(Items[index]);
                BookmarkChanged?.Invoke(this, new BookMementoCollectionChangedArgs(HistoryChangedType.Update, setting.Place));

            }
            else
            {
                Items.Add(setting);
                BookmarkChanged?.Invoke(this, new BookMementoCollectionChangedArgs(HistoryChangedType.Add, setting.Place));
            }

        }

        // 削除
        public void Remove(string place)
        {
            var item = Items.FirstOrDefault(e => e.Place == place);
            if (item != null)
            {
                Items.Remove(item);
                BookmarkChanged?.Invoke(this, new BookMementoCollectionChangedArgs(HistoryChangedType.Remove, item.Place));
            }
        }

        // 更新
        public void Update(Book book)
        {
            if (book?.Place == null) return;
            if (book.Pages.Count <= 0) return;

            var item = ModelContext.Bookmarks.Find(book.Place);
            if (item != null)
            {
                int index = Items.IndexOf(item);
                Items[index] = book.CreateMemento();
                BookmarkChanged?.Invoke(this, new BookMementoCollectionChangedArgs(HistoryChangedType.Update, Items[index].Place));
            }
        }

        // 検索
        public Book.Memento Find(string place)
        {
            return Items.FirstOrDefault(e => e.Place == place);
        }


        #region Memento

        /// <summary>
        /// 履歴Memento
        /// </summary>
        [DataContract]
        public class Memento
        {
            [DataMember]
            public List<Book.Memento> Items { get; set; }


            private void Constructor()
            {
                Items = new List<Book.Memento>();
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

            // 結合
            public void Merge(Memento memento)
            {
                Items = Items.Concat(memento?.Items).Distinct(new Book.MementoPlaceCompare()).ToList();
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
                using (XmlReader xr = XmlReader.Create(path))
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
            memento.Items = new List<Book.Memento>(this.Items);
            if (removeTemporary)
            {
                memento.Items.RemoveAll((e) => e.Place.StartsWith(Temporary.TempDirectory));
            }

            return memento;
        }

        // memento適用
        public void Restore(Memento memento)
        {
            this.Items = new ObservableCollection<Book.Memento>(memento.Items);
        }

        #endregion
    }
}
