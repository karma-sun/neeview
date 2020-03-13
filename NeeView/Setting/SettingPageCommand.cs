using NeeView.Windows.Property;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NeeView.Setting
{
    class SettingPageCommand : SettingPage
    {
        public SettingPageCommand() : base(Properties.Resources.SettingPageCommand)
        {
            this.Children = new List<SettingPage>
            {
                new SettingPageCommandMain(),
            };

            this.Items = new List<SettingItem>
            {
                new SettingItemSection(Properties.Resources.SettingPageCommandGeneralAdvance,
                    new SettingItemProperty(PropertyMemberElement.Create(MainWindowModel.Current, nameof(MainWindowModel.IsAccessKeyEnabled))),
                    new SettingItemProperty(PropertyMemberElement.Create(CommandTable.Current, nameof(CommandTable.IsReversePageMove))),
                    new SettingItemSubProperty(PropertyMemberElement.Create(CommandTable.Current, nameof(CommandTable.IsReversePageMoveWheel)))
                    {
                        IsEnabled = new IsEnabledPropertyValue(CommandTable.Current, nameof(CommandTable.IsReversePageMove)),
                    }),
                new SettingItemSection(Properties.Resources.SettingPageCommandScipt,
                    new SettingItemProperty(PropertyMemberElement.Create(CommandTable.Current, nameof(CommandTable.IsScriptFolderEnabled))),
                    new SettingItemProperty(PropertyMemberElement.Create(CommandTable.Current, nameof(CommandTable.ScriptFolder)))
                    {
                        IsEnabled = new IsEnabledPropertyValue(CommandTable.Current, nameof(CommandTable.IsScriptFolderEnabled)),
                    }),
            };
        }
    }

    class SettingPageCommandMain : SettingPage
    {
        public SettingPageCommandMain() : base(Properties.Resources.SettingPageCommandMain)
        {
            this.IsScrollEnabled = false;

            this.Items = new List<SettingItem>
            {
                new SettingItemCommand(),
            };
        }
    }

}
