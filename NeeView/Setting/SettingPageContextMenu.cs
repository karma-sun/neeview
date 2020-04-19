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

            var section = new SettingItemSection(Properties.Resources.SettingPageContextMenuEdit);
            section.Children.Add(new SettingItemContextMenu() { SearchResultItem = new SettingItemLink(Properties.Resources.SettingPageContextMenuEdit, linkCommand) { IsContentOnly = true } });
            this.Items = new List<SettingItem>() { section };
        }
    }
}
