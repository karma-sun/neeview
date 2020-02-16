using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NeeView.Setting
{
    class SettingPageContextMenu : SettingPage
    {
        public SettingPageContextMenu() : base(Properties.Resources.SettingPageContextMenu)
        {
             this.IsScrollEnabled = false;

            this.Items = new List<SettingItem>
            {
                new SettingItemContextMenu(),
            };
        }
    }
}
