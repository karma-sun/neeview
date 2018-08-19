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
                new SettingPageEnvironmentDetail(),
                new SettingPageEnvironmentSetup(),
                new SettingPageEnvironmentSaveData(),
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
                    new SettingItemProperty(PropertyMemberElement.Create(SlideShow.Current, nameof(SlideShow.IsAutoPlaySlideShow)))),

                new SettingItemSection(Properties.Resources.SettingPageGeneralBootBootDetail, Properties.Resources.SettingPageGeneralBootBootDetailTips,
                    new SettingItemProperty(PropertyMemberElement.Create(App.Current, nameof(App.IsRestoreSecondWindow)))),
            };
        }
    }

    public class SettingPageEnvironmentDetail : SettingPage
    {
        public SettingPageEnvironmentDetail() : base(Properties.Resources.SettingPageGeneralDetail)
        {
            this.Items = new List<SettingItem>
            {
                new SettingItemSection(Properties.Resources.SettingPageGeneralDetailLanguage,
                    new SettingItemProperty(PropertyMemberElement.Create(App.Current, nameof(App.Language)))),

                new SettingItemSection(Properties.Resources.SettingPageGeneralDetailDetail,
                    new SettingItemProperty(PropertyMemberElement.Create(BookHub.Current, nameof(BookHub.IsArchiveRecursive))),
                    new SettingItemProperty(PropertyMemberElement.Create(FileIOProfile.Current, nameof(FileIOProfile.IsRemoveConfirmed))),
                    new SettingItemProperty(PropertyMemberElement.Create(MenuBar.Current, nameof(MenuBar.IsCaptionEmulateInFullScreen))),
                    new SettingItemProperty(PropertyMemberElement.Create(MemoryControl.Current, nameof(MemoryControl.IsAutoGC))),
                    new SettingItemGroup(
                        new SettingItemProperty(PropertyMemberElement.Create(App.Current, nameof(App.IsNetworkEnabled))),
                        new SettingItemProperty(PropertyMemberElement.Create(App.Current, nameof(App.IsSettingBackup))))
                    {
                        Visibility = new VisibilityPropertyValue(Config.Current.IsAppxPackage ? Visibility.Collapsed : Visibility.Visible)
                    }),
                    ////new SettingItemProperty(PropertyMemberElement.Create(App.Current, nameof(App.IsIgnoreWindowDpi)))),
            };
        }
    }


    public class SettingPageEnvironmentSaveData : SettingPage
    {
        public SettingPageEnvironmentSaveData() : base(Properties.Resources.SettingPageGeneralSaveData)
        {
            this.Items = new List<SettingItem>
            {
                new SettingItemSection(Properties.Resources.SettingPageGeneralSaveDataTypes,
                    new SettingItemProperty(PropertyMemberElement.Create(App.Current, nameof(App.IsSaveHistory))),
                    new SettingItemProperty(PropertyMemberElement.Create(App.Current, nameof(App.IsSaveBookmark))),
                    new SettingItemProperty(PropertyMemberElement.Create(App.Current, nameof(App.IsSavePagemark)))),

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
            Config.Current.RemoveApplicationData(window);
        }

        #endregion
    }
}
