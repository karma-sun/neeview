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
                    new SettingItemProperty(PropertyMemberElement.Create(Config.Current.Command, nameof(CommandConfig.IsReversePageMove))),
                    new SettingItemSubProperty(PropertyMemberElement.Create(Config.Current.Command, nameof(CommandConfig.IsReversePageMoveWheel)))
                    {
                        IsEnabled = new IsEnabledPropertyValue(Config.Current.Command, nameof(CommandConfig.IsReversePageMove)),
                    }),
                new SettingItemSection(Properties.Resources.SettingPageCommandScipt,
                    new SettingItemProperty(PropertyMemberElement.Create(Config.Current.Script, nameof(ScriptConfig.IsScriptFolderEnabled))),
                    new SettingItemProperty(PropertyMemberElement.Create(Config.Current.Script, nameof(ScriptConfig.ScriptFolder), Config.Current.Script.GetDefaultScriptFolder()))
                    {
                        IsEnabled = new IsEnabledPropertyValue(Config.Current.Script, nameof(ScriptConfig.IsScriptFolderEnabled)),
                        IsStretch = true,
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
