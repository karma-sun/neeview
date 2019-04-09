﻿using NeeView.Windows.Property;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;

namespace NeeView
{
    /// <summary>
    /// Book設定項目番号
    /// </summary>
    public enum BookMementoBit
    {
        // 現在ページ
        Page,

        // 1ページ表示 or 2ページ表示
        PageMode,

        // 右開き or 左開き
        BookReadOrder,

        // 横長ページ分割 (1ページモード)
        IsSupportedDividePage,

        // 最初のページを単独表示 
        IsSupportedSingleFirstPage,

        // 最後のページを単独表示
        IsSupportedSingleLastPage,

        // 横長ページを2ページ分とみなす(2ページモード)
        IsSupportedWidePage,

        // フォルダーの再帰
        IsRecursiveFolder,

        // ページ並び順
        SortMode,
    };

    /// <summary>
    /// Book設定フィルタ
    /// </summary>
    [DataContract]
    public class BookMementoFilter
    {
        [DataMember]
        public Dictionary<BookMementoBit, bool> Flags { get; set; }


        [PropertyMember("@ParamBookFilterPage")]
        public bool Page
        {
            get { return Flags[BookMementoBit.Page]; }
            set { Flags[BookMementoBit.Page] = value; }
        }

        [PropertyMember("@ParamBookFilterPageMode")]
        public bool PageMode
        {
            get { return Flags[BookMementoBit.PageMode]; }
            set { Flags[BookMementoBit.PageMode] = value; }
        }

        [PropertyMember("@ParamBookFilterBookReadOrder")]
        public bool BookReadOrder
        {
            get { return Flags[BookMementoBit.BookReadOrder]; }
            set { Flags[BookMementoBit.BookReadOrder] = value; }
        }

        [PropertyMember("@ParamBookFilterIsSupportedDividePage")]
        public bool IsSupportedDividePage
        {
            get { return Flags[BookMementoBit.IsSupportedDividePage]; }
            set { Flags[BookMementoBit.IsSupportedDividePage] = value; }
        }

        [PropertyMember("@ParamBookFilterIsSupportedSingleFirstPage")]
        public bool IsSupportedSingleFirstPage
        {
            get { return Flags[BookMementoBit.IsSupportedSingleFirstPage]; }
            set { Flags[BookMementoBit.IsSupportedSingleFirstPage] = value; }
        }

        [PropertyMember("@ParamBookFilterIsSupportedSingleLastPage")]
        public bool IsSupportedSingleLastPage
        {
            get { return Flags[BookMementoBit.IsSupportedSingleLastPage]; }
            set { Flags[BookMementoBit.IsSupportedSingleLastPage] = value; }
        }

        [PropertyMember("@ParamBookFilterIsSupportedWidePage")]
        public bool IsSupportedWidePage
        {
            get { return Flags[BookMementoBit.IsSupportedWidePage]; }
            set { Flags[BookMementoBit.IsSupportedWidePage] = value; }
        }

        [PropertyMember("@ParamBookFilterIsRecursiveFolder")]
        public bool IsRecursiveFolder
        {
            get { return Flags[BookMementoBit.IsRecursiveFolder]; }
            set { Flags[BookMementoBit.IsRecursiveFolder] = value; }
        }

        [PropertyMember("@ParamBookFilterSortMode")]
        public bool SortMode
        {
            get { return Flags[BookMementoBit.SortMode]; }
            set { Flags[BookMementoBit.SortMode] = value; }
        }

        //
        public BookMementoFilter(bool def = false)
        {
            Flags = Enum.GetValues(typeof(BookMementoBit)).OfType<BookMementoBit>().ToDictionary(e => e, e => def);
        }

        /// <summary>
        /// デシリアライズ終端処理
        /// </summary>
        /// <param name="c"></param>
        [OnDeserialized]
        private void Deserialized(StreamingContext c)
        {
            // 項目数の保証
            foreach (BookMementoBit key in Enum.GetValues(typeof(BookMementoBit)))
            {
                if (!Flags.ContainsKey(key)) Flags.Add(key, true);
            }
        }
    }

}