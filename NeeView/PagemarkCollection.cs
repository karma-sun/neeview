﻿// Copyright (c) 2016 Mitsuhiro Ito (nee)
//
// This software is released under the MIT License.
// http://opensource.org/licenses/mit-license.php

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

namespace NeeView
{
    public class PagemarkCollection : INotifyPropertyChanged
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

        // ページマークされているブック情報
        private ObservableCollection<BookMementoUnitNode> _Items;
        public ObservableCollection<BookMementoUnitNode> Items
        {
            get { return _Items; }
            private set
            {
                _Items = value;
                BindingOperations.EnableCollectionSynchronization(_Items, new object());
                OnPropertyChanged();
            }
        }

        //
        public PagemarkCollection()
        {
            Items = new ObservableCollection<BookMementoUnitNode>();
            Marks = new ObservableCollection<Pagemark>();
        }

        // クリア
        public void Clear()
        {
            // new
            foreach (var node in Items)
            {
                node.Value.PagemarkNode = null;
            }
            Items.Clear();
        }


        // 設定
        public void Load(IEnumerable<Book.Memento> items)
        {
            Clear();

            //
            foreach (var item in items)
            {
                var unit = ModelContext.BookMementoCollection.Find(item.Place);

                if (unit == null)
                {
                    unit = new BookMementoUnit();

                    unit.Memento = item;
                    unit.PagemarkNode = new BookMementoUnitNode(unit);
                    Items.Add(unit.PagemarkNode);

                    ModelContext.BookMementoCollection.Add(unit);
                }
                else
                {
                    unit.Memento = item;
                    unit.PagemarkNode = new BookMementoUnitNode(unit);
                    Items.Add(unit.PagemarkNode);
                }
            }
        }


        // 追加
        private BookMementoUnit Add(BookMementoUnit unit, Book.Memento memento)
        {
            if (memento == null) return unit;

            try
            {
                if (unit == null)
                {
                    unit = new BookMementoUnit();

                    unit.Memento = memento;

                    unit.PagemarkNode = new BookMementoUnitNode(unit);
                    Items.Add(unit.PagemarkNode);

                    ModelContext.BookMementoCollection.Add(unit);
                }
                else if (unit.PagemarkNode != null)
                {
                    unit.Memento = memento;
                }
                else
                {
                    unit.Memento = memento;

                    unit.PagemarkNode = new BookMementoUnitNode(unit);
                    Items.Add(unit.PagemarkNode);
                }

                return unit;
            }
            catch (Exception e)
            {
                Debug.WriteLine(e.Message);
                return null;
            }
        }


        // 削除
        private BookMementoUnit Remove(string place)
        {
            return Remove(ModelContext.BookMementoCollection.Find(place));
        }

        // 削除
        private BookMementoUnit Remove(BookMementoUnit unit)
        {
            if (unit != null && unit.PagemarkNode != null)
            {
                Items.Remove(unit.PagemarkNode);
                unit.PagemarkNode = null;
            }
            return unit;
        }


        // 更新
        public void Update(Book book)
        {
            if (book?.Place == null) return;
            if (book.Pages.Count <= 0) return;

            Update(ModelContext.BookMementoCollection.Find(book.Place), book.CreateMemento());
        }

        // 更新
        public BookMementoUnit Update(BookMementoUnit unit, Book.Memento memento)
        {
            if (memento == null) return unit;
            Debug.Assert(unit == null || unit.Memento.Place == memento.Place);

            if (_Marks.Any(e => e.Place == memento.Place))
            {
                return Add(unit, memento);
            }
            else
            {
                return Remove(unit);
            }
        }

        // Marksに存在しないItemを削除
        public void Validate()
        {
            var removes = Items.Where(item => !_Marks.Any(e => e.Place == item.Value.Memento.Place)).ToList();
            removes.ForEach(e => Remove(e.Value));
        }

        // 検索
        public BookMementoUnit Find(string place)
        {
            if (place == null) return null;
            var unit = ModelContext.BookMementoCollection.Find(place);
            return unit?.PagemarkNode != null ? unit : null;
        }


        // ====
        private ObservableCollection<Pagemark> _Marks;
        public ObservableCollection<Pagemark> Marks
        {
            get { return _Marks; }
            private set
            {
                _Marks = value;
                BindingOperations.EnableCollectionSynchronization(_Marks, new object());
                OnPropertyChanged();
            }
        }


        #region Property: SelectedItem
        private Pagemark _SelectedItem;
        public Pagemark SelectedItem
        {
            get { return _SelectedItem; }
            set
            {
                if (_SelectedItem != value)
                {
                    _SelectedItem = value;
                    OnPropertyChanged();
                }
            }
        }
        #endregion


