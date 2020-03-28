using NeeLaboratory.ComponentModel;
using NeeView.Windows.Property;
using System;
using System.Text.Json.Serialization;

namespace NeeView
{
    public class BookSettingConfig : BindableBase, ICloneable, IEquatable<BookSettingConfig>
    {
        // ページ
        [JsonIgnore, PropertyMapIgnoreAttribute]
        [PropertyMember("@ParamBookPage")]
        public string Page
        {
            get { return null; }
            set { }
        }

        // 1ページ表示 or 2ページ表示
        [PropertyMember("@ParamBookPageMode")]
        public PageMode PageMode { get; set; } = PageMode.SinglePage;

        // 右開き or 左開き
        [PropertyMember("@ParamBookBookReadOrder")]
        public PageReadOrder BookReadOrder { get; set; } = PageReadOrder.RightToLeft;

        // 横長ページ分割 (1ページモード)
        [PropertyMember("@ParamBookIsSupportedDividePage")]
        public bool IsSupportedDividePage { get; set; }

        // 最初のページを単独表示 
        [PropertyMember("@ParamBookIsSupportedSingleFirstPage")]
        public bool IsSupportedSingleFirstPage { get; set; }

        // 最後のページを単独表示
        [PropertyMember("@ParamBookIsSupportedSingleLastPage")]
        public bool IsSupportedSingleLastPage { get; set; }

        // 横長ページを2ページ分とみなす(2ページモード)
        [PropertyMember("@ParamBookIsSupportedWidePage")]
        public bool IsSupportedWidePage { get; set; } = true;

        // フォルダーの再帰
        [PropertyMember("@ParamBookIsRecursiveFolder", Tips = "@ParamBookIsRecursiveFolderTips")]
        public bool IsRecursiveFolder { get; set; }

        // ページ並び順
        [PropertyMember("@ParamBookSortMode")]
        public PageSortMode SortMode { get; set; } = PageSortMode.FileName;


        public object Clone()
        {
            return MemberwiseClone();
        }

        public bool Equals(BookSettingConfig other)
        {
            return other != null &&
                this.Page == other.Page &&
                this.PageMode == other.PageMode &&
                this.BookReadOrder == other.BookReadOrder &&
                this.IsSupportedDividePage == other.IsSupportedDividePage &&
                this.IsSupportedSingleFirstPage == other.IsSupportedSingleFirstPage &&
                this.IsSupportedSingleLastPage == other.IsSupportedSingleLastPage &&
                this.IsSupportedWidePage == other.IsSupportedWidePage &&
                this.IsRecursiveFolder == other.IsRecursiveFolder &&
                this.SortMode == other.SortMode;
        }
    }

}