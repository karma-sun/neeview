// Copyright (c) 2016 Mitsuhiro Ito (nee)
//
// This software is released under the MIT License.
// http://opensource.org/licenses/mit-license.php

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Data;
using System.Xml;

namespace NeeView
{
    // HistoryChangedイベントの種類
    public enum HistoryChangedType
    {
        Load,
        Clear,
        Add,
        Update,
        Remove,
    }

    // HistoryChangedイベントの引数
    public class BookMementoCollectionChangedArgs
    {
        public HistoryChangedType HistoryChangedType { get; set; }
        public string Key { get; set; }

        public BookMementoCollectionChangedArgs(HistoryChangedType type, string key)
        {
            HistoryChangedType = type;
            Key = key;
        }
    }

    /// <summary>
    /// 履歴
    /// </summary>
    public class BookHistory : INotifyPropertyChanged
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

        // 履歴に追加、削除された
        public event EventHandler<BookMementoCollectionChangedArgs> HistoryChanged;

        // 履歴
        private ObservableCollection<Book.Memento> _Items;
        public ObservableCollection<Book.Memento> Items
        {
            get { return _Items; }
            private set { _Items = value; OnPropertyChanged(); }
        }

        // 履歴保持最大数
        private int _MaxHistoryCount = 100;
        public int MaxHistoryCount
        {
            get { return _MaxHistoryCount; }
            set { _MaxHistoryCount = value; Resize(); }
        }

        public BookHistory()
        {
            Items = new ObservableCollection<Book.Memento>();
            BindingOperations.EnableCollectionSynchronization(Items, new object());
        }


        // 履歴クリア
        public void Clear()
        {
            Items.Clear();
            HistoryChanged?.Invoke(this, new BookMementoCollectionChangedArgs(HistoryChangedType.Clear, null));
        }

        // 履歴サイズ調整
        private void Resize()
        {
            while (Items.Count > MaxHistoryCount)
            {
                var path = Items.Last().Place;
                Items.RemoveAt(Items.Count - 1);
                HistoryChanged?.Invoke(this, new BookMementoCollectionChangedArgs(HistoryChangedType.Remove, path));
            }
        }


        // 履歴追加
        public void Add(Book book, bool isKeepOrder)
        {
            if (book?.Place == null) return;
            if (book.Pages.Count <= 0) return;

            var item = Items.FirstOrDefault(e => e.Place == book.Place);
            var setting = book.CreateMemento();
            if (item != null)
            {
                int oldIndex = Items.IndexOf(item);
                int newIndex = isKeepOrder ? oldIndex : 0;
                if (oldIndex != newIndex) Items.Move(oldIndex, newIndex);
                setting.CopyTo(Items[newIndex]);
                HistoryChanged?.Invoke(this, new BookMementoCollectionChangedArgs(HistoryChangedType.Update, setting.Place));
            }
            else
            {
                Items.Insert(0, setting);
                HistoryChanged?.Invoke(this, new BookMementoCollectionChangedArgs(HistoryChangedType.Add, setting.Place));
                Resize();
            }
        }

        // 履歴削除
        public void Remove(string place)
        {
            var item = Items.FirstOrDefault(e => e.Place == place);
            if (item != null)
            {
                Items.Remove(item);
                HistoryChanged?.Invoke(this, new BookMementoCollectionChangedArgs(HistoryChangedType.Remove, item.Place));
            }
        }

        // 履歴検索
        public Book.Memento Find(string place)
        {
            return Items.FirstOrDefault(e => e.Place == place);
        }

        // 最近使った履歴のリストアップ
        public List<Book.Memento> ListUp(int size)
        {
            var list = new List<Book.Memento>();
            foreach (var item in Items)
            {
                if (list.Count >= size) break;
                list.Add(item);
            }
            return list;
        }


        #region Memento

        /// <summary>
        /// 履歴Memento
        /// </summary>
        [DataContract]
        public class Memento
        {
            [DataMember(Name = "History")]
            public List<Book.Memento> Items { get; set; }

            [DataMember]
            public int MaxHistoryCount { get; set; }

            private void Constructor()
            {
                Items = new List<Book.Memento>();
                MaxHistoryCount = 100;
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
                if (MaxHistoryCount < memento.MaxHistoryCount) MaxHistoryCount = memento.MaxHistoryCount;
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
            memento.Items = this.Items.ToList();
            if (removeTemporary)
            {
                memento.Items.RemoveAll((e) => e.Place.StartsWith(Temporary.TempDirectory));
            }

            memento.MaxHistoryCount = this.MaxHistoryCount;
            return memento;
        }

        // memento適用
        public void Restore(Memento memento)
        {
            this.Items = new ObservableCollection<Book.Memento>(memento.Items);
            BindingOperations.EnableCollectionSynchronization(this.Items, new object());
            this.MaxHistoryCount = memento.MaxHistoryCount;
            this.HistoryChanged?.Invoke(this, new BookMementoCollectionChangedArgs(HistoryChangedType.Load, null));
        }

        #endregion
    }
}
