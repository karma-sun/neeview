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
        public SettingPageEnvironmentGeneral() : base("環境全般")
        {
            this.Items = new List<SettingItem>
            {
                new SettingItemSection("テーマ",
                    new SettingItemProperty(PropertyMemberElement.Create(MainWindowModel.Current, nameof(MainWindowModel.PanelColor)))),

                new SettingItemSection("詳細設定",
                    new SettingItemProperty(PropertyMemberElement.Create(FileIOProfile.Current, nameof(FileIOProfile.IsRemoveConfirmed))),
                    new SettingItemProperty(PropertyMemberElement.Create(MenuBar.Current, nameof(MenuBar.IsCaptionEmulateInFullScreen))),
                    new SettingItemProperty(PropertyMemberElement.Create(MemoryControl.Current, nameof(MemoryControl.IsAutoGC))),
                    new SettingItemProperty(PropertyMemberElement.Create(App.Current, nameof(App.IsNetworkEnabled)))),
                    ////new SettingItemProperty(PropertyMemberElement.Create(App.Current, nameof(App.IsIgnoreWindowDpi)))),
            };
        }
    }

    public class SettingPageEnvironmentSetup : SettingPage
    {
        public SettingPageEnvironmentSetup() : base("起動")
        {
            this.Items = new List<SettingItem>
            {
                new SettingItemSection("起動",
                    new SettingItemProperty(PropertyMemberElement.Create(App.Current, nameof(App.IsMultiBootEnabled))),
                    new SettingItemProperty(PropertyMemberElement.Create(App.Current, nameof(App.IsSaveWindowPlacement))),
                    new SettingItemProperty(PropertyMemberElement.Create(App.Current, nameof(App.IsSaveFullScreen))),
                    new SettingItemProperty(PropertyMemberElement.Create(App.Current, nameof(App.IsOpenLastBook))),
                    new SettingItemProperty(PropertyMemberElement.Create(SlideShow.Current, nameof(SlideShow.IsAutoPlaySlideShow)))),

                new SettingItemSection("詳細設定",
                    new SettingItemProperty(PropertyMemberElement.Create(App.Current, nameof(App.IsRestoreSecondWindow)))),
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
                    new SettingItemProperty(PropertyMemberElement.Create(ArchiverManager.Current, nameof(ArchiverManager.ExcludePattern))) { IsStretch = true },
                    new SettingItemProperty(PropertyMemberElement.Create(SevenZipArchiverProfile.Current, nameof(SevenZipArchiverProfile.X86DllPath)))
                    {
                        Visibility = new VisibilityPropertyValue(Config.IsX64 ? Visibility.Collapsed : Visibility.Visible),
                        IsStretch = true,
                    },
                    new SettingItemProperty(PropertyMemberElement.Create(SevenZipArchiverProfile.Current, nameof(SevenZipArchiverProfile.X64DllPath)))
                    {
                        Visibility = new VisibilityPropertyValue(Config.IsX64 ? Visibility.Visible : Visibility.Collapsed),
                        IsStretch = true
                    },
                    new SettingItemProperty(PropertyMemberElement.Create(SevenZipArchiverProfile.Current, nameof(SevenZipArchiverProfile.SupportFileTypes)), new SettingItemCollectionControl() { Collection = SevenZipArchiverProfile.Current.SupportFileTypes, AddDialogHeader = "拡張子" }),
                    new SettingItemProperty(PropertyMemberElement.Create(SevenZipArchiverProfile.Current, nameof(SevenZipArchiverProfile.LockTime))),
                    new SettingItemProperty(PropertyMemberElement.Create(SevenZipArchiverProfile.Current, nameof(SevenZipArchiverProfile.IsPreExtract))),
                    new SettingItemProperty(PropertyMemberElement.Create(SevenZipArchiverProfile.Current, nameof(SevenZipArchiverProfile.PreExtractSolidSize))))
                {
                    IsEnabled = new IsEnabledPropertyValue(ArchiverManager.Current, nameof(ArchiverManager.IsEnabled)),
                },
            };
        }
    }

    public class SettingPageArchivePdf : SettingPage
    {
        public SettingPageArchivePdf() : base("PDF")
        {
            this.Items = new List<SettingItem>
            {
                new SettingItemGroup(
                    new SettingItemSection("機能",
                        new SettingItemProperty(PropertyMemberElement.Create(ArchiverManager.Current, nameof(ArchiverManager.IsPdfEnabled)))),
                    new SettingItemSection("詳細設定",
                        new SettingItemProperty(PropertyMemberElement.Create(PdfArchiverProfile.Current, nameof(PdfArchiverProfile.RenderSize)))))
                {
                    IsEnabled = new IsEnabledPropertyValue(ArchiverManager.Current, nameof(ArchiverManager.IsEnabled)),
                },
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

                new SettingItemSection("履歴保存数制限", "履歴ファイルに保存される履歴数上限を設定します。アプリ動作中の履歴は制限されません。",
                    new SettingItemIndexValue<int>(PropertyMemberElement.Create(BookHistory.Current, nameof(BookHistory.LimitSize)), new HistoryLimitSize(), false),
                    new SettingItemIndexValue<TimeSpan>(PropertyMemberElement.Create(BookHistory.Current, nameof(BookHistory.LimitSpan)), new HistoryLimitSpan(), false)),

                new SettingItemSection("履歴削除",
                    new SettingItemGroup(
                        new SettingItemButton("履歴を削除する", RemoveHistory) { IsContentOnly = true })),

                new SettingItemSection("詳細設定",
                    new SettingItemProperty(PropertyMemberElement.Create(App.Current, nameof(App.IsDisableSave)))),
            };
        }

        #region Commands

        /// <summary>
        /// RemoveHistory command.
        /// </summary>
        public RelayCommand<UIElement> RemoveHistory
        {
            get { return _RemoveHistory = _RemoveHistory ?? new RelayCommand<UIElement>(RemoveHistory_Executed); }
        }

        //
        private RelayCommand<UIElement> _RemoveHistory;

        //
        private void RemoveHistory_Executed(UIElement element)
        {
            BookHistory.Current.Clear();

            var dialog = new MessageDialog("", "履歴を削除しました");
            if (element != null)
            {
                dialog.Owner = Window.GetWindow(element);
            }
            dialog.ShowDialog();
        }

        #endregion

        #region IndexValues

        /// <summary>
        /// 履歴サイズテーブル
        /// </summary>
        public class HistoryLimitSize : IndexIntValue
        {
            private static List<int> _values = new List<int>
        {
            0, 1, 10, 20, 50, 100, 200, 500, 1000, -1
        };

            public HistoryLimitSize() : base(_values)
            {
            }

            //
            public HistoryLimitSize(int value) : base(_values)
            {
                Value = value;
            }

            //
            public override string ValueString => Value == -1 ? "制限なし" : Value.ToString();
        }

        /// <summary>
        /// 履歴期限テーブル
        /// </summary>
        public class HistoryLimitSpan : IndexTimeSpanValue
        {
            private static List<TimeSpan> _values = new List<TimeSpan>() {
                TimeSpan.FromDays(1),
                TimeSpan.FromDays(2),
                TimeSpan.FromDays(3),
                TimeSpan.FromDays(7),
                TimeSpan.FromDays(15),
                TimeSpan.FromDays(30),
                TimeSpan.FromDays(100),
                default(TimeSpan),
            };

            //
            public HistoryLimitSpan() : base(_values)
            {
            }

            //
            public HistoryLimitSpan(TimeSpan value) : base(_values)
            {
                Value = value;
            }

            //
            public override string ValueString => Value == default(TimeSpan) ? "制限なし" : $"{Value.Days}日前まで";
        }

        #endregion
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
                    Tips = "ユーザーデータを削除し、アプリケーションを終了します。アンインストール前に履歴等を完全に削除したい場合に使用します。",
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