        // となりを取得
        public Pagemark GetNeighbor(Pagemark mark)
        {
            if (Marks == null || Marks.Count <= 0) return null;

            int index = Marks.IndexOf(mark);
            if (index < 0) return Marks[0];

            if (index + 1 < Marks.Count)
            {
                return Marks[index + 1];
            }
            else if (index > 0)
            {
                return Marks[index - 1];
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
                return Marks.Count > 0;
            }
            else
            {
                int index = Marks.IndexOf(SelectedItem) + direction;
                return (index >= 0 && index < Marks.Count);
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
                SelectedItem = direction >= 0 ? Marks.FirstOrDefault() : Marks.LastOrDefault();
            }
            else
            {
                int index = Marks.IndexOf(SelectedItem) + direction;
                if (index >= 0 && index <Marks.Count)
                {
                    SelectedItem = Marks[index];
                }
            }

            return SelectedItem;
        }



        /// <summary>
        /// マーカー追加
        /// </summary>
        /// <param name="mark"></param>
        public void Add(Pagemark mark)
        {
            if (!Marks.Contains(mark))
            {
                Marks.Add(mark);
            }
        }

        /// <summary>
        /// マーカー削除
        /// </summary>
        /// <param name="mark"></param>
        public void Remove(Pagemark mark)
        {
            Marks.Remove(mark);
        }


        /// <summary>
        /// マーカー追加/削除
        /// </summary>
        /// <param name="mark"></param>
        /// <returns></returns>
        public bool Toggle(Pagemark mark)
        {
            var index = Marks.IndexOf(mark);
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
        /// 指定フォルダのマーカー収集
        /// </summary>
        /// <param name="place"></param>
        /// <returns></returns>
        public List<Pagemark> Collect(string place)
        {
            return Marks.Where(e => e.Place == place).ToList();
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

            [DataMember]
            public List<Pagemark> Marks { get; set; }

            private void Constructor()
            {
                Items = new List<Book.Memento>();
                Marks = new List<Pagemark>();
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
            Validate();

            var memento = new Memento();

            memento.Items = removeTemporary
                ? this.Items.Select(e => e.Value.Memento).Where(e => !e.Place.StartsWith(Temporary.TempDirectory)).ToList()
                : this.Items.Select(e => e.Value.Memento).ToList();

            memento.Marks = removeTemporary
                ? this.Marks.Where(e => !e.Place.StartsWith(Temporary.TempDirectory)).ToList()
                : this.Marks.ToList();

            return memento;
        }

        // memento適用
        public void Restore(Memento memento)
        {
            this.Marks = new ObservableCollection<Pagemark>(memento.Marks);

            this.Load(memento.Items);
        }

        #endregion
    }

    /// <summary>
    /// ページマーカー
    /// </summary>
    [DataContract]
    public class Pagemark : IEquatable<Pagemark>, IHasPage
    {
        [DataMember]
        public string Place { get; set; }
        [DataMember]
        public string EntryName { get; set; }

        //
        public string PlaceShort => LoosePath.GetFileName(Place);
        //
        public string PageShort => LoosePath.GetFileName(EntryName);

        // for ToolTops
        public string Detail => Place + "\n" + EntryName;


        public Pagemark()
        { }

        public Pagemark(string place, string page)
        {
            Place = place;
            EntryName = page;
        }


        //otherと自分自身が等価のときはtrueを返す
        public bool Equals(Pagemark other)
        {
            //objがnullのときは、等価でない
            if (other == null)
            {
                return false;
            }

            //Numberで比較する
            return (this.Place == other.Place && this.EntryName == other.EntryName);
        }

        //objと自分自身が等価のときはtrueを返す
        public override bool Equals(object obj)
        {
            //objがnullか、型が違うときは、等価でない
            if (obj == null || this.GetType() != obj.GetType())
            {
                return false;
            }

            return this.Equals((Pagemark)obj);
        }

        //Equalsがtrueを返すときに同じ値を返す
        public override int GetHashCode()
        {
            return this.Place.GetHashCode() ^ this.EntryName.GetHashCode();
        }


        #region for Thumbnail

        public static event EventHandler<Page> ThumbnailChanged;

        // サムネイル用。保存しません
        #region Property: ArchivePage
        private ArchivePage _ArchivePage;
        public ArchivePage ArchivePage
        {
            get
            {
                if (_ArchivePage == null)
                {
                    _ArchivePage = new ArchivePage(Place, EntryName);
                    _ArchivePage.ThumbnailChanged += (s, e) => ThumbnailChanged?.Invoke(this, _ArchivePage);
                }
                return _ArchivePage;
            }
            set { _ArchivePage = value; }
        }
        #endregion

        public Page GetPage()
        {
            return ArchivePage;
        }
        #endregion
    }


}