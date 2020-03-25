using NeeView.Windows.Property;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NeeView.Setting
{
    public class SettingPageBook : SettingPage
    {
        public SettingPageBook() : base(Properties.Resources.SettingPageBook)
        {
            this.Children = new List<SettingPage>
            {
                new SettingPageBookVisual(),
                new SettingPageBookPageSetting(),
                new SettingPageBookMove(),
            };

            this.Items = new List<SettingItem>
            {
                new SettingItemSection(Properties.Resources.SettingPageBookGeneralGeneral,

                    new SettingItemProperty(PropertyMemberElement.Create(Config.Current.Layout.Background, nameof(BackgroundConfig.PageBackgroundColor))),
                    new SettingItemProperty(PropertyMemberElement.Create(Config.Current.System, nameof(SystemConfig.IsOpenbookAtCurrentPlace))),
                    new SettingItemProperty(PropertyMemberElement.Create(Config.Current.Book, nameof(BookConfig.Excludes)), new SettingItemCollectionControl() { Collection = Config.Current.Book.Excludes, AddDialogHeader=Properties.Resources.WordExcludePath }),
                    new SettingItemProperty(PropertyMemberElement.Create(Config.Current.Book, nameof(BookConfig.WideRatio)))),
            };
        }
    }

    public class SettingPageBookVisual : SettingPage
    {
        public SettingPageBookVisual() : base(Properties.Resources.SettingPageBookVisual)
        {
            this.Items = new List<SettingItem>
            {
                new SettingItemSection(Properties.Resources.SettingPageBookVisualVisual,
                    new SettingItemProperty(PropertyMemberElement.Create(Config.Current.Layout.Notice, nameof(NoticeConfig.IsBusyMarkEnabled))),
                    new SettingItemProperty(PropertyMemberElement.Create(Config.Current.System, nameof(SystemConfig.IsIgnoreImageDpi))),
                    new SettingItemProperty(PropertyMemberElement.Create(Config.Current.Book, nameof(BookConfig.ContentsSpace)))),
            };
        }
    }

    public class SettingPageBookMove : SettingPage
    {
        public SettingPageBookMove() : base(Properties.Resources.SettingPageBookMove)
        {
            this.Items = new List<SettingItem>
            {
                new SettingItemSection(Properties.Resources.SettingPageBookMoveBook,
                    new SettingItemProperty(PropertyMemberElement.Create(Config.Current.Layout.Bookshelf, nameof(BookshelfPanelConfig.IsCruise)))),

                new SettingItemSection(Properties.Resources.SettingPageBookMovePage,
                    new SettingItemProperty(PropertyMemberElement.Create(Config.Current.Book, nameof(BookConfig.IsPrioritizePageMove))),
                    new SettingItemProperty(PropertyMemberElement.Create(Config.Current.Book, nameof(BookConfig.IsMultiplePageMove))),
                    new SettingItemProperty(PropertyMemberElement.Create(Config.Current.Book, nameof(BookConfig.PageEndAction))),
                    new SettingItemProperty(PropertyMemberElement.Create(Config.Current.Book, nameof(BookConfig.IsNotifyPageLoop)))),

                new SettingItemSection(Properties.Resources.SettingPageBookMoveAdvance,
                    new SettingItemProperty(PropertyMemberElement.Create(Config.Current.Book, nameof(BookConfig.TerminalSound)))),
            };
        }
    }

    public class SettingPageBookPageSetting : SettingPage
    {
        public SettingPageBookPageSetting() : base(Properties.Resources.SettingPageBookPageSetting)
        {
            this.Items = new List<SettingItem>
            {
                new SettingItemSection(Properties.Resources.SettingPageBookPageSetting, Properties.Resources.SettingPageBookPageSettingTips,
                    new SettingItemMultiProperty(
                            PropertyMemberElement.Create(BookSettingPresenter.Current.DefaultSetting, nameof(BookSettingConfig.Page)),
                            PropertyMemberElement.Create(BookSettingPresenter.Current.Generater, nameof(BookSettingPolicyConfig.Page)))
                    {
                        Content1 = Properties.Resources.WordFirstPage,
                    },
                    new SettingItemMultiProperty(
                            PropertyMemberElement.Create(BookSettingPresenter.Current.DefaultSetting, nameof(BookSettingConfig.SortMode)),
                            PropertyMemberElement.Create(BookSettingPresenter.Current.Generater, nameof(BookSettingPolicyConfig.SortMode))),
                    new SettingItemMultiProperty(
                            PropertyMemberElement.Create(BookSettingPresenter.Current.DefaultSetting, nameof(BookSettingConfig.PageMode)),
                            PropertyMemberElement.Create(BookSettingPresenter.Current.Generater, nameof(BookSettingPolicyConfig.PageMode))),
                    new SettingItemMultiProperty(
                            PropertyMemberElement.Create(BookSettingPresenter.Current.DefaultSetting, nameof(BookSettingConfig.BookReadOrder)),
                            PropertyMemberElement.Create(BookSettingPresenter.Current.Generater, nameof(BookSettingPolicyConfig.BookReadOrder))),
                    new SettingItemMultiProperty(
                            PropertyMemberElement.Create(BookSettingPresenter.Current.DefaultSetting, nameof(BookSettingConfig.IsSupportedDividePage)),
                            PropertyMemberElement.Create(BookSettingPresenter.Current.Generater, nameof(BookSettingPolicyConfig.IsSupportedDividePage))),
                    new SettingItemMultiProperty(
                            PropertyMemberElement.Create(BookSettingPresenter.Current.DefaultSetting, nameof(BookSettingConfig.IsSupportedWidePage)),
                            PropertyMemberElement.Create(BookSettingPresenter.Current.Generater, nameof(BookSettingPolicyConfig.IsSupportedWidePage))),
                    new SettingItemMultiProperty(
                            PropertyMemberElement.Create(BookSettingPresenter.Current.DefaultSetting, nameof(BookSettingConfig.IsSupportedSingleFirstPage)),
                            PropertyMemberElement.Create(BookSettingPresenter.Current.Generater, nameof(BookSettingPolicyConfig.IsSupportedSingleFirstPage))),
                    new SettingItemMultiProperty(
                            PropertyMemberElement.Create(BookSettingPresenter.Current.DefaultSetting, nameof(BookSettingConfig.IsSupportedSingleLastPage)),
                            PropertyMemberElement.Create(BookSettingPresenter.Current.Generater, nameof(BookSettingPolicyConfig.IsSupportedSingleLastPage))),
                    new SettingItemMultiProperty(
                            PropertyMemberElement.Create(BookSettingPresenter.Current.DefaultSetting, nameof(BookSettingConfig.IsRecursiveFolder)),
                            PropertyMemberElement.Create(BookSettingPresenter.Current.Generater, nameof(BookSettingPolicyConfig.IsRecursiveFolder)))),

                new SettingItemSection(Properties.Resources.SettingPageBookSubFolder,
                    new SettingItemProperty(PropertyMemberElement.Create(Config.Current.Book, nameof(BookConfig.IsConfirmRecursive))),
                    new SettingItemProperty(PropertyMemberElement.Create(Config.Current.Book, nameof(BookConfig.IsAutoRecursive)))),

                new SettingItemSection(Properties.Resources.SettingPageBookPageSettingAdvance,
                    new SettingItemProperty(PropertyMemberElement.Create(Config.Current.Book, nameof(BookConfig.IsSortFileFirst)))),
            };
        }

    }

}
