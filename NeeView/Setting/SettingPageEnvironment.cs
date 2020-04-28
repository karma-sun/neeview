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

            var section = new SettingItemSection(Properties.Resources.SettingPageGeneral);

            section.Children.Add(new SettingItemProperty(PropertyMemberElement.Create(Config.Current.System, nameof(SystemConfig.Language))));
            section.Children.Add(new SettingItemProperty(PropertyMemberElement.Create(Config.Current.System, nameof(SystemConfig.ArchiveRecursiveMode))));
            section.Children.Add(new SettingItemProperty(PropertyMemberElement.Create(Config.Current.System, nameof(SystemConfig.BookPageCollectMode))));
            section.Children.Add(new SettingItemProperty(PropertyMemberElement.Create(Config.Current.System, nameof(SystemConfig.IsNaturalSortEnabled))));
            section.Children.Add(new SettingItemProperty(PropertyMemberElement.Create(Config.Current.System, nameof(SystemConfig.IsRemoveConfirmed))));
            section.Children.Add(new SettingItemProperty(PropertyMemberElement.Create(Config.Current.System, nameof(SystemConfig.IsRemoveExplorerDialogEnabled))));
            section.Children.Add(new SettingItemProperty(PropertyMemberElement.Create(Config.Current.Window, nameof(WindowConfig.IsCaptionEmulateInFullScreen))));

            if (!Environment.IsAppxPackage)
            {
                section.Children.Add(new SettingItemProperty(PropertyMemberElement.Create(Config.Current.System, nameof(SystemConfig.IsNetworkEnabled))));
            }

            if (Environment.IsZipLikePackage)
            {
                section.Children.Add(new SettingItemProperty(PropertyMemberElement.Create(ExplorerContextMenu.Current, nameof(ExplorerContextMenu.IsEnabled))));
            }

            this.Items = new List<SettingItem>() { section };
        }
    }

    public class SettingPageEnvironmentSetup : SettingPage
    {
        public SettingPageEnvironmentSetup() : base(Properties.Resources.SettingPageGeneralBoot)
        {
            var section = new SettingItemSection(Properties.Resources.SettingPageGeneralBoot, Properties.Resources.SettingPageGeneralBootBootTips);
            section.Children.Add(new SettingItemProperty(PropertyMemberElement.Create(Config.Current.StartUp, nameof(StartUpConfig.IsSplashScreenEnabled))));
            section.Children.Add(new SettingItemProperty(PropertyMemberElement.Create(Config.Current.StartUp, nameof(StartUpConfig.IsMultiBootEnabled))));
            section.Children.Add(new SettingItemProperty(PropertyMemberElement.Create(Config.Current.StartUp, nameof(StartUpConfig.IsRestoreWindowPlacement))));
            section.Children.Add(new SettingItemProperty(PropertyMemberElement.Create(Config.Current.StartUp, nameof(StartUpConfig.IsRestoreFullScreen))));
            section.Children.Add(new SettingItemProperty(PropertyMemberElement.Create(Config.Current.StartUp, nameof(StartUpConfig.IsOpenLastBook))));
            section.Children.Add(new SettingItemProperty(PropertyMemberElement.Create(Config.Current.StartUp, nameof(StartUpConfig.IsOpenLastFolder))));
            section.Children.Add(new SettingItemProperty(PropertyMemberElement.Create(Config.Current.StartUp, nameof(StartUpConfig.IsAutoPlaySlideShow))));
            section.Children.Add(new SettingItemProperty(PropertyMemberElement.Create(Config.Current.StartUp, nameof(StartUpConfig.IsRestoreSecondWindowPlacement))));

            this.Items = new List<SettingItem>() { section };
        }
    }

    public class SettingPageEnvironmentSaveData : SettingPage
    {
        public SettingPageEnvironmentSaveData() : base(Properties.Resources.SettingPageGeneralSaveData)
        {
            var section = new SettingItemSection(Properties.Resources.SettingPageGeneralSaveDataTypes, Properties.Resources.SettingPageGeneralSaveDataTypesTips);

            section.Children.Add(new SettingItemProperty(PropertyMemberElement.Create(Config.Current.History, nameof(HistoryConfig.IsSaveHistory))));
            section.Children.Add(new SettingItemProperty(PropertyMemberElement.Create(Config.Current.History, nameof(HistoryConfig.HistoryFilePath), new PropertyMemberElementOptions() { EmptyValue = SaveData.DefaultHistoryFilePath }))
            {
                IsStretch = true,
                IsEnabled = new IsEnabledPropertyValue(Config.Current.History, nameof(HistoryConfig.IsSaveHistory))
            });
            section.Children.Add(new SettingItemProperty(PropertyMemberElement.Create(Config.Current.Bookmark, nameof(BookmarkConfig.IsSaveBookmark))));
            section.Children.Add(new SettingItemProperty(PropertyMemberElement.Create(Config.Current.Bookmark, nameof(BookmarkConfig.BookmarkFilePath), new PropertyMemberElementOptions() { EmptyValue = SaveData.DefaultBookmarkFilePath }))
            {
                IsStretch = true,
                IsEnabled = new IsEnabledPropertyValue(Config.Current.Bookmark, nameof(BookmarkConfig.IsSaveBookmark))
            });
            section.Children.Add(new SettingItemProperty(PropertyMemberElement.Create(Config.Current.Pagemark, nameof(PagemarkConfig.IsSavePagemark))));
            section.Children.Add(new SettingItemProperty(PropertyMemberElement.Create(Config.Current.Pagemark, nameof(PagemarkConfig.PagemarkFilePath), new PropertyMemberElementOptions() { EmptyValue = SaveData.DefaultPagemarkFilePath }))
            {
                IsStretch = true,
                IsEnabled = new IsEnabledPropertyValue(Config.Current.Pagemark, nameof(PagemarkConfig.IsSavePagemark))
            });
            section.Children.Add(new SettingItemProperty(PropertyMemberElement.Create(Config.Current.System, nameof(SystemConfig.IsSyncUserSetting))));

            if (!Environment.IsAppxPackage)
            {
                section.Children.Add(new SettingItemProperty(PropertyMemberElement.Create(Config.Current.System, nameof(SystemConfig.IsSettingBackup))));
            }

            section.Children.Add(new SettingItemProperty(PropertyMemberElement.Create(Config.Current.Thumbnail, nameof(ThumbnailConfig.ThumbnailCacheFilePath), new PropertyMemberElementOptions() { EmptyValue = ThumbnailCache.DefaultThumbnailCacheFilePath })) { IsStretch = true });

            section.Children.Add(new SettingItemProperty(PropertyMemberElement.Create(Config.Current.System, nameof(SystemConfig.TemporaryDirectory), new PropertyMemberElementOptions() { EmptyValue = Temporary.TempRootPathDefault })) { IsStretch = true });

            if (Environment.ConfigType == "Debug" || Environment.IsUseLocalApplicationDataFolder)
            {
                section.Children.Add(new SettingItemButton(Properties.Resources.SettingPageGeneralSaveDataRemove, Properties.Resources.SettingItemRemove, RemoveAllData) { Tips = Properties.Resources.SettingItemRemoveTips, });
            }

            this.Items = new List<SettingItem>() { section };
        }

        #region Commands

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
            var section = new SettingItemSection(Properties.Resources.SettingPageEnvironmentMemoryAndPerformance);
            section.Children.Add(new SettingItemProperty(PropertyMemberElement.Create(Config.Current.Performance, nameof(PerformanceConfig.CacheMemorySize))));
            section.Children.Add(new SettingItemProperty(PropertyMemberElement.Create(Config.Current.Performance, nameof(PerformanceConfig.PreLoadSize))));
            section.Children.Add(new SettingItemProperty(PropertyMemberElement.Create(Config.Current.Performance, nameof(PerformanceConfig.JobWorkerSize))));
            section.Children.Add(new SettingItemProperty(PropertyMemberElement.Create(Config.Current.Performance, nameof(PerformanceConfig.MaximumSize))));
            section.Children.Add(new SettingItemProperty(PropertyMemberElement.Create(Config.Current.Performance, nameof(PerformanceConfig.IsLimitSourceSize))));
            section.Children.Add(new SettingItemProperty(PropertyMemberElement.Create(Config.Current.Performance, nameof(PerformanceConfig.IsLoadingPageVisible))));
            section.Children.Add(new SettingItemProperty(PropertyMemberElement.Create(Config.Current.Performance, nameof(PerformanceConfig.PreExtractSolidSize))));
            section.Children.Add(new SettingItemProperty(PropertyMemberElement.Create(Config.Current.Performance, nameof(PerformanceConfig.IsPreExtractToMemory))));
            section.Children.Add(new SettingItemProperty(PropertyMemberElement.Create(Config.Current.Thumbnail, nameof(ThumbnailConfig.ThumbnailBookCapacity))));
            section.Children.Add(new SettingItemProperty(PropertyMemberElement.Create(Config.Current.Thumbnail, nameof(ThumbnailConfig.ThumbnailPageCapacity))));

            this.Items = new List<SettingItem>() { section };
        }
    }
}
