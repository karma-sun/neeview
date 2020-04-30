using NeeLaboratory.Windows.Input;
using NeeView.Windows.Property;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NeeView.Setting
{
    /// <summary>
    /// SettingPage: Command
    /// </summary>
    class SettingPageCommand : SettingPage
    {
        public SettingPageCommand() : base(Properties.Resources.SettingPageCommand)
        {
            this.Children = new List<SettingPage>
            {
                new SettingPageCommandList(),
                new SettingPageContextMenu(),
            };

            this.Items = new List<SettingItem>();

            var section = new SettingItemSection(Properties.Resources.SettingPageCommandGeneralAdvance);
            section.Children.Add(new SettingItemProperty(PropertyMemberElement.Create(Config.Current.Command, nameof(CommandConfig.IsAccessKeyEnabled))));
            section.Children.Add(new SettingItemProperty(PropertyMemberElement.Create(Config.Current.Command, nameof(CommandConfig.IsReversePageMove))));
            section.Children.Add(new SettingItemSubProperty(PropertyMemberElement.Create(Config.Current.Command, nameof(CommandConfig.IsReversePageMoveWheel)))
            {
                IsEnabled = new IsEnabledPropertyValue(Config.Current.Command, nameof(CommandConfig.IsReversePageMove)),
            });
            this.Items.Add(section);

            section = new SettingItemSection(Properties.Resources.SettingPageCommandScipt);
            section.Children.Add(new SettingItemProperty(PropertyMemberElement.Create(Config.Current.Script, nameof(ScriptConfig.IsScriptFolderEnabled))));
            section.Children.Add(new SettingItemProperty(PropertyMemberElement.Create(Config.Current.Script, nameof(ScriptConfig.ScriptFolder), new PropertyMemberElementOptions() { EmptyValue = Config.Current.Script.GetDefaultScriptFolder() })) 
            { 
                IsStretch = true,
            });
            this.Items.Add(section);
        }
    }


    /// <summary>
    /// SettingPage: CommandList
    /// </summary>
    class SettingPageCommandList : SettingPage
    {
        public SettingPageCommandList() : base(Properties.Resources.SettingPageCommandMain)
        {
            var linkCommand = new RelayCommand(() => this.IsSelected = true);

            this.IsScrollEnabled = false;

            var section = new SettingItemSection(Properties.Resources.SettingPageCommandMain);
            section.Children.Add(new SettingItemCommand() { SearchResultItem = new SettingItemLink(Properties.Resources.SettingPageCommandMain, linkCommand) { IsContentOnly = true } });
            this.Items = new List<SettingItem>() { section };
        }
    }

    /// <summary>
    /// SettingPage: ContextMenu
    /// </summary>
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
