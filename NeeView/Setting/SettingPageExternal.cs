using NeeView.Windows.Property;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NeeView.Setting
{
    public class SettingPageExternal : SettingPage
    {
        public SettingPageExternal() : base(Properties.Resources.SettingPageExternal)
        {
            this.Children = new List<SettingPage>
            {
                new SettingPageExternalProgram(),
            };

            this.Items = new List<SettingItem>
            {
                new SettingItemSection(Properties.Resources.SettingPageExternalGeneralCopyToClipboard,
                    new SettingItemProperty(PropertyMemberElement.Create(BookOperation.Current.ClipboardUtility, nameof(ClipboardUtility.MultiPageOption))),
                    new SettingItemProperty(PropertyMemberElement.Create(BookOperation.Current.ClipboardUtility, nameof(ClipboardUtility.ArchiveOption))),
                    new SettingItemGroup(
                        new SettingItemProperty(PropertyMemberElement.Create(BookOperation.Current.ClipboardUtility, nameof(ClipboardUtility.ArchiveSeparater))))
                        {
                            VisibleTrigger = new DataTriggerSource(BookOperation.Current.ClipboardUtility, nameof(ClipboardUtility.ArchiveOption), ArchiveOptionType.SendArchivePath, true),
                        }),

                new SettingItemSection(Properties.Resources.SettingPageExternalGeneralFromBrowser,
                    new SettingItemProperty(PropertyMemberElement.Create(Config.Current.System, nameof(SystemConfig.DownloadPath))) { IsStretch = true }),
            };
        }
    }

    public class SettingPageExternalProgram : SettingPage
    {
        public SettingPageExternalProgram() : base(Properties.Resources.SettingPageExternalProgram)
        {
            this.Items = new List<SettingItem>
            {
                new SettingItemSection(Properties.Resources.SettingPageExternalProgramSetting,
                    new SettingItemProperty(PropertyMemberElement.Create(BookOperation.Current.ExternalApplication, nameof(ExternalApplication.ProgramType))),
                    new SettingItemGroup(
                        new SettingItemProperty(PropertyMemberElement.Create(BookOperation.Current.ExternalApplication, nameof(ExternalApplication.Command))) { IsStretch = true },
                        new SettingItemProperty(PropertyMemberElement.Create(BookOperation.Current.ExternalApplication, nameof(ExternalApplication.Parameter))) { IsStretch = true })
                    {
                        VisibleTrigger = new DataTriggerSource(BookOperation.Current.ExternalApplication, nameof(ExternalApplication.ProgramType), ExternalProgramType.Normal, true),
                    },
                    new SettingItemGroup(
                        new SettingItemProperty(PropertyMemberElement.Create(BookOperation.Current.ExternalApplication, nameof(ExternalApplication.Protocol))) { IsStretch = true })
                    {
                        VisibleTrigger = new DataTriggerSource(BookOperation.Current.ExternalApplication, nameof(ExternalApplication.ProgramType), ExternalProgramType.Protocol, true),
                    },
                    new SettingItemProperty(PropertyMemberElement.Create(BookOperation.Current.ExternalApplication, nameof(ExternalApplication.MultiPageOption))),
                    new SettingItemProperty(PropertyMemberElement.Create(BookOperation.Current.ExternalApplication, nameof(ExternalApplication.ArchiveOption))),
                    new SettingItemGroup(
                        new SettingItemProperty(PropertyMemberElement.Create(BookOperation.Current.ExternalApplication, nameof(ExternalApplication.ArchiveSeparater))))
                        {
                            VisibleTrigger = new DataTriggerSource(BookOperation.Current.ExternalApplication, nameof(ExternalApplication.ArchiveOption), ArchiveOptionType.SendArchivePath, true),
                        }),
            };
        }
    }
}
