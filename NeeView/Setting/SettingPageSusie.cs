using NeeView.Susie;
using NeeView.Windows.Property;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NeeView.Setting
{
    public class SettingPageSusie : SettingPage
    {
        public SettingPageSusie() : base(Properties.Resources.SettingPageSusie)
        {
            this.Children = new List<SettingPage>
            {
                new SettingPageSusieINPlugin(),
                new SettingPageSusieAMPlugin(),
            };

            this.Items = new List<SettingItem>
            {
                new SettingItemSection(Properties.Resources.SettingPageSusieGeneralGeneral,
                    Properties.Resources.SettingPageSusieGeneralGeneralTips,
                    new SettingItemProperty(PropertyMemberElement.Create(SusiePluginManager.Current, nameof(SusiePluginManager.IsEnabled))),
                    new SettingItemProperty(PropertyMemberElement.Create(SusiePluginManager.Current, nameof(SusiePluginManager.SusiePluginPath)))
                    {
                        IsStretch = true,
                        IsEnabled = new IsEnabledPropertyValue(SusiePluginManager.Current, nameof(SusiePluginManager.IsEnabled)),
                    }),

                new SettingItemSection(Properties.Resources.SettingPageSusieGeneralPriority,
                    new SettingItemProperty(PropertyMemberElement.Create(SusiePluginManager.Current, nameof(SusiePluginManager.IsFirstOrderSusieImage))),
                    new SettingItemProperty(PropertyMemberElement.Create(SusiePluginManager.Current, nameof(SusiePluginManager.IsFirstOrderSusieArchive))))
                {
                    IsEnabled = new IsEnabledPropertyValue(SusiePluginManager.Current, nameof(SusiePluginManager.IsEnabled))
                },
            };
        }
    }

    public class SettingPageSusieINPlugin : SettingPage
    {
        public SettingPageSusieINPlugin() : base(Properties.Resources.SettingPageSusieImagePlugin)
        {
            this.IsScrollEnabled = false;

            this.Items = new List<SettingItem>
            {
                new SettingItemGroup(
                    new SettingItemSusiePlugin(SusiePluginType.Image))
                {
                    IsEnabled = new IsEnabledPropertyValue(SusiePluginManager.Current, nameof(SusiePluginManager.IsEnabled)),
                },
            };
        }
    }

    public class SettingPageSusieAMPlugin : SettingPage
    {
        public SettingPageSusieAMPlugin() : base(Properties.Resources.SettingPageSusieArchivePlugin)
        {
            this.IsScrollEnabled = false;

            this.Items = new List<SettingItem>
            {
                new SettingItemGroup(
                    new SettingItemSusiePlugin(SusiePluginType.Archive))
                {
                    IsEnabled = new IsEnabledPropertyValue(SusiePluginManager.Current, nameof(SusiePluginManager.IsEnabled)),
                }
            };
        }
    }
}
