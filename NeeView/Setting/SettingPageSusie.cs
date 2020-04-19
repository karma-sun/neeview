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
            this.Items = new List<SettingItem>();

            var section = new SettingItemSection(Properties.Resources.SettingPageSusieGeneralGeneral, Properties.Resources.SettingPageSusieGeneralGeneralTips);
            section.Children.Add(new SettingItemProperty(PropertyMemberElement.Create(Config.Current.Susie, nameof(SusieConfig.IsEnabled))));
            section.Children.Add(new SettingItemProperty(PropertyMemberElement.Create(Config.Current.Susie, nameof(SusieConfig.SusiePluginPath)))
            {
                IsStretch = true,
                IsEnabled = new IsEnabledPropertyValue(Config.Current.Susie, nameof(SusieConfig.IsEnabled)),
            });
            section.Children.Add(new SettingItemProperty(PropertyMemberElement.Create(Config.Current.Susie, nameof(SusieConfig.IsFirstOrderSusieImage))));
            section.Children.Add(new SettingItemProperty(PropertyMemberElement.Create(Config.Current.Susie, nameof(SusieConfig.IsFirstOrderSusieArchive))));
            this.Items.Add(section);

            section = new SettingItemSection(Properties.Resources.SettingPageSusieImagePlugin);
            section.Children.Add(new SettingItemSusiePlugin(SusiePluginType.Image));
            this.Items.Add(section);

            section = new SettingItemSection(Properties.Resources.SettingPageSusieArchivePlugin);
            section.Children.Add(new SettingItemSusiePlugin(SusiePluginType.Archive));
            this.Items.Add(section);
        }
    }

}
