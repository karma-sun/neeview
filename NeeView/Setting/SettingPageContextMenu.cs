using NeeLaboratory.Windows.Input;
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
            var linkCommand = new RelayCommand(() => this.IsSelected = true);

            this.IsScrollEnabled = false;

            this.Items = new List<SettingItem>
            {
                new SettingItemSection(Properties.Resources.SettingPageContextMenuEdit,
                    new SettingItemContextMenu()
                    {
                        SearchResultItem = new SettingItemLink(Properties.Resources.SettingPageContextMenuEdit, linkCommand){ IsContentOnly = true }
                    }
                )
            };
        }
    }
}
