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
        public SettingPageEnvironment() : base("全般")
        {
            this.Children = new List<SettingPage>
            {
                new SettingPageEnvironmentSetup(),
                new SettingPageEnvironmentDetail(),
            };

            if (Config.Current.IsUseLocalApplicationDataFolder && !Config.Current.IsAppxPackage)
            {
                this.Children.Add(new SettingPageEnvironmentRemove());
            }
        }
    }

    public class SettingPageEnvironmentSetup : SettingPage
    {
        public SettingPageEnvironmentSetup() : base("起動設定")
        {
            this.Items = new List<SettingItem>
            {
                new SettingItemSection("起動設定", "※この設定は、設定ウィンドウを閉じた後に反映されます。",
                    new SettingItemProperty(PropertyMemberElement.Create(App.Current, nameof(App.IsMultiBootEnabled))),
                    new SettingItemProperty(PropertyMemberElement.Create(App.Current, nameof(App.IsSaveWindowPlacement))),
                    new SettingItemProperty(PropertyMemberElement.Create(App.Current, nameof(App.IsSaveFullScreen))),
                    new SettingItemProperty(PropertyMemberElement.Create(App.Current, nameof(App.IsOpenLastBook))),
                    new SettingItemProperty(PropertyMemberElement.Create(SlideShow.Current, nameof(SlideShow.IsAutoPlaySlideShow)))),

                new SettingItemSection("詳細設定", "※この設定は、設定ウィンドウを閉じた後に反映されます。",
                    new SettingItemProperty(PropertyMemberElement.Create(App.Current, nameof(App.IsRestoreSecondWindow)))),
            };
        }
    }

    public class SettingPageEnvironmentDetail : SettingPage
    {
        public SettingPageEnvironmentDetail() : base("詳細設定")
        {
            this.Items = new List<SettingItem>
            {
                new SettingItemSection("詳細設定",
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

    public class SettingPageEnvironmentRemove : SettingPage
    {
        public SettingPageEnvironmentRemove() : base("データの削除")
        {
            this.Items = new List<SettingItem>
            {
                new SettingItemSection("ユーザーデータ削除",
                    new SettingItemButton("全てのユーザーデータを削除する", RemoveAllData) { IsContentOnly = true })
                {
                    Tips = "ユーザーデータを削除してアプリケーションを終了します。アンインストール前に履歴等を完全に削除したい場合に使用します。",
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
