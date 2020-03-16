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
                    new SettingItemProperty(PropertyMemberElement.Create(FileIOProfile.Current, nameof(FileIOProfile.IsRemoveConfirmed))),
                    new SettingItemProperty(PropertyMemberElement.Create(FileIOProfile.Current, nameof(FileIOProfile.IsRemoveExplorerDialogEnabled))),
                    new SettingItemProperty(PropertyMemberElement.Create(MenuBar.Current, nameof(MenuBar.IsCaptionEmulateInFullScreen))),
                    new SettingItemProperty(PropertyMemberElement.Create(App.Current, nameof(App.IsNetworkEnabled)))
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
                    new SettingItemProperty(PropertyMemberElement.Create(App.Current, nameof(App.IsSplashScreenEnabled))),
                    new SettingItemProperty(PropertyMemberElement.Create(App.Current, nameof(App.IsMultiBootEnabled))),
                    new SettingItemProperty(PropertyMemberElement.Create(App.Current, nameof(App.IsSaveWindowPlacement))),
                    new SettingItemProperty(PropertyMemberElement.Create(App.Current, nameof(App.IsSaveFullScreen))),
                    new SettingItemProperty(PropertyMemberElement.Create(App.Current, nameof(App.IsOpenLastBook))),
                    new SettingItemProperty(PropertyMemberElement.Create(BookHistoryCollection.Current, nameof(BookHistoryCollection.IsKeepLastFolder))),
                    new SettingItemProperty(PropertyMemberElement.Create(SlideShow.Current, nameof(SlideShow.IsAutoPlaySlideShow)))),

                new SettingItemSection(Properties.Resources.SettingPageGeneralBootBootDetail, Properties.Resources.SettingPageGeneralBootBootDetailTips,
                    new SettingItemProperty(PropertyMemberElement.Create(App.Current, nameof(App.IsRestoreSecondWindow)))),
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
                    new SettingItemProperty(PropertyMemberElement.Create(App.Current, nameof(App.IsSaveHistory))),
                    new SettingItemProperty(PropertyMemberElement.Create(App.Current, nameof(App.HistoryFilePath)))
                    {
                        IsStretch = true,
                        IsEnabled = new IsEnabledPropertyValue(App.Current, nameof(App.IsSaveHistory))
                    },
                    new SettingItemProperty(PropertyMemberElement.Create(App.Current, nameof(App.IsSaveBookmark))),
                    new SettingItemProperty(PropertyMemberElement.Create(App.Current, nameof(App.BookmarkFilePath)))
                    {
                        IsStretch = true,
                        IsEnabled = new IsEnabledPropertyValue(App.Current, nameof(App.IsSaveBookmark))
                    },
                    new SettingItemProperty(PropertyMemberElement.Create(App.Current, nameof(App.IsSavePagemark))),
                    new SettingItemProperty(PropertyMemberElement.Create(App.Current, nameof(App.PagemarkFilePath)))
                    {
                        IsStretch = true,
                        IsEnabled = new IsEnabledPropertyValue(App.Current, nameof(App.IsSavePagemark))
                    },
                    new SettingItemProperty(PropertyMemberElement.Create(App.Current, nameof(App.IsSyncUserSetting))),
                    new SettingItemProperty(PropertyMemberElement.Create(App.Current, nameof(App.IsSettingBackup)))
                    {
                        Visibility = new VisibilityPropertyValue(Environment.IsAppxPackage ? Visibility.Collapsed : Visibility.Visible)
                    }),

                new SettingItemSection(Properties.Resources.SettingPageGeneralLocationTypes, Properties.Resources.SettingPageGeneralLocationTypesTips,
                    new SettingItemProperty(PropertyMemberElement.Create(App.Current, nameof(App.TemporaryDirectory))) { IsStretch = true },
                    new SettingItemProperty(PropertyMemberElement.Create(App.Current, nameof(App.CacheDirectory))) { IsStretch = true }),

                new SettingItemSection(Properties.Resources.SettingPageGeneralSaveDataRemove,
                    new SettingItemButton(Properties.Resources.SettingItemRemove, RemoveAllData) { IsContentOnly = true })
                {
                    Tips = Properties.Resources.SettingItemRemoveTips,
#if !DEBUG
                    Visibility = new VisibilityPropertyValue(Config.Current.IsUseLocalApplicationDataFolder && !Config.Current.IsAppxPackage ? Visibility.Visible : Visibility.Collapsed)
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
                new SettingItemProperty(PropertyMemberElement.Create(Config.Current.Performance, nameof(PerformanceConfig.ThumbnailBookCapacity))),
                new SettingItemProperty(PropertyMemberElement.Create(Config.Current.Performance, nameof(PerformanceConfig.ThumbnailPageCapacity))),
            };
        }
    }
}
