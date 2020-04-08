using NeeLaboratory.Windows.Input;
using NeeView.Windows.Property;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace NeeView.Setting
{
    public class SettingPageEnvironment : SettingPage
    {
        public SettingPageEnvironment() : base(Properties.Resources.SettingPageGeneral)
        {
            this.Children = new List<SettingPage>
            {
                new SettingPageEnvironmentSetup(),
                new SettingPageEnvironmentSaveData(),
                new SettingPageEnvironmentMemoryAndPerformance(),
            };

            this.Items = new List<SettingItem>
            {
                new SettingItemSection(Properties.Resources.SettingPageGeneralDetailLanguage,
                    new SettingItemProperty(PropertyMemberElement.Create(Config.Current.System, nameof(SystemConfig.Language)))),

                new SettingItemSection(Properties.Resources.SettingPageGeneralDetailDetail,
                    new SettingItemProperty(PropertyMemberElement.Create(Config.Current.System, nameof(SystemConfig.ArchiveRecursiveMode))),
                    new SettingItemProperty(PropertyMemberElement.Create(Config.Current.System, nameof(SystemConfig.BookPageCollectMode))),
                    new SettingItemProperty(PropertyMemberElement.Create(Config.Current.System, nameof(SystemConfig.IsNaturalSortEnabled))),
                    new SettingItemProperty(PropertyMemberElement.Create(Config.Current.System, nameof(SystemConfig.IsRemoveConfirmed))),
                    new SettingItemProperty(PropertyMemberElement.Create(Config.Current.System, nameof(SystemConfig.IsRemoveExplorerDialogEnabled))),
                    new SettingItemProperty(PropertyMemberElement.Create(Config.Current.Window, nameof(WindowConfig.IsCaptionEmulateInFullScreen))),
                    new SettingItemProperty(PropertyMemberElement.Create(Config.Current.System, nameof(SystemConfig.IsNetworkEnabled)))
                    {
                        Visibility = new VisibilityPropertyValue(Environment.IsAppxPackage ? Visibility.Collapsed : Visibility.Visible)
                    }),

                new SettingItemSection(Properties.Resources.SettingPageGeneralDetailExplorer,
                    new SettingItemProperty(PropertyMemberElement.Create(ExplorerContextMenu.Current, nameof(ExplorerContextMenu.IsEnabled))))
                {
                    Visibility = new VisibilityPropertyValue(Environment.IsZipLikePackage ? Visibility.Visible : Visibility.Collapsed)
                },
            };
        }
    }

    public class SettingPageEnvironmentSetup : SettingPage
    {
        public SettingPageEnvironmentSetup() : base(Properties.Resources.SettingPageGeneralBoot)
        {
            this.Items = new List<SettingItem>
            {
                new SettingItemSection(Properties.Resources.SettingPageGeneralBootBoot, Properties.Resources.SettingPageGeneralBootBootTips,
                    new SettingItemProperty(PropertyMemberElement.Create(Config.Current.StartUp, nameof(StartUpConfig.IsSplashScreenEnabled))),
                    new SettingItemProperty(PropertyMemberElement.Create(Config.Current.StartUp, nameof(StartUpConfig.IsMultiBootEnabled))),
                    new SettingItemProperty(PropertyMemberElement.Create(Config.Current.StartUp, nameof(StartUpConfig.IsRestoreWindowPlacement))),
                    new SettingItemProperty(PropertyMemberElement.Create(Config.Current.StartUp, nameof(StartUpConfig.IsRestoreFullScreen))),
                    new SettingItemProperty(PropertyMemberElement.Create(Config.Current.StartUp, nameof(StartUpConfig.IsOpenLastBook))),
                    new SettingItemProperty(PropertyMemberElement.Create(Config.Current.StartUp, nameof(StartUpConfig.IsOpenLastFolder))),
                    new SettingItemProperty(PropertyMemberElement.Create(Config.Current.StartUp, nameof(StartUpConfig.IsAutoPlaySlideShow)))),

                new SettingItemSection(Properties.Resources.SettingPageGeneralBootBootDetail, Properties.Resources.SettingPageGeneralBootBootDetailTips,
                    new SettingItemProperty(PropertyMemberElement.Create(Config.Current.StartUp, nameof(StartUpConfig.IsRestoreSecondWindowPlacement)))),
            };
        }
    }

    public class SettingPageEnvironmentSaveData : SettingPage
    {
        public SettingPageEnvironmentSaveData() : base(Properties.Resources.SettingPageGeneralSaveData)
        {
            this.Items = new List<SettingItem>
            {
                new SettingItemSection(Properties.Resources.SettingPageGeneralSaveDataTypes, Properties.Resources.SettingPageGeneralSaveDataTypesTips,
                    new SettingItemProperty(PropertyMemberElement.Create(Config.Current.History, nameof(HistoryConfig.IsSaveHistory))),
                    new SettingItemProperty(PropertyMemberElement.Create(Config.Current.History, nameof(HistoryConfig.HistoryFilePath), SaveData.DefaultHistoryFilePath))
                    {
                        IsStretch = true,
                        IsEnabled = new IsEnabledPropertyValue(Config.Current.History, nameof(HistoryConfig.IsSaveHistory))
                    },
                    new SettingItemProperty(PropertyMemberElement.Create(Config.Current.Bookmark, nameof(BookmarkConfig.IsSaveBookmark))),
                    new SettingItemProperty(PropertyMemberElement.Create(Config.Current.Bookmark, nameof(BookmarkConfig.BookmarkFilePath), SaveData.DefaultBookmarkFilePath))
                    {
                        IsStretch = true,
                        IsEnabled = new IsEnabledPropertyValue(Config.Current.Bookmark, nameof(BookmarkConfig.IsSaveBookmark))
                    },
                    new SettingItemProperty(PropertyMemberElement.Create(Config.Current.Pagemark, nameof(PagemarkConfig.IsSavePagemark))),
                    new SettingItemProperty(PropertyMemberElement.Create(Config.Current.Pagemark, nameof(PagemarkConfig.PagemarkFilePath), SaveData.DefaultPagemarkFilePath))
                    {
                        IsStretch = true,
                        IsEnabled = new IsEnabledPropertyValue(Config.Current.Pagemark, nameof(PagemarkConfig.IsSavePagemark))
                    },
                    new SettingItemProperty(PropertyMemberElement.Create(Config.Current.System, nameof(SystemConfig.IsSyncUserSetting))),
                    new SettingItemProperty(PropertyMemberElement.Create(Config.Current.System, nameof(SystemConfig.IsSettingBackup)))
                    {
                        Visibility = new VisibilityPropertyValue(Environment.IsAppxPackage ? Visibility.Collapsed : Visibility.Visible)
                    }),

                new SettingItemSection(Properties.Resources.SettingPageGeneralLocationTypes, Properties.Resources.SettingPageGeneralLocationTypesTips,
                    new SettingItemProperty(PropertyMemberElement.Create(Config.Current.System, nameof(SystemConfig.TemporaryDirectory), Temporary.TempRootPathDefault)) { IsStretch = true },
                    new SettingItemProperty(PropertyMemberElement.Create(Config.Current.System, nameof(SystemConfig.CacheDirectory), ThumbnailCache.CacheFolderPathDefault)) { IsStretch = true }),

                new SettingItemSection(Properties.Resources.SettingPageGeneralSaveDataRemove,
                    new SettingItemButton(Properties.Resources.SettingItemRemove, RemoveAllData) { IsContentOnly = true })
                {
                    Tips = Properties.Resources.SettingItemRemoveTips,
#if !DEBUG
                    Visibility = new VisibilityPropertyValue(Environment.IsUseLocalApplicationDataFolder && !Environment.IsAppxPackage ? Visibility.Visible : Visibility.Collapsed)
#endif
                },
            };
        }

        #region Commands

        /// <summary>
        /// RemoveAllData command.
        /// </summary>
        private RelayCommand<UIElement> _RemoveAllData;
        public RelayCommand<UIElement> RemoveAllData
        {
            get { return _RemoveAllData = _RemoveAllData ?? new RelayCommand<UIElement>(RemoveAllData_Executed); }
        }

        private void RemoveAllData_Executed(UIElement element)
        {
            var window = element != null ? Window.GetWindow(element) : null;
            Environment.RemoveApplicationData(window);
        }

        #endregion
    }

    public class SettingPageEnvironmentMemoryAndPerformance : SettingPage
    {
        public SettingPageEnvironmentMemoryAndPerformance() : base(Properties.Resources.SettingPageEnvironmentMemoryAndPerformance)
        {
            this.Items = new List<SettingItem>
            {
                new SettingItemProperty(PropertyMemberElement.Create(Config.Current.Performance, nameof(PerformanceConfig.CacheMemorySize))),
                new SettingItemProperty(PropertyMemberElement.Create(Config.Current.Performance, nameof(PerformanceConfig.PreLoadSize))),
                new SettingItemProperty(PropertyMemberElement.Create(Config.Current.Performance, nameof(PerformanceConfig.JobWorkerSize))),
                new SettingItemProperty(PropertyMemberElement.Create(Config.Current.Performance, nameof(PerformanceConfig.MaximumSize))),
                new SettingItemProperty(PropertyMemberElement.Create(Config.Current.Performance, nameof(PerformanceConfig.IsLimitSourceSize))),
                new SettingItemProperty(PropertyMemberElement.Create(Config.Current.Performance, nameof(PerformanceConfig.IsLoadingPageVisible))),
                new SettingItemProperty(PropertyMemberElement.Create(Config.Current.Performance, nameof(PerformanceConfig.PreExtractSolidSize))),
                new SettingItemProperty(PropertyMemberElement.Create(Config.Current.Performance, nameof(PerformanceConfig.IsPreExtractToMemory))),
                new SettingItemProperty(PropertyMemberElement.Create(Config.Current.Thumbnail, nameof(ThumbnailConfig.ThumbnailBookCapacity))),
                new SettingItemProperty(PropertyMemberElement.Create(Config.Current.Thumbnail, nameof(ThumbnailConfig.ThumbnailPageCapacity))),
            };
        }
    }
}
