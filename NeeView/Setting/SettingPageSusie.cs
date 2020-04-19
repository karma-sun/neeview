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
            this.Items = new List<SettingItem>
            {
                new SettingItemSection(Properties.Resources.SettingPageSusieGeneralGeneral, Properties.Resources.SettingPageSusieGeneralGeneralTips,
                
                    new SettingItemProperty(PropertyMemberElement.Create(Config.Current.Susie, nameof(SusieConfig.IsEnabled))),
                    new SettingItemProperty(PropertyMemberElement.Create(Config.Current.Susie, nameof(SusieConfig.SusiePluginPath)))
                    {
                        IsStretch = true,
                        IsEnabled = new IsEnabledPropertyValue(Config.Current.Susie, nameof(SusieConfig.IsEnabled)),
                    },

                    new SettingItemProperty(PropertyMemberElement.Create(Config.Current.Susie, nameof(SusieConfig.IsFirstOrderSusieImage))),
                    new SettingItemProperty(PropertyMemberElement.Create(Config.Current.Susie, nameof(SusieConfig.IsFirstOrderSusieArchive)))
                ),

                new SettingItemSection(Properties.Resources.SettingPageSusieImagePlugin,
                    new SettingItemSusiePlugin(SusiePluginType.Image)
                ),

                new SettingItemSection(Properties.Resources.SettingPageSusieArchivePlugin,
                    new SettingItemSusiePlugin(SusiePluginType.Archive)
                ),

            };
        }
    }

}
