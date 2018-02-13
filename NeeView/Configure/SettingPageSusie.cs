using NeeView.Windows.Property;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NeeView.Configure
{
    public class SettingPageSusie : SettingPage
    {
        public SettingPageSusie() : base("Susie")
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
        public SettingPageSusieGeneral() : base("Susie全般")
        {
            this.Items = new List<SettingItem>
            {
                new SettingItemSection("全般",
                    new SettingItemProperty(PropertyMemberElement.Create(SusieContext.Current, nameof(SusieContext.IsEnableSusie))),
                    new SettingItemProperty(PropertyMemberElement.Create(SusieContext.Current, nameof(SusieContext.SusiePluginPath)))
                    {
                        IsEnabled = new IsEnabledPropertyValue(SusieContext.Current, nameof(SusieContext.IsEnableSusie)),
                    }),

                new SettingItemSection("優先設定",
                    new SettingItemProperty(PropertyMemberElement.Create(SusieContext.Current, nameof(SusieContext.IsFirstOrderSusieImage))),
                    new SettingItemProperty(PropertyMemberElement.Create(SusieContext.Current, nameof(SusieContext.IsFirstOrderSusieArchive))))
                {
                    IsEnabled = new IsEnabledPropertyValue(SusieContext.Current, nameof(SusieContext.IsEnableSusie)),
                },
            };
        }
    }

    public class SettingPageSusieINPlugin : SettingPage
    {
        public SettingPageSusieINPlugin() : base("画像プラグイン")
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
        public SettingPageSusieAMPlugin() : base("書庫プラグイン")
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
