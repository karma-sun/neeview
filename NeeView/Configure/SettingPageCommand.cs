using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NeeView.Configure
{
    class SettingPageCommand : SettingPage
    {
        public SettingPageCommand() : base("コマンド")
        {
            this.Children = new List<SettingPage>
            {
                new SettingPageCommandGeneral(),
            };
        }
    }

    class SettingPageCommandGeneral : SettingPage
    {
        public SettingPageCommandGeneral() : base("コマンド設定")
        {
            this.IsScrollEnabled = false;

            this.Items = new List<SettingItem>
            {
                new SettingItemCommand(),
            };
        }
    }
}
