using NeeLaboratory.Windows.Input;
using NeeView.Data;
using NeeView.Windows.Property;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace NeeView.Setting
{
    /// <summary>
    /// Setting: General
    /// </summary>
    public class SettingPageGeneral : SettingPage
    {
        public SettingPageGeneral() : base(Properties.Resources.SettingPage_General)
        {
            this.Children = new List<SettingPage>
            {
                new SettingPageStartUp(),
                new SettingPageSaveData(),
                new SettingPageMemoryAndPerformance(),
                new SettingPageThumbnail(),
                new SettingPageNotify(),
            };

            var section = new SettingItemSection(Properties.Resources.SettingPage_General);

            var cultureMap = Environment.Cultures.Select(e => CultureInfo.GetCultureInfo(e)).OrderBy(e => e.NativeName).ToDictionary(e => e.Name, e => e.NativeName);
            section.Children.Add(new SettingItemProperty(PropertyMemberElement.Create(Config.Current.System, nameof(SystemConfig.Language), new PropertyMemberElementOptions() { StringMap = cultureMap })));

            section.Children.Add(new SettingItemProperty(PropertyMemberElement.Create(Config.Current.System, nameof(SystemConfig.ArchiveRecursiveMode))));
            section.Children.Add(new SettingItemProperty(PropertyMemberElement.Create(Config.Current.System, nameof(SystemConfig.BookPageCollectMode))));
            section.Children.Add(new SettingItemProperty(PropertyMemberElement.Create(Config.Current.System, nameof(SystemConfig.IsNaturalSortEnabled))));
            section.Children.Add(new SettingItemProperty(PropertyMemberElement.Create(Config.Current.System, nameof(SystemConfig.IsRemoveConfirmed))));
            section.Children.Add(new SettingItemProperty(PropertyMemberElement.Create(Config.Current.System, nameof(SystemConfig.IsRemoveWantNukeWarning))));

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


    /// <summary>
    /// Setting: StartUp
    /// </summary>
    public class SettingPageStartUp : SettingPage
    {
        public SettingPageStartUp() : base(Properties.Resources.SettingPage_General_Boot)
        {
            var section = new SettingItemSection(Properties.Resources.SettingPage_General_Boot, Properties.Resources.SettingPage_General_Boot_Remarks);
            section.Children.Add(new SettingItemProperty(PropertyMemberElement.Create(Config.Current.StartUp, nameof(StartUpConfig.IsSplashScreenEnabled))));
            section.Children.Add(new SettingItemProperty(PropertyMemberElement.Create(Config.Current.StartUp, nameof(StartUpConfig.IsMultiBootEnabled))));
            section.Children.Add(new SettingItemProperty(PropertyMemberElement.Create(Config.Current.StartUp, nameof(StartUpConfig.IsRestoreWindowPlacement))));
            section.Children.Add(new SettingItemProperty(PropertyMemberElement.Create(Config.Current.Window, nameof(WindowConfig.IsRestoreAeroSnapPlacement)))
            {
                IsEnabled = new IsEnabledPropertyValue(Config.Current.StartUp, nameof(StartUpConfig.IsRestoreWindowPlacement))
            });
            section.Children.Add(new SettingItemProperty(PropertyMemberElement.Create(Config.Current.StartUp, nameof(StartUpConfig.IsRestoreFullScreen)))
            {
                IsEnabled = new IsEnabledPropertyValue(Config.Current.StartUp, nameof(StartUpConfig.IsRestoreWindowPlacement))
            });
            section.Children.Add(new SettingItemProperty(PropertyMemberElement.Create(Config.Current.StartUp, nameof(StartUpConfig.IsOpenLastBook))));
            section.Children.Add(new SettingItemProperty(PropertyMemberElement.Create(Config.Current.StartUp, nameof(StartUpConfig.IsOpenLastFolder))));
            section.Children.Add(new SettingItemProperty(PropertyMemberElement.Create(Config.Current.StartUp, nameof(StartUpConfig.IsAutoPlaySlideShow))));
            section.Children.Add(new SettingItemProperty(PropertyMemberElement.Create(Config.Current.StartUp, nameof(StartUpConfig.IsRestoreSecondWindowPlacement))));

            this.Items = new List<SettingItem>() { section };
        }
    }


    /// <summary>
    /// Setting: SaveData
    /// </summary>
    public class SettingPageSaveData : SettingPage
    {
        public SettingPageSaveData() : base(Properties.Resources.SettingPage_General_SaveData)
        {
            var section = new SettingItemSection(Properties.Resources.SettingPage_General_SaveDataTypes, Properties.Resources.SettingPage_General_SaveDataTypes_Remarks);

            section.Children.Add(new SettingItemProperty(PropertyMemberElement.Create(Config.Current.History, nameof(HistoryConfig.IsSaveHistory))));
            section.Children.Add(new SettingItemProperty(PropertyMemberElement.Create(Config.Current.History, nameof(HistoryConfig.HistoryFilePath)))
            {
                IsStretch = true,
                IsEnabled = new IsEnabledPropertyValue(Config.Current.History, nameof(HistoryConfig.IsSaveHistory))
            });
            section.Children.Add(new SettingItemProperty(PropertyMemberElement.Create(Config.Current.Bookmark, nameof(BookmarkConfig.IsSaveBookmark))));
            section.Children.Add(new SettingItemProperty(PropertyMemberElement.Create(Config.Current.Bookmark, nameof(BookmarkConfig.BookmarkFilePath)))
            {
                IsStretch = true,
                IsEnabled = new IsEnabledPropertyValue(Config.Current.Bookmark, nameof(BookmarkConfig.IsSaveBookmark))
            });
            section.Children.Add(new SettingItemProperty(PropertyMemberElement.Create(Config.Current.Playlist, nameof(PlaylistConfig.PlaylistFolder)))
            {
                IsStretch = true,
            });
            section.Children.Add(new SettingItemProperty(PropertyMemberElement.Create(Config.Current.System, nameof(SystemConfig.IsSyncUserSetting))));
  
            if (!Environment.IsAppxPackage)
            {
                section.Children.Add(new SettingItemProperty(PropertyMemberElement.Create(Config.Current.System, nameof(SystemConfig.IsSettingBackup))));
            }

            section.Children.Add(new SettingItemProperty(PropertyMemberElement.Create(Config.Current.Thumbnail, nameof(ThumbnailConfig.ThumbnailCacheFilePath))) { IsStretch = true });

            section.Children.Add(new SettingItemProperty(PropertyMemberElement.Create(Config.Current.System, nameof(SystemConfig.TemporaryDirectory))) { IsStretch = true });

            if (Environment.ConfigType == "Debug" || (Environment.IsUseLocalApplicationDataFolder && !Environment.IsAppxPackage))
            {
                section.Children.Add(new SettingItemButton(Properties.Resources.SettingPage_General_SaveDataRemove, Properties.Resources.SettingItem_Remove, RemoveAllData) { Tips = Properties.Resources.SettingItem_Remove_Remarks, });
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


    /// <summary>
    /// Setting: MemoryAndPerformance
    /// </summary>
    public class SettingPageMemoryAndPerformance : SettingPage
    {
        public SettingPageMemoryAndPerformance() : base(Properties.Resources.SettingPage_MemoryAndPerformance)
        {
            var section = new SettingItemSection(Properties.Resources.SettingPage_MemoryAndPerformance);
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


    /// <summary>
    /// Setting: Thumbnail
    /// </summary>
    public class SettingPageThumbnail : SettingPage
    {
        public SettingPageThumbnail() : base(Properties.Resources.SettingPage_Thumbnail)
        {
            this.Items = new List<SettingItem>();

            var section = new SettingItemSection(Properties.Resources.SettingPage_Thumbnail_Cache);
            section.Children.Add(new SettingItemProperty(PropertyMemberElement.Create(Config.Current.Thumbnail, nameof(ThumbnailConfig.IsCacheEnabled))));
            section.Children.Add(new SettingItemIndexValue<TimeSpan>(PropertyMemberElement.Create(Config.Current.Thumbnail, nameof(ThumbnailConfig.CacheLimitSpan)), new CacheLimitSpan(), false));
            section.Children.Add(new SettingItemButton(Properties.Resources.SettingPage_Thumbnail_CacheClear, Properties.Resources.SettingPage_Thumbnail_CacheClearButton, RemoveCache));
            this.Items.Add(section);

            section = new SettingItemSection(Properties.Resources.SettingPage_Thumbnail_Advance);
            section.Children.Add(new SettingItemProperty(PropertyMemberElement.Create(Config.Current.Thumbnail, nameof(ThumbnailConfig.ImageWidth))));
            section.Children.Add(new SettingItemProperty(PropertyMemberElement.Create(Config.Current.Thumbnail, nameof(ThumbnailConfig.Format))));
            section.Children.Add(new SettingItemProperty(PropertyMemberElement.Create(Config.Current.Thumbnail, nameof(ThumbnailConfig.Quality))));
            this.Items.Add(section);
        }

        #region Commands

        private RelayCommand<UIElement> _RemoveCache;
        public RelayCommand<UIElement> RemoveCache
        {
            get { return _RemoveCache = _RemoveCache ?? new RelayCommand<UIElement>(RemoveCache_Executed); }
        }

        private void RemoveCache_Executed(UIElement element)
        {
            try
            {
                ThumbnailCache.Current.Remove();

                var dialog = new MessageDialog("", Properties.Resources.CacheDeletedDialog_Title);
                if (element != null)
                {
                    dialog.Owner = Window.GetWindow(element);
                }
                dialog.ShowDialog();
            }
            catch (Exception ex)
            {
                var dialog = new MessageDialog(ex.Message, Properties.Resources.CacheDeletedFailedDialog_Title);
                if (element != null)
                {
                    dialog.Owner = Window.GetWindow(element);
                }
                dialog.ShowDialog();
            }
        }

        #endregion

        /// <summary>
        /// 履歴期限テーブル
        /// </summary>
        public class CacheLimitSpan : IndexTimeSpanValue
        {
            private static List<TimeSpan> _values = new List<TimeSpan>() {
                TimeSpan.FromDays(2),
                TimeSpan.FromDays(3),
                TimeSpan.FromDays(7),
                TimeSpan.FromDays(15),
                TimeSpan.FromDays(30),
                TimeSpan.FromDays(100),
                TimeSpan.FromDays(365),
                default(TimeSpan),
            };

            public CacheLimitSpan() : base(_values)
            {
            }

            public CacheLimitSpan(TimeSpan value) : base(_values)
            {
                Value = value;
            }

            public override string ValueString => Value == default(TimeSpan) ? Properties.Resources.Word_NoLimit : string.Format(Properties.Resources.Word_DaysAgo, Value.Days);
        }
    }


    /// <summary>
    /// Setting: Notify
    /// </summary>
    public class SettingPageNotify : SettingPage
    {
        public SettingPageNotify() : base(Properties.Resources.SettingPage_Notify)
        {
            var section = new SettingItemSection(Properties.Resources.SettingPage_Notify_Display);
            section.Children.Add(new SettingItemProperty(PropertyMemberElement.Create(Config.Current.Notice, nameof(NoticeConfig.NoticeShowMessageStyle))));
            section.Children.Add(new SettingItemProperty(PropertyMemberElement.Create(Config.Current.Notice, nameof(NoticeConfig.BookNameShowMessageStyle))));
            section.Children.Add(new SettingItemProperty(PropertyMemberElement.Create(Config.Current.Notice, nameof(NoticeConfig.CommandShowMessageStyle))));
            section.Children.Add(new SettingItemProperty(PropertyMemberElement.Create(Config.Current.Notice, nameof(NoticeConfig.GestureShowMessageStyle))));
            section.Children.Add(new SettingItemProperty(PropertyMemberElement.Create(Config.Current.Notice, nameof(NoticeConfig.NowLoadingShowMessageStyle))));
            section.Children.Add(new SettingItemProperty(PropertyMemberElement.Create(Config.Current.Notice, nameof(NoticeConfig.ViewTransformShowMessageStyle))));
            section.Children.Add(new SettingItemProperty(PropertyMemberElement.Create(Config.Current.Notice, nameof(NoticeConfig.IsOriginalScaleShowMessage))));
            section.Children.Add(new SettingItemProperty(PropertyMemberElement.Create(Config.Current.Notice, nameof(NoticeConfig.IsEmptyMessageEnabled))));
            section.Children.Add(new SettingItemProperty(PropertyMemberElement.Create(Config.Current.Notice, nameof(NoticeConfig.IsBusyMarkEnabled))));

            this.Items = new List<SettingItem>() { section };
        }
    }

}
