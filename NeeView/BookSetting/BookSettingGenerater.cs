using NeeView.Windows.Property;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Runtime.Serialization;

namespace NeeView
{
    [DataContract]
    public class BookSettingGenerater
    {
        public BookSettingGenerater()
        {
            Selector = CreateDefaultFilter();
        }

        public Dictionary<BookSettingKey, BookSettingSelectMode> Selector { get; private set; }


        [DataMember]
        [PropertyMember]
        public BookSettingPageSelectMode Page
        {
            get => Selector[BookSettingKey.Page].ToPageSelectMode();
            set => Selector[BookSettingKey.Page] = value.ToNormalSelectMode();
        }

        [DataMember]
        [PropertyMember]
        public BookSettingSelectMode PageMode
        {
            get => Selector[BookSettingKey.PageMode];
            set => Selector[BookSettingKey.PageMode] = value;
        }

        [DataMember]
        [PropertyMember]
        public BookSettingSelectMode BookReadOrder
        {
            get => Selector[BookSettingKey.BookReadOrder];
            set => Selector[BookSettingKey.BookReadOrder] = value;
        }

        [DataMember]
        [PropertyMember]
        public BookSettingSelectMode IsSupportedDividePage
        {
            get => Selector[BookSettingKey.IsSupportedDividePage];
            set => Selector[BookSettingKey.IsSupportedDividePage] = value;
        }

        [DataMember]
        [PropertyMember]
        public BookSettingSelectMode IsSupportedSingleFirstPage
        {
            get => Selector[BookSettingKey.IsSupportedSingleFirstPage];
            set => Selector[BookSettingKey.IsSupportedSingleFirstPage] = value;
        }

        [DataMember]
        [PropertyMember]
        public BookSettingSelectMode IsSupportedSingleLastPage
        {
            get => Selector[BookSettingKey.IsSupportedSingleLastPage];
            set => Selector[BookSettingKey.IsSupportedSingleLastPage] = value;
        }

        [DataMember]
        [PropertyMember]
        public BookSettingSelectMode IsSupportedWidePage
        {
            get => Selector[BookSettingKey.IsSupportedWidePage];
            set => Selector[BookSettingKey.IsSupportedWidePage] = value;
        }

        [DataMember]
        [PropertyMember]
        public BookSettingSelectMode IsRecursiveFolder
        {
            get => Selector[BookSettingKey.IsRecursiveFolder];
            set => Selector[BookSettingKey.IsRecursiveFolder] = value;
        }

        [DataMember]
        [PropertyMember]
        public BookSettingSelectMode SortMode
        {
            get => Selector[BookSettingKey.SortMode];
            set => Selector[BookSettingKey.SortMode] = value;
        }


        [OnDeserializing]
        private void Deserializing(StreamingContext c)
        {
            Selector = CreateDefaultFilter();
        }


        private Dictionary<BookSettingKey, BookSettingSelectMode> CreateDefaultFilter()
        {
            return new Dictionary<BookSettingKey, BookSettingSelectMode>()
            {
                [BookSettingKey.Page] = BookSettingSelectMode.RestoreOrDefault,
                [BookSettingKey.PageMode] = BookSettingSelectMode.RestoreOrDefault,
                [BookSettingKey.BookReadOrder] = BookSettingSelectMode.RestoreOrDefault,
                [BookSettingKey.IsSupportedDividePage] = BookSettingSelectMode.RestoreOrDefault,
                [BookSettingKey.IsSupportedSingleFirstPage] = BookSettingSelectMode.RestoreOrDefault,
                [BookSettingKey.IsSupportedSingleLastPage] = BookSettingSelectMode.RestoreOrDefault,
                [BookSettingKey.IsSupportedWidePage] = BookSettingSelectMode.RestoreOrDefault,
                [BookSettingKey.IsRecursiveFolder] = BookSettingSelectMode.RestoreOrDefault,
                [BookSettingKey.SortMode] = BookSettingSelectMode.RestoreOrDefault,
            };
        }

        /// <summary>
        /// BookSetting生成
        /// </summary>
        /// <param name="def">標準設定</param>
        /// <param name="current">現在設定</param>
        /// <param name="restore">記録設定</param>
        /// <param name="isDefaultRecursive">再帰、ただし記録優先。フォルダーの既定が再帰設定の場合</param>
        /// <returns></returns>
        public BookSetting Mix(BookSetting def, BookSetting current, BookSetting restore, bool isDefaultRecursive)
        {
            Debug.Assert(def != null);
            Debug.Assert(current != null);
            Debug.Assert(current.Page == null);

            BookSetting param = new BookSetting();

            foreach (BookSettingKey key in Enum.GetValues(typeof(BookSettingKey)))
            {
                switch (Selector[key])
                {
                    case BookSettingSelectMode.Default:
                        param[key] = def[key];
                        break;

                    case BookSettingSelectMode.Continue:
                        param[key] = current[key];
                        break;

                    case BookSettingSelectMode.RestoreOrDefault:
                        param[key] = restore != null ? restore[key] : def[key];
                        break;

                    case BookSettingSelectMode.RestoreOrContinue:
                        param[key] = restore != null ? restore[key] : current[key];
                        break;
                }
            }

            if (isDefaultRecursive && restore == null)
            {
                param.IsRecursiveFolder = true;
            }

            return param;
        }

        public BookSettingGenerater Clone()
        {
            var clone = (BookSettingGenerater)MemberwiseClone();
            clone.Selector = new Dictionary<BookSettingKey, BookSettingSelectMode>(Selector);
            return clone;
        }
    }
}
