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
                new SettingPageSusieGeneral(),
                new SettingPageSusieINPlugin(),
                new SettingPageSusieAMPlugin(),
            };
        }
    }

    public class SettingPageSusieGeneral : SettingPage
    {
        public SettingPageSusieGeneral() : base(Properties.Resources.SettingPageSusieGeneral)
        {
            this.Items = new List<SettingItem>
            {
                new SettingItemSection(Properties.Resources.SettingPageSusieGeneralGeneral,
                    new SettingItemProperty(PropertyMemberElement.Create(SusieContext.Current, nameof(SusieContext.IsEnableSusie))),
                    new SettingItemGroup(
                        new SettingItemProperty(PropertyMemberElement.Create(SusieContext.Current, nameof(SusieContext.SusiePluginPath))) {IsStretch = true },
                        new SettingItemProperty(PropertyMemberElement.Create(SusieContext.Current, nameof(SusieContext.IsPluginCacheEnabled))))
                    {
                        IsEnabled = new IsEnabledPropertyValue(SusieContext.Current, nameof(SusieContext.IsEnableSusie)),
                    }),

                new SettingItemSection(Properties.Resources.SettingPageSusieGeneralPriority,
                    new SettingItemProperty(PropertyMemberElement.Create(SusieContext.Current, nameof(SusieContext.IsFirstOrderSusieImage))),
                    new SettingItemProperty(PropertyMemberElement.Create(SusieContext.Current, nameof(SusieContext.IsFirstOrderSusieArchive))))
                {
                    IsEnabled = new IsEnabledPropertyValue(SusieContext.Current, nameof(SusieContext.IsEnableSusie))
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
                    new SettingItemSusiePlugin(Susie.SusiePluginType.Image))
                {
                    IsEnabled = new IsEnabledPropertyValue(SusieContext.Current, nameof(SusieContext.IsEnableSusie)),
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
                    new SettingItemSusiePlugin(Susie.SusiePluginType.Archive))
                {
                    IsEnabled = new IsEnabledPropertyValue(SusieContext.Current, nameof(SusieContext.IsEnableSusie)),
                }
            };
        }
    }
}
