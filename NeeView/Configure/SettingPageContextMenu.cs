using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NeeView.Configure
{
    class SettingPageContextMenu : SettingPage
    {
        public SettingPageContextMenu() : base("コンテキストメニュー")
        {
            this.Children = new List<SettingPage>
            {
                new SettingPageContextMenuEdit(),
            };
        }
    }

    class SettingPageContextMenuEdit : SettingPage
    {
        public SettingPageContextMenuEdit() : base("設定")
        {
            this.Items = new List<SettingItem>
            {
                new SettingItemContextMenu(),
            };
        }
    }
}
