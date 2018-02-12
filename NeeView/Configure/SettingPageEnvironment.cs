using NeeLaboratory.Windows.Input;
using NeeView.Windows.Property;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace NeeView.Configure
{
    public class SettingPageEnvironment : SettingPage
    {
        public SettingPageEnvironment() : base("環境")
        {
            this.Children = new List<SettingPage>
            {
                new SettingPageEnvironmentGeneral(),
                new SettingPageEnvironmentSetup(),
                new SettingPageArchiveGeneral(),
                new SettingPageArchivePdf(),
                new SettingPageHistory(),
            };

            if (Config.Current.IsUseLocalApplicationDataFolder && !Config.Current.IsAppxPackage)
            {
                this.Children.Add(new SettingPageEnvironmentRemove());
            }
        }
    }


    public class SettingPageEnvironmentGeneral : SettingPage
    {
        public SettingPageEnvironmentGeneral() : base("全般")
        {
            this.Items = new List<SettingItem>
            {
                new SettingItemSection("テーマ",
                    new SettingItemProperty(PropertyMemberElement.Create(MainWindowModel.Current, nameof(MainWindowModel.PanelColor)))),

                new SettingItemSection("詳細設定",
                    new SettingItemProperty(PropertyMemberElement.Create(MemoryControl.Current, nameof(MemoryControl.IsAutoGC))),
                    new SettingItemProperty(PropertyMemberElement.Create(App.Current, nameof(App.IsNetworkEnabled))),
                    new SettingItemProperty(PropertyMemberElement.Create(App.Current, nameof(App.IsIgnoreWindowDpi))),
                    new SettingItemProperty(PropertyMemberElement.Create(FileIOProfile.Current, nameof(FileIOProfile.IsRemoveConfirmed))),
                    new SettingItemProperty(PropertyMemberElement.Create(MenuBar.Current, nameof(MenuBar.IsCaptionEmulateInFullScreen)))),
            };
        }
    }

    public class SettingPageEnvironmentSetup : SettingPage
    {
        public SettingPageEnvironmentSetup() : base("起動設定")
        {
            this.Items = new List<SettingItem>
            {
                new SettingItemSection("起動設定",
                    new SettingItemProperty(PropertyMemberElement.Create(App.Current, nameof(App.IsMultiBootEnabled))),
                    new SettingItemProperty(PropertyMemberElement.Create(App.Current, nameof(App.IsSaveWindowPlacement))),
                    new SettingItemProperty(PropertyMemberElement.Create(App.Current, nameof(App.IsRestoreSecondWindow)))
                    {
                        IsEnabled = new IsEnabledPropertyValue(App.Current, nameof(App.IsSaveWindowPlacement))
                    },
                    new SettingItemProperty(PropertyMemberElement.Create(App.Current, nameof(App.IsSaveFullScreen))),
                    new SettingItemProperty(PropertyMemberElement.Create(App.Current, nameof(App.IsOpenLastBook))),
                    new SettingItemProperty(PropertyMemberElement.Create(SlideShow.Current, nameof(SlideShow.IsAutoPlaySlideShow)))),
            };
        }
    }


    public class SettingPageArchiveGeneral : SettingPage
    {
        public SettingPageArchiveGeneral() : base("圧縮ファイル")
        {
            this.Items = new List<SettingItem>
            {
                new SettingItemSection("機能",
                    new SettingItemProperty(PropertyMemberElement.Create(ArchiverManager.Current, nameof(ArchiverManager.IsEnabled))),
                    new SettingItemProperty(PropertyMemberElement.Create(BookHub.Current, nameof(BookHub.IsArchiveRecursive)))
                    {
                        IsEnabled = new IsEnabledPropertyValue(ArchiverManager.Current, nameof(ArchiverManager.IsEnabled)),
                    }),

                new SettingItemSection("詳細設定",
                    new SettingItemProperty(PropertyMemberElement.Create(ArchiverManager.Current, nameof(ArchiverManager.ExcludePattern))),
                    new SettingItemProperty(PropertyMemberElement.Create(SevenZipArchiverProfile.Current, nameof(SevenZipArchiverProfile.X86DllPath))),
                    new SettingItemProperty(PropertyMemberElement.Create(SevenZipArchiverProfile.Current, nameof(SevenZipArchiverProfile.X64DllPath))),
                    new SettingItemProperty(PropertyMemberElement.Create(SevenZipArchiverProfile.Current, nameof(SevenZipArchiverProfile.SupportFileTypes))),
                    new SettingItemProperty(PropertyMemberElement.Create(SevenZipArchiverProfile.Current, nameof(SevenZipArchiverProfile.LockTime))),
                    new SettingItemProperty(PropertyMemberElement.Create(SevenZipArchiverProfile.Current, nameof(SevenZipArchiverProfile.IsPreExtract))),
                    new SettingItemProperty(PropertyMemberElement.Create(SevenZipArchiverProfile.Current, nameof(SevenZipArchiverProfile.PreExtractSolidSize)))),
            };
        }
    }

    public class SettingPageArchivePdf : SettingPage
    {
        public SettingPageArchivePdf() : base("PDF")
        {
            this.Items = new List<SettingItem>
            {
                new SettingItemSection("機能",
                    new SettingItemProperty(PropertyMemberElement.Create(ArchiverManager.Current, nameof(ArchiverManager.IsPdfEnabled)))),

                new SettingItemSection("詳細設定",
                    new SettingItemProperty(PropertyMemberElement.Create(PdfArchiverProfile.Current, nameof(PdfArchiverProfile.RenderSize)))),
            };
        }
    }


    public class SettingPageHistory : SettingPage
    {
        public SettingPageHistory() : base("履歴設定")
        {
            this.Items = new List<SettingItem>
            {
                new SettingItemSection("全般",
                    new SettingItemProperty(PropertyMemberElement.Create(BookHub.Current, nameof(BookHub.HistoryEntryPageCount))),
                    new SettingItemProperty(PropertyMemberElement.Create(BookHub.Current, nameof(BookHub.IsInnerArchiveHistoryEnabled))),
                    new SettingItemProperty(PropertyMemberElement.Create(BookHub.Current, nameof(BookHub.IsUncHistoryEnabled)))),

                new SettingItemSection("履歴保存数制限",
                    new SettingItemIndexValue<int>(PropertyMemberElement.Create(BookHistory.Current, nameof(BookHistory.LimitSize)), new HistoryLimitSize()),
                    new SettingItemIndexValue<TimeSpan>(PropertyMemberElement.Create(BookHistory.Current, nameof(BookHistory.LimitSpan)), new HistoryLimitSpan())),

                new SettingItemSection("履歴削除",
                    new SettingItemGroup(
                        new SettingItemButton("履歴を", "削除する",  RemoveHistory))),

                new SettingItemSection("詳細設定",
                    new SettingItemProperty(PropertyMemberElement.Create(App.Current, nameof(App.IsDisableSave)))),
            };
        }

        #region Commands

        /// <summary>
        /// RemoveHistory command.
        /// </summary>
        public RelayCommand RemoveHistory
        {
            get { return _RemoveHistory = _RemoveHistory ?? new RelayCommand(RemoveHistory_Executed); }
        }

        //
        private RelayCommand _RemoveHistory;

        //
        private void RemoveHistory_Executed()
        {
            BookHistory.Current.Clear();

            // TODO:
            //var dialog = new MessageDialog("", "履歴を削除しました");
            //dialog.Owner = this;
            //dialog.ShowDialog();
        }

        #endregion
    }

    public class SettingPageEnvironmentRemove : SettingPage
    {
        public SettingPageEnvironmentRemove() : base("データの削除")
        {
            this.Items = new List<SettingItem>
            {
                new SettingItemSection("ユーザーデータ",
                    new SettingItemButton("全てのユーザーデータを削除する", new WarningText("削除する"), RemoveAllData)
                    {
                        Tips = "ユーザデータを削除し、アプリケーションを終了します。\nアンインストール前に履歴等を完全に削除したい場合に使用します",
                    }),
            };
        }

        #region Commands

        /// <summary>
        /// RemoveAllData command.
        /// </summary>
        private RelayCommand _RemoveAllData;
        public RelayCommand RemoveAllData
        {
            get { return _RemoveAllData = _RemoveAllData ?? new RelayCommand(RemoveAllData_Executed); }
        }

        private void RemoveAllData_Executed()
        {
            throw new NotImplementedException();
        }

        #endregion
    }
}
