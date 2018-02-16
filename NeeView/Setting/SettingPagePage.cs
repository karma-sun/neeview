using NeeView.Windows.Property;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NeeView.Setting
{
    public class SettingPagePage : SettingPage
    {
        public SettingPagePage() : base("ページ設定")
        {
            this.Children = new List<SettingPage>
            {
                new SettingPagePageDefault(),
                new SettingPagePageRecovery(),
            };
        }
    }

    public class SettingPagePageDefault : SettingPage
    {
        public SettingPagePageDefault() : base("既定のページ設定")
        {
            this.Items = new List<SettingItem>
            {
                new SettingItemSection("既定のページ設定", "「ページ設定の初期化」で使用される設定です。",
                    new SettingItemProperty(PropertyMemberElement.Create(BookSetting.Current.BookMementoDefault, nameof(Book.Memento.PageMode))),
                    new SettingItemProperty(PropertyMemberElement.Create(BookSetting.Current.BookMementoDefault, nameof(Book.Memento.BookReadOrder))),
                    new SettingItemProperty(PropertyMemberElement.Create(BookSetting.Current.BookMementoDefault, nameof(Book.Memento.IsSupportedDividePage))),
                    new SettingItemProperty(PropertyMemberElement.Create(BookSetting.Current.BookMementoDefault, nameof(Book.Memento.IsSupportedWidePage))),
                    new SettingItemProperty(PropertyMemberElement.Create(BookSetting.Current.BookMementoDefault, nameof(Book.Memento.IsSupportedSingleFirstPage))),
                    new SettingItemProperty(PropertyMemberElement.Create(BookSetting.Current.BookMementoDefault, nameof(Book.Memento.IsSupportedSingleLastPage))),
                    new SettingItemProperty(PropertyMemberElement.Create(BookSetting.Current.BookMementoDefault, nameof(Book.Memento.IsRecursiveFolder))),
                    new SettingItemProperty(PropertyMemberElement.Create(BookSetting.Current.BookMementoDefault, nameof(Book.Memento.SortMode)))),
            };
        }
    }

    public class SettingPagePageRecovery : SettingPage
    {
        public SettingPagePageRecovery() : base("復元項目")
        {
            this.Items = new List<SettingItem>
            {
                new SettingItemSection("履歴、ブックマークから復元するページ設定項目", "開いたことがあるブックの場合、前回の情報から設定の復元をします。復元しない項目は既定のページ設定もしくは直前の設定が使用されます。",
                    new SettingItemProperty(PropertyMemberElement.Create(BookSetting.Current.HistoryMementoFilter, nameof(BookMementoFilter.Page))),
                    new SettingItemProperty(PropertyMemberElement.Create(BookSetting.Current.HistoryMementoFilter, nameof(BookMementoFilter.PageMode))),
                    new SettingItemProperty(PropertyMemberElement.Create(BookSetting.Current.HistoryMementoFilter, nameof(BookMementoFilter.BookReadOrder))),
                    new SettingItemProperty(PropertyMemberElement.Create(BookSetting.Current.HistoryMementoFilter, nameof(BookMementoFilter.IsSupportedDividePage))),
                    new SettingItemProperty(PropertyMemberElement.Create(BookSetting.Current.HistoryMementoFilter, nameof(BookMementoFilter.IsSupportedWidePage))),
                    new SettingItemProperty(PropertyMemberElement.Create(BookSetting.Current.HistoryMementoFilter, nameof(BookMementoFilter.IsSupportedSingleFirstPage))),
                    new SettingItemProperty(PropertyMemberElement.Create(BookSetting.Current.HistoryMementoFilter, nameof(BookMementoFilter.IsSupportedSingleLastPage))),
                    new SettingItemProperty(PropertyMemberElement.Create(BookSetting.Current.HistoryMementoFilter, nameof(BookMementoFilter.IsRecursiveFolder))),
                    new SettingItemProperty(PropertyMemberElement.Create(BookSetting.Current.HistoryMementoFilter, nameof(BookMementoFilter.SortMode)))),

                new SettingItemSection("既定のページ設定の使用",
                    new SettingItemProperty(PropertyMemberElement.Create(BookSetting.Current, nameof(BookSetting.IsUseBookMementoDefault)))),
            };
        }
    }
}
