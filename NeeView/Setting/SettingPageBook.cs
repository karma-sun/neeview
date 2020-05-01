using NeeView.Windows.Property;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NeeView.Setting
{
    /// <summary>
    /// Setting: Book
    /// </summary>
    public class SettingPageBook : SettingPage
    {
        public SettingPageBook() : base(Properties.Resources.SettingPageBook)
        {
            this.Children = new List<SettingPage>
            {
                new SettingPageBookPageSetting(),
                new SettingPageBookMove(),
            };

            var section = new SettingItemSection(Properties.Resources.SettingPageBookGeneral);
            section.Children.Add(new SettingItemProperty(PropertyMemberElement.Create(Config.Current.System, nameof(SystemConfig.IsOpenbookAtCurrentPlace))));
            section.Children.Add(new SettingItemProperty(PropertyMemberElement.Create(Config.Current.Book, nameof(BookConfig.Excludes)), new SettingItemCollectionControl() { Collection = Config.Current.Book.Excludes, AddDialogHeader = Properties.Resources.WordExcludePath }));
            section.Children.Add(new SettingItemProperty(PropertyMemberElement.Create(Config.Current.Book, nameof(BookConfig.WideRatio))));
            section.Children.Add(new SettingItemProperty(PropertyMemberElement.Create(Config.Current.Book, nameof(BookConfig.ContentsSpace))));
            section.Children.Add(new SettingItemProperty(PropertyMemberElement.Create(Config.Current.Book, nameof(BookConfig.BookPageSize))));
            section.Children.Add(new SettingItemProperty(PropertyMemberElement.Create(Config.Current.Book, nameof(BookConfig.IsSortFileFirst))));
            section.Children.Add(new SettingItemProperty(PropertyMemberElement.Create(Config.Current.Book, nameof(BookConfig.ResetPageWhenRandomSort))));
            section.Children.Add(new SettingItemProperty(PropertyMemberElement.Create(Config.Current.Book, nameof(BookConfig.IsInsertDummyPage))));
            this.Items = new List<SettingItem>() { section };
        }
    }

    /// <summary>
    /// SettingPage: BookMove
    /// </summary>
    public class SettingPageBookMove : SettingPage
    {
        public SettingPageBookMove() : base(Properties.Resources.SettingPageBookMove)
        {
            this.Items = new List<SettingItem>();

            var section = new SettingItemSection(Properties.Resources.SettingPageBookMoveBook);
            section.Children.Add(new SettingItemProperty(PropertyMemberElement.Create(Config.Current.Bookshelf, nameof(BookshelfConfig.IsCruise))));
            this.Items.Add(section);

            section = new SettingItemSection(Properties.Resources.SettingPageBookMovePage);
            section.Children.Add(new SettingItemProperty(PropertyMemberElement.Create(Config.Current.Book, nameof(BookConfig.IsPrioritizePageMove))));
            section.Children.Add(new SettingItemProperty(PropertyMemberElement.Create(Config.Current.Book, nameof(BookConfig.IsMultiplePageMove))));
            section.Children.Add(new SettingItemProperty(PropertyMemberElement.Create(Config.Current.Book, nameof(BookConfig.PageEndAction))));
            section.Children.Add(new SettingItemProperty(PropertyMemberElement.Create(Config.Current.Book, nameof(BookConfig.IsNotifyPageLoop))));
            section.Children.Add(new SettingItemProperty(PropertyMemberElement.Create(Config.Current.Book, nameof(BookConfig.TerminalSound))));
            this.Items.Add(section);
        }
    }

    /// <summary>
    /// SettingPage: BookPageSetting
    /// </summary>
    public class SettingPageBookPageSetting : SettingPage
    {
        public SettingPageBookPageSetting() : base(Properties.Resources.SettingPageBookPageSetting)
        {
            this.Items = new List<SettingItem>();

            var section = new SettingItemSection(Properties.Resources.SettingPageBookPageSetting, Properties.Resources.SettingPageBookPageSettingTips);
            section.Children.Add(new SettingItemMultiProperty(
                PropertyMemberElement.Create(BookSettingPresenter.Current.DefaultSetting, nameof(BookSettingConfig.Page)),
                PropertyMemberElement.Create(BookSettingPresenter.Current.Generater, nameof(BookSettingPolicyConfig.Page)))
            {
                Content1 = Properties.Resources.WordFirstPage,
            });
            section.Children.Add(new SettingItemMultiProperty(
                PropertyMemberElement.Create(BookSettingPresenter.Current.DefaultSetting, nameof(BookSettingConfig.SortMode)),
                PropertyMemberElement.Create(BookSettingPresenter.Current.Generater, nameof(BookSettingPolicyConfig.SortMode))));
            section.Children.Add(new SettingItemMultiProperty(
                PropertyMemberElement.Create(BookSettingPresenter.Current.DefaultSetting, nameof(BookSettingConfig.PageMode)),
                PropertyMemberElement.Create(BookSettingPresenter.Current.Generater, nameof(BookSettingPolicyConfig.PageMode))));
            section.Children.Add(new SettingItemMultiProperty(
                PropertyMemberElement.Create(BookSettingPresenter.Current.DefaultSetting, nameof(BookSettingConfig.BookReadOrder)),
                PropertyMemberElement.Create(BookSettingPresenter.Current.Generater, nameof(BookSettingPolicyConfig.BookReadOrder))));
            section.Children.Add(new SettingItemMultiProperty(
                PropertyMemberElement.Create(BookSettingPresenter.Current.DefaultSetting, nameof(BookSettingConfig.IsSupportedDividePage)),
                PropertyMemberElement.Create(BookSettingPresenter.Current.Generater, nameof(BookSettingPolicyConfig.IsSupportedDividePage))));
            section.Children.Add(new SettingItemMultiProperty(
                PropertyMemberElement.Create(BookSettingPresenter.Current.DefaultSetting, nameof(BookSettingConfig.IsSupportedWidePage)),
                PropertyMemberElement.Create(BookSettingPresenter.Current.Generater, nameof(BookSettingPolicyConfig.IsSupportedWidePage))));
            section.Children.Add(new SettingItemMultiProperty(
                PropertyMemberElement.Create(BookSettingPresenter.Current.DefaultSetting, nameof(BookSettingConfig.IsSupportedSingleFirstPage)),
                PropertyMemberElement.Create(BookSettingPresenter.Current.Generater, nameof(BookSettingPolicyConfig.IsSupportedSingleFirstPage))));
            section.Children.Add(new SettingItemMultiProperty(
                PropertyMemberElement.Create(BookSettingPresenter.Current.DefaultSetting, nameof(BookSettingConfig.IsSupportedSingleLastPage)),
                PropertyMemberElement.Create(BookSettingPresenter.Current.Generater, nameof(BookSettingPolicyConfig.IsSupportedSingleLastPage))));
            section.Children.Add(new SettingItemMultiProperty(
                PropertyMemberElement.Create(BookSettingPresenter.Current.DefaultSetting, nameof(BookSettingConfig.IsRecursiveFolder)),
                PropertyMemberElement.Create(BookSettingPresenter.Current.Generater, nameof(BookSettingPolicyConfig.IsRecursiveFolder))));
            this.Items.Add(section);

            section = new SettingItemSection(Properties.Resources.SettingPageBookSubFolder);
            section.Children.Add(new SettingItemProperty(PropertyMemberElement.Create(Config.Current.Book, nameof(BookConfig.IsConfirmRecursive))));
            section.Children.Add(new SettingItemProperty(PropertyMemberElement.Create(Config.Current.Book, nameof(BookConfig.IsAutoRecursive))));
            this.Items.Add(section);
        }

    }

}
