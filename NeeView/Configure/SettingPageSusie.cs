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
        public SettingPageSusieGeneral() : base("全般")
        {
            this.Items = new List<SettingItem>
            {
                new SettingItemSection("全般",
                    new SettingItemProperty(PropertyMemberElement.Create(SusieContext.Current, nameof(SusieContext.IsEnableSusie))),
                    new SettingItemProperty(PropertyMemberElement.Create(SusieContext.Current, nameof(SusieContext.SusiePluginPath)))),
            };
        }
    }

    public class SettingPageSusieINPlugin : SettingPage
    {
        public SettingPageSusieINPlugin() : base("画像プラグイン")
        {
            this.Items = new List<SettingItem>
            {
                new SettingItemSection("Susie画像プラグイン",
                    new SettingItemProperty(PropertyMemberElement.Create(SusieContext.Current, nameof(SusieContext.IsFirstOrderSusieImage))),
                    new SettingItemSusiePlugin(Susie.SusiePluginType.Image)),
            };
        }
    }

    public class SettingPageSusieAMPlugin : SettingPage
    {
        public SettingPageSusieAMPlugin() : base("書庫プラグイン")
        {
            this.Items = new List<SettingItem>
            {
                new SettingItemSection("Susie書庫プラグイン",
                    new SettingItemProperty(PropertyMemberElement.Create(SusieContext.Current, nameof(SusieContext.IsFirstOrderSusieArchive))),
                    new SettingItemSusiePlugin(Susie.SusiePluginType.Archive)),
            };
        }
    }
}
