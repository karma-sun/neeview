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
                new SettingPageBookGeneral(),
                new SettingPageBookSubFolder(),
                new SettingPageBookVisual(),
                new SettingPageBookMove(),
            };
        }
    }

    public class SettingPageBookGeneral : SettingPage
    {
        public SettingPageBookGeneral() : base(Properties.Resources.SettingPageBookGeneral)
        {
            this.Items = new List<SettingItem>
            {
                new SettingItemSection(Properties.Resources.SettingPageBookGeneralGeneral,
                    new SettingItemProperty(PropertyMemberElement.Create(BookProfile.Current, nameof(BookProfile.IsEnableAnimatedGif))),
                    new SettingItemProperty(PropertyMemberElement.Create(BookProfile.Current, nameof(BookProfile.IsEnableNoSupportFile))),
                    new SettingItemProperty(PropertyMemberElement.Create(BookProfile.Current, nameof(BookProfile.PreLoadMode))),
                    new SettingItemProperty(PropertyMemberElement.Create(ContentCanvasBrush.Current, nameof(ContentCanvasBrush.PageBackgroundColor)))),

                new SettingItemSection(Properties.Resources.SettingPageBookGeneralAdvance,
                    new SettingItemProperty(PropertyMemberElement.Create(JobEngine.Current, nameof(JobEngine.WorkerSize))),
                    new SettingItemProperty(PropertyMemberElement.Create(PictureProfile.Current, nameof(PictureProfile.MaximumSize))),
                    new SettingItemProperty(PropertyMemberElement.Create(PictureProfile.Current, nameof(PictureProfile.IsLimitSourceSize))),
                    new SettingItemProperty(PropertyMemberElement.Create(MainWindowModel.Current, nameof(MainWindowModel.IsOpenbookAtCurrentPlace))),
                    new SettingItemProperty(PropertyMemberElement.Create(BookProfile.Current, nameof(BookProfile.Excludes)), new SettingItemCollectionControl() { Collection = BookProfile.Current.Excludes, AddDialogHeader=Properties.Resources.WordExcludePath }),
                    new SettingItemProperty(PropertyMemberElement.Create(BookProfile.Current, nameof(BookProfile.PreloadLimitSize))),
                    new SettingItemProperty(PropertyMemberElement.Create(BookProfile.Current, nameof(BookProfile.WideRatio)))),
            };
        }
    }

    public class SettingPageBookSubFolder : SettingPage
    {
        public SettingPageBookSubFolder() : base(Properties.Resources.SettingPageBookSubFolder)
        {
            this.Items = new List<SettingItem>
            {
                new SettingItemSection(Properties.Resources.SettingPageBookSubFolderConfirm,
                    new SettingItemProperty(PropertyMemberElement.Create(BookHub.Current, nameof(BookHub.IsConfirmRecursive)))),

                new SettingItemSection(Properties.Resources.SettingPageBookSubFolderAuto,
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
        public SettingPageBookVisual() : base(Properties.Resources.SettingPageBookVisual)
        {
            this.Items = new List<SettingItem>
            {
                new SettingItemSection(Properties.Resources.SettingPageBookVisualVisual,
                    new SettingItemProperty(PropertyMemberElement.Create(MainWindowModel.Current, nameof(MainWindowModel.IsVisibleBusy))),
                    new SettingItemProperty(PropertyMemberElement.Create(App.Current, nameof(App.IsIgnoreImageDpi))),
                    new SettingItemProperty(PropertyMemberElement.Create(BookProfile.Current, nameof(BookProfile.LoadingPageView))),
                    new SettingItemProperty(PropertyMemberElement.Create(ContentCanvas.Current, nameof(ContentCanvas.ContentsSpace)))),
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
                    new SettingItemProperty(PropertyMemberElement.Create(FolderList.Current, nameof(FolderList.IsCruise)))),

                new SettingItemSection(Properties.Resources.SettingPageBookMovePage,
                    new SettingItemProperty(PropertyMemberElement.Create(BookProfile.Current, nameof(BookProfile.IsPrioritizePageMove))),
                    new SettingItemProperty(PropertyMemberElement.Create(BookProfile.Current, nameof(BookProfile.IsMultiplePageMove))),
                    new SettingItemProperty(PropertyMemberElement.Create(BookOperation.Current, nameof(BookOperation.PageEndAction)))),

                new SettingItemSection(Properties.Resources.SettingPageBookMoveAdvance,
                    new SettingItemProperty(PropertyMemberElement.Create(SoundPlayerService.Current, nameof(SoundPlayerService.SeCannotMove)))),
            };
        }
    }
}
