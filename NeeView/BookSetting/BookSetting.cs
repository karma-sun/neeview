using NeeView.Windows.Property;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace NeeView
{
    [DataContract]
    public class BookSetting
    {
        public BookSetting()
        {
            Constructor();
        }

        private void Constructor()
        {
            Items = new Dictionary<BookSettingKey, object>
            {
                [BookSettingKey.Page] = null,
                [BookSettingKey.PageMode] = PageMode.SinglePage,
                [BookSettingKey.BookReadOrder] = PageReadOrder.RightToLeft,
                [BookSettingKey.IsSupportedDividePage] = false,
                [BookSettingKey.IsSupportedSingleFirstPage] = false,
                [BookSettingKey.IsSupportedSingleLastPage] = false,
                [BookSettingKey.IsSupportedWidePage] = true,
                [BookSettingKey.IsRecursiveFolder] = false,
                [BookSettingKey.SortMode] = PageSortMode.FileName,
            };
        }

        public object this[BookSettingKey key]
        {
            get { return Items[key]; }
            set
            {
                Items[key] = value;
            }
        }


        public Dictionary<BookSettingKey, object> Items { get; private set; }


        [PropertyMember("@ParamBookPage")]
        public string Page
        {
            get => GetValue<string>(BookSettingKey.Page);
            set => SetValue<string>(BookSettingKey.Page, value);
        }

        [DataMember]
        [PropertyMember("@ParamBookPageMode")]
        public PageMode PageMode
        {
            get => GetValue<PageMode>(BookSettingKey.PageMode);
            set => SetValue<PageMode>(BookSettingKey.PageMode, value);
        }

        [DataMember]
        [PropertyMember("@ParamBookBookReadOrder")]
        public PageReadOrder BookReadOrder
        {
            get => GetValue<PageReadOrder>(BookSettingKey.BookReadOrder);
            set => SetValue<PageReadOrder>(BookSettingKey.BookReadOrder, value);
        }

        [DataMember]
        [PropertyMember("@ParamBookIsSupportedDividePage")]
        public bool IsSupportedDividePage
        {
            get => GetValue<bool>(BookSettingKey.IsSupportedDividePage);
            set => SetValue<bool>(BookSettingKey.IsSupportedDividePage, value);
        }

        [DataMember]
        [PropertyMember("@ParamBookIsSupportedSingleFirstPage")]
        public bool IsSupportedSingleFirstPage
        {
            get => GetValue<bool>(BookSettingKey.IsSupportedSingleFirstPage);
            set => SetValue<bool>(BookSettingKey.IsSupportedSingleFirstPage, value);
        }

        [DataMember]
        [PropertyMember("@ParamBookIsSupportedSingleLastPage")]
        public bool IsSupportedSingleLastPage
        {
            get => GetValue<bool>(BookSettingKey.IsSupportedSingleLastPage);
            set => SetValue<bool>(BookSettingKey.IsSupportedSingleLastPage, value);
        }

        [DataMember]
        [PropertyMember("@ParamBookIsSupportedWidePage")]
        public bool IsSupportedWidePage
        {
            get => GetValue<bool>(BookSettingKey.IsSupportedWidePage);
            set => SetValue<bool>(BookSettingKey.IsSupportedWidePage, value);
        }

        [DataMember]
        [PropertyMember("@ParamBookIsRecursiveFolder", Tips = "@ParamBookIsRecursiveFolderTips")]
        public bool IsRecursiveFolder
        {
            get => GetValue<bool>(BookSettingKey.IsRecursiveFolder);
            set => SetValue<bool>(BookSettingKey.IsRecursiveFolder, value);
        }

        [DataMember]
        [PropertyMember("@ParamBookSortMode")]
        public PageSortMode SortMode
        {
            get => GetValue<PageSortMode>(BookSettingKey.SortMode);
            set => SetValue<PageSortMode>(BookSettingKey.SortMode, value);
        }


        [OnDeserializing]
        private void Deserializing(StreamingContext c)
        {
            Constructor();
        }


        public T GetValue<T>(BookSettingKey key)
        {
            return (T)Items[key];
        }

        public void SetValue<T>(BookSettingKey key, T value)
        {
            Items[key] = value;
        }

        public BookSetting Clone()
        {
            var clone = (BookSetting)this.MemberwiseClone();
            clone.Items = new Dictionary<BookSettingKey, object>(Items);
            return clone;
        }

        public static BookSetting FromBookMement(Book.Memento memento)
        {
            if (memento == null) return null;

            var collection = new BookSetting();

            collection.Page = memento.Page;
            collection.PageMode = memento.PageMode;
            collection.BookReadOrder = memento.BookReadOrder;
            collection.IsSupportedDividePage = memento.IsSupportedDividePage;
            collection.IsSupportedSingleFirstPage = memento.IsSupportedSingleFirstPage;
            collection.IsSupportedSingleLastPage = memento.IsSupportedSingleLastPage;
            collection.IsSupportedWidePage = memento.IsSupportedWidePage;
            collection.IsRecursiveFolder = memento.IsRecursiveFolder;
            collection.SortMode = memento.SortMode;

            return collection;
        }

        public Book.Memento ToBookMemento()
        {
            var memento = new Book.Memento();

            memento.Page = this.Page;
            memento.PageMode = this.PageMode;
            memento.BookReadOrder = this.BookReadOrder;
            memento.IsSupportedDividePage = this.IsSupportedDividePage;
            memento.IsSupportedSingleFirstPage = this.IsSupportedSingleFirstPage;
            memento.IsSupportedSingleLastPage = this.IsSupportedSingleLastPage;
            memento.IsSupportedWidePage = this.IsSupportedWidePage;
            memento.IsRecursiveFolder = this.IsRecursiveFolder;
            memento.SortMode = this.SortMode;

            return memento;
        }
    }
}
