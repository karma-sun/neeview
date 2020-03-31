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
                    new SettingItemProperty(PropertyMemberElement.Create(Config.Current.Clipboard, nameof(ClipboardConfig.MultiPagePolicy))),
                    new SettingItemProperty(PropertyMemberElement.Create(Config.Current.Clipboard, nameof(ClipboardConfig.ArchivePolicy))),
                    new SettingItemGroup(
                        new SettingItemProperty(PropertyMemberElement.Create(Config.Current.Clipboard, nameof(ClipboardConfig.ArchiveSeparater))))
                        {
                            VisibleTrigger = new DataTriggerSource(Config.Current.Clipboard, nameof(ClipboardConfig.ArchivePolicy), ArchivePolicy.SendArchivePath, true),
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
                    new SettingItemProperty(PropertyMemberElement.Create(Config.Current.External, nameof(ExternalConfig.Command))) { IsStretch = true },
                    new SettingItemProperty(PropertyMemberElement.Create(Config.Current.External, nameof(ExternalConfig.Parameter))) { IsStretch = true },
                    new SettingItemProperty(PropertyMemberElement.Create(Config.Current.External, nameof(ExternalConfig.MultiPagePolicy))),
                    new SettingItemProperty(PropertyMemberElement.Create(Config.Current.External, nameof(ExternalConfig.ArchivePolicy))),
                    new SettingItemGroup(
                        new SettingItemProperty(PropertyMemberElement.Create(Config.Current.External, nameof(ExternalConfig.ArchiveSeparater))))
                        {
                            VisibleTrigger = new DataTriggerSource(Config.Current.External, nameof(ExternalConfig.ArchivePolicy), ArchivePolicy.SendArchivePath, true),
                        }),
            };
        }
    }
}
