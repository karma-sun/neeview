using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NeeView.Setting
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
        public SettingPageContextMenuEdit() : base("コンテキストメニュー設定")
        {
            this.IsScrollEnabled = false;

            this.Items = new List<SettingItem>
            {
                new SettingItemContextMenu(),
            };
        }
    }
}
