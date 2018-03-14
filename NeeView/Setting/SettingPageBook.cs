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
        public SettingPageBook() : base("ブック")
        {
            this.Children = new List<SettingPage>
            {
                new SettingPageBookGeneral(),
                new SettingPageBookSubFolder(),
                new SettingPageBookVisual(),
                new SettingPageBookSendPage(),
            };
        }
    }

    public class SettingPageBookGeneral : SettingPage
    {
        public SettingPageBookGeneral() : base("ブック全般")
        {
            this.Items = new List<SettingItem>
            {
                new SettingItemSection("全般",
                    new SettingItemProperty(PropertyMemberElement.Create(BookProfile.Current, nameof(BookProfile.IsEnableAnimatedGif))),
                    new SettingItemProperty(PropertyMemberElement.Create(BookProfile.Current, nameof(BookProfile.IsEnableNoSupportFile))),
                    new SettingItemProperty(PropertyMemberElement.Create(BookProfile.Current, nameof(BookProfile.PreLoadMode)))),

                new SettingItemSection("詳細設定",
                    new SettingItemProperty(PropertyMemberElement.Create(JobEngine.Current, nameof(JobEngine.WorkerSize))),
                    new SettingItemProperty(PropertyMemberElement.Create(PictureProfile.Current, nameof(PictureProfile.MaximumSize))),
                    new SettingItemProperty(PropertyMemberElement.Create(PictureProfile.Current, nameof(PictureProfile.IsLimitSourceSize))),
                    new SettingItemProperty(PropertyMemberElement.Create(MainWindowModel.Current, nameof(MainWindowModel.IsOpenbookAtCurrentPlace))),
                    new SettingItemProperty(PropertyMemberElement.Create(BookProfile.Current, nameof(BookProfile.Excludes)), new SettingItemCollectionControl() { Collection = BookProfile.Current.Excludes, AddDialogHeader="除外するパス" }),
                    new SettingItemProperty(PropertyMemberElement.Create(BookProfile.Current, nameof(BookProfile.PreloadLimitSize))),
                    new SettingItemProperty(PropertyMemberElement.Create(BookProfile.Current, nameof(BookProfile.WideRatio)))),
            };
        }
    }

    public class SettingPageBookSubFolder : SettingPage
    {
        public SettingPageBookSubFolder() : base("サブフォルダー")
        {
            this.Items = new List<SettingItem>
            {
                new SettingItemSection("サブフォルダー読み込み問い合わせ",
                    new SettingItemProperty(PropertyMemberElement.Create(BookHub.Current, nameof(BookHub.IsConfirmRecursive)))),

                new SettingItemSection("サブフォルダー読み込み自動判定",
                    new SettingItemProperty(PropertyMemberElement.Create(BookHub.Current, nameof(BookHub.IsAutoRecursive))),
                    new SettingItemProperty(PropertyMemberElement.Create(BookHub.Current, nameof(BookHub.IsAutoRecursiveWithAllFiles)))
                    {
                        IsEnabled = new IsEnabledPropertyValue(BookHub.Current, nameof(BookHub.IsAutoRecursive))
                    }),
            };
        }
    }

    public class SettingPageBookVisual : SettingPage
    {
        public SettingPageBookVisual() : base("ページ表示")
        {
            this.Items = new List<SettingItem>
            {
                new SettingItemSection("ページ表示",
                    new SettingItemProperty(PropertyMemberElement.Create(MainWindowModel.Current, nameof(MainWindowModel.IsVisibleBusy))),
                    new SettingItemProperty(PropertyMemberElement.Create(App.Current, nameof(App.IsIgnoreImageDpi))),
                    new SettingItemProperty(PropertyMemberElement.Create(BookProfile.Current, nameof(BookProfile.LoadingPageView))),
                    new SettingItemProperty(PropertyMemberElement.Create(ContentCanvas.Current, nameof(ContentCanvas.ContentsSpace)))),
            };
        }
    }

    public class SettingPageBookSendPage : SettingPage
    {
        public SettingPageBookSendPage() : base("移動")
        {
            this.Items = new List<SettingItem>
            {
                new SettingItemSection("ブック移動",
                    new SettingItemProperty(PropertyMemberElement.Create(FolderList.Current, nameof(FolderList.IsCruise)))),

                new SettingItemSection("ページ移動",
                    new SettingItemProperty(PropertyMemberElement.Create(BookProfile.Current, nameof(BookProfile.IsPrioritizePageMove))),
                    new SettingItemProperty(PropertyMemberElement.Create(BookProfile.Current, nameof(BookProfile.IsMultiplePageMove))),
                    new SettingItemProperty(PropertyMemberElement.Create(BookOperation.Current, nameof(BookOperation.PageEndAction)))),
            };
        }
    }
}
