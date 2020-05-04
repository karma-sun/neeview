using NeeLaboratory.Windows.Input;
using NeeView.Data;
using NeeView.Windows.Property;
using System;
using System.Collections.Generic;
using System.Windows;

namespace NeeView.Setting
{
    /// <summary>
    /// Setting: History
    /// </summary>
    public class SettingPageHistory : SettingPage
    {
        public SettingPageHistory() : base(Properties.Resources.SettingPageHistory)
        {
            this.Children = new List<SettingPage>
            {
                new SettingPageHistoryPageView(),
            };

            this.Items = new List<SettingItem>();

            var section = new SettingItemSection(Properties.Resources.SettingPageHistoryGeneral);
            section.Children.Add(new SettingItemIndexValue<int>(PropertyMemberElement.Create(Config.Current.History, nameof(HistoryConfig.HistoryEntryPageCount)), new HistoryEntryPageCount(), true));
            section.Children.Add(new SettingItemProperty(PropertyMemberElement.Create(Config.Current.History, nameof(HistoryConfig.IsInnerArchiveHistoryEnabled))));
            section.Children.Add(new SettingItemProperty(PropertyMemberElement.Create(Config.Current.History, nameof(HistoryConfig.IsUncHistoryEnabled))));
            section.Children.Add(new SettingItemProperty(PropertyMemberElement.Create(Config.Current.History, nameof(HistoryConfig.IsForceUpdateHistory))));
            section.Children.Add(new SettingItemProperty(PropertyMemberElement.Create(Config.Current.History, nameof(HistoryConfig.IsAutoCleanupEnabled))));
            section.Children.Add(new SettingItemButton(Properties.Resources.SettingPageHistoryGeneralDelete, Properties.Resources.SettingPageHistoryGeneralDeleteButton, RemoveHistory));
            this.Items.Add(section);

            section = new SettingItemSection(Properties.Resources.SettingPageHistoryGeneralLimit, Properties.Resources.SettingPageHistoryGeneralLimitTips);
            section.Children.Add(new SettingItemIndexValue<int>(PropertyMemberElement.Create(Config.Current.History, nameof(HistoryConfig.LimitSize)), new HistoryLimitSize(), false));
            section.Children.Add(new SettingItemIndexValue<TimeSpan>(PropertyMemberElement.Create(Config.Current.History, nameof(HistoryConfig.LimitSpan)), new HistoryLimitSpan(), false));
            this.Items.Add(section);
        }

        #region Commands

        private RelayCommand<UIElement> _RemoveHistory;
        public RelayCommand<UIElement> RemoveHistory
        {
            get { return _RemoveHistory = _RemoveHistory ?? new RelayCommand<UIElement>(RemoveHistory_Executed); }
        }

        private void RemoveHistory_Executed(UIElement element)
        {
            BookHistoryCollection.Current.Clear();

            var dialog = new MessageDialog("", Properties.Resources.DialogHistoryDeletedTitle);
            if (element != null)
            {
                dialog.Owner = Window.GetWindow(element);
            }
            dialog.ShowDialog();
        }

        #endregion

        #region IndexValues

        #region IndexValue

        /// <summary>
        /// 履歴登録開始テーブル
        /// </summary>
        public class HistoryEntryPageCount : IndexIntValue
        {
            private static List<int> _values = new List<int>
            {
                0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 20, 50, 100,
            };

            public HistoryEntryPageCount() : base(_values)
            {
                IsValueSyncIndex = false;
            }

            public HistoryEntryPageCount(int value) : base(_values)
            {
                IsValueSyncIndex = false;
                Value = value;
            }

            public override string ValueString => $"{Value} {Properties.Resources.WordPage}";
        }

        #endregion

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

            public HistoryLimitSize(int value) : base(_values)
            {
                Value = value;
            }

            public override string ValueString => Value == -1 ? Properties.Resources.WordNoLimit : Value.ToString();
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
                TimeSpan.FromDays(365),
                default(TimeSpan),
            };

            public HistoryLimitSpan() : base(_values)
            {
            }

            public HistoryLimitSpan(TimeSpan value) : base(_values)
            {
                Value = value;
            }

            public override string ValueString => Value == default(TimeSpan) ? Properties.Resources.WordNoLimit : string.Format(Properties.Resources.WordDaysAgo, Value.Days);
        }

        #endregion
    }


    /// <summary>
    /// Setting: HistoryPageView
    /// </summary>
    public class SettingPageHistoryPageView : SettingPage
    {
        public SettingPageHistoryPageView() : base(Properties.Resources.SettingPageHistoryPageViewRecord)
        {
            var section = new SettingItemSection(Properties.Resources.SettingPageHistoryPageViewRecord);
            section.Children.Add(new SettingItemProperty(PropertyMemberElement.Create(Config.Current.PageViewRecorder, nameof(PageViewRecorderConfig.IsSavePageViewRecord))));
            section.Children.Add(new SettingItemProperty(PropertyMemberElement.Create(Config.Current.PageViewRecorder, nameof(PageViewRecorderConfig.PageViewRecordFilePath))));

            this.Items = new List<SettingItem>() { section };
        }
    }
}
