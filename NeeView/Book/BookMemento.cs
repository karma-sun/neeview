using NeeView.Windows.Property;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.Serialization;

namespace NeeView
{
    public partial class Book
    {
        [DataContract]
        public class Memento
        {
            // フォルダーの場所
            [DataMember(EmitDefaultValue = false)]
            public string Place { get; set; }

            // ディレクトリ？
            [DataMember(EmitDefaultValue = false)]
            public bool IsDirectorty { get; set; }

            // 名前
            public string Name => Place.EndsWith(@":\") ? Place : System.IO.Path.GetFileName(Place);

            // 現在ページ
            [DataMember(EmitDefaultValue = false)]
            public string Page { get; set; }

            // 1ページ表示 or 2ページ表示
            [DataMember(Name = "PageModeV2")]
            [PropertyMember("@ParamBookPageMode")]
            public PageMode PageMode { get; set; }

            // 右開き or 左開き
            [DataMember]
            [PropertyMember("@ParamBookBookReadOrder")]
            public PageReadOrder BookReadOrder { get; set; }

            // 横長ページ分割 (1ページモード)
            [DataMember]
            [PropertyMember("@ParamBookIsSupportedDividePage")]
            public bool IsSupportedDividePage { get; set; }

            // 最初のページを単独表示 
            [DataMember]
            [PropertyMember("@ParamBookIsSupportedSingleFirstPage")]
            public bool IsSupportedSingleFirstPage { get; set; }

            // 最後のページを単独表示
            [DataMember]
            [PropertyMember("@ParamBookIsSupportedSingleLastPage")]
            public bool IsSupportedSingleLastPage { get; set; }

            // 横長ページを2ページ分とみなす(2ページモード)
            [DataMember]
            [PropertyMember("@ParamBookIsSupportedWidePage")]
            public bool IsSupportedWidePage { get; set; } = true;

            // フォルダーの再帰
            [DataMember]
            [PropertyMember("@ParamBookIsRecursiveFolder", Tips = "@ParamBookIsRecursiveFolderTips")]
            public bool IsRecursiveFolder { get; set; }

            // ページ並び順
            [DataMember]
            [PropertyMember("@ParamBookSortMode")]
            public PageSortMode SortMode { get; set; }

            // 最終アクセス日
            [Obsolete]
            [DataMember(Order = 12, EmitDefaultValue = false)]
            public DateTime LastAccessTime { get; set; }


            /// <summary>
            /// 複製
            /// </summary>
            public Memento Clone()
            {
                return (Memento)this.MemberwiseClone();
            }


            /// <summary>
            /// 項目のフィルタリング。フラグの立っている項目を上書き
            /// </summary>
            /// <param name="filter">フィルタービット列</param>
            /// <param name="overwrite">上書き既定値</param>
            public void Write(BookMementoFilter filter, Memento overwrite)
            {
                // 現在ページ
                if (filter.Flags[BookMementoBit.Page])
                {
                    this.Page = overwrite.Page;
                }
                // 1ページ表示 or 2ページ表示
                if (filter.Flags[BookMementoBit.PageMode])
                {
                    this.PageMode = overwrite.PageMode;
                }
                // 右開き or 左開き
                if (filter.Flags[BookMementoBit.BookReadOrder])
                {
                    this.BookReadOrder = overwrite.BookReadOrder;
                }
                // 横長ページ分割 (1ページモード)
                if (filter.Flags[BookMementoBit.IsSupportedDividePage])
                {
                    this.IsSupportedDividePage = overwrite.IsSupportedDividePage;
                }
                // 最初のページを単独表示 
                if (filter.Flags[BookMementoBit.IsSupportedSingleFirstPage])
                {
                    this.IsSupportedSingleFirstPage = overwrite.IsSupportedSingleFirstPage;
                }
                // 最後のページを単独表示
                if (filter.Flags[BookMementoBit.IsSupportedSingleLastPage])
                {
                    this.IsSupportedSingleLastPage = overwrite.IsSupportedSingleLastPage;
                }
                // 横長ページを2ページ分とみなす(2ページモード)
                if (filter.Flags[BookMementoBit.IsSupportedWidePage])
                {
                    this.IsSupportedWidePage = overwrite.IsSupportedWidePage;
                }
                // フォルダーの再帰
                if (filter.Flags[BookMementoBit.IsRecursiveFolder])
                {
                    this.IsRecursiveFolder = overwrite.IsRecursiveFolder;
                }
                // ページ並び順
                if (filter.Flags[BookMementoBit.SortMode])
                {
                    this.SortMode = overwrite.SortMode;
                }
            }


            // 保存用バリデート
            // このmementoは履歴とデフォルト設定の２つに使われるが、デフォルト設定には本の場所やページ等は不要
            public void ValidateForDefault()
            {
                Place = null;
                Page = null;
                IsDirectorty = false;
            }

            // バリデートされたクローン
            public Memento ValidatedClone()
            {
                var clone = this.Clone();
                clone.ValidateForDefault();
                return clone;
            }
        }

        // 重複チェック用
        public class MementoPlaceCompare : IEqualityComparer<Memento>
        {
            public bool Equals(Memento m1, Memento m2)
            {
                if (m1 == null && m2 == null)
                    return true;
                else if (m1 == null | m2 == null)
                    return false;
                else if (m1.Place == m2.Place)
                    return true;
                else
                    return false;
            }

            public int GetHashCode(Memento m)
            {
                return m.Place.GetHashCode();
            }
        }
    }
}

