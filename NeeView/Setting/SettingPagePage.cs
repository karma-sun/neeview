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
        public SettingPagePage() : base(Properties.Resources.SettingPagePage)
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
        public SettingPagePageDefault() : base(Properties.Resources.SettingPagePageDefault)
        {
            this.Items = new List<SettingItem>
            {
                new SettingItemSection(Properties.Resources.SettingPagePageDefaultSetting, Properties.Resources.SettingPagePageDefaultSettingTips,
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
        public SettingPagePageRecovery() : base(Properties.Resources.SettingPagePageRecovery)
        {
            this.Items = new List<SettingItem>
            {
                new SettingItemSection(Properties.Resources.SettingPagePageRecoveryUseDefault,
                    new SettingItemProperty(PropertyMemberElement.Create(BookSetting.Current, nameof(BookSetting.IsUseBookMementoDefault)))),

                new SettingItemSection(Properties.Resources.SettingPagePageRecoveryItems, Properties.Resources.SettingPagePageRecoveryItemsTips,
                    new SettingItemSubProperty(PropertyMemberElement.Create(BookSetting.Current.HistoryMementoFilter, nameof(BookMementoFilter.Page))),
                    new SettingItemSubProperty(PropertyMemberElement.Create(BookSetting.Current.HistoryMementoFilter, nameof(BookMementoFilter.PageMode))),
                    new SettingItemSubProperty(PropertyMemberElement.Create(BookSetting.Current.HistoryMementoFilter, nameof(BookMementoFilter.BookReadOrder))),
                    new SettingItemSubProperty(PropertyMemberElement.Create(BookSetting.Current.HistoryMementoFilter, nameof(BookMementoFilter.IsSupportedDividePage))),
                    new SettingItemSubProperty(PropertyMemberElement.Create(BookSetting.Current.HistoryMementoFilter, nameof(BookMementoFilter.IsSupportedWidePage))),
                    new SettingItemSubProperty(PropertyMemberElement.Create(BookSetting.Current.HistoryMementoFilter, nameof(BookMementoFilter.IsSupportedSingleFirstPage))),
                    new SettingItemSubProperty(PropertyMemberElement.Create(BookSetting.Current.HistoryMementoFilter, nameof(BookMementoFilter.IsSupportedSingleLastPage))),
                    new SettingItemSubProperty(PropertyMemberElement.Create(BookSetting.Current.HistoryMementoFilter, nameof(BookMementoFilter.IsRecursiveFolder))),
                    new SettingItemSubProperty(PropertyMemberElement.Create(BookSetting.Current.HistoryMementoFilter, nameof(BookMementoFilter.SortMode)))),
            };
        }
    }
}
