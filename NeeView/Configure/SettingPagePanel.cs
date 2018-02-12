using NeeView.Effects;
using NeeView.Windows.Property;
using System.Collections.Generic;

namespace NeeView.Configure
{
    public class SettingPagePanel : SettingPage
    {
        public SettingPagePanel() : base("パネル")
        {
            this.Children = new List<SettingPage>
            {
                new SettingPagePanelGeneral(),
                new SettingPagePanelFolderList(),
                new SettingPagePanelFileInfo(),
                new SettingPagePanelEffect(),
            };
        }
    }


    public class SettingPagePanelGeneral : SettingPage
    {
        public SettingPagePanelGeneral() : base("全般")
        {
            this.Items = new List<SettingItem>
            {
                 new SettingItemSection("表示",
                    new SettingItemProperty(PropertyMemberElement.Create(MainWindowModel.Current, nameof(MainWindowModel.IsHidePanelInFullscreen))),
                    new SettingItemProperty(PropertyMemberElement.Create(ThumbnailProfile.Current, nameof(ThumbnailProfile.IsThumbnailPopup)))),

                new SettingItemSection("操作",
                    new SettingItemProperty(PropertyMemberElement.Create(SidePanelProfile.Current, nameof(SidePanelProfile.IsLeftRightKeyEnabled))),
                    new SettingItemProperty(PropertyMemberElement.Create(SidePanel.Current, nameof(SidePanel.IsManipulationBoundaryFeedbackEnabled)))),
            };
        }
    }

    public class SettingPagePanelFolderList : SettingPage
    {
        public SettingPagePanelFolderList() : base("フォルダーリスト")
        {
            this.Items = new List<SettingItem>
            {
                 new SettingItemSection("全般",
                    new SettingItemProperty(PropertyMemberElement.Create(FolderList.Current, nameof(FolderList.Home))) {IsStretch = true},
                    new SettingItemProperty(PropertyMemberElement.Create(BookHistory.Current, nameof(BookHistory.IsKeepFolderStatus)))),

                new SettingItemSection("表示",
                    new SettingItemProperty(PropertyMemberElement.Create(FolderList.Current, nameof(FolderList.IsVisibleBookmarkMark))),
                    new SettingItemProperty(PropertyMemberElement.Create(FolderList.Current, nameof(FolderList.IsVisibleHistoryMark))),
                    new SettingItemProperty(PropertyMemberElement.Create(FolderList.Current, nameof(FolderList.FolderIconLayout)))),

                new SettingItemSection("詳細設定",
                    new SettingItemProperty(PropertyMemberElement.Create(FolderList.Current, nameof(FolderList.IsInsertItem)))),
            };
        }
    }

    public class SettingPagePanelFileInfo : SettingPage
    {
        public SettingPagePanelFileInfo() : base("ファイル情報パネル")
        {
            this.Items = new List<SettingItem>
            {
                new SettingItemSection("表示",
                    new SettingItemProperty(PropertyMemberElement.Create(FileInformation.Current, nameof(FileInformation.IsVisibleFilePath))),
                    new SettingItemProperty(PropertyMemberElement.Create(FileInformation.Current, nameof(FileInformation.IsUseExifDateTime))),
                    new SettingItemProperty(PropertyMemberElement.Create(FileInformation.Current, nameof(FileInformation.IsVisibleBitsPerPixel))),
                    new SettingItemProperty(PropertyMemberElement.Create(FileInformation.Current, nameof(FileInformation.IsVisibleLoader)))),
            };
        }
    }

    public class SettingPagePanelEffect : SettingPage
    {
        public SettingPagePanelEffect() : base("エフェクトパネル")
        {
            this.Items = new List<SettingItem>
            {
                new SettingItemSection("表示",
                    new SettingItemProperty(PropertyMemberElement.Create(ImageEffect.Current, nameof(ImageEffect.IsHsvMode)))),

                new SettingItemSection("詳細設定",
                    new SettingItemProperty(PropertyMemberElement.Create(PictureProfile.Current, nameof(PictureProfile.IsMagicScaleSimdEnabled)))),
            };
        }
    }
}
